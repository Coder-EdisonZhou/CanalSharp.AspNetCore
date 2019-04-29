using CanalSharp.AspNetCore.Infrastructure;
using CanalSharp.AspNetCore.Utils;
using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Com.Alibaba.Otter.Canal.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CanalSharp.AspNetCore.CanalSharp
{
    public class CanalClientHandler : ICanalClientHandler, IDisposable
    {
        private readonly CanalOption _xdpCanalOption;
        private readonly ILogger<ICanalClientHandler> _canalLogger;
        private readonly CancellationTokenSource _cts;
        private readonly OutputOptions _options;

        private ICanalConnector _canalConnector;
        private ICanalRepository _canalRepository;
        private bool _disposed;
        private Task _compositeTask;

        public CanalClientHandler(CanalOption xdpCanalOption, OutputOptions options, ILogger<ICanalClientHandler> canalLogger)
        {
            _xdpCanalOption = xdpCanalOption;
            _canalLogger = canalLogger;
            _options = options;
            _cts = new CancellationTokenSource();
        }

        public void Initialize()
        {
            _canalLogger?.LogDebug("Starting to initialize log database.");

            _canalRepository = CanalRepositoryFactory.GetCanalRepositoryInstance(_options);
            _canalRepository.InitializeAsync();

            _canalLogger?.LogDebug("Finished to initialize log database.");
        }

        public void Start()
        {
            _canalLogger?.LogDebug($"### [{_xdpCanalOption.LogSource}] Canal Client starting...");

            Task.Factory.StartNew(
                () =>
                {
                    // 创建一个简单CanalClient连接对象（此对象不支持集群）
                    // 传入参数分别为 Canal-Server地址、端口、Destination、用户名、密码
                    _canalConnector = CanalConnectors.NewSingleConnector(
                        _xdpCanalOption.CanalServerIP,
                        _xdpCanalOption.CanalServerPort,
                        _xdpCanalOption.Destination,
                        _xdpCanalOption.UserName,
                        _xdpCanalOption.Password);

                    // 连接 Canal
                    _canalConnector.Connect();

                    // 订阅，同时传入Filter，如果不传则以Canal的Filter为准。
                    // Filter是一种过滤规则，通过该规则的表数据变更才会传递过来
                    _canalConnector.Subscribe(_xdpCanalOption.Filter);

                    while (true)
                    {
                        // 获取数据 BufferSize表示数据大小，单位为字节，默认会发送ACK来表示消费成功
                        var message = _canalConnector.Get(_xdpCanalOption.BufferSize);
                        // 也可以使用下面这个GetWithoutAck方法表示不发送ACK来表示消费成功
                        //var message = _canalConnector.GetWithoutAck(_xdpCanalOption.BufferSize);

                        // 批次id 可用于回滚
                        var batchId = message.Id;

                        if (batchId == -1 || message.Entries.Count <= 0)
                        {
                            Thread.Sleep(_xdpCanalOption.SleepTime);
                            continue;
                        }

                        // 记录变更数据到自定义业务持久化存储区域
                        if (message.Entries.Count > 0)
                        {
                            RecordChanges(message.Entries);
                        }
                    }
                },
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            _compositeTask = Task.CompletedTask;

            _canalLogger?.LogDebug($"### [{_xdpCanalOption.LogSource}] Canal Client started...");
        }

        public void Stop()
        {
            _canalLogger?.LogDebug($"### [{_xdpCanalOption.LogSource}] Canal Client stopping...");

            Dispose();

            _canalLogger?.LogDebug($"### [{_xdpCanalOption.LogSource}] Canal Client stoped...");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cts.Cancel();

            try
            {
                _compositeTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException ex)
            {
                var innerEx = ex.InnerExceptions[0];
                if (!(innerEx is OperationCanceledException))
                {
                    _canalLogger?.LogError($"### [{_xdpCanalOption.LogSource}] {innerEx.Message}");
                }
            }
        }

        #region 私有内部方法

        // 根据不同事件类型进行记录
        private void RecordChanges(List<Entry> entrys)
        {
            // 一个entry表示一个数据库变更
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
                {
                    continue;
                }

                RowChange rowChange = null;

                try
                {
                    // 获取行变更
                    rowChange = RowChange.Parser.ParseFrom(entry.StoreValue);
                }
                catch (Exception ex)
                {
                    _canalLogger?.LogError($"### [{_xdpCanalOption.LogSource}] {ex.Message}");
                    continue;
                }

                if (rowChange != null)
                {
                    // 获取变更行的事件类型
                    EventType eventType = rowChange.EventType;

                    foreach (var rowData in rowChange.RowDatas)
                    {
                        if (eventType == EventType.Delete)
                        {
                            // 如果是Delete事件类型
                            PostRecords(rowData.BeforeColumns.ToList(), entry, "Delete");
                        }
                        else if (eventType == EventType.Insert)
                        {
                            // 如果是Insert事件类型
                            PostRecords(rowData.AfterColumns.ToList(), entry, "Insert");
                        }
                        else
                        {
                            // 如果是Update事件类型
                            PostUpdatedRecords(rowData.BeforeColumns.ToList(), rowData.AfterColumns.ToList(), entry);
                        }
                    }
                }
            }
        }

        // 发送Insert或Delete事件类型的变更记录到指定服务的数据存储中
        private void PostRecords(List<Column> columns, Entry entry, string eventType)
        {
            _canalLogger?.LogDebug($"[{_xdpCanalOption.LogSource}]", $"### One {eventType} event on {entry.Header.SchemaName} recording.");

            StringBuilder recordBuilder = new StringBuilder();
            recordBuilder.Append("{");

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (i == columns.Count - 1)
                {
                    recordBuilder.Append($"\"{column.Name}\":{column.Value ?? string.Empty}");
                }
                else
                {
                    recordBuilder.Append($"\"{column.Name}\":{column.Value ?? string.Empty},");
                }
            }

            recordBuilder.Append("}");

            List<ChangeLog> changeLogs = new List<ChangeLog>();
            ChangeLog changeLog = new ChangeLog
            {
                SchemaName = entry.Header.SchemaName,
                TableName = entry.Header.TableName,
                EventType = entry.Header.EventType.ToString(),
                ExecuteTime = DateConvertUtil.ToDateTime(entry.Header.ExecuteTime)
            };

            switch (entry.Header.EventType)
            {
                case EventType.Insert:
                    changeLog.CurrentValue = recordBuilder.ToString();
                    break;
                case EventType.Delete:
                    changeLog.PreviousValue = recordBuilder.ToString();
                    break;
            }

            changeLogs.Add(changeLog);
            _canalRepository = CanalRepositoryFactory.GetCanalRepositoryInstance(_options);
            _canalRepository.SaveChangeHistoriesAsync(changeLogs);

            _canalLogger?.LogDebug($"[{_xdpCanalOption.LogSource}]", $"### One {eventType} event on {entry.Header.SchemaName} recorded.");
        }

        // 发送Update事件类型的变更记录到指定的数据存储中
        private void PostUpdatedRecords(List<Column> beforeColumns, List<Column> afterColumns, Entry entry)
        {
            _canalLogger?.LogDebug($"[{_xdpCanalOption.LogSource}]", $"### One Update event on {entry.Header.SchemaName} recording.");

            List<ChangeLog> changeLogs = new List<ChangeLog>();

            foreach (var before in beforeColumns)
            {
                foreach (var after in afterColumns)
                {
                    if (after.Updated && before.Index == after.Index)
                    {
                        ChangeLog changeLog = new ChangeLog
                        {
                            SchemaName = entry.Header.SchemaName,
                            TableName = entry.Header.TableName,
                            EventType = entry.Header.EventType.ToString(),
                            PreviousValue = before.Value,
                            CurrentValue = after.Value,
                            ExecuteTime = DateConvertUtil.ToDateTime(entry.Header.ExecuteTime),
                            ColumnName = after.Name
                        };

                        changeLogs.Add(changeLog);
                    }
                }
            }

            _canalRepository = CanalRepositoryFactory.GetCanalRepositoryInstance(_options);
            _canalRepository.SaveChangeHistoriesAsync(changeLogs);

            _canalLogger?.LogDebug($"[{_xdpCanalOption.LogSource}]", $"### One Update event on {entry.Header.SchemaName} recorded.");
        }

        #endregion
    }
}
