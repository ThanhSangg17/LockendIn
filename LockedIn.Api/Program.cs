using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Interfaces;
using LockedIn.DataAccess.Repositories;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.Services;

namespace LockedIn.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 1. Connection String & DbContext
            builder.Services.AddDbContext<LockedInDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
            builder.Services.AddScoped<IPtProfileRepository, PtProfileRepository>();
            builder.Services.AddScoped<IPtDocumentRepository, PtDocumentRepository>();
            builder.Services.AddScoped<IPackageRepository, PackageRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentWebhookLogRepository, PaymentWebhookLogRepository>();
            builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
            builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
            builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();
            builder.Services.AddScoped<IAiUsageLogRepository, AiUsageLogRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
            builder.Services.AddScoped<IDisputeEvidenceRepository, DisputeEvidenceRepository>();
            builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // 4. Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IPtProfileService, PtProfileService>();
            builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
            builder.Services.AddScoped<IPackageService, PackageService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
            builder.Services.AddScoped<IConversationService, ConversationService>();
            builder.Services.AddScoped<IMealPlanService, MealPlanService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IDisputeService, DisputeService>();
            builder.Services.AddScoped<ISettlementService, SettlementService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Current User Context Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // 5. JWT Authentication
            var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("JWT Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            // 6. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 7. Swagger Services with Bearer Auth Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LockedIn API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter JWT token with Bearer prefix. Example: Bearer eyJhbGciOi...",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

