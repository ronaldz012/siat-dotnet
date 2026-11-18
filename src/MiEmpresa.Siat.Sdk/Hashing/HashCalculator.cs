namespace MiEmpresa.Siat.Sdk.Hashing;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

public static class HashCalculator
{
    public static (string HashArchivo, string ArchivoBase64) ComprimirYHashear(string xmlContent)
    {
        var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
        var compressed = ComprimirGzip(xmlBytes);
        var hash = CalcularSha256(compressed);
        var archivoBase64 = Convert.ToBase64String(compressed);
        return (hash, archivoBase64);
    }

    public static (string HashArchivo, string ArchivoBase64) ComprimirYHashear(byte[] xmlBytes)
    {
        var compressed = ComprimirGzip(xmlBytes);
        var hash = CalcularSha256(compressed);
        var archivoBase64 = Convert.ToBase64String(compressed);
        return (hash, archivoBase64);
    }

    public static string CalcularSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(data);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static byte[] ComprimirGzip(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static byte[] DescomprimirGzip(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}