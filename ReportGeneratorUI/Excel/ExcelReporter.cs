using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

namespace ReportGenerator;
internal static class ExcelReporter
{
    public static void SaveReport(string path, IEnumerable<ProductHierarchy> hierarchy)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage();
        var title = $"{DateTime.Now:dd/mm/yyyy HH.MM}";
        var sheet = package.Workbook.Worksheets.Add(title);

        sheet.Cells["A1"].Value = "Изделие";
        sheet.Cells["B1"].Value = "Кол-во";
        sheet.Cells["C1"].Value = "Стоимость";
        sheet.Cells["D1"].Value = "Цена";
        sheet.Cells["E1"].Value = "Кол-во входящих";

        int row = 1;
        foreach (var item in hierarchy)
        {
            row++;
            sheet.Cells[row, 1].Value = new string(' ', (int)item.Level * 2) + item.Name;
            sheet.Cells[row, 2].Value = item.Count;
            sheet.Cells[row, 3].Value = item.Cost;
            sheet.Cells[row, 4].Value = item.Price;
            sheet.Cells[row, 5].Value = item.InclusionCount;
        }

        sheet.Columns.AutoFit();
        
        var header = sheet.Cells[1, 1, 1, 5].Style;
        header.Font.Bold = true;
        header.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        header.VerticalAlignment = ExcelVerticalAlignment.Center;
        header.Fill.SetBackground(Color.LightBlue, ExcelFillStyle.Solid);
        header.Border.BorderAround(ExcelBorderStyle.Thin);

        var data = sheet.Cells[2,1, row, 5].Style;
        data.Fill.SetBackground(Color.LightCyan, ExcelFillStyle.Solid);
        data.Border.BorderAround(ExcelBorderStyle.Thin);
        
        var excelfilebytes = package.GetAsByteArray();
        File.WriteAllBytes(path, excelfilebytes);
    }
}
