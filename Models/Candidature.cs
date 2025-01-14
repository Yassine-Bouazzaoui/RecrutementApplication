using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecrutementApplication.Models
{
    public class Candidature
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CandidatId { get; set; }

        public ApplicationUser Candidat { get; set; }
        [Required]
        [Display(Name = "Offre")]
        public int OffreId { get; set; }
        public Offre Offre { get; set; }
        public string? LettreMotivation { get; set; }
        public DateOnly? DatePostulation { get; set; }
    }
}
