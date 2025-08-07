using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftManagement;
using ShiftManagement.Data;
using ShiftManagement.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<HolidayService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<UserService>();

// 1️⃣ Đăng ký DbContext
builder.Services.AddDbContext<ShiftManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Đăng ký MemoryCache
builder.Services.AddMemoryCache();

// 3️⃣ Cấu hình CORS cho phép frontend truy cập API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendLocalhost", policy =>
    policy.WithOrigins(
        "http://localhost:49250", // FE Vite
        "https://localhost:49250", // FE Vite HTTPS (nếu dùng)
        "https://localhost:7250"
        // ... các domain khác
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
});

// 4️⃣ Cấu hình Controllers & JSON
builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// 5️⃣ Cấu hình Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddAuthorization();

// 6️⃣ Swagger + JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ShiftManagement API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// 7️⃣ Pipeline Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ⚠️ Bật CORS trước Authentication & Authorization
app.UseCors("AllowFrontendLocalhost");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();

// 8️⃣ TỰ ĐỘNG MIGRATE VÀ SEED DATA (nếu có SeedData)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShiftManagementContext>();
    context.Database.Migrate(); // Tự động cập nhật/migrate DB
    // Nếu có class SeedData thì gọi, nếu không hãy xóa dòng sau:
    SeedData.Initialize(context); // Seed dữ liệu mẫu (bạn cần có class SeedData)
}

app.Run();