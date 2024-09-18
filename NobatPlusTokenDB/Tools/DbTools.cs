using NobatPlusDATA.DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Tools
{
    public static class DbTools
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


    }
}