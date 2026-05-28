
using ExcelReportAnalyzer.Background;
using ExcelReportAnalyzer.Database;
using ExcelReportAnalyzer.Interfaces;
using ExcelReportAnalyzer.Parsers;
using ExcelReportAnalyzer.Services;
using Microsoft.EntityFrameworkCore;

namespace ExcelReportAnalyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
            );

            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<ExcelParser>();
            builder.Services.AddHostedService<ReportWorker>();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();

            app.Run();
        }
    }
}