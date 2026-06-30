using System.Collections.Concurrent;

namespace NobatPlusAPI.Tools
{
    /// <summary>
    /// ذخیره‌گاه موقت کدهای تأیید OTP با منطق انقضا.
    /// از ConcurrentDictionary استفاده می‌کند — thread-safe و بدون نیاز به migration.
    /// پس از تأیید موفق، کد بلافاصله حذف می‌شود (single-use).
    /// توجه: برای محیط multi-server باید به DistributedCache یا Redis منتقل شود.
    /// </summary>
    public static class OtpStore
    {
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _store = new();

        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

        public static void Save(string mobileNumber, string code, TimeSpan? expiry = null)
        {
            var normalizedMobile = NormalizeMobile(mobileNumber);
            _store[normalizedMobile] = (code, DateTime.UtcNow.Add(expiry ?? DefaultExpiry));
            CleanExpired();
        }

        public static bool Verify(string mobileNumber, string code)
        {
            var normalizedMobile = NormalizeMobile(mobileNumber);
            if (_store.TryGetValue(normalizedMobile, out var entry))
            {
                _store.TryRemove(normalizedMobile, out _);
                if (DateTime.UtcNow <= entry.Expiry && entry.Code == code)
                    return true;
            }
            return false;
        }

        private static string NormalizeMobile(string mobile)
            => (mobile ?? "").Trim().TrimStart('+').TrimStart('0');

        private static void CleanExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var key in _store.Keys.ToList())
            {
                if (_store.TryGetValue(key, out var entry) && entry.Expiry < now)
                    _store.TryRemove(key, out _);
            }
        }
    }
}
