using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RFIDP2P3_API.Helpers;

public class MfaLogHelper
{
    private static readonly object _lock = new();
    private const string DefaultDir = "Logs";
    
    // Cache timezone agar tidak Find setiap kali
    private static readonly TimeZoneInfo _tz =
        TryGetTz("SE Asia Standard Time") ?? TryGetTz("Asia/Jakarta") ?? TimeZoneInfo.Utc;

    // Compiled regex untuk performa
    private static readonly Regex RxOtp = new(
        @"(?i)(""?(OTP|Code)""?\s*[:=]\s*""?)([0-9A-Za-z]+)(""?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RxSecret = new(
        @"(?i)(""?Secret""?\s*[:=]\s*""?)([0-9A-Za-z]+)(""?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Opsional tambahan (aktifkan kalau perlu)
    private static readonly Regex RxToken = new(
        @"(?i)(""?(Token|Authorization)""?\s*[:=]\s*""?)([A-Za-z0-9\-\._~\+\/=]+)(""?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RxEmail = new(
        @"(?i)(""?(Email|E?mail)""?\s*[:=]\s*""?)([^""\s@]+@[^""\s@]+\.[^""\s@]+)(""?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    
    public static void Info(string context, object? data = null) => Write("INFO", context, data);
    public static void Error(string context, object? data = null) => Write("ERROR", context, data);


    private static void Write(string level, string context, object? data)
    {
        try
        {
            Directory.CreateDirectory(DefaultDir);

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz);
            var fileDate = nowLocal.ToString("yyyy-MM-dd");
            var file = Path.Combine(DefaultDir, $"mfa-{fileDate}.log");

            // Jaga supaya log tetap single-line (hindari injeksi newline)
            context = OneLine(context);

            string payload = data == null ? "" :
                " | data=" + MaskSensitive(JsonSerializer.Serialize(data));

            var line = $"{nowLocal:yyyy-MM-dd HH:mm:ss.fff zzz} WIB | {level} | {context}{payload}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(file, line, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            try
            {
                Console.Error.WriteLine($"[LogWriteError] {DateTime.UtcNow:o} {ex.Message}");
            }
            catch { /* swallow */ }
        }
    }
    
    private static string MaskSensitive(string text)
    {
        // Single-line sanitizer dulu
        text = OneLine(text);

        // OTP / Code → mask tail (sisa 2)
        text = RxOtp.Replace(text, m => $"{m.Groups[1].Value}{MaskTail(m.Groups[3].Value, 2)}{m.Groups[4].Value}");

        // Secret → mask head (sisa 3)
        text = RxSecret.Replace(text, m => $"{m.Groups[1].Value}{MaskHead(m.Groups[2].Value, 3)}{m.Groups[3].Value}");

        // Tambahan opsional
        text = RxToken.Replace(text, m => $"{m.Groups[1].Value}{MaskHead(m.Groups[3].Value, 3)}{m.Groups[4].Value}");
        text = RxEmail.Replace(text, m => $"{m.Groups[1].Value}{MaskEmail(m.Groups[3].Value)}{m.Groups[4].Value}");

        return text;
    }
    
    private static string OneLine(string s) =>
        (s ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static string MaskTail(string s, int keep)
    {
        if (string.IsNullOrEmpty(s)) return s;
        keep = Math.Min(keep, s.Length);
        return new string('*', s.Length - keep) + s[^keep..];
    }

    private static string MaskHead(string s, int keep)
    {
        if (string.IsNullOrEmpty(s)) return s;
        keep = Math.Min(keep, s.Length);
        return s[..keep] + new string('*', s.Length - keep);
    }

    private static string MaskEmail(string email)
    {
        try
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var local = parts[0];
            var maskedLocal = local.Length <= 2 ? new string('*', local.Length)
                : local[..2] + new string('*', Math.Max(0, local.Length - 2));
            return maskedLocal + "@" + parts[1];
        }
        catch { return email; }
    }

    private static TimeZoneInfo? TryGetTz(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { return null; }
    }
}