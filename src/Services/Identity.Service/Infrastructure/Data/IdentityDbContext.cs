using Microsoft.EntityFrameworkCore;
using Identity.Service.Domain.Entities;

namespace Identity.Service.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").IsRequired();
            });
            entity.OwnsOne(e => e.Password, password =>
            {
                password.Property(p => p.Hash).HasColumnName("PasswordHash").IsRequired();
            });
            entity.HasMany(e => e.Roles)
                  .WithMany()
                  .UsingEntity(j => j.ToTable("UserRoles"));
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne<User>()
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
