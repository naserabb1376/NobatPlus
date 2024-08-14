using NobatPlusDATA.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Tools
{
    public static class DbTools
    {
        private static NobatPlusContext _context;

        public static NobatPlusContext GetDbContext()
        {
            if (_context == null)
            {
                _context = new NobatPlusContext();
            }
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
    }
}
