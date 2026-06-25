using Microsoft.EntityFrameworkCore;
using MyImagePrinting.Models;

namespace MyImagePrinting.Data;

// Represents the MyImageDB SQL Server database.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PrintSize> PrintSizes => Set<PrintSize>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A username and customer email must be unique.
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(customer => customer.Email)
            .IsUnique();

        // Create the one-to-one relationship between Customer and User.
        modelBuilder.Entity<Customer>()
            .HasOne(customer => customer.User)
            .WithOne(user => user.Customer)
            .HasForeignKey<User>(user => user.CustId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create the one-to-many relationship between Customer and Orders.
        modelBuilder.Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustId)
            .OnDelete(DeleteBehavior.Restrict);

        // Create the one-to-many relationship between Order and OrderDetails.
        modelBuilder.Entity<OrderDetail>()
            .HasOne(detail => detail.Order)
            .WithMany(order => order.OrderDetails)
            .HasForeignKey(detail => detail.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create the relationship between PrintSize and OrderDetails.
        modelBuilder.Entity<OrderDetail>()
            .HasOne(detail => detail.PrintSize)
            .WithMany(size => size.OrderDetails)
            .HasForeignKey(detail => detail.PrintSizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // One order can have only one payment record.
        modelBuilder.Entity<Payment>()
            .HasOne(payment => payment.Order)
            .WithOne(order => order.Payment)
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasIndex(payment => payment.OrderId)
            .IsUnique();

        // Add default print sizes required by the project specification.
        modelBuilder.Entity<PrintSize>().HasData(
            new PrintSize { PrintSizeId = 1, SizeName = "4 x 6", Price = 50.00m },
            new PrintSize { PrintSizeId = 2, SizeName = "5 x 7", Price = 80.00m },
            new PrintSize { PrintSizeId = 3, SizeName = "8 x 10", Price = 150.00m },
            new PrintSize { PrintSizeId = 4, SizeName = "10 x 12", Price = 250.00m }
        );
    }
}
