using NPOI.SS.UserModel;

namespace BinusSchool.Common.Extensions
{
    public static class NpoiExtensions
    {
        public static bool IsCellNullOrEmpty(this ISheet sheet, int rowIndex, int cellIndex)
        {
            if (sheet != null)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null)
                {
                    ICell cell = row.GetCell(cellIndex);
                    return cell.IsNullOrEmpty();
                }
            }
            return true;
        }

        public static bool IsNullOrEmpty(this ICell cell)
        {
            if (cell != null)
            {
                // Uncomment the following lines if you consider a cell 
                // with no value but filled with color to be non-empty.
                //if (cell.CellStyle != null && cell.CellStyle.FillBackgroundColorColor != null)
                //    return false;

                switch (cell.CellType)
                {
                    case CellType.String:
                        return string.IsNullOrWhiteSpace(cell.StringCellValue);
                    case CellType.Boolean:
                    case CellType.Numeric:
                    case CellType.Formula:
                    case CellType.Error:
                        return false;
                }
            }
            // null, blank or unknown
            return true;
        }

        public static string? CellValue(this IRow row, int cellIndex)
        {
            var result = string.Empty;
            if (row != null)
            {
                ICell cell = row.GetCell(cellIndex);
                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        result = cell.NumericCellValue.ToString();
                        break;
                    case CellType.String:
                        result = cell.StringCellValue;
                        break;
                    case CellType.Boolean:
                        result = cell.BooleanCellValue.ToString();
                        break;
                    case CellType.Formula:
                    // Don't use this extension for returning formula value, check to documentation
                    // https://poi.apache.org/components/spreadsheet/eval.html
                    case CellType.Unknown:
                    case CellType.Blank:
                    case CellType.Error:
                    default:
                        return result;
                }
            }

            return result;
        }
    }
}
