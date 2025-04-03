using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Threading;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval.ApprovalReportScore
{
    public class ExportExcelScoreApprovalReportRequest
    {
        public GetApprovalReportScoreResult GetApprovalReportScore { get; set; }
    }

    public class ExportExcelScoreApprovalReportRequest_ExcelSheet
    {
        public XSSFWorkbook Workbook { get; set; }
        public ICellStyle BoldStyleHeader { get; set; }
        public ICellStyle BoldStyleBody { get; set; }
        public ICellStyle BoldStyle { get; set; }
        public ICellStyle NormalStyleHeader { get; set; }
        public ICellStyle NormalStyleBodyCenter { get; set; }
        public ICellStyle NormalStyleBodyLeft { get; set; }
        public ICellStyle GrayBackground { get; set; }
        public DateTime Date { get; set; }
    }
}
