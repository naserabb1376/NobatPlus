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
        private static NobatPlusContext _context;

        public enum DiscountType
        {
            All = 0,
            Admin = 1,
            Customer = 2, 
            Stylist = 3, 
            StylistCustomer = 4, 
            Service = 5, 
            AdminService = 6,
            StylistService = 7,           
        }

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


           public static IQueryable<T> ToPaging<T>(this IQueryable<T>? list, int pageIndex = 1, int pageSize = 20)
        {
            if (pageSize > 0)
            {
                int skipSize = (pageIndex - 1) * pageSize;
                list = list.Skip(skipSize).Take(pageSize);
            }
            return list;
        }


        public static string ToShamsi(this string value) // use this word for use the method for all DateTime variables in project
        {
            var date = value.Split(' ')[0].Split('/');
            DateTime dateTime = new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]));
            PersianCalendar persianCalendar = new PersianCalendar();
            return persianCalendar.GetYear(dateTime) + "/" + persianCalendar.GetMonth(dateTime).ToString("00") + "/" + persianCalendar.GetDayOfMonth(dateTime).ToString("00") + " - " + DateTime.Now.ToString("HH:mm");
        }

        public static string FixPrice(this decimal value)
        {
            return value.ToString("#,0");
        }
        public static string ToHash(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string ToUnHash(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        public static void SaveLog(object log)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{log.ToString()}");
            sb.AppendLine(DateTime.Now.ToShortTimeString());
            sb.AppendLine($"--------------------------------");
            System.IO.File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(),
                      "wwwroot",
                      "log.txt"), sb.ToString());
        }

        public static string GenerateToken()
        {
            byte[] randomBytes = new byte[10]; // اندازه توکن را می‌توانید تغییر دهید
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }
            return $"{Convert.ToBase64String(randomBytes)}{Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy/MM/dd - HH:mm").ToShamsi()))}";
        }

        public static decimal RetreivePrice(this string finalPrice)
        {
            finalPrice = finalPrice.ToEnglishNumbers();
            for (int i = 0; i < finalPrice.Length; i++)
            {
                if (finalPrice[i] == ',') finalPrice.Remove(i, 1);
            }
            return decimal.Parse(finalPrice);
        }

        public static string ToEnglishNumbers(this string persianStr)
        {
            string[] persianDigits = { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };
            string[] englishDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            for (int i = 0; i < persianDigits.Length; i++)
            {
                persianStr = persianStr.Replace(persianDigits[i], englishDigits[i]);
            }

            return persianStr;
        }


        public static int GetPageCount(int totalItemsCount, int pageItemsCount)
        {
            if(totalItemsCount == 0) return 1;
            int pageCount = totalItemsCount / pageItemsCount;
            if (totalItemsCount % pageItemsCount != 0)
            {
                pageCount++;
            }
            return pageCount;
        }

        public static string ToSummary(this string? value)
        {
            string summary = value ?? "";
            if (value != null && value.Length > 70)
                summary = value.Substring(0, 70) + "...";
            return summary;
        }
    }
}
