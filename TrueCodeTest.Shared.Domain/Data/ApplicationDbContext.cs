using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.Shared.Domain.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Currency> Currencies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserCurrency> UserCurrencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currency");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Rate).HasColumnName("rate").HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<UserCurrency>(entity =>
        {
            entity.ToTable("user_currency");
            entity.HasKey(e => new { e.UserId, e.CurrencyId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.FavoriteCurrencies)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
