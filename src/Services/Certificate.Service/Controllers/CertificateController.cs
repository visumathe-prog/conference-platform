using Microsoft.AspNetCore.Mvc;
using Certificate.Service.Services;
using Certificate.Service.Models;

namespace Certificate.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificateController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] CertificateData data)
    {
        var pdfBytes = await _certificateService.GenerateCertificateAsync(data);
        var fileName = $"certificate_{data.EventId}_{data.UserId}.pdf";
        
        var filePath = await _certificateService.SaveCertificateAsync(pdfBytes, fileName);
        
        return Ok(new { certificateUrl = filePath });
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> Download(string fileName)
    {
        var pdfBytes = await _certificateService.GetCertificateAsync(fileName);
        if (pdfBytes is null)
            return NotFound();

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpPost("verify/{certificateId}")]
    public IActionResult Verify(string certificateId)
    {
        var isValid = _certificateService.VerifyCertificate(certificateId);
        return Ok(new { isValid, certificateId });
    }
}
