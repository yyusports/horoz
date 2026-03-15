using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Sportify.Data;
using Sportify.Models;
using Sportify.Models.ViewModels;
using Sportify.Repositories;
using Sportify.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace Sportify.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly ISportDataService _sportDataService;

        public AccountController(IUserRepository userRepository, IEmailService emailService, AppDbContext context, ISportDataService sportDataService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _context = context;
            _sportDataService = sportDataService;
        }

        // GEÇİCİ: Test verilerini temizlemek için (Kullandıktan sonra sileceğiz)
        [HttpGet("/Account/ResetData")]
        public async Task<IActionResult> ResetData()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Kullanicilar; DELETE FROM EpostaOnaylar;");
            return Content("Veritabanı temizlendi! Artık aynı e-postalarla tekrar kayıt olabilirsiniz.");
        }

        // ── 1. ADIM: E-POSTA GİRİŞİ ───────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EmailRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Zaten kayıtlı mı?
            if (await _userRepository.EmailMevcutMuAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(model);
            }

            var onayKodu = new Random().Next(100000, 999999).ToString();

            try
            {
                // Mevcut geçici kaydı güncelle veya yenisini oluştur
                var eskiOnay = await _context.EpostaOnaylar.FirstOrDefaultAsync(x => x.Email == model.Email);
                if (eskiOnay != null)
                {
                    eskiOnay.OnayKodu = onayKodu;
                    eskiOnay.IsOnaylandi = false;
                    eskiOnay.OlusturmaTarihi = DateTime.UtcNow;
                }
                else
                {
                    await _context.EpostaOnaylar.AddAsync(new EpostaOnay { Email = model.Email, OnayKodu = onayKodu });
                }
                await _context.SaveChangesAsync();

                // E-posta gönder (Şık bir HTML Tasarımı ile)
                string emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #00d4ff, #7c3aed); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 28px;'>⚽ Sportify</h1>
                            <p style='margin: 5px 0 0; opacity: 0.9;'>Spor Dünyasına Hoş Geldiniz!</p>
                        </div>
                        <div style='padding: 30px; background-color: #fcfcfc; color: #333;'>
                            <h2 style='color: #444; margin-top: 0;'>Doğrulama Kodunuz</h2>
                            <p style='font-size: 16px; line-height: 1.5;'>Merhaba,</p>
                            <p style='font-size: 16px; line-height: 1.5;'>Sizleri aramızda görmek bizler için büyük mutluluk! Kayıt işlemini tamamlamak için aşağıdaki 6 haneli doğrulama kodunu kullanın:</p>
                            
                            <div style='background-color: #f0f7ff; border: 2px dashed #00d4ff; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0;'>
                                <span style='font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #7c3aed;'>{onayKodu}</span>
                            </div>
                            
                            <p style='font-size: 14px; color: #777; line-height: 1.5;'>Bu kod 10 dakika boyunca geçerlidir. Eğer bu işlemi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.</p>
                        </div>
                        <div style='background-color: #f1f1f1; padding: 20px; text-align: center; color: #999; font-size: 12px;'>
                            &copy; 2026 Sportify - Tüm Hakları Saklıdır.<br>
                            Heyecan dolu bir deneyim dileriz! 🏀🎾🏐
                        </div>
                    </div>";

                await _emailService.GonderAsync(model.Email, "Sportify Doğrulama Kodunuz", emailBody);

                TempData["Email"] = model.Email;
                return RedirectToAction("VerifyEmail", new { email = model.Email });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"E-posta gönderilirken hata oluştu: {ex.Message}");
                return View(model);
            }
        }

        // ── 2. ADIM: E-POSTA DOĞRULAMA ────────────────────────
        [HttpGet]
        public IActionResult VerifyEmail(string? email)
        {
            var targetEmail = email ?? TempData.Peek("Email")?.ToString();
            if (string.IsNullOrEmpty(targetEmail)) return RedirectToAction("Register");

            ViewBag.Email = targetEmail;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(string email, string kod)
        {
            var onay = await _context.EpostaOnaylar.FirstOrDefaultAsync(x => x.Email == email && x.OnayKodu == kod);
            
            if (onay == null)
            {
                ViewBag.Hata = "Geçersiz veya hatalı kod.";
                ViewBag.Email = email;
                return View();
            }

            onay.IsOnaylandi = true;
            await _context.SaveChangesAsync();

            TempData["Email"] = email;
            return RedirectToAction("CompleteRegistration", new { email = email });
        }

        // ── 3. ADIM: KAYIT TAMAMLAMA ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> CompleteRegistration(string? email)
        {
            var targetEmail = email ?? TempData.Peek("Email")?.ToString();
            if (string.IsNullOrEmpty(targetEmail)) return RedirectToAction("Register");

            // Onaylanmış mı kontrol et
            var onayliMi = await _context.EpostaOnaylar.AnyAsync(x => x.Email == targetEmail && x.IsOnaylandi);
            if (!onayliMi) return RedirectToAction("Register");

            return View(new CompleteRegisterViewModel { Email = targetEmail });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRegistration(CompleteRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Mail hala onaylı mı?
            var onay = await _context.EpostaOnaylar.FirstOrDefaultAsync(x => x.Email == model.Email && x.IsOnaylandi);
            if (onay == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta doğrulama süreci geçersiz kılınmış.");
                return RedirectToAction("Register");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var (hash, salt) = SifreHashle(model.Sifre);
                var kullanici = new Kullanici
                {
                    KullaniciAdi = model.KullaniciAdi,
                    Email = model.Email,
                    SifreHash = hash,
                    SifreSalt = salt,
                    IsEmailOnayli = true, // Artık onaylı
                    OlusturmaTarihi = DateTime.UtcNow
                };

                await _userRepository.EkleAsync(kullanici);
                
                // Onay tablosundan sil (temizlik)
                _context.EpostaOnaylar.Remove(onay);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mesaj"] = "Kaydınız başarıyla tamamlandı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Kayıt sırasında hata: {ex.Message}");
                return View(model);
            }
        }

        // ── GİRİŞ YAP ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var kullanici = await _userRepository.EmailIleGetirAsync(model.Email);

            if (kullanici == null || !SifreDogrula(model.Sifre, kullanici.SifreHash, kullanici.SifreSalt))
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            if (!kullanici.IsEmailOnayli)
            {
                ModelState.AddModelError(string.Empty, "Lütfen e-posta adresinizi doğrulayın.");
                TempData["Email"] = kullanici.Email;
                return RedirectToAction("VerifyEmail", new { email = kullanici.Email });
            }

            // Claims oluştur ve cookie yaz
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
                new Claim(ClaimTypes.Email, kullanici.Email),
                new Claim("ProfilResmi", kullanici.ProfilResmiYolu ?? "")
            };

            var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(kimlik);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.BeniHatirla,
                ExpiresUtc   = model.BeniHatirla
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ── ÇIKIŞ ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ── YARDIMCI METODLAR ──────────────────────────────────
        private static (string hash, string salt) SifreHashle(string sifre)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            // var saltBase64 = Convert.ToBase64String(saltBytes); // Not needed for KeyDerivation.Pbkdf2 directly

            var hashBytes = KeyDerivation.Pbkdf2(
                password:   sifre,
                salt:       saltBytes,
                prf:        KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        private static bool SifreDogrula(string sifre, string hashStr, string saltStr)
        {
            var saltBytes = Convert.FromBase64String(saltStr);

            var hashBytes = KeyDerivation.Pbkdf2(
                password:   sifre,
                salt:       saltBytes,
                prf:        KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            return Convert.ToBase64String(hashBytes) == hashStr;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int takimId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, message = "Oturum açmanız gerekiyor." });

            // Takım veritabanında var mı kontrol et
            var takimExists = await _context.Takimlar.AnyAsync(t => t.Id == takimId);
            if (!takimExists)
            {
                // Değilse API'den getir ve kaydet
                // Not: Lig ve sezon bilgisi burada varsayılan olarak veriliyor. 
                // Gerçek senaryoda bu bilgiler de JS'den gelebilir.
                var apiTakim = await _sportDataService.GetTeamDetailsAsync(takimId, 39, 2024);
                if (apiTakim != null)
                {
                    // EF Core kimlik ekleme hatasını aşmak için RAW SQL ile manuel kimlik ekleme
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var sql = @"
                            SET IDENTITY_INSERT Takimlar ON;
                            INSERT INTO Takimlar (
                                Id, Ad, Logo, Ulke, KurulusYili, Stadyum,
                                Istatistikler_Oynanan, Istatistikler_Galibiyet, 
                                Istatistikler_Beraberlik, Istatistikler_Maglubiyet,
                                Istatistikler_AtilanGol, Istatistikler_YenilenGol
                            ) VALUES (
                                {0}, {1}, {2}, {3}, {4}, {5},
                                {6}, {7}, {8}, {9}, {10}, {11}
                            );
                            SET IDENTITY_INSERT Takimlar OFF;";

                        await _context.Database.ExecuteSqlRawAsync(sql,
                            apiTakim.Id,
                            apiTakim.Ad ?? "",
                            apiTakim.Logo ?? "",
                            apiTakim.Ulke ?? "",
                            apiTakim.KurulusYili,
                            apiTakim.Stadyum ?? "",
                            apiTakim.Istatistikler?.Oynanan,
                            apiTakim.Istatistikler?.Galibiyet,
                            apiTakim.Istatistikler?.Beraberlik,
                            apiTakim.Istatistikler?.Maglubiyet,
                            apiTakim.Istatistikler?.AtilanGol,
                            apiTakim.Istatistikler?.YenilenGol
                        );

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Takım bilgileri API'den alınamadı." });
                }
            }

            var isAdded = await _userRepository.ToggleFavoriteTeamAsync(userId, takimId);

            return Json(new { success = true, isAdded = isAdded });
        }

        // ── PROFİL SAYFASI ────────────────────────────────────
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string? username)
        {
            var targetUsername = username ?? User.Identity?.Name;
            
            if (string.IsNullOrEmpty(targetUsername))
                return RedirectToAction("Login");

            var kullanici = await _userRepository.KullaniciAdiIleGetirAsync(targetUsername);

            if (kullanici == null)
                return NotFound("Kullanıcı bulunamadı.");

            // Favori takımları getir
            var favoriTakimlar = await _userRepository.GetFavoriteTeamsAsync(kullanici.Id);
            ViewBag.FavoriteTeams = favoriTakimlar;

            return View(kullanici);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile profilResmi)
        {
            if (profilResmi == null || profilResmi.Length == 0)
            {
                TempData["Hata"] = "Lütfen bir dosya seçin.";
                return RedirectToAction("Profile");
            }

            // Dosya tipi kontrolü
            var uzanti = Path.GetExtension(profilResmi.FileName).ToLower();
            var izinVerilenler = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!izinVerilenler.Contains(uzanti))
            {
                TempData["Hata"] = "Sadece JPG, PNG veya WEBP formatları yüklenebilir.";
                return RedirectToAction("Profile");
            }

            // Boyut kontrolü (Maksimum 2MB)
            if (profilResmi.Length > 2 * 1024 * 1024)
            {
                TempData["Hata"] = "Dosya boyutu 2MB'dan büyük olamaz.";
                return RedirectToAction("Profile");
            }

            var kullanici = await _userRepository.KullaniciAdiIleGetirAsync(User.Identity!.Name!);
            if (kullanici == null) return NotFound();

            // Dosya adı oluştur ve kaydet
            var dosyaAdi = $"{kullanici.Id}_{DateTime.Now.Ticks}{uzanti}";
            var yuklemeYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "profiles", dosyaAdi);

            using (var stream = new FileStream(yuklemeYolu, FileMode.Create))
            {
                await profilResmi.CopyToAsync(stream);
            }

            // Eski dosyayı sil (isteğe bağlı ama önerilir)
            if (!string.IsNullOrEmpty(kullanici.ProfilResmiYolu))
            {
                var eskiYol = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", kullanici.ProfilResmiYolu.TrimStart('/'));
                if (System.IO.File.Exists(eskiYol)) System.IO.File.Delete(eskiYol);
            }

            // Veritabanını güncelle
            kullanici.ProfilResmiYolu = $"/img/profiles/{dosyaAdi}";
            await _userRepository.GuncelleAsync(kullanici);

            // ⚠️ KRİTİK: Sidebar'daki fotoğrafın anında güncellenmesi için Claims'i yenile
            var existingClaims = User.Claims.ToList();
            var photoClaim = existingClaims.FirstOrDefault(c => c.Type == "ProfilResmi");
            if (photoClaim != null) existingClaims.Remove(photoClaim);
            existingClaims.Add(new Claim("ProfilResmi", kullanici.ProfilResmiYolu));

            var newIdentity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(newIdentity));

            TempData["Mesaj"] = "Profil fotoğrafınız başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }
    }
}
