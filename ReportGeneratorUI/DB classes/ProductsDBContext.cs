using Microsoft.EntityFrameworkCore;
using ReportGeneratorUI.Properties;
using System.Configuration;
using System.Security.Policy;

namespace ReportGenerator;

public partial class ProductsDBContext : DbContext
{
    public ProductsDBContext()
    {
    }

    public ProductsDBContext(DbContextOptions<ProductsDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Link> Links { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<ProductHierarchy> ProductHierarchy { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) 
        {
            Settings settings = new Settings();

            string connectionString = settings.ProductsDatabase;
            optionsBuilder.UseSqlite(connectionString); 
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductHierarchy>(entity =>
        {
            entity.HasNoKey();
            entity.HasOne(d => d.ProductNavigation)
                .WithMany()
                .HasForeignKey(d => d.Id);

        });
        modelBuilder.Entity<Link>(entity =>
        {
            //entity.HasNoKey();
            entity.HasKey(e => new { e.UpProduct, e.Product });

            entity.HasIndex(e => new { e.Product, e.UpProduct }, "IX_Links_Product_UpProduct")
                .IsUnique();

            entity.Property(e => e.Count).HasDefaultValueSql("1");

            entity.HasOne(d => d.ProductNavigation)
                .WithMany()
                .HasForeignKey(d => d.Product)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UpProductNavigation)
                .WithMany()
                .HasForeignKey(d => d.UpProduct)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.HasIndex(e => e.Id, "IX_Product_Id")
                .IsUnique();

            entity.HasIndex(e => e.Name, "IX_Product_Name")
                .IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
