using Sportify.Models;

namespace Sportify.Repositories
{
    public interface IUserRepository
    {
        /// <summary>E-posta ile kullanıcı getirir. Bulunamazsa null döner.</summary>
        Task<Kullanici?> EmailIleGetirAsync(string email);

        /// <summary>Verilen e-posta adresi veritabanında mevcut mu?</summary>
        Task<bool> EmailMevcutMuAsync(string email);

        /// <summary>Yeni kullanıcıyı veritabanına ekler.</summary>
        Task EkleAsync(Kullanici kullanici);

        /// <summary>Kullanıcının e-posta onay kodunu günceller.</summary>
        Task OnayKodunuGuncelleAsync(int kullaniciId, string kod);

        /// <summary>Kullanıcının e-postasını onaylı olarak işaretler ve kodu temizler.</summary>
        Task EmailOnaylaAsync(int kullaniciId);

        /// <summary>Kullanıcı adına göre kullanıcı getirir.</summary>
        Task<Kullanici?> KullaniciAdiIleGetirAsync(string kullaniciAdi);

        /// <summary>Kullanıcı bilgilerini günceller.</summary>
        Task GuncelleAsync(Kullanici kullanici);

        /// <summary>Kullanıcının bir takımı favori durumunu değiştirir (varsa kaldırır, yoksa ekler).</summary>
        Task<bool> ToggleFavoriteTeamAsync(int kullaniciId, int takimId);

        /// <summary>Kullanıcının favori takımlarını getirir.</summary>
        Task<List<Takim>> GetFavoriteTeamsAsync(int kullaniciId);
    }
}
