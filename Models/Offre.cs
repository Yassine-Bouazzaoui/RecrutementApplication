using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace RecrutementApplication.Models
{
    public enum TypeContrat { CDD, CDI, INTERN }
    public enum Profile { Deug, Licence, Master, Ingenieur }
    public class Offre
    {
        [Key]
        public int Id { get; set; }
        public string? rectuteurId { get; set; }

        [DisplayName("Contract Type")]
        public TypeContrat Type { get; set; }
        [DisplayName("Offer's Sector")]
        public string Secteur { get; set; }
        [DisplayName("Profile")]
        public Profile Profil { get; set; }
        [DisplayName("Poste")]
        public string Poste { get; set; }
        [DisplayName("Rémuneration")]
        public string Remuneration { get; set; }
        [DisplayName("Description de l'offre")]
        public string Description { get; set; }
        [DisplayName("Responsibilités")]
        public string Responsibilites { get; set; }
        [DisplayName("Qualifications de l'offre")]
        public string Qualifications { get; set; }
        [DisplayName("Localisation de l'offre")]
        public string Location { get; set; }
        [DisplayName("Date de publication")]
        public DateOnly DatePub { get; set; }
        [DisplayName("Date de fin")]
        public DateOnly DeadLine { get; set; }
        [DisplayName("Le nom de l'entreprise")]
        public string? Entreprise { get; set; }
        public string? EntrepriseLogo { get; set; }
        }
}
