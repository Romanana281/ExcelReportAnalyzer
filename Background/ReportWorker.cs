
using ExcelReportAnalyzer.Database;
using ExcelReportAnalyzer.Parsers;
using ExcelReportAnalyzer.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExcelReportAnalyzer.Background
{
    public class ReportWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReportWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var parser = scope.ServiceProvider.GetRequiredService<ExcelParser>();

                    var reports = await db.ReportTasks
                        .Where(x => x.Status == Status.Uploaded)
                        .ToListAsync(stoppingToken);

                    foreach (var report in reports)
                    {
                        try
                        {
                            report.Status = Status.Processing;
                            await db.SaveChangesAsync(stoppingToken);

                            var (rows, errors) = parser.Parse(report.Id, report.FilePath);

                            if (rows.Count != 0)
                            {
                                await db.ReportRows.AddRangeAsync(rows, stoppingToken);
                                await db.SaveChangesAsync(stoppingToken);
                            }

                            if (errors.Count != 0)
                            {
                                await db.ValidationErrors.AddRangeAsync(errors, stoppingToken);
                                await db.SaveChangesAsync(stoppingToken);
                            }

                            report.TotalRows = rows.Count;
                            report.ErrorCount = errors.Count;
                            report.Status = Status.Completed;
                            report.ProcessedAt = DateTime.UtcNow;

                            await db.SaveChangesAsync(stoppingToken);
                        }
                        catch
                        {
                            db.ChangeTracker.Clear();
                            db.ReportTasks.Attach(report);

                            report.Status = Status.Failed;
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }

                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}