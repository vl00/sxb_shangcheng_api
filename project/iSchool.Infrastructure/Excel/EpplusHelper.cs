using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public static partial class EpplusHelper
    {
        public static ExcelPackage TryGetExcelPackage(string path, out Exception ex)
        {
            return TryGetExcelPackage(new FileInfo(path), out ex);
        }

        public static ExcelPackage TryGetExcelPackage(FileInfo fi, out Exception ex)
        {
            try
            {
                var pkg = new ExcelPackage(fi);
                ex = null;
                return pkg;
            }
            catch (Exception ex1)
            {
                ex = ex1;
                return null;
            }
        }

        public static ExcelWorksheet WorkSheetGetOrAdd(this ExcelPackage pkg, string sheetName)
        {
            return pkg.Workbook.Worksheets[sheetName] ?? pkg.Workbook.Worksheets.Add(sheetName);
        }

        public static string CellStrValue(this object v, bool trimStart = true, bool trimEnd = true)
        {
            // 粘贴复制到xlsx时可能会多出空格
            var str = v?.ToString();
            if (string.IsNullOrEmpty(str)) return str;
            if (trimStart) str = str.TrimStart();
            if (trimEnd) str = str.TrimEnd();
            return str;
        }

        /// <summary>
        /// CheckIfEmptyRow(sheet, row, (col, v) => ...)
        /// </summary>
        /// <returns></returns>
        public static bool CheckIfEmptyRow(ExcelWorksheet sheet, int row, Func<int, object, bool> func = null)
        {
            if (row < 1 || row > sheet.Dimension.Rows) return true;
            for (var col = 1; col <= sheet.Dimension.Columns; col++)
            {
                switch (func)
                {
                    case null when sheet.Cells[row, col].Value?.ToString() is string s && !string.IsNullOrWhiteSpace(s):
                        return false;
                    case null:
                        break;
                    default:
                        if (!func(col, sheet.Cells[row, col].Value)) return false;
                        else break;
                }
            }
            return true;
        }

        /// <summary>
        /// 遍历第row行的列 并查找符合条件的cell字符串值
        /// </summary>
        public static IEnumerable<(string Value, int Row, int Col)> FindXlsxRowStringValues(ExcelWorksheet sheet, int row, Func<string, bool> funcField = null)
        {
            for (var col = 1; col <= sheet.Dimension.Columns; col++)
            {
                var v = sheet.Cells[row, col].Value?.ToString()?.Trim();
                if (funcField == null || funcField(v))
                {
                    yield return (v, row, col);
                }
            }
        }

        /// <summary>
        /// 第1行作为字段名, 查找第row行的字段值的cell x.
        /// 找不到时 x.Col==-1
        /// </summary>
        public static (object Value, int Row, int Col) FindXlsxCellByRow1Field(ExcelWorksheet sheet, int row, string field)
        {
            return FindXlsxCellByRowField(sheet, row, field, 1);
        }
        /// <summary>
        /// 第rowForField行作为字段名, 查找第row行的字段值的cell x.
        /// 找不到时 x.Col==-1
        /// </summary>
        public static (object Value, int Row, int Col) FindXlsxCellByRowField(ExcelWorksheet sheet, int row, string field, int rowForField)
        {
            var col = -1;
            for (var i = 1; i <= sheet.Dimension.Columns; i++)
            {
                var cv = sheet.Cells[rowForField, i].Value?.ToString()?.Trim();
                if (string.Equals(cv, field, StringComparison.OrdinalIgnoreCase))
                {
                    col = i;
                    break;
                }
            }
            if (col == -1) return (null, row, -1);
            return (sheet.Cells[row, col].Value, row, col);
        }

        /// <summary>
        /// 先找row行col列的值; 如果为空, 则找此列上一行的值, 直至找到不为空值的cell x.
        /// 找不到时 x.Row == -1
        /// </summary>
        public static (string Value, int Row) FindXlsxRowValueByUpward(ExcelWorksheet sheet, int row, int col)
        {
            for (; row > 0; row--)
            {
                var v = sheet.Cells[row, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(v)) return (v, row);
            }
            return (null, -1);
        }


        public static Task SaveAndDispose(ExcelPackage excel, int ms = 1000)
        {
            excel.Save();
            excel.Dispose();
            return Task.Delay(ms);
        }


    }
}
