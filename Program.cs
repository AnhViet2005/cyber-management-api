using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 👉 Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
}

// Middleware
//if (app.Environment.IsDevelopment())

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();