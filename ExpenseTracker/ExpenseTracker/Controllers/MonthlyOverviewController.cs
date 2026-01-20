using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Packaging;

namespace ExpenseTracker.Controllers
{
    public class MonthlyOverviewController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MonthlyOverviewController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Default to current month if not specified
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            // Get all transactions for selected month
            var transactions = await _context.Transactions
                .Where(t => t.Date.Month == selectedMonth && t.Date.Year == selectedYear)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Calculate totals
            var totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpense = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpense;

            // Income by Category
            var incomeByCategory = transactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Expense by Category
            var expenseByCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Generate month/year options for dropdown (last 12 months)
            var monthYearOptions = new List<dynamic>();
            for (int i = 0; i < 12; i++)
            {
                var date = DateTime.Now.AddMonths(-i);
                dynamic option = new System.Dynamic.ExpandoObject();
                option.Month = date.Month;
                option.Year = date.Year;
                option.Display = date.ToString("MMMM yyyy");
                monthYearOptions.Add(option);
            }

            // Pass data to view
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.MonthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy");
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;
            ViewBag.Transactions = transactions;
            ViewBag.IncomeByCategory = incomeByCategory;
            ViewBag.ExpenseByCategory = expenseByCategory;
            ViewBag.MonthYearOptions = monthYearOptions;

