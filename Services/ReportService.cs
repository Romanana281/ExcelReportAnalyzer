using ExcelReportAnalyzer.Database;
using ExcelReportAnalyzer.Interfaces;
using ExcelReportAnalyzer.Models.Entities;
using ExcelReportAnalyzer.Models.Enums;
using ExcelReportAnalyzer.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace ExcelReportAnalyzer.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationContext _db;

        public ReportService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<List<ReportResponse>> GetAllReports()
        {
            var reports = await _db.ReportTasks.Select(report => new ReportResponse
            {
                Id = report.Id,
                Name = report.FileName,
                Status = report.Status,
                Date = report.CreatedAt
            }).ToListAsync();

            return reports;
        }

        public async Task<ReportResponse> GetReportById(Guid id)
        {
            var report = await _db.ReportTasks.Select(report => new ReportResponse
            {
                Id = report.Id,
                Name = report.FileName,
                Status = report.Status,
                Date = report.CreatedAt
            }
            ).FirstOrDefaultAsync();

            if (report == null)
            {
                throw new ArgumentException("Id not found");
            }

            return report;
        }

        public async Task<ReportSummaryResponse?> GetSummary(Guid id)
        {
            var reportExists = await _db.ReportTasks.AnyAsync(x => x.Id == id);

            if (!reportExists)
            {
                throw new ArgumentException("Id not found.");
            }

            var hasReportRows = await _db.ReportRows.AnyAsync(x => x.ReportId == id);

            if (!hasReportRows)
            {
                throw new ArgumentException("Failed to process the report. Errors were detected during data parsing.");
            }

            var reportRows = await _db.ReportRows.Where(x => x.ReportId == id).ToListAsync();
            var reportTask = await _db.ReportTasks.FindAsync(id);

            return new ReportSummaryResponse
            {
                Id = id,
                TotalSum = reportRows.Sum(x => x.Amount) ?? 0,
                AvgSum = reportRows.Average(x => x.Amount) ?? 0,
                CountRows = reportTask?.TotalRows ?? 0,
                CountErors = reportTask?.ErrorCount ?? 0,
                TopClient = reportRows
                    .GroupBy(x => x.Client)
                    .MaxBy(g => g.Sum(x => x.Amount))
                    ?.Key
            };
        }

        public async Task<List<ErrorResponse>> GetErrors(Guid id)
        {
            var reportExists = await _db.ReportTasks.AnyAsync(x => x.Id == id);

            if (!reportExists)
            {
                throw new ArgumentException("Id not found.");
            }

            var hasValidationErrors = await _db.ValidationErrors.AnyAsync(x => x.ReportId == id);

            if (!hasValidationErrors)
            {
                throw new ArgumentException("Failed to process the report. Errors were detected during data parsing.");
            }

            return await _db.ValidationErrors
                .Where(x => x.ReportId == id)
                .Select(x => new ErrorResponse
                {
                    RowNumber = x.RowNumber,
                    ColumnName = x.ColumnName,
                    ErrorMessage = x.ErrorMessage
                })
                .OrderBy(x => x.RowNumber)
                .ToListAsync();
        }

        public async Task<Status> GetStatus(Guid id)
        {
            var report = await _db.ReportTasks.FindAsync(id);

            if (report != null)
            {
                return report.Status;
            }

            throw new ArgumentException("Id not found.");
        }

        public async Task<Guid> UploadReport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is missing or empty");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".xlsx")
            {
                throw new ArgumentException("Invalid file format. Only .xlsx is supported");
            }

            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }

            var Id = Guid.NewGuid();
            string filePath = Path.Combine(reportsPath, $"{Id}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var reportTask = new ReportTask
            {
                Id = Id,
                FileName = file.FileName,
                FilePath = filePath,
                Status = Status.Uploaded,
                CreatedAt = DateTime.UtcNow,
            };

            _db.ReportTasks.Add(reportTask);
            await _db.SaveChangesAsync();

            return Id;
        }

        public async Task<Guid> Delete(Guid id)
        {
            var reportTask = await _db.ReportTasks.FirstOrDefaultAsync(x => x.Id == id);
            if (reportTask != null)
            {
                _db.ReportTasks.Remove(reportTask);

                var reportRows = await _db.ReportRows
                                    .Where(x => x.ReportId == id)
                                    .ToListAsync();

                _db.ReportRows.RemoveRange(reportRows);

                var validationError = await _db.ValidationErrors
                    .Where(x => x.ReportId == id)
                    .ToListAsync();

                if (validationError != null)
                {
                    _db.ValidationErrors.RemoveRange(validationError);
                }

                if (File.Exists(reportTask.FilePath))
                {
                    File.Delete(reportTask.FilePath);
                }

                await _db.SaveChangesAsync();
                return id;
            }

            return Guid.Empty;
        }
    }
}