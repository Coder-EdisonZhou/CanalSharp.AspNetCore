using System;

namespace CanalSharp.AspNetCore.Middleware.Utils
{
    /// <summary>
    /// 日期格式帮助类
    /// 支持日期与时间戳格式的互相转换
    /// </summary>
    public static class DateConvertUtil
    {
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime ToDateTime(long timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long time = timeStamp * 10000;
            TimeSpan toNow = new TimeSpan(time);

            return dtStart.Add(toNow);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">DateTime时间格式</param>
        /// <returns>Long型Unix时间戳格式</returns>
        public static long ToTimeStamp(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalMilliseconds;
        }
    }
}
