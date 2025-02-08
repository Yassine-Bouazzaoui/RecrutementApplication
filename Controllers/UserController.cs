using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecrutementApplication.Data;
using RecrutementApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using RecrutementApplication.Utility;

namespace RecrutementApplication.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<UserController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public IActionResult Index(string secteur = null, Profile? profil = null, decimal? remuneration = null)
        {
            var Offers = _context.Offers.AsQueryable();

            if (!string.IsNullOrEmpty(secteur))
                Offers = Offers.Where(o => o.Secteur == secteur);

            if (profil.HasValue)
                Offers = Offers.Where(o => o.Profil == profil.Value);

            if (remuneration.HasValue) 
            {
                Offers = Offers.Where(o => o.Remuneration >= remuneration.Value); 
            }

            return View(Offers.ToList());
        }


        [AllowAnonymous]
        public IActionResult DetailsOffre(int id)
        {
            var offre = _context.Offers.Find(id);
            if (offre == null) return NotFound();

            return View("~/Views/Offres/Details.cshtml", offre); 
        }


            [Authorize(Roles = "Recruteur")]
        public async Task<IActionResult> MesOffres()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mesOffres = await _context.Offers
                .Where(o => o.rectuteurId == userId)
                .OrderByDescending(o => o.DatePub)
                .ToListAsync();

            var user = await _userManager.FindByIdAsync(userId);

            foreach (var offre in mesOffres)
            {
                offre.Entreprise = user.Entreprise;
                offre.EntrepriseLogo = user.EntrepriseLogo;
            }

            return View(mesOffres);
        }

        [Authorize(Roles = "Recruteur")]
        public IActionResult CreateOffre()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Recruteur")]
        public IActionResult CreateOffre(Offre offre)
        {
            if (ModelState.IsValid)
            {
                offre.rectuteurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                offre.DatePub = DateTime.Now;

                if (offre.DeadLine <= offre.DatePub)
                {
                    ModelState.AddModelError("DeadLine", "La date de fin doit être postérieure à la date de publication.");
                    return View(offre);
                }
                var recruteur = _context.Users.FirstOrDefault(r => r.Id == offre.rectuteurId);
                offre.Entreprise = recruteur.Entreprise;

                if (recruteur != null && !string.IsNullOrEmpty(recruteur.EntrepriseLogo))
                {
                    offre.EntrepriseLogo = recruteur.EntrepriseLogo;
                }


                _context.Offers.Add(offre);
                _context.SaveChanges();

                return RedirectToAction("MesOffres");
            }

            return View(offre);
        }


        [Authorize(Roles = "Recruteur")]
        public IActionResult EditOffre(int id)
        {
            var offre = _context.Offers.Find(id);
            if (offre == null || offre.rectuteurId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            return View(offre);
        }

        [HttpPost]
        [Authorize(Roles = "Recruteur")]
        public IActionResult EditOffre(Offre offre)
        {
            if (ModelState.IsValid)
            {
                _context.Offers.Update(offre);
                _context.SaveChanges();
                return RedirectToAction("MesOffres");
            }
            return View(offre);
        }

        [Authorize(Roles = "Recruteur")]
        public IActionResult DeleteOffre(int id)
        {
            var offre = _context.Offers.Find(id);
            if (offre == null || offre.rectuteurId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            return View(offre);
        }

        [HttpPost, ActionName("DeleteOffre")]
        [Authorize(Roles = "Recruteur")]
        public IActionResult DeleteOffreConfirmed(int id)
        {
            var offre = _context.Offers.Find(id);
            if (offre != null)
            {
                _context.Offers.Remove(offre);
                _context.SaveChanges();
            }
            return RedirectToAction("MesOffres");
        }

        [HttpPost]
        [Authorize(Roles = "Candidat")]
        public IActionResult Postuler(int offreId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var candidature = new Candidature
            {
                CandidatId = userId,
                OffreId = offreId,
                DatePostulation = DateOnly.FromDateTime(DateTime.Now),
                Status = StatusCandidature.EnAttente 
            };
            _context.Candidatures.Add(candidature);
            _context.SaveChanges();
            return RedirectToAction("HistoriqueCandidatures");
        }

        [HttpPost]
        [Authorize(Roles = "Recruteur")]
        public IActionResult UpdateCandidatureStatus(int candidatureId, StatusCandidature newStatus)
        {
            var candidature = _context.Candidatures.Find(candidatureId);
            if (candidature == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var offre = _context.Offers.Find(candidature.OffreId);
            if (offre.rectuteurId != userId) return Unauthorized();

            candidature.Status = newStatus;
            _context.SaveChanges();

            return RedirectToAction("VoirCandidats", new { offreId = candidature.OffreId });
        }


        [Authorize(Roles = "Candidat")]
        public async Task<IActionResult> HistoriqueCandidatures()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var candidatures = await _context.Candidatures
                    .Where(c => c.CandidatId == userId)
                    .Include(c => c.Offre)
                    .OrderByDescending(c => c.DatePostulation) 
                    .ToListAsync();

                if (TempData["Success"] != null)
                {
                    ViewBag.SuccessMessage = TempData["Success"].ToString();
                }

                return View(candidatures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'historique des candidatures");
                TempData["Error"] = "Une erreur est survenue lors du chargement de l'historique.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Recruteur")]
        public IActionResult VoirCandidats(int offreId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var offre = _context.Offers.FirstOrDefault(o => o.Id == offreId && o.rectuteurId == userId);

            if (offre == null) return Unauthorized();

            var candidatures = _context.Candidatures
                .Where(c => c.OffreId == offreId)
                .Include(c => c.Candidat)
                .ToList();

            if (!candidatures.Any())
            {
                ViewBag.Message = "Aucune candidature pour cette offre pour le moment.";
            }

            return View("VoirCandidats", candidatures);
        }

        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(user);
        }

        [Authorize(Roles = "Candidat")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var candidature = await _context.Candidatures
                .FirstOrDefaultAsync(c => c.Id == id && c.CandidatId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (candidature == null)
            {
                TempData["Error"] = "Candidature non trouvée ou accès non autorisé.";
                return RedirectToAction("HistoriqueCandidatures");
            }

            if (candidature.Status != StatusCandidature.EnAttente)
            {
                TempData["Error"] = "Seules les candidatures en attente peuvent être retirées.";
                return RedirectToAction("HistoriqueCandidatures");
            }

            try
            {
                _context.Candidatures.Remove(candidature);
                await _context.SaveChangesAsync();
                ViewBag.SuccessMessage = "Votre candidature a été retirée avec succès.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Une erreur est survenue lors du retrait de la candidature.";
            }

            return RedirectToAction("HistoriqueCandidatures");
        }
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null)
                return BadRequest("No file uploaded");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(profilePicture.ContentType))
                return BadRequest("Invalid file type. Please upload an image file.");

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "ProfilPics", user.ProfilePicture);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            var fileName = await profilePicture.SaveProfilePictureAsync(_webHostEnvironment.WebRootPath);
            user.ProfilePicture = fileName;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Profile");
        }
    }
}
