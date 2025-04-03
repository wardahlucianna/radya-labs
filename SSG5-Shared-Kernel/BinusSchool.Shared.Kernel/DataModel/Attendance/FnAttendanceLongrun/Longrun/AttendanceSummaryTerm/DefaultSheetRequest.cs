using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class DefaultSheetRequest
    {
        public XSSFWorkbook Workbook { get; set; }
        public ICellStyle BoldStyleHeader { get; set; }
        public ICellStyle NormalStyleHeader { get; set; }
        public ICellStyle BoldStyle { get; set; }
        public ICellStyle NormalStyleBody { get; set; }
        public ICellStyle BoldStyleBody { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
