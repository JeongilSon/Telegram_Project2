using Microsoft.EntityFrameworkCore;
using Telegram_Project2_Web.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// CORS 정책 추가
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("MariaDbConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// HTTP 전용으로 설정 (HTTPS 리디렉션 제거)
app.UseDefaultFiles(); // wwwroot의 index.html, default.htm 등을 기본 파일로 설정
app.UseStaticFiles();  // wwwroot 폴더의 정적 파일(html, css, js, 이미지 등) 제공
app.UseRouting(); // 라우팅 규칙 적용
app.UseCors("AllowAll"); // CORS 정책 적용
app.UseAuthorization();
app.MapControllers();
app.Run();
