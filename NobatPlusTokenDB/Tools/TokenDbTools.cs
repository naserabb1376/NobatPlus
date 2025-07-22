using NobatPlusTokenDB.DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusTokenDB.Tools
{
    public static class TokenDbTools
    {
        private static RefreshTokenDBContext _context;

        public static RefreshTokenDBContext GetDbContext()
        {
            //if (_context == null)
            //{
            _context = new RefreshTokenDBContext();
            //}
            return _context;
        }

        public static List<T> ToPaging<T>(this List<T> list, int pageIndex = 1, int pageSize = 20)
        {
            if (pageSize > 0)
            {
                int skipSize = (pageIndex - 1) * pageSize;
                list = list.Skip(skipSize).Take(pageSize).ToList();
            }
            return list;
        }

        public static IQueryable<T> ToPaging<T>(this IQueryable<T>? list, int pageIndex = 1, int pageSize = 20)
        {
            if (pageSize > 0)
            {
                int skipSize = (pageIndex - 1) * pageSize;
                list = list.Skip(skipSize).Take(pageSize);
            }
            return list;
        }

       

        public static int GetPageCount(int totalItemsCount, int pageItemsCount)
        {
            if (totalItemsCount == 0 || pageItemsCount == 0) return 1;
            int pageCount = totalItemsCount / pageItemsCount;
            if (totalItemsCount % pageItemsCount != 0)
            {
                pageCount++;
            }
            return pageCount;
        }

        public static string ToShamsiString(this DateTime miladiDate)
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(miladiDate);
            int month = pc.GetMonth(miladiDate);
            int day = pc.GetDayOfMonth(miladiDate);
            int hour = pc.GetHour(miladiDate);
            int min = pc.GetMinute(miladiDate);
            int sec = pc.GetSecond(miladiDate);

            return $"{year:0000}/{month:00}/{day:00} {hour:00}:{min:00}:{sec:00}";
        }

        public static DateTime ToShamsi(this DateTime miladiDate)
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(miladiDate);
            int month = pc.GetMonth(miladiDate);
            int day = pc.GetDayOfMonth(miladiDate);
            int hour = pc.GetHour(miladiDate);
            int minute = pc.GetMinute(miladiDate);
            int second = pc.GetSecond(miladiDate);

            // ساخت یک DateTime با تاریخ شمسی (اما همچنان نوع آن Gregorian خواهد بود)
            return new DateTime(year, month, day, hour, minute, second, pc);
        }

        public static TimeSpan StringToTimeSpan(this string time)
        {
            return TimeSpan.ParseExact(time, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }

        public static string TimeSpanToString(this TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}