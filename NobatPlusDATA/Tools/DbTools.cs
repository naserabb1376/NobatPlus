using NobatPlusDATA.DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace NobatPlusDATA.Tools
{
    public static class DbTools
    {
        private static NobatPlusContext _context;

        public enum BaseRole : long
        {
            Customer = 1,
            Stylist = 2,
            Salon = 3,
            Admin = 4
        }

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

        public sealed class BookingDurationRow
        {
            public long BookingID { get; set; }
            public int TotalDurationMinutes { get; set; }
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
            if (pageIndex <= 0)
            {
                list = list.Take(0);
            }
            else if (pageIndex > 0 && pageSize > 0)
            {
                int skipSize = (pageIndex - 1) * pageSize;
                list = list.Skip(skipSize).Take(pageSize);
            }
            return list;
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, string sorting)
        {
            if (string.IsNullOrEmpty(sorting))
            {
                return query;
            }

            // تجزیه رشته مرتب سازی به ترتیب‌های جداگانه
            var sortingOptions = sorting.Split(',');

            // ساختن دستور مرتب سازی
            var sortingString = new StringBuilder();
            foreach (var sortOption in sortingOptions)
            {
                var sortParts = sortOption.Trim().Split('-');

                if (sortParts.Length == 2)
                {
                    var field = sortParts[0];
                    var direction = sortParts[1].ToLower();

                    // اضافه کردن به رشته دستور مرتب سازی
                    if (sortingString.Length > 0)
                    {
                        sortingString.Append(", ");
                    }

                    sortingString.Append($"{field} {direction}");
                }
            }

            // اعمال مرتب سازی به صورت پویا با استفاده از System.Linq.Dynamic.Core
            return query.OrderBy(sortingString.ToString());
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

        public static string FixPrice(this decimal value)
        {
            return value.ToString("#,0");
        }
        private const int PasswordSaltSize = 16;
        private const int PasswordHashSize = 32;
        private const int PasswordIterations = 100_000;
        private const string PasswordHashPrefix = "PBKDF2-SHA256";

        public static string ToHash(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            var salt = RandomNumberGenerator.GetBytes(PasswordSaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                value,
                salt,
                PasswordIterations,
                HashAlgorithmName.SHA256,
                PasswordHashSize);

            return $"{PasswordHashPrefix}${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(this string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            if (storedHash.StartsWith($"{PasswordHashPrefix}$", StringComparison.Ordinal))
                return VerifyPbkdf2Password(password, storedHash);

            var legacyHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacyHash),
                Encoding.UTF8.GetBytes(storedHash));
        }

        public static bool NeedsPasswordRehash(this string storedHash)
        {
            return string.IsNullOrEmpty(storedHash) ||
                   !storedHash.StartsWith($"{PasswordHashPrefix}$", StringComparison.Ordinal);
        }

        private static bool VerifyPbkdf2Password(string password, string storedHash)
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4 || parts[0] != PasswordHashPrefix)
                return false;

            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
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
            return $"{Convert.ToBase64String(randomBytes)}{Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTime.Now.ToShamsiString()))}";
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
            if (totalItemsCount == 0 || pageItemsCount == 0) return 1;
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

        public static DateTime StringToDate(this string stringDateTime)
        {
            if (string.IsNullOrEmpty(stringDateTime)) return DateTime.Now.ToShamsi();
            if (stringDateTime.Split(' ').Length < 2)
            {
                stringDateTime += " 00:00:00";
            }
            var arr = stringDateTime.Split(' ');
            var stringDateArr = arr[0].Split('/');
            var stringTimeArr = arr[1].Split(':');
            var date = new DateTime(int.Parse(stringDateArr[0]), int.Parse(stringDateArr[1]), int.Parse(stringDateArr[2]), int.Parse(stringTimeArr[0]), int.Parse(stringTimeArr[1]), int.Parse(stringTimeArr[2]), 0);
            return date;
        }

        public static string DateToString(this DateTime date)
        {
            string stringDate = $"{date.Year}/{date.Month}/{date.Day} {date.Hour}:{date.Minute}";
            return stringDate;
        }

   
    }
}
