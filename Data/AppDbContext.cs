using Microsoft.EntityFrameworkCore;
using ConnectDB.Models;

namespace ConnectDB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNIQUE Username
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Username)
                .IsUnique();

            // Quan hệ Room - Computer (1-n)
            modelBuilder.Entity<Computer>()
                .HasOne(c => c.Room)
                .WithMany(r => r.Computers)
                .HasForeignKey(c => c.RoomId);

            // Quan hệ Customer - Session (1-n)
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId);

            // Quan hệ Computer - Session (1-n)
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Computer)
                .WithMany()
                .HasForeignKey(s => s.ComputerId);

            // Quan hệ Session - Order (1-n)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Session)
                .WithMany()
                .HasForeignKey(o => o.SessionId);

            // Quan hệ Order - OrderDetail (1-n)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany()
                .HasForeignKey(od => od.OrderId);

            // Quan hệ Service - OrderDetail (1-n)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Service)
                .WithMany()
                .HasForeignKey(od => od.ServiceId);

            // Quan hệ Session - Payment (1-1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Session)
                .WithOne()
                .HasForeignKey<Payment>(p => p.SessionId);

            // Seed Data
            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, RoomName = "Phòng VIP 01", Location = "Tầng 1" },
                new Room { RoomId = 2, RoomName = "Phòng Máy Thường", Location = "Tầng 2" }
            );

            modelBuilder.Entity<Computer>().HasData(
                new Computer { ComputerId = 1, ComputerName = "VIP-01", Status = "Available", RoomId = 1 },
                new Computer { ComputerId = 2, ComputerName = "VIP-02", Status = "Available", RoomId = 1 },
                new Computer { ComputerId = 3, ComputerName = "NORM-01", Status = "Available", RoomId = 2 },
                new Computer { ComputerId = 4, ComputerName = "NORM-02", Status = "Available", RoomId = 2 }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, Name = "Mì Tôm Trứng", Price = 25000 },
                new Service { ServiceId = 2, Name = "Sting Dâu", Price = 15000 },
                new Service { ServiceId = 3, Name = "Cơm Chiên Dương Châu", Price = 45000 },
                new Service { ServiceId = 4, Name = "Xúc Xích Đức", Price = 12000 }
            );
        }
    }
}