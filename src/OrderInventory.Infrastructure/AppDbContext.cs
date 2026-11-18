using Microsoft.EntityFrameworkCore;
using OrderInventory.Domain.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Veritabanındaki tablolarımız
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();

            entity.Property(p => p.Id).HasColumnName("ID");
            entity.Property(p => p.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(p => p.UpdatedDate).HasColumnName("UPDATED_DATE");
            entity.Property(p => p.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(p => p.Sku).HasColumnName("SKU").IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).HasColumnName("NAME").IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasColumnName("PRICE").HasColumnType("decimal(18,2)");
            entity.Property(p => p.StockQuantity).HasColumnName("STOCK_QUANTITY");
            entity.Property<uint>("xmin").IsRowVersion();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Id).HasColumnName("ID");
            entity.Property(o => o.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(o => o.UpdatedDate).HasColumnName("UPDATED_DATE");
            entity.Property(o => o.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(o => o.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(o => o.Status).HasColumnName("STATUS").IsRequired();
            entity.Property(o => o.TotalAmount).HasColumnName("TOTAL_AMOUNT").HasColumnType("decimal(18,2)");

            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.Id).HasColumnName("ID");
            entity.Property(oi => oi.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(oi => oi.UpdatedDate).HasColumnName("UPDATED_DATE");
            entity.Property(oi => oi.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(oi => oi.OrderId).HasColumnName("ORDER_ID");
            entity.Property(oi => oi.ProductId).HasColumnName("PRODUCT_ID");
            entity.Property(oi => oi.Quantity).HasColumnName("QUANTITY");
            entity.Property(oi => oi.UnitPrice).HasColumnName("UNIT_PRICE").HasColumnType("decimal(18,2)");

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}