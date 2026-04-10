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
                new Room { RoomId = 1, Name = "Phòng VIP 01", Type = "VIP" },
                new Room { RoomId = 2, Name = "Phòng Máy Thường", Type = "Normal" }
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

            // 1. Seed Customer
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Username = "anhviet", Fullname = "Anh Việt Admin", Balance = 100000 },
                new Customer { CustomerId = 2, Username = "khachhang01", Fullname = "Nguyễn Văn A", Balance = 50000 }
            );

            // 2. Seed Session (Phiên chơi)
            modelBuilder.Entity<Session>().HasData(
                new Session { SessionId = 1, CustomerId = 1, ComputerId = 1, StartTime = DateTime.Parse("2026-04-10 08:00:00"), Status = "Playing", HourlyRate = 10000 },
                new Session { SessionId = 2, CustomerId = 2, ComputerId = 3, StartTime = DateTime.Parse("2026-04-10 07:00:00"), EndTime = DateTime.Parse("2026-04-10 09:00:00"), Status = "Done", HourlyRate = 8000 }
            );

            // 3. Seed Order (Đơn hàng gọi món)
            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, SessionId = 1, OrderTime = DateTime.Parse("2026-04-10 08:30:00") },
                new Order { OrderId = 2, SessionId = 2, OrderTime = DateTime.Parse("2026-04-10 07:30:00") }
            );

            // 4. Seed OrderDetail (Chi tiết món ăn)
            modelBuilder.Entity<OrderDetail>().HasData(
                new OrderDetail { OrderDetailId = 1, OrderId = 1, ServiceId = 1, Quantity = 1, Price = 25000 }, // Mì tôm cho anhviet
                new OrderDetail { OrderDetailId = 2, OrderId = 1, ServiceId = 2, Quantity = 1, Price = 15000 }, // Sting cho anhviet
                new OrderDetail { OrderDetailId = 3, OrderId = 2, ServiceId = 4, Quantity = 2, Price = 12000 }  // 2 Xúc xích cho khách 01
            );

            // 5. Seed Payment (Thanh toán cho khách đã chơi xong)
            modelBuilder.Entity<Payment>().HasData(
                new Payment { PaymentId = 1, SessionId = 2, TotalAmount = 16000 + 24000, PaymentMethod = "Cash", PaymentTime = DateTime.Parse("2026-04-10 09:05:00") } 
            );
        }
    }
}