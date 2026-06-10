using System.Text;
using HomeServiceProvider.DataAccess;
using HomeServiceProvider.Helpers;
using HomeServiceProvider.Middleware;
using HomeServiceProvider.Services;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database & Repository (Phase 1 extension method) ───────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── 2. JWT Token Generator (Singleton — stateless helper) ─────────────────────
builder.Services.AddSingleton<JwtTokenGenerator>();

// ── 3. Application Services (Scoped — one per request) ────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProviderService, ProviderService>();

// ── Phase 3: Scheduling Services ─────────────────────────────────────────────
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// ── Phase 4: AI Matching ──────────────────────────────────────────────────────
builder.Services.AddScoped<IMatchingService, MatchingService>();

// ── Phase 5: Messaging ────────────────────────────────────────────────────────
builder.Services.AddScoped<IMessageService, MessageService>();

// ── Phase 6: Invoicing ────────────────────────────────────────────────────────
builder.Services.AddSingleton<InvoicePdfGenerator>();   // stateless, safe as singleton
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ── Phase 7: Reviews & Moderation ────────────────────────────────────────────
builder.Services.AddScoped<IReviewService, ReviewService>();

// ── 4. JWT Authentication ─────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero  // No grace period on token expiry
    };
});

builder.Services.AddAuthorization();

// ── 5. CORS (allow React frontend during development) ─────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5173")  // Vite default port
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── 6. Controllers + Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer support to Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGc..."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline (ORDER MATTERS) ───────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();  // Must be FIRST — catches all errors

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // Serves files from wwwroot/ folder
app.UseCors("ReactDev");

app.UseAuthentication();   // Reads JWT and populates HttpContext.User
app.UseAuthorization();    // Enforces [Authorize] attributes

app.MapControllers();

// ── Seed essential data (PricingRules) ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HomeServiceProvider.DataAccess.Data.AppDbContext>();
    await HomeServiceProvider.DataAccess.Data.Seeding.DbSeeder.SeedAsync(context);
}

app.Run();