            return View();
        }

        public async Task<IActionResult> ExportToExcel(int month, int year)
        {
            try
            {
                // Get all transactions for selected month
                var transactions = await _context.Transactions
                    .Where(t => t.Date.Month == month && t.Date.Year == year)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                // Calculate totals
                var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
                var totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
                var balance = totalIncome - totalExpense;

                // Income by category
                var incomeByCategory = transactions
                    .Where(t => t.Type == "Income")
                    .GroupBy(t => t.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(t => t.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToList();

                // Expense by category
                var expenseByCategory = transactions
                    .Where(t => t.Type == "Expense")
                    .GroupBy(t => t.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Amount = g.Sum(t => t.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToList();

                var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

                // Create Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    // ========== SUMMARY SHEET ==========
                    var summarySheet = workbook.Worksheets.Add("Summary");

                    // Title
                    summarySheet.Cell("A1").Value = "MONTHLY REPORT";
                    summarySheet.Cell("A1").Style.Font.Bold = true;
                    summarySheet.Cell("A1").Style.Font.FontSize = 18;
                    summarySheet.Range("A1:B1").Merge();

                    summarySheet.Cell("A2").Value = monthName;
                    summarySheet.Cell("A2").Style.Font.FontSize = 14;
                    summarySheet.Range("A2:B2").Merge();

                    // Summary section
                    summarySheet.Cell("A4").Value = "Summary";
                    summarySheet.Cell("A4").Style.Font.Bold = true;
                    summarySheet.Cell("A4").Style.Font.FontSize = 12;

                    summarySheet.Cell("A5").Value = "Total Income:";
                    summarySheet.Cell("B5").Value = totalIncome;
                    summarySheet.Cell("B5").Style.NumberFormat.Format = "#,##0.00";
                    summarySheet.Cell("B5").Style.Font.FontColor = XLColor.Green;

                    summarySheet.Cell("A6").Value = "Total Expense:";
                    summarySheet.Cell("B6").Value = totalExpense;
                    summarySheet.Cell("B6").Style.NumberFormat.Format = "#,##0.00";
                    summarySheet.Cell("B6").Style.Font.FontColor = XLColor.Red;

                    summarySheet.Cell("A7").Value = "Balance:";
                    summarySheet.Cell("B7").Value = balance;
                    summarySheet.Cell("B7").Style.NumberFormat.Format = "#,##0.00";
                    summarySheet.Cell("B7").Style.Font.Bold = true;
                    summarySheet.Cell("B7").Style.Font.FontColor = balance >= 0 ? XLColor.Green : XLColor.Red;

                    // Income Breakdown
                    if (incomeByCategory.Any())
                    {
                        summarySheet.Cell("A9").Value = "Income Breakdown";
                        summarySheet.Cell("A9").Style.Font.Bold = true;
                        summarySheet.Cell("A10").Value = "Category";
                        summarySheet.Cell("B10").Value = "Transactions";
                        summarySheet.Cell("C10").Value = "Amount";
                        summarySheet.Range("A10:C10").Style.Font.Bold = true;
                        summarySheet.Range("A10:C10").Style.Fill.BackgroundColor = XLColor.LightGreen;

                        int incomeRow = 11;
                        foreach (var item in incomeByCategory)
                        {
                            summarySheet.Cell($"A{incomeRow}").Value = item.Category;
                            summarySheet.Cell($"B{incomeRow}").Value = item.Count;
                            summarySheet.Cell($"C{incomeRow}").Value = item.Amount;
                            summarySheet.Cell($"C{incomeRow}").Style.NumberFormat.Format = "#,##0.00";
                            incomeRow++;
                        }
                    }

                    // Expense Breakdown
                    if (expenseByCategory.Any())
                    {
                        int expenseStartRow = incomeByCategory.Any() ? 11 + incomeByCategory.Count + 2 : 9;
                        summarySheet.Cell($"A{expenseStartRow}").Value = "Expense Breakdown";
                        summarySheet.Cell($"A{expenseStartRow}").Style.Font.Bold = true;

                        int headerRow = expenseStartRow + 1;
                        summarySheet.Cell($"A{headerRow}").Value = "Category";
                        summarySheet.Cell($"B{headerRow}").Value = "Transactions";
                        summarySheet.Cell($"C{headerRow}").Value = "Amount";
                        summarySheet.Range($"A{headerRow}:C{headerRow}").Style.Font.Bold = true;
                        summarySheet.Range($"A{headerRow}:C{headerRow}").Style.Fill.BackgroundColor = XLColor.LightPink;

                        int expenseRow = headerRow + 1;
                        foreach (var item in expenseByCategory)
                        {
                            summarySheet.Cell($"A{expenseRow}").Value = item.Category;
                            summarySheet.Cell($"B{expenseRow}").Value = item.Count;
                            summarySheet.Cell($"C{expenseRow}").Value = item.Amount;
                            summarySheet.Cell($"C{expenseRow}").Style.NumberFormat.Format = "#,##0.00";
                            expenseRow++;
                        }
                    }

                    summarySheet.Columns().AdjustToContents();

                    // ========== ALL TRANSACTIONS SHEET ==========
                    var transSheet = workbook.Worksheets.Add("All Transactions");

                    // Headers
                    transSheet.Cell("A1").Value = "Date";
                    transSheet.Cell("B1").Value = "Type";
                    transSheet.Cell("C1").Value = "Category";
                    transSheet.Cell("D1").Value = "Description";
                    transSheet.Cell("E1").Value = "Amount";

                    // Style header
                    var headerRange = transSheet.Range("A1:E1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Add transaction data
                    int row = 2;
                    foreach (var transaction in transactions)
                    {
                        transSheet.Cell($"A{row}").Value = transaction.Date.ToString("dd-MMM-yyyy");
                        transSheet.Cell($"B{row}").Value = transaction.Type;
                        transSheet.Cell($"C{row}").Value = transaction.Category;
                        transSheet.Cell($"D{row}").Value = transaction.Description ?? "-";
                        transSheet.Cell($"E{row}").Value = transaction.Amount;
                        transSheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00";

                        // Color code by type
                        if (transaction.Type == "Income")
                        {
                            transSheet.Cell($"B{row}").Style.Font.FontColor = XLColor.Green;
                        }
                        else
                        {
                            transSheet.Cell($"B{row}").Style.Font.FontColor = XLColor.Red;
                        }

                        row++;
                    }

                    // Add total row
                    if (transactions.Any())
                    {
                        transSheet.Cell($"D{row}").Value = "TOTAL:";
                        transSheet.Cell($"D{row}").Style.Font.Bold = true;
                        transSheet.Cell($"E{row}").FormulaA1 = $"=SUM(E2:E{row - 1})";
                        transSheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00";
                        transSheet.Cell($"E{row}").Style.Font.Bold = true;
                        transSheet.Range($"D{row}:E{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    transSheet.Columns().AdjustToContents();

                    // Save to stream and return file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        var fileName = $"Monthly_Report_{monthName.Replace(" ", "_")}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                // If export fails, redirect back with error message
                TempData["ErrorMessage"] = $"Failed to export: {ex.Message}";
                return RedirectToAction("Index", new { month = month, year = year });
            }
        }
    }
}