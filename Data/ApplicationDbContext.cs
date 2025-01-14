using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecrutementApplication.Data;
using RecrutementApplication.Models;

namespace RecrutementApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Offre> Offers { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Candidature> Candidatures { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Candidature>()
        .HasOne(c => c.Offre)
        .WithMany()
        .HasForeignKey(c => c.OffreId)
        .OnDelete(DeleteBehavior.Cascade);

        }

    }
}