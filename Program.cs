using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftManagement;
using ShiftManagement.Data;
using ShiftManagement.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 0️⃣ Đăng ký các dịch vụ hiện có (module cũ)
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

builder.Services.AddDbContext<SamsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SamsConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "sams")));

// 2️⃣ MemoryCache
builder.Services.AddMemoryCache();

// 2.1️⃣ AutoMapper (nạp các Profile, bao gồm SamsProfile)
builder.Services.AddAutoMapper(typeof(ShiftManagement.Profiles.SamsProfile).Assembly);
// Nếu có nhiều Profile ở các assembly khác: builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 2.2️⃣ (Tuỳ chọn) Dùng cho resolver lấy user hiện tại
builder.Services.AddHttpContextAccessor();

// 3️⃣ CORS cho frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendLocalhost", policy =>
        policy.WithOrigins(
                "http://localhost:49250",
                "https://localhost:49250",
                "https://localhost:7250",
                "https://localhost:4200",
                "http://localhost:4200"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

// 4️⃣ Controllers & JSON
builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// 5️⃣ Authentication (JWT)
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

// 6️⃣ Swagger + JWT
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

// Bật CORS trước AuthN/AuthZ
app.UseCors("AllowFrontendLocalhost");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();

// 8️⃣ Tự động migrate và seed dữ liệu cho cả 2 DB
using (var scope = app.Services.CreateScope())
{
    // DB cũ
    var main = scope.ServiceProvider.GetRequiredService<ShiftManagementContext>();
    main.Database.Migrate();
    SeedData.Initialize(main); // Seed cho hệ thống cũ

    // DB Sams (DB riêng)
    var sams = scope.ServiceProvider.GetRequiredService<SamsDbContext>();
    sams.Database.Migrate();
    SeedDataSams.Initialize(sams); // Seed cho Sams (mới)

    // (Tuỳ chọn) Kiểm tra cấu hình AutoMapper để phát hiện lỗi mapping sớm
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

app.Run();