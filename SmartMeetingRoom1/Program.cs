using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartMeetingRoom1.Models;
using SmartMeetingRoom1.Services;
using SmartMeetingRoom1.Interfaces;

namespace SmartMeetingRoom1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddScoped<IMeeting, IMeetingServices>();
            builder.Services.AddScoped<IRoom, IRoomServices>();
            builder.Services.AddScoped<IUser, IUserServices>();
            builder.Services.AddScoped<INotification, INotificationServices>();
            builder.Services.AddScoped<IMinute, IMinuteServices>();
            builder.Services.AddScoped<IMeetingAttendee, IMeetingAttendeeServices>();
            builder.Services.AddScoped<IMeetingAttendeeServices, IMeetingAttendeeServices>();
            builder.Services.AddScoped<IActionItem, IActionItemServices>();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllers();

            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Smart Meeting Room API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

         
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = ""; 
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.DocumentTitle = "Smart Meeting Room API";
            });

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
