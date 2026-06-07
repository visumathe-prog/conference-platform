using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using Certificate.Service.Models;

namespace Certificate.Service.Services;

public class PdfCertificateService : ICertificateService
{
    private readonly string _storagePath;
    private readonly string _baseUrl;

    public PdfCertificateService(IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _storagePath = configuration["Certificate:StoragePath"] ?? "./certificates";
        _baseUrl = configuration["Certificate:BaseUrl"] ?? "https://certificates.conference.com";
        
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<byte[]> GenerateCertificateAsync(CertificateData data)
    {
        var qrCodeData = GenerateQrCode($"{_baseUrl}/verify/{data.CertificateId}");
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.Background(Colors.White);
                
                page.Header()
                    .AlignCenter()
                    .Text("CERTIFICATE OF ATTENDANCE")
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);
                
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Item().AlignCenter().Text($"This certificate is awarded to")
                            .FontSize(14).Italic();
                        
                        column.Item().AlignCenter().Text($"{data.UserFirstName} {data.UserLastName}")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                        
                        column.Item().AlignCenter().Text($"for attending the conference")
                            .FontSize(14);
                        
                        column.Item().AlignCenter().Text($"{data.EventTitle}")
                            .FontSize(18).Bold();
                        
                        column.Item().AlignCenter().Text($"held on {data.EventDate:MMMM dd, yyyy}")
                            .FontSize(12);
                        
                        column.Item().PaddingTop(1, Unit.Centimetre).AlignCenter().Text($"Certificate ID: {data.CertificateId}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                        
                        column.Item().PaddingTop(1, Unit.Centimetre).AlignCenter().Image(Convert.FromBase64String(qrCodeData))
                            .FitArea(80, 80);
                    });
                
                page.Footer()
                    .AlignCenter()
                    .Text("© Conference Platform. All rights reserved.")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
            });
        });
        
        var pdfBytes = document.GeneratePdf();
        return await Task.FromResult(pdfBytes);
    }

    public async Task<string> SaveCertificateAsync(byte[] pdfBytes, string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        await File.WriteAllBytesAsync(filePath, pdfBytes);
        return $"{_baseUrl}/download/{fileName}";
    }

    public async Task<byte[]?> GetCertificateAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath))
            return null;
        
        return await File.ReadAllBytesAsync(filePath);
    }

    public bool VerifyCertificate(string certificateId)
    {
        var certificates = Directory.GetFiles(_storagePath, "*.pdf");
        return certificates.Any(c => c.Contains(certificateId));
    }

    private string GenerateQrCode(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new Base64QRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
