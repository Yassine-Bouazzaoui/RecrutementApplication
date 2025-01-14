using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecrutementApplication.Data;
using RecrutementApplication.Models;

namespace RecrutementApplication.Controllers
{
    public class OffresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OffresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Offres
      
        public async Task<IActionResult> Index(string secteur, string profil, decimal? minRemuneration)
        {
            var offres = _context.Offers.AsQueryable();

            // Filtrer par secteur si un secteur est spécifié
            if (!string.IsNullOrEmpty(secteur))
            {
                offres = offres.Where(o => o.Secteur.Contains(secteur));
            }

            // Filtrer par profil si un profil est spécifié
            if (!string.IsNullOrEmpty(profil))
            {
                if (Enum.TryParse<Profile>(profil, true, out var parsedProfil))
                {
                    offres = offres.Where(o => o.Profil == parsedProfil);
                }
                else
                {
                    ModelState.AddModelError("", "Le profil spécifié est invalide.");
                }
            }

            // Filtrer par rémunération minimale si spécifiée
            if (minRemuneration.HasValue)
            {
                offres = offres.Where(o => decimal.Parse(o.Remuneration) >= minRemuneration.Value);
            }

            // Retourner la vue avec les offres filtrées
            return View(await offres.ToListAsync());
        }


        // GET: Offres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offre = await _context.Offers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (offre == null)
            {
                return NotFound();
            }

            return View(offre);
        }
    }
}
