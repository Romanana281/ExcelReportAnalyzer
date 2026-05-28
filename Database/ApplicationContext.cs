using ExcelReportAnalyzer.Models;
using ExcelReportAnalyzer.Models.Entities;
using ExcelReportAnalyzer.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ExcelReportAnalyzer.Database
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<ReportTask> ReportTasks => Set<ReportTask>();
        public DbSet<ReportRow> ReportRows => Set<ReportRow>();
        public DbSet<ValidationError> ValidationErrors => Set<ValidationError>();
    }
}