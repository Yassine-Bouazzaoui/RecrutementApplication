using System.ComponentModel.DataAnnotations;

namespace RecrutementApplication.Models.ViewModels
{
    public class Candidature
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CandidatId { get; set; }
        [Required]
        public int OffreId { get; set; }
        public string? LettreMotivation { get; set; }
        public DateOnly? DatePostulation { get; set; }
    }
}
