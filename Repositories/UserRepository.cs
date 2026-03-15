using Microsoft.EntityFrameworkCore;
using Sportify.Data;
using Sportify.Models;

namespace Sportify.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Kullanici?> EmailIleGetirAsync(string email)
        {
            return await _db.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailMevcutMuAsync(string email)
        {
            return await _db.Kullanicilar
                .AnyAsync(k => k.Email.ToLower() == email.ToLower());
        }

        public async Task EkleAsync(Kullanici kullanici)
        {
            await _db.Kullanicilar.AddAsync(kullanici);
            await _db.SaveChangesAsync();
        }

        public async Task OnayKodunuGuncelleAsync(int kullaniciId, string kod)
        {
            var kullanici = await _db.Kullanicilar.FindAsync(kullaniciId);
            if (kullanici != null)
            {
                kullanici.EmailOnayKodu = kod;
                await _db.SaveChangesAsync();
            }
        }

        public async Task EmailOnaylaAsync(int kullaniciId)
        {
            var kullanici = await _db.Kullanicilar.FindAsync(kullaniciId);
            if (kullanici != null)
            {
                kullanici.IsEmailOnayli = true;
                kullanici.EmailOnayKodu = null;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<Kullanici?> KullaniciAdiIleGetirAsync(string kullaniciAdi)
        {
            return await _db.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == kullaniciAdi.ToLower());
        }

        public async Task GuncelleAsync(Kullanici kullanici)
        {
            _db.Kullanicilar.Update(kullanici);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ToggleFavoriteTeamAsync(int kullaniciId, int takimId)
        {
            var favori = await _db.KullaniciTakimlar
                .FirstOrDefaultAsync(kt => kt.KullaniciId == kullaniciId && kt.TakimId == takimId);

            if (favori != null)
            {
                _db.KullaniciTakimlar.Remove(favori);
                await _db.SaveChangesAsync();
                return false; // Kaldırıldı
            }
            else
            {
                var yeniFavori = new KullaniciTakim
                {
                    KullaniciId = kullaniciId,
                    TakimId = takimId
                };
                await _db.KullaniciTakimlar.AddAsync(yeniFavori);
                await _db.SaveChangesAsync();
                return true; // Eklendi
            }
        }

        public async Task<List<Takim>> GetFavoriteTeamsAsync(int kullaniciId)
        {
            return await _db.KullaniciTakimlar
                .Where(kt => kt.KullaniciId == kullaniciId)
                .Select(kt => kt.Takim)
                .ToListAsync();
        }
    }
}
