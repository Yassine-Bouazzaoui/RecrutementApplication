namespace RecrutementApplication.Models.ViewModels
{
    public class Postule
    {
        public ApplicationUser User { get; set; }
        public CandidatureViewModel Candidature { get; set; }
    }
}
