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

        public static string ToShamsi(this string value) // use this word for use the method for all DateTime variables in project
        {
            var date = value.Split(' ')[0].Split('/');
            var time = value.Split(' ')[1].Split(':');
            DateTime dateTime = new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]), Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
            PersianCalendar persianCalendar = new PersianCalendar();
            string shamsiDate = $"{persianCalendar.GetYear(dateTime)}/{persianCalendar.GetMonth(dateTime)}/{persianCalendar.GetDayOfMonth(dateTime)} {persianCalendar.GetHour(dateTime)}:{persianCalendar.GetMinute(dateTime)}:{persianCalendar.GetSecond(dateTime)}";
            return shamsiDate;
        }

        public static DateTime ToShamsi(this DateTime value) // use this word for use the method for all DateTime variables in project
        {
            var strDate = $"{value.Year}/{value.Month}/{value.Day} {value.Hour}:{value.Minute}:{value.Second}";
            var strshamsi = ToShamsi(strDate);
            var date = strshamsi.Split(' ')[0].Split('/');
            var time = strshamsi.Split(' ')[1].Split(':');
            DateTime shamsiDate = new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]), Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
            return shamsiDate;

        }
    }
}