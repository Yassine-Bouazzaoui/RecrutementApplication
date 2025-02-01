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

        // Afficher toutes les Offers
        [AllowAnonymous]
        public IActionResult Index(string secteur = null, Profile? profil = null, decimal? remuneration = null)
        {
            var Offers = _context.Offers.AsQueryable();

            if (!string.IsNullOrEmpty(secteur))
                Offers = Offers.Where(o => o.Secteur == secteur);

            if (profil.HasValue)
                Offers = Offers.Where(o => o.Profil == profil.Value);

            if (remuneration.HasValue) // Vérifie si la valeur de 'remuneration' est présente
            {
                Offers = Offers.Where(o => o.Remuneration >= remuneration.Value); // Comparer avec un decimal
            }

            return View(Offers.ToList());
        }


        [AllowAnonymous]
        public IActionResult DetailsOffre(int id)
        {
            var offre = _context.Offers.Find(id);
            if (offre == null) return NotFound();

            return View("~/Views/Offres/Details.cshtml", offre);  // Spécifiez le chemin complet vers la vue        }
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
                return RedirectToAction("Index");
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
                Status = StatusCandidature.EnAttente // Statut par défaut
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

            // Vérifier que le recruteur est bien le propriétaire de l'offre
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
                    .OrderByDescending(c => c.DatePostulation) // Les plus récentes d'abord
                    .ToListAsync();

                // Ajouter un message de succès si présent dans TempData
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
        // Voir les candidats pour une offre
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

        // Modifier le profil
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

        // Afficher le profil
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

            // Vérifier le type de fichier
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(profilePicture.ContentType))
                return BadRequest("Invalid file type. Please upload an image file.");

            // Supprimer l'ancienne image si elle existe
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "ProfilPics", user.ProfilePicture);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Sauvegarder la nouvelle image
            var fileName = await profilePicture.SaveProfilePictureAsync(_webHostEnvironment.WebRootPath);
            user.ProfilePicture = fileName;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Profile");
        }
    }
}
