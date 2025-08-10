using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartMeetingRoom1.Models;
using SmartMeetingRoom1.Services;
using SmartMeetingRoom1.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Authorization;
namespace SmartMeetingRoom1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

           
            builder.Services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 6;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

           
            var jwt = builder.Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            
            builder.Services.AddScoped<IMeeting, IMeetingServices>();
            builder.Services.AddScoped<IRoom, IRoomServices>();
            builder.Services.AddScoped<IUser, IUserServices>();
            builder.Services.AddScoped<INotification, INotificationServices>();
            builder.Services.AddScoped<IMinute, IMinuteServices>();
            builder.Services.AddScoped<IMeetingAttendee, IMeetingAttendeeServices>();
            builder.Services.AddScoped<IActionItem, IActionItemServices>();
            builder.Services.AddScoped<IAttachment, IAttachmentServices>();
            builder.Services.AddScoped<IToken,ITokenService>();

           
            builder.Services.AddControllers(o =>
            {
                o.Filters.Add(new AuthorizeFilter());
            });

            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart Meeting Room API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter: Bearer {your JWT}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });

            var app = builder.Build();

           
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();

                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                foreach (var role in new[] { "Admin", "User" })
                    if (!await roleMgr.RoleExistsAsync(role))
                        await roleMgr.CreateAsync(new IdentityRole<int>(role));
            }

            app.UseHttpsRedirection();

           
            app.UseAuthentication();   
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.DocumentTitle = "Smart Meeting Room API";
            });

            app.MapControllers();
            app.Run();
        }
    }
}
