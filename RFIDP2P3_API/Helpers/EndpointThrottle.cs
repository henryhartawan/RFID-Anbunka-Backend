using System.Collections.Concurrent;

namespace RFIDP2P3_API.Helpers
{

    using AttemptMap = System.Collections.Concurrent.ConcurrentDictionary<
        string, (int Count, System.DateTime WindowStartUtc, System.DateTime LastHitUtc)>;

    public static class EndpointThrottle
    {
        public static string BuildKey(HttpContext ctx, string? userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
                return $"uid:{userId}";

            if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) &&
                !string.IsNullOrWhiteSpace(xff))
                return $"ip:{xff.ToString().Split(',')[0].Trim()}";

            return $"ip:{ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        }

        public static bool ShouldThrottle(
            AttemptMap dict,
            string key,
            int maxCount,
            TimeSpan window,
            out int retryAfterSec,
            out int currentCount,
            out int windowLeftSec)
        {
            retryAfterSec = 0;
            currentCount = 0;
            windowLeftSec = 0;

            if (!dict.TryGetValue(key, out var st)) return false;

            var now = DateTime.UtcNow;
            var elapsed = now - st.WindowStartUtc;

            if (elapsed >= window) return false;

            currentCount = st.Count;
            windowLeftSec = Math.Max(0, (int)(window - elapsed).TotalSeconds);

            if (st.Count >= maxCount)
            {
                retryAfterSec = windowLeftSec > 0 ? windowLeftSec : 1;
                return true;
            }

            return false;
        }

        // Hit setiap request (contoh: anti-spam kirim OTP)
        public static void RegisterHit(
            AttemptMap dict,
            string key,
            TimeSpan window)
        {
            var now = DateTime.UtcNow;
            dict.AddOrUpdate(
                key,
                _ => (1, now, now),
                (_, old) =>
                {
                    if (now - old.WindowStartUtc >= window)
                        return (1, now, now);

                    return (old.Count + 1, old.WindowStartUtc, now);
                });
        }

        // Hit hanya saat gagal
        public static void RegisterFailure(
            AttemptMap dict,
            string key,
            TimeSpan window)
            => RegisterHit(dict, key, window);

        public static void Reset(AttemptMap dict, string key)
            => dict.TryRemove(key, out _);

        public static (int count, int leftSec) GetState(
            AttemptMap dict,
            string key,
            TimeSpan window)
        {
            if (!dict.TryGetValue(key, out var st)) return (0, 0);

            var now = DateTime.UtcNow;
            var elapsed = now - st.WindowStartUtc;
            if (elapsed >= window) return (0, 0);

            var left = Math.Max(0, (int)(window - elapsed).TotalSeconds);
            return (st.Count, left);
        }
    }
}
