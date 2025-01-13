using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RecrutementApplication.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage = "Veuillez saisir votre Nom")]
        public string Nom { get; set; }
        [Required(ErrorMessage = "Veuillez saisir votre Prenom")]
        public string Prenom { get; set; }
        [Required(ErrorMessage = "Veuillez saisir votre age")]
        public string? Mail { get; set; }
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public string? Titre { get; set; }
        public string? Diplome { get; set; }
        public Profile? Profil { get; set; }
        public int? NbAnsExp { get; set; }
        public string? CV { get; set; }
        public string? Entreprise { get; set; }
        public string? EntrepriseLogo { get; set; }
    }
}
