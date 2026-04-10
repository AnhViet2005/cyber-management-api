using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 👉 Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Ưu tiên đọc từ DATABASE_URL (Render), nếu không có mới tìm trong appsettings
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (connectionString != null && (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
    {
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var host = databaseUri.Host;
        var port = databaseUri.Port > 0 ? databaseUri.Port : 5432;
        var database = databaseUri.AbsolutePath.TrimStart('/');
        var user = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }

    options.UseNpgsql(connectionString);
});

builder.Services.AddControllers();

// Swagger (có cũng được, không bắt buộc)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Tự động tạo Database và Bảng nếu chưa có (Dành cho Render)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.Migrate(); // Nếu dùng migration
    db.Database.EnsureCreated();

    // 👉 Ép nạp dữ liệu mẫu AN TOÀN (Safe Seeding)
    try 
    {
        if (!db.Customers.Any())
        {
            // Kiểm tra và nạp Phòng nếu chưa có
            if (!db.Rooms.Any())
            {
                db.Rooms.AddRange(
                    new ConnectDB.Models.Room { Name = "Phòng VIP 01", Type = "VIP" },
                    new ConnectDB.Models.Room { Name = "Phòng Máy Thường", Type = "Normal" }
                );
                db.SaveChanges();
            }

            var room1 = db.Rooms.First();
            
            // Nạp Máy tính nếu chưa có
            if (!db.Computers.Any())
            {
                db.Computers.AddRange(
                    new ConnectDB.Models.Computer { ComputerName = "VIP-01", Status = "Available", RoomId = room1.RoomId },
                    new ConnectDB.Models.Computer { ComputerName = "NORM-01", Status = "Available", RoomId = room1.RoomId }
                );
            }

            // Nạp Khách hàng
            var cus1 = new ConnectDB.Models.Customer { Username = "anhviet", Fullname = "Anh Việt Admin", Balance = 100000 };
            db.Customers.Add(cus1);
            db.SaveChanges();

            // Nạp Phiên chơi (Sử dụng thời gian chuẩn UTC cho PostgreSQL)
            var session1 = new ConnectDB.Models.Session 
            { 
                CustomerId = cus1.CustomerId, 
                ComputerId = db.Computers.First().ComputerId, 
                StartTime = DateTime.UtcNow, 
                Status = "Playing", 
                HourlyRate = 10000 
            };
            db.Sessions.Add(session1);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Nếu lỗi nạp dữ liệu thì chỉ ghi log, không làm sập App
        Console.WriteLine("Lỗi nạp dữ liệu mẫu: " + ex.Message);
    }
}

// Middleware
//if (app.Environment.IsDevelopment())

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();