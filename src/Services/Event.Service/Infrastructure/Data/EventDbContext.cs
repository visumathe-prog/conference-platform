using Microsoft.EntityFrameworkCore;
using Event.Service.Domain.Entities;

namespace Event.Service.Infrastructure.Data;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Date);
        });
    }
}
