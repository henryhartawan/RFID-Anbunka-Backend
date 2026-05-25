namespace RFIDP2P3_API.Helpers;

public static class FileHelper
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
    {
        // --- DOCUMENTS ---
        { ".xlsx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".pptx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        
        { ".xls", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".doc", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".ppt", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        
        { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },

        // --- IMAGES ---
        { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".heic", new List<byte[]> { 
            new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 },
            new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x69, 0x66, 0x31 } 
        }},

        // --- VIDEOS ---
        { ".mkv", new List<byte[]> { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } },
        
        { ".mp4", new List<byte[]> { 
            new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 },
            new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }
        }},
        { ".mov", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70 } } },
        { ".3gp", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70, 0x33, 0x67, 0x70 } } }
    };
    
    public static (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file, int maxSizeInMb, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0) return (false, "No file uploaded.");

        if (file.Length > maxSizeInMb * 1024 * 1024)
            return (false, $"File size cannot exceed {maxSizeInMb}MB.");

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            return (false, $"Invalid format. Allowed: {string.Join(", ", allowedExtensions)}");

        if (_fileSignatures.TryGetValue(extension, out var signatures))
        {
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
                file.OpenReadStream().Position = 0;

                bool isMatch = signatures.Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
                if (!isMatch) return (false, "File content is spoofed or corrupted.");
            }
        }

        return (true, string.Empty);
    }

    public static string GenerateSafeUniqueFileName(string originalFileName)
    {
        string ext = Path.GetExtension(originalFileName);
        string uniqueName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        return uniqueName;
    }

    public static async Task<string> SaveFileAsync(IFormFile file, string destinationFolder)
    {
        if (!Directory.Exists(destinationFolder))
            Directory.CreateDirectory(destinationFolder);

        string safeName = GenerateSafeUniqueFileName(file.FileName);
        string fullPath = Path.Combine(destinationFolder, safeName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return safeName;
    }
}