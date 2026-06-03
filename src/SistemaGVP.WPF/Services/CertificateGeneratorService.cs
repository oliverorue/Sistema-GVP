using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SistemaGVP.WPF.Services;

public static class CertificateGeneratorService
{
    private static readonly string CertDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GVP");
    private static readonly string CertPath = Path.Combine(CertDir, "gvp-scanner.pfx");
    private const string CertPassword = "gvp-scanner-2025"; // fixed password for reusability

    public static X509Certificate2 GetOrCreateCertificate()
    {
        if (File.Exists(CertPath))
        {
            try
            {
                return new X509Certificate2(CertPath, CertPassword);
            }
            catch
            {
                // Corrupt cert, regenerate
                File.Delete(CertPath);
            }
        }

        Directory.CreateDirectory(CertDir);

        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=GVP Local Scanner",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        // Basic constraints: not a CA
        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, true));

        // Key usage
        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        // Subject Alternative Name: localhost + IPs
        var san = new SubjectAlternativeNameBuilder();
        san.AddIpAddress(IPAddress.Loopback);
        san.AddIpAddress(IPAddress.IPv6Loopback);
        san.AddDnsName("localhost");

        // Add local LAN IPs
        var hostName = Dns.GetHostName();
        try
        {
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    san.AddIpAddress(ip);
                }
            }
        }
        catch { }
        san.AddDnsName(hostName);

        req.CertificateExtensions.Add(san.Build());

        var cert = req.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(10));

        // Export with private key
        var pfxBytes = cert.Export(X509ContentType.Pfx, CertPassword);
        File.WriteAllBytes(CertPath, pfxBytes);

        return new X509Certificate2(pfxBytes, CertPassword);
    }
}
