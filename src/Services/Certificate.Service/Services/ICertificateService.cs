using Certificate.Service.Models;

namespace Certificate.Service.Services;

public interface ICertificateService
{
    Task<byte[]> GenerateCertificateAsync(CertificateData data);
    Task<string> SaveCertificateAsync(byte[] pdfBytes, string fileName);
    Task<byte[]?> GetCertificateAsync(string fileName);
    bool VerifyCertificate(string certificateId);
}
