namespace Sportify.Services
{
    public class EmailAyarlari
    {
        public string SmtpSunucu { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderAd { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
    }
}
