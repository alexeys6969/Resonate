using Microsoft.Win32;
using Resonate.Context;
using Resonate.Model;
using Resonate.Model.SaleClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Resonate.Windows
{
    public partial class ReportWindow : Window
    {
        private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");

        public ReportWindow()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private async void GenerateReport(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate?.Date;
            DateTime? endDate = EndDatePicker.SelectedDate?.Date;

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                new InfoWindow("Дата начала периода не может быть больше даты окончания.").Show();
                return;
            }

            GenerateButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            string previousContent = GenerateButton.Content?.ToString() ?? "Сформировать";
            GenerateButton.Content = "Формирование...";

            try
            {
                SalesReportData reportData = await BuildSalesReportDataAsync(startDate, endDate);

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить отчёт по продажам",
                    Filter = "Документ Word (*.docx)|*.docx",
                    FileName = $"Отчет_по_продажам_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                CreateWordReport(saveDialog.FileName, reportData);
                new InfoWindow($"Отчёт успешно сохранён:\n{saveDialog.FileName}").Show();
                Close();
            }
            catch (Exception ex)
            {
                new InfoWindow("Не удалось сформировать отчёт: " + ex.Message).Show();
            }
            finally
            {
                GenerateButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                GenerateButton.Content = previousContent;
            }
        }

        private async Task<SalesReportData> BuildSalesReportDataAsync(DateTime? startDate, DateTime? endDate)
        {
            Task<List<Sale>> salesTask = SaleContext.GetSales();
            Task<List<Product>> productsTask = ProductContext.GetProducts();
            Task<List<Category>> categoriesTask = CategoryContext.GetCategories();
            Task<Employees> employeeTask = EmployeeContext.GetCurrentEmployee(MainWindow.Token);

            await Task.WhenAll(salesTask, productsTask, categoriesTask, employeeTask);

            List<Sale> sales = salesTask.Result ?? new List<Sale>();
            Dictionary<int, Product> productsById = (productsTask.Result ?? new List<Product>())
                .Where(product => product != null)
                .GroupBy(product => product.Id)
                .ToDictionary(group => group.Key, group => group.First());

            Dictionary<int, Category> categoriesById = (categoriesTask.Result ?? new List<Category>())
                .Where(category => category != null)
                .GroupBy(category => category.Id)
                .ToDictionary(group => group.Key, group => group.First());

            Employees employee = employeeTask.Result;

            List<Sale> filteredSales = sales
                .Where(sale => sale != null)
                .Where(sale => !startDate.HasValue || sale.Sale_Date.Date >= startDate.Value)
                .Where(sale => !endDate.HasValue || sale.Sale_Date.Date <= endDate.Value)
                .OrderBy(sale => sale.Sale_Date)
                .ToList();

            List<ReportSaleItem> reportItems = filteredSales
                .SelectMany(sale => (sale.Sale_Items ?? new List<SaleItem>())
                    .Where(item => item != null)
                    .Select(item => new ReportSaleItem
                    {
                        ProductName = ResolveProductName(item, productsById),
                        CategoryName = ResolveCategoryName(item, productsById, categoriesById),
                        Quantity = item.Quantity,
                        Amount = item.Quantity * item.Price_At_Sale
                    }))
                .ToList();

            int salesCount = filteredSales.Count;
            decimal totalRevenue = filteredSales.Sum(sale => sale.Total_Amount);
            decimal averageCheck = salesCount > 0 ? totalRevenue / salesCount : 0m;

            List<ProductReportRow> productRows = reportItems
                .GroupBy(item => item.ProductName)
                .Select(group => new ProductReportRow
                {
                    ProductName = group.Key,
                    Quantity = group.Sum(item => item.Quantity),
                    AveragePrice = group.Sum(item => item.Amount) / Math.Max(group.Sum(item => item.Quantity), 1),
                    Amount = group.Sum(item => item.Amount)
                })
                .OrderByDescending(row => row.Amount)
                .ThenBy(row => row.ProductName)
                .ToList();

            List<TopProductRow> topProducts = productRows
                .OrderByDescending(row => row.Quantity)
                .ThenByDescending(row => row.Amount)
                .Take(5)
                .Select(row => new TopProductRow
                {
                    ProductName = row.ProductName,
                    Quantity = row.Quantity
                })
                .ToList();

            List<CategoryReportRow> categoryRows = reportItems
                .GroupBy(item => item.CategoryName)
                .Select(group => new CategoryReportRow
                {
                    CategoryName = group.Key,
                    Amount = group.Sum(item => item.Amount)
                })
                .OrderByDescending(row => row.Amount)
                .ThenBy(row => row.CategoryName)
                .ToList();

            List<SalesByDateRow> salesByDateRows = filteredSales
                .GroupBy(sale => sale.Sale_Date.Date)
                .Select(group => new SalesByDateRow
                {
                    Date = group.Key,
                    SalesCount = group.Count(),
                    Amount = group.Sum(sale => sale.Total_Amount)
                })
                .OrderBy(row => row.Date)
                .ToList();

            string mostPopularProduct = topProducts.Count > 0 ? topProducts[0].ProductName : "Нет данных";
            string mostProfitableCategory = categoryRows.Count > 0 ? categoryRows[0].CategoryName : "Нет данных";

            return new SalesReportData
            {
                PeriodText = BuildPeriodText(startDate, endDate, filteredSales),
                GeneratedAtText = DateTime.Now.ToString("dd.MM.yyyy HH:mm", RussianCulture),
                ResponsibleEmployee = employee != null && !string.IsNullOrWhiteSpace(employee.Full_Name) ? employee.Full_Name : "Не определён",
                SalesCount = salesCount,
                TotalRevenue = totalRevenue,
                AverageCheck = averageCheck,
                CustomerCountText = "Не ведется в системе",
                ProductRows = productRows,
                TopProducts = topProducts,
                CategoryRows = categoryRows,
                SalesByDateRows = salesByDateRows,
                MostPopularProduct = mostPopularProduct,
                MostProfitableCategory = mostProfitableCategory,
                Conclusion = BuildConclusion(salesCount, totalRevenue, averageCheck, mostPopularProduct, mostProfitableCategory)
            };
        }

        private static void CreateWordReport(string filePath, SalesReportData reportData)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (Package package = Package.Open(filePath, FileMode.Create))
            {
                Uri documentUri = PackUriHelper.CreatePartUri(new Uri("/word/document.xml", UriKind.Relative));
                Uri stylesUri = PackUriHelper.CreatePartUri(new Uri("/word/styles.xml", UriKind.Relative));

                PackagePart documentPart = package.CreatePart(
                    documentUri,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml",
                    CompressionOption.Maximum);

                PackagePart stylesPart = package.CreatePart(
                    stylesUri,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml",
                    CompressionOption.Maximum);

                package.CreateRelationship(
                    documentUri,
                    TargetMode.Internal,
                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");

                documentPart.CreateRelationship(
                    stylesUri,
                    TargetMode.Internal,
                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles");

                WritePackagePart(documentPart, BuildDocumentXml(reportData));
                WritePackagePart(stylesPart, BuildStylesXml());
            }
        }

        private static void WritePackagePart(PackagePart part, string xml)
        {
            using (Stream stream = part.GetStream(FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(xml);
            }
        }

        private static string BuildDocumentXml(SalesReportData reportData)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            builder.Append("<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">");
            builder.Append("<w:body>");

            builder.Append(Paragraph("ОТЧЕТ ПО ПРОДАЖАМ", "ReportTitle"));

            builder.Append(Paragraph("1. Общая информация", "ReportSection"));
            builder.Append(Paragraph("Период отчета: " + reportData.PeriodText, "ReportBody"));
            builder.Append(Paragraph("Дата формирования отчета: " + reportData.GeneratedAtText, "ReportBody"));
            builder.Append(Paragraph("Ответственный сотрудник: " + reportData.ResponsibleEmployee, "ReportBody"));

            builder.Append(Paragraph("2. Общие показатели продаж", "ReportSection"));
            builder.Append(Table(
                new[] { 5400, 3600 },
                new[] { "Показатель", "Значение" },
                new List<string[]>
                {
                    new[] { "Количество продаж", reportData.SalesCount.ToString() },
                    new[] { "Общая сумма продаж", FormatMoney(reportData.TotalRevenue) },
                    new[] { "Средний чек", FormatMoney(reportData.AverageCheck) },
                    new[] { "Количество клиентов", reportData.CustomerCountText }
                }));

            builder.Append(Paragraph("3. Продажи по товарам", "ReportSection"));
            builder.Append(Table(
                new[] { 700, 3400, 1300, 1700, 1700 },
                new[] { "№", "Наименование товара", "Количество", "Цена", "Сумма" },
                reportData.ProductRows.Count > 0
                    ? reportData.ProductRows.Select((row, index) => new[]
                    {
                        (index + 1).ToString(),
                        row.ProductName,
                        row.Quantity.ToString(),
                        FormatMoney(row.AveragePrice),
                        FormatMoney(row.Amount)
                    }).ToList()
                    : new List<string[]> { new[] { "-", "Нет данных за выбранный период", "-", "-", "-" } }));

            builder.Append(Paragraph("4. Самые продаваемые товары", "ReportSection"));
            builder.Append(Table(
                new[] { 6400, 2600 },
                new[] { "Товар", "Количество продаж" },
                reportData.TopProducts.Count > 0
                    ? reportData.TopProducts.Select(row => new[] { row.ProductName, row.Quantity.ToString() }).ToList()
                    : new List<string[]> { new[] { "Нет данных", "-" } }));

            builder.Append(Paragraph("5. Продажи по категориям", "ReportSection"));
            builder.Append(Table(
                new[] { 6400, 2600 },
                new[] { "Категория", "Сумма продаж" },
                reportData.CategoryRows.Count > 0
                    ? reportData.CategoryRows.Select(row => new[] { row.CategoryName, FormatMoney(row.Amount) }).ToList()
                    : new List<string[]> { new[] { "Нет данных", "-" } }));

            builder.Append(Paragraph("6. Продажи по датам", "ReportSection"));
            builder.Append(Table(
                new[] { 2800, 2600, 3600 },
                new[] { "Дата", "Количество продаж", "Сумма" },
                reportData.SalesByDateRows.Count > 0
                    ? reportData.SalesByDateRows.Select(row => new[]
                    {
                        row.Date.ToString("dd.MM.yyyy", RussianCulture),
                        row.SalesCount.ToString(),
                        FormatMoney(row.Amount)
                    }).ToList()
                    : new List<string[]> { new[] { "Нет данных", "-", "-" } }));

            builder.Append(Paragraph("8. Итоги", "ReportSection"));
            builder.Append(Paragraph("Общая выручка: " + FormatMoney(reportData.TotalRevenue), "ReportBody"));
            builder.Append(Paragraph("Наиболее популярный товар: " + reportData.MostPopularProduct, "ReportBody"));
            builder.Append(Paragraph("Наиболее прибыльная категория: " + reportData.MostProfitableCategory, "ReportBody"));
            builder.Append(Paragraph("Вывод по продажам: " + reportData.Conclusion, "ReportBody"));

            builder.Append("<w:sectPr>");
            builder.Append("<w:pgSz w:w=\"11906\" w:h=\"16838\"/>");
            builder.Append("<w:pgMar w:top=\"1134\" w:right=\"1134\" w:bottom=\"1134\" w:left=\"1134\" w:header=\"708\" w:footer=\"708\" w:gutter=\"0\"/>");
            builder.Append("</w:sectPr>");

            builder.Append("</w:body>");
            builder.Append("</w:document>");
            return builder.ToString();
        }

        private static string BuildStylesXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                "<w:styles xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">" +
                "<w:style w:type=\"paragraph\" w:default=\"1\" w:styleId=\"Normal\">" +
                "<w:name w:val=\"Normal\"/>" +
                "<w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"both\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:after=\"120\"/><w:ind w:firstLine=\"709\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/></w:rPr>" +
                "</w:style>" +
                "<w:style w:type=\"paragraph\" w:styleId=\"ReportTitle\">" +
                "<w:name w:val=\"ReportTitle\"/><w:basedOn w:val=\"Normal\"/><w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"center\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:after=\"240\"/><w:ind w:firstLine=\"0\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/><w:b/><w:bCs/></w:rPr>" +
                "</w:style>" +
                "<w:style w:type=\"paragraph\" w:styleId=\"ReportSection\">" +
                "<w:name w:val=\"ReportSection\"/><w:basedOn w:val=\"Normal\"/><w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"left\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:before=\"160\" w:after=\"120\"/><w:ind w:firstLine=\"0\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/><w:b/><w:bCs/></w:rPr>" +
                "</w:style>" +
                "<w:style w:type=\"paragraph\" w:styleId=\"ReportBody\">" +
                "<w:name w:val=\"ReportBody\"/><w:basedOn w:val=\"Normal\"/><w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"both\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:after=\"120\"/><w:ind w:firstLine=\"709\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/></w:rPr>" +
                "</w:style>" +
                "<w:style w:type=\"paragraph\" w:styleId=\"ReportTable\">" +
                "<w:name w:val=\"ReportTable\"/><w:basedOn w:val=\"Normal\"/><w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"left\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:after=\"0\"/><w:ind w:firstLine=\"0\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/></w:rPr>" +
                "</w:style>" +
                "<w:style w:type=\"paragraph\" w:styleId=\"ReportTableHeader\">" +
                "<w:name w:val=\"ReportTableHeader\"/><w:basedOn w:val=\"ReportTable\"/><w:qFormat/>" +
                "<w:pPr><w:jc w:val=\"center\"/><w:spacing w:line=\"360\" w:lineRule=\"auto\" w:after=\"0\"/><w:ind w:firstLine=\"0\"/></w:pPr>" +
                "<w:rPr><w:rFonts w:ascii=\"Times New Roman\" w:hAnsi=\"Times New Roman\" w:cs=\"Times New Roman\"/><w:sz w:val=\"28\"/><w:szCs w:val=\"28\"/><w:b/><w:bCs/></w:rPr>" +
                "</w:style>" +
                "</w:styles>";
        }

        private static string Paragraph(string text, string styleId)
        {
            return "<w:p><w:pPr><w:pStyle w:val=\"" + styleId + "\"/></w:pPr><w:r><w:t>" + EscapeXml(text) + "</w:t></w:r></w:p>";
        }

        private static string Table(int[] columnWidths, string[] headers, List<string[]> rows)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<w:tbl>");
            builder.Append("<w:tblPr>");
            builder.Append("<w:tblW w:w=\"0\" w:type=\"auto\"/>");
            builder.Append("<w:tblBorders>");
            builder.Append("<w:top w:val=\"single\" w:sz=\"8\" w:space=\"0\" w:color=\"808080\"/>");
            builder.Append("<w:left w:val=\"single\" w:sz=\"8\" w:space=\"0\" w:color=\"808080\"/>");
            builder.Append("<w:bottom w:val=\"single\" w:sz=\"8\" w:space=\"0\" w:color=\"808080\"/>");
            builder.Append("<w:right w:val=\"single\" w:sz=\"8\" w:space=\"0\" w:color=\"808080\"/>");
            builder.Append("<w:insideH w:val=\"single\" w:sz=\"6\" w:space=\"0\" w:color=\"A0A0A0\"/>");
            builder.Append("<w:insideV w:val=\"single\" w:sz=\"6\" w:space=\"0\" w:color=\"A0A0A0\"/>");
            builder.Append("</w:tblBorders>");
            builder.Append("<w:tblCellMar><w:top w:w=\"80\" w:type=\"dxa\"/><w:left w:w=\"90\" w:type=\"dxa\"/><w:bottom w:w=\"80\" w:type=\"dxa\"/><w:right w:w=\"90\" w:type=\"dxa\"/></w:tblCellMar>");
            builder.Append("</w:tblPr>");

            builder.Append("<w:tblGrid>");
            foreach (int width in columnWidths)
                builder.Append("<w:gridCol w:w=\"" + width + "\"/>");
            builder.Append("</w:tblGrid>");

            builder.Append("<w:tr>");
            for (int i = 0; i < headers.Length; i++)
                builder.Append(TableCell(headers[i], columnWidths[i], "ReportTableHeader"));
            builder.Append("</w:tr>");

            foreach (string[] row in rows)
            {
                builder.Append("<w:tr>");
                for (int i = 0; i < row.Length && i < columnWidths.Length; i++)
                    builder.Append(TableCell(row[i], columnWidths[i], "ReportTable"));
                builder.Append("</w:tr>");
            }

            builder.Append("</w:tbl>");
            return builder.ToString();
        }

        private static string TableCell(string text, int width, string styleId)
        {
            return "<w:tc>" +
                   "<w:tcPr><w:tcW w:w=\"" + width + "\" w:type=\"dxa\"/></w:tcPr>" +
                   "<w:p><w:pPr><w:pStyle w:val=\"" + styleId + "\"/></w:pPr><w:r><w:t>" + EscapeXml(text) + "</w:t></w:r></w:p>" +
                   "</w:tc>";
        }

        private static string EscapeXml(string value)
        {
            return SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;
        }

        private static string BuildPeriodText(DateTime? startDate, DateTime? endDate, List<Sale> filteredSales)
        {
            if (startDate.HasValue && endDate.HasValue)
                return $"{startDate.Value:dd.MM.yyyy} - {endDate.Value:dd.MM.yyyy}";

            if (filteredSales.Count == 0)
                return "Период не определён";

            DateTime minDate = filteredSales.Min(sale => sale.Sale_Date).Date;
            DateTime maxDate = filteredSales.Max(sale => sale.Sale_Date).Date;
            return $"{minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}";
        }

        private static string BuildConclusion(int salesCount, decimal totalRevenue, decimal averageCheck, string mostPopularProduct, string mostProfitableCategory)
        {
            if (salesCount == 0)
                return "За выбранный период продажи отсутствуют.";

            return string.Format(
                RussianCulture,
                "За выбранный период оформлено {0} продаж на сумму {1}. Средний чек составил {2}. Лидером по спросу стал товар \"{3}\", а максимальную выручку принесла категория \"{4}\".",
                salesCount,
                FormatMoney(totalRevenue),
                FormatMoney(averageCheck),
                mostPopularProduct,
                mostProfitableCategory);
        }

        private static string ResolveProductName(SaleItem item, Dictionary<int, Product> productsById)
        {
            if (item?.Product != null && !string.IsNullOrWhiteSpace(item.Product.Name))
                return item.Product.Name;

            if (item != null && productsById.ContainsKey(item.Product_id))
                return productsById[item.Product_id].ToString();

            return item != null ? $"Товар #{item.Product_id}" : "Неизвестный товар";
        }

        private static string ResolveCategoryName(SaleItem item, Dictionary<int, Product> productsById, Dictionary<int, Category> categoriesById)
        {
            if (item?.Product?.Category != null && !string.IsNullOrWhiteSpace(item.Product.Category.Name))
                return item.Product.Category.Name;

            if (item != null && productsById.ContainsKey(item.Product_id))
            {
                Product product = productsById[item.Product_id];
                if (product.Category != null && !string.IsNullOrWhiteSpace(product.Category.Name))
                    return product.Category.Name;

                if (categoriesById.ContainsKey(product.Category_Id))
                    return categoriesById[product.Category_Id].ToString();
            }

            return "Без категории";
        }

        private static string FormatMoney(decimal value)
        {
            return string.Format(RussianCulture, "{0:N2} ₽", value);
        }

        private void CancelReport(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class ReportSaleItem
        {
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public int Quantity { get; set; }
            public decimal Amount { get; set; }
        }

        private class ProductReportRow
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal AveragePrice { get; set; }
            public decimal Amount { get; set; }
        }

        private class TopProductRow
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
        }

        private class CategoryReportRow
        {
            public string CategoryName { get; set; }
            public decimal Amount { get; set; }
        }

        private class SalesByDateRow
        {
            public DateTime Date { get; set; }
            public int SalesCount { get; set; }
            public decimal Amount { get; set; }
        }

        private class SalesReportData
        {
            public string PeriodText { get; set; }
            public string GeneratedAtText { get; set; }
            public string ResponsibleEmployee { get; set; }
            public int SalesCount { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal AverageCheck { get; set; }
            public string CustomerCountText { get; set; }
            public List<ProductReportRow> ProductRows { get; set; }
            public List<TopProductRow> TopProducts { get; set; }
            public List<CategoryReportRow> CategoryRows { get; set; }
            public List<SalesByDateRow> SalesByDateRows { get; set; }
            public string MostPopularProduct { get; set; }
            public string MostProfitableCategory { get; set; }
            public string Conclusion { get; set; }
        }
    }
}
