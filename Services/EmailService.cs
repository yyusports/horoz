using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Sportify.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailAyarlari _ayarlar;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailAyarlari> ayarlar, ILogger<EmailService> logger)
        {
            _ayarlar = ayarlar.Value;
            _logger = logger;
        }

        public async Task GonderAsync(string aliciEmail, string konu, string icerik)
        {
            try
            {
                using var client = new SmtpClient(_ayarlar.SmtpSunucu, _ayarlar.SmtpPort)
                {
                    EnableSsl = true, // Güvenli bağlantı için şarttır
                    Credentials = new NetworkCredential(_ayarlar.SenderEmail, _ayarlar.SenderPassword),
                    Timeout = 10000 // 10 saniye zaman aşımı
                };

                var mesaj = new MailMessage
                {
                    From       = new MailAddress(_ayarlar.SenderEmail, _ayarlar.SenderAd),
                    Subject    = konu,
                    Body       = icerik,
                    IsBodyHtml = true
                };
                mesaj.To.Add(aliciEmail);

                await client.SendMailAsync(mesaj);
                _logger.LogInformation("E-posta başarıyla gönderildi: {Email}", aliciEmail);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP Hatası: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                throw new Exception($"SMTP Hatası ({ex.StatusCode}): {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Genel E-posta Hatası: {Message}", ex.Message);
                throw new Exception($"Bağlantı Hatası: {ex.Message}");
            }
        }
    }
}
