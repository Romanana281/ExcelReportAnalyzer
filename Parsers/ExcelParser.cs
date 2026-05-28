using ExcelReportAnalyzer.Models.Entities;
using ClosedXML.Excel;

namespace ExcelReportAnalyzer.Parsers
{
    public class ExcelParser
    {
        public (List<ReportRow>, List<ValidationError>) Parse(Guid id, string path)
        {
            var validRows = new List<ReportRow>();
            var errors = new List<ValidationError>();
            var seen = new HashSet<string>();

            using (var workbook = new XLWorkbook(path))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();

                foreach (var row in rows)
                {
                    int currentRowNum = row.RowNumber();
                    bool isRowValid = true;

                    if (currentRowNum == 1) continue;

                    var dateCell = row.Cell(1);
                    if (dateCell.TryGetValue<DateTime>(out var dateTime))
                    {
                        dateTime = dateTime.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                            : dateTime.ToUniversalTime();

                        if (dateTime.Date > DateTime.UtcNow.Date)
                        {
                            AddErrors(errors, id, currentRowNum, "Date", "Date cannot be in the future");
                            isRowValid = false;
                        }
                    }
                    else
                    {
                        AddErrors(errors, id, currentRowNum, "Date", "Invalid or empty date");
                        isRowValid = false;
                    }

                    var clientCell = row.Cell(2);
                    string parsedClient = clientCell.GetValue<string>().Trim();
                    if (string.IsNullOrEmpty(parsedClient))
                    {
                        AddErrors(errors, id, currentRowNum, "Client", "Client is required");
                        isRowValid = false;

                    }

                    var productCell = row.Cell(3);
                    string parsedProduct = productCell.GetValue<string>().Trim();
                    if (string.IsNullOrEmpty(parsedProduct))
                    {
                        AddErrors(errors, id, currentRowNum, "Product", "Product is required");
                        isRowValid = false;

                    }

                    var amountCell = row.Cell(4);
                    decimal parsedAmount = 0;
                    if (!amountCell.TryGetValue<decimal>(out parsedAmount))
                    {
                        AddErrors(errors, id, currentRowNum, "Amount", "Invalid or empty amount");
                        isRowValid = false;

                    }
                    else if (parsedAmount < 0)
                    {
                        AddErrors(errors, id, currentRowNum, "Amount", "Amount cannot be negative");
                        isRowValid = false;

                    }

                    if (isRowValid)
                    {
                        string key = $"{dateTime.Date:yyyy-MM-dd}|{parsedClient}|{parsedProduct}|{parsedAmount}";

                        if (!seen.Add(key))
                        {
                            AddErrors(errors, id, currentRowNum, "Duplicate", $"Duplicate entry detected (row {currentRowNum})");
                            isRowValid = false;
                        }
                    }

                    if (isRowValid)
                    {
                        validRows.Add(
                            new ReportRow
                            {
                                ReportId = id,
                                Date = dateTime,
                                Client = parsedClient,
                                Product = parsedProduct,
                                Amount = parsedAmount
                            }
                        );
                    }
                }
            }
            return (validRows, errors);
        }

        private static void AddErrors(List<ValidationError> errors, Guid id, int rowNumber, string columnName, string message)
        {
            errors.Add(
               new ValidationError
               {
                   ReportId = id,
                   RowNumber = rowNumber,
                   ColumnName = columnName,
                   ErrorMessage = message
               }
            );
        }

    }
}