using GymRatService.BLL.Core.Interfaces;
using GymRatService.BLL.Services;
using GymRatService.DAL.Core;
using GymRatService.DAL.Repos;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymRatService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<DBContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddScoped<IQueryHandler, QueryHandler>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
