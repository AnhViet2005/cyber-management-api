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
    db.Database.EnsureCreated(); // Đơn giản nhất là EnsureCreated để tạo bảng từ Model

    // 👉 Ép nạp dữ liệu mẫu nếu bảng Customers đang trống
    if (!db.Customers.Any())
    {
        var room1 = new ConnectDB.Models.Room { Name = "Phòng VIP 01", Type = "VIP" };
        var room2 = new ConnectDB.Models.Room { Name = "Phòng Máy Thường", Type = "Normal" };
        db.Rooms.AddRange(room1, room2);
        db.SaveChanges(); // Lưu để lấy ID cho các bảng sau

        var sv1 = new ConnectDB.Models.Service { Name = "Mì Tôm Trứng", Price = 25000 };
        var sv2 = new ConnectDB.Models.Service { Name = "Sting Dâu", Price = 15000 };
        db.Services.AddRange(sv1, sv2);

        var c1 = new ConnectDB.Models.Computer { ComputerName = "VIP-01", Status = "Available", RoomId = room1.RoomId };
        var c2 = new ConnectDB.Models.Computer { ComputerName = "NORM-01", Status = "Available", RoomId = room2.RoomId };
        db.Computers.AddRange(c1, c2);

        var cus1 = new ConnectDB.Models.Customer { Username = "anhviet", Fullname = "Anh Việt Admin", Balance = 100000 };
        db.Customers.Add(cus1);
        db.SaveChanges();

        var session1 = new ConnectDB.Models.Session { CustomerId = cus1.CustomerId, ComputerId = c1.ComputerId, StartTime = DateTime.UtcNow, Status = "Playing", HourlyRate = 10000 };
        db.Sessions.Add(session1);
        db.SaveChanges();
    }
}

// Middleware
//if (app.Environment.IsDevelopment())

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();