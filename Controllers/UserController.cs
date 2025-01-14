using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecrutementApplication.Data;
using RecrutementApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace RecrutementApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Afficher toutes les Offers
        [AllowAnonymous]
        public IActionResult Index(string secteur = null, Profile? profil = null, string remuneration = null)
        {
            var Offers = _context.Offers.AsQueryable();

            if (!string.IsNullOrEmpty(secteur))
                Offers = Offers.Where(o => o.Secteur == secteur);

            if (profil.HasValue)
                Offers = Offers.Where(o => o.Profil == profil.Value);

            if (!string.IsNullOrEmpty(remuneration))
                Offers = Offers.Where(o => o.Remuneration == remuneration);

            return View(Offers.ToList());
        }

        // Afficher les détails d'une offre
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
            // Récupérer l'ID de l'utilisateur connecté
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Récupérer les offres de l'utilisateur, ordonnées par date de publication décroissante
            var mesOffres = await _context.Offers
                .Where(o => o.rectuteurId == userId)
                .OrderByDescending(o => o.DatePub)
                .ToListAsync();

            // Récupérer les informations de l'utilisateur pour afficher le nom de l'entreprise
            var user = await _userManager.FindByIdAsync(userId);

            // Pour chaque offre, ajouter les informations de l'entreprise
            foreach (var offre in mesOffres)
            {
                offre.Entreprise = user.Entreprise;
                offre.EntrepriseLogo = user.EntrepriseLogo;
            }

            return View(mesOffres);
        }

        // Ajouter une offre (recruteur uniquement)
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
                // Attribuer les valeurs calculées
                offre.rectuteurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                offre.DatePub = DateTime.Now;

                // Valider la cohérence des dates
                if (offre.DeadLine <= offre.DatePub)
                {
                    ModelState.AddModelError("DeadLine", "La date de fin doit être postérieure à la date de publication.");
                    return View(offre);
                }

                // Enregistrer dans la base de données
                _context.Offers.Add(offre);
                _context.SaveChanges();

                return RedirectToAction("MesOffres");
            }

            return View(offre);
        }


        // Modifier une offre (recruteur uniquement)
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

        // Supprimer une offre (recruteur uniquement)
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
            return RedirectToAction("Index");
        }
        // Postuler à une offre
        [Authorize(Roles = "Candidat")]
        public IActionResult Postuler(int offreId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var candidature = new Candidature
            {
                CandidatId = userId,
                OffreId = offreId,
                DatePostulation = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Candidatures.Add(candidature);
            _context.SaveChanges();

            return RedirectToAction("HistoriqueCandidatures");
        }

        // Consulter l'historique des candidatures
        [Authorize(Roles = "Candidat")]
        public IActionResult HistoriqueCandidatures()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var candidatures = _context.Candidatures
                .Where(c => c.CandidatId == userId)
                .Include(c => c.Offre)
                .ToList();

            return View(candidatures);
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

    }
}
