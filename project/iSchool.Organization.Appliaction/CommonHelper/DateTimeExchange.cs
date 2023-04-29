using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    /// <summary>
    /// 时间格式转换
    /// </summary>
    public class DateTimeExchange
    {
        /// <summary>
        ///  时间戳转本地时间-时间戳精确到秒
        /// </summary> 
        public static DateTime ToLocalTimeDateBySeconds(long unix)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(unix);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        ///  时间转时间戳Unix-时间戳精确到秒
        /// </summary> 
        public static long ToUnixTimestampBySeconds(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeSeconds();
        }


        /// <summary>
        ///  时间戳转本地时间-时间戳精确到毫秒
        /// </summary> 
        public static DateTime ToLocalTimeDateByMilliseconds(long unix)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(unix);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        ///  时间转时间戳Unix-时间戳精确到毫秒
        /// </summary> 
        public static long ToUnixTimestampByMilliseconds(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 随机取时间段
        /// (string,string)(开始时间,结束时间)
        /// </summary>
        /// <param name="adddays"></param>
        /// <returns></returns>
        public static (string, string) GetTandomTime(int adddays)
        {
            DateTime dateTime = new DateTime(2020,11,30);
            //int year = dateTime.Year-1;
            int year = 2020;
            Random r = new Random();
            //int month = r.Next(1, dateTime.Month);
            int month = r.Next(1, 11);
            int day = 1;
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                //case 12:
                    day = r.Next(1, 31);
                    break;
                case 4:
                case 6:
                case 9:
                case 11:
                    day = r.Next(1, 30);
                    break;
                default:
                    day = r.Next(1, 28);
                    break;
            }
            DateTime time = new DateTime(year, month, day);
            if (time > dateTime)
                time = dateTime;
            var time1 = time.ToString("yyyy/MM/dd");
            var time2 = time.AddDays(adddays).ToString("yyyy/MM/dd");
            if (adddays > 0)
                return (time1, time2);
            else
                return (time2, time1);
        }
    }
}
