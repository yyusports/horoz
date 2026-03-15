namespace Sportify.Services
{
    public interface IEmailService
    {
        /// <summary>Belirtilen adrese e-posta gönderir. Hata durumunda exception fırlatır.</summary>
        Task GonderAsync(string aliciEmail, string konu, string icerik);
    }
}
