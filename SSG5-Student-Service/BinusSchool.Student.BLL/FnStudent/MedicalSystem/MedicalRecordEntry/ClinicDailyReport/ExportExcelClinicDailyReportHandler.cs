using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class ExportExcelClinicDailyReportHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMedicalSystem _medical;
        private readonly IMachineDateTime _time;

        public ExportExcelClinicDailyReportHandler(IStudentDbContext context, IMedicalSystem medical, IMachineDateTime time)
        {
            _context = context;
            _medical = medical;
            _time = time;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = Request.ValidateParams<ClinicDailyReportDataRequest>
                (nameof(ClinicDailyReportDataRequest.IdSchool),
                 nameof(ClinicDailyReportDataRequest.Date));

            #region Clinic Visit
            var clinicVisitData = new GetClinicDailyReportVisitDataResponse();

            var clinicVisit = await _medical.GetClinicDailyReportVisitData(new ClinicDailyReportDataRequest
            {
                IdSchool = request.IdSchool,
                Date = request.Date,
            });

            clinicVisitData = clinicVisit.Payload;
            #endregion

            #region Leave Early School
            var leaveEarlySchoolData = new List<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse>();

            var leaveEarlySchool = await _medical.GetClinicDailyReportOfLeaveSchoolEarlyStudent(new ClinicDailyReportDataRequest
            {
                IdSchool = request.IdSchool,
                Date = request.Date,
            });

            leaveEarlySchoolData.AddRange(leaveEarlySchool.Payload);
            #endregion

            #region Injury Visit
            var injuryVisitData = new GetClinicDailyReportInjuryVisitResponse();

            var injuryVisit = await _medical.GetClinicDailyReportInjuryVisit(new ClinicDailyReportDataRequest
            {
                IdSchool = request.IdSchool,
                Date = request.Date,
            });

            injuryVisitData = injuryVisit.Payload;
            #endregion

            #region Exclude Injury Visit
            var excludeInjuryVisitData = new GetClinicDailyReportExcludeInjuryVisitReponse();

            var excludeInjuryVisit = await _medical.GetClinicDailyReportExcludeInjuryVisit(new ClinicDailyReportDataRequest
            {
                IdSchool = request.IdSchool,
                Date = request.Date,
            });

            excludeInjuryVisitData = excludeInjuryVisit.Payload;
            #endregion

            var getSchool = _context.Entity<MsSchool>()
                .Where(a => a.Id == request.IdSchool)
                .Select(a => a.Description)
                .FirstOrDefault();

            DateTime time = _time.ServerTime;

            var title = "Clinic Daily Report";

            if (clinicVisitData == null && leaveEarlySchoolData == null && injuryVisitData == null && excludeInjuryVisitData == null)
            {
                var generateExcel = GenerateBlankExcel(title);
                return new FileContentResult(generateExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"ClinicDailyReport_{time.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcel = GenerateExcel(title, getSchool, time.ToString("dd MMMM yyyy"), clinicVisitData, leaveEarlySchoolData, injuryVisitData, excludeInjuryVisitData);
                return new FileContentResult(generateExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"ClinicDailyReport_{time.Ticks}.xlsx"
                };
            }
        }

        public byte[] GenerateBlankExcel(string title)
        {
            var result = new byte[0];

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var validateTitle = regex.Replace(title, " ");

            using(var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet(validateTitle);

                #region Cell Style
                ICellStyle style = workbook.CreateCellStyle();

                // set border
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;

                // set font
                IFont font = workbook.CreateFont();

                font.FontName = "Aptos Narrow";
                font.FontHeightInPoints = 11;
                style.SetFont(font);
                #endregion

                #region Header
                IRow row = sheet.CreateRow(2);
                #endregion

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcel(string title, string schoolName, string generateDate, GetClinicDailyReportVisitDataResponse clinicVisit, List<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse> leaveEarlySchool, GetClinicDailyReportInjuryVisitResponse injuryVisit, GetClinicDailyReportExcludeInjuryVisitReponse excludeInjuryVisit)
        {
            var result = new byte[0];

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var validateTitle = regex.Replace(title, " ");

            using(var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet(validateTitle);

                #region Cell Style
                ICellStyle headerStyle = workbook.CreateCellStyle();
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle titleStyle = workbook.CreateCellStyle();

                // set border
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;

                // set font
                IFont headerFont = workbook.CreateFont();
                headerFont.FontName = "Aptos Narrow";
                headerFont.FontHeightInPoints = 11;
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                IFont font = workbook.CreateFont();
                font.FontName = "Aptos Narrow";
                font.FontHeightInPoints = 11;
                style.SetFont(font);

                IFont titleFont = workbook.CreateFont();
                titleFont.FontName = "Aptos Narrow";
                titleFont.FontHeightInPoints = 11;
                titleFont.IsBold = true;
                titleStyle.SetFont(titleFont);

                // set allignment
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.Alignment = HorizontalAlignment.Center;

                style.VerticalAlignment = VerticalAlignment.Center;
                style.Alignment = HorizontalAlignment.Center;

                titleStyle.VerticalAlignment = VerticalAlignment.Center;
                titleStyle.Alignment = HorizontalAlignment.Justify;
                #endregion

                int rowCell = 0;

                #region Title
                IRow titleRow = sheet.CreateRow(rowCell);
                var titleRowCell = titleRow.CreateCell(0);
                titleRowCell.SetCellValue($"Clinic Daily Report - {schoolName}");
                titleRowCell.CellStyle = headerStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;

                IRow dateRow = sheet.CreateRow(rowCell);
                var dateRowCell = dateRow.CreateCell(0);
                dateRowCell.SetCellValue($"Generate Date Report: {generateDate}");
                dateRowCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;
                #endregion

                #region Visit Data
                IRow visit = sheet.CreateRow(rowCell);
                ICell visitCell = visit.CreateCell(0);
                visitCell.SetCellValue($"Total clinic visit: {clinicVisit.TotalClinicVisit.TotalVisit} visits (Teacher/Staff: {clinicVisit.TotalClinicVisit.Staff}, Students: {clinicVisit.TotalClinicVisit.Student}, Other: {clinicVisit.TotalClinicVisit.OtherPatient})");
                visitCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell++;

                IRow visitor = sheet.CreateRow(rowCell);
                ICell visitorCell = visitor.CreateCell(0);
                visitorCell.SetCellValue($"Total clinic visitor: {clinicVisit.TotalClinicVisitor.TotalVisitor} visitors (Teacher/Staff: {clinicVisit.TotalClinicVisitor.Staff}, Students: {clinicVisit.TotalClinicVisitor.Student}, Other: {clinicVisit.TotalClinicVisitor.OtherPatient})");
                visitorCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;
                #endregion

                #region Leave School Early
                IRow leaveSchoolEarlyTitle = sheet.CreateRow(rowCell);
                ICell leaveSchoolEarlyTitleCell = leaveSchoolEarlyTitle.CreateCell(0);
                leaveSchoolEarlyTitleCell.SetCellValue($"Student Leave School Early due to Sickness");
                leaveSchoolEarlyTitleCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;

                var leaveSchoolEarlyHeaderList = new string[]
                {
                    "No", "Time", "Name", "Grade", "Location", "Teacher in Charge", "Notes"
                };

                int leaveSchoolEarlyIndexHeader = 0;

                IRow leaveSchoolEarlyHeader = sheet.CreateRow(rowCell);

                foreach (var lseHeader in leaveSchoolEarlyHeaderList)
                {
                    ICell leaveSchoolEarlyHeaderCell = leaveSchoolEarlyHeader.CreateCell(leaveSchoolEarlyIndexHeader);
                    leaveSchoolEarlyHeaderCell.SetCellValue(lseHeader);
                    leaveSchoolEarlyHeaderCell.CellStyle = style;
                    leaveSchoolEarlyIndexHeader++;
                }

                rowCell++;

                int lseBodyIndex = 0;

                if (leaveEarlySchool.Count == 0)
                {
                    IRow lseBodyRow = sheet.CreateRow(rowCell + lseBodyIndex);

                    ICell number = lseBodyRow.CreateCell(0);
                    number.SetCellValue("-");
                    number.CellStyle = style;

                    ICell time = lseBodyRow.CreateCell(1);
                    time.SetCellValue("-");
                    time.CellStyle = style;

                    ICell name = lseBodyRow.CreateCell(2);
                    name.SetCellValue("-");
                    name.CellStyle = style;

                    ICell grade = lseBodyRow.CreateCell(3);
                    grade.SetCellValue("-");
                    grade.CellStyle = style;

                    ICell location = lseBodyRow.CreateCell(4);
                    location.SetCellValue("-");
                    location.CellStyle = style;

                    ICell teacher = lseBodyRow.CreateCell(5);
                    teacher.SetCellValue("-");
                    teacher.CellStyle = style;

                    ICell notes = lseBodyRow.CreateCell(6);
                    notes.SetCellValue("-");
                    notes.CellStyle = style;

                    rowCell = rowCell + 2;
                }
                else
                {
                    int no = 1;

                    foreach (var lse in leaveEarlySchool)
                    {
                        IRow lseBodyRow = sheet.CreateRow(rowCell + lseBodyIndex);

                        ICell number = lseBodyRow.CreateCell(0);
                        number.SetCellValue(no.ToString());
                        number.CellStyle = style;

                        ICell time = lseBodyRow.CreateCell(1);
                        time.SetCellValue($"{lse.Time.CheckIn} - {lse.Time.CheckOut}");
                        time.CellStyle = style;

                        ICell name = lseBodyRow.CreateCell(2);
                        name.SetCellValue(lse.Name);
                        name.CellStyle = style;

                        ICell grade = lseBodyRow.CreateCell(3);
                        grade.SetCellValue(lse.Grade.Description);
                        grade.CellStyle = style;

                        ICell location = lseBodyRow.CreateCell(4);
                        location.SetCellValue(lse.Location ?? "-");
                        location.CellStyle = style;

                        ICell teacher = lseBodyRow.CreateCell(5);
                        teacher.SetCellValue(lse.Teacher ?? "-");
                        teacher.CellStyle = style;

                        ICell notes = lseBodyRow.CreateCell(6);
                        notes.SetCellValue(lse.Notes ?? "-");
                        notes.CellStyle = style;

                        lseBodyIndex++;
                        no++;
                    }

                    rowCell = (rowCell + lseBodyIndex) + 1;
                }
                #endregion

                #region Injury Visit
                IRow injuryVisitTitle = sheet.CreateRow(rowCell);
                ICell injuryVisitTitleCell = injuryVisitTitle.CreateCell(0);
                injuryVisitTitleCell.SetCellValue($"Total clinic visitations (injury): {injuryVisit.Summary.TotalOccurrence} occured (from {injuryVisit.Summary.UniqueIndividual} people)");
                injuryVisitTitleCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;

                var injuryVisitHeaderList = new string[]
                {
                    "No", "Time", "Name", "Grade", "Location", "Teacher in Charge", "Notes"
                };

                int injuryVisitIndexHeader = 0;

                IRow injuryVisitHeader = sheet.CreateRow(rowCell);

                foreach (var ivHeader in injuryVisitHeaderList)
                {
                    ICell injuryVisitCellHeader = injuryVisitHeader.CreateCell(injuryVisitIndexHeader);
                    injuryVisitCellHeader.SetCellValue(ivHeader);
                    injuryVisitCellHeader.CellStyle = style;
                    injuryVisitIndexHeader++;
                }

                rowCell++;

                int ivIndexBody = 0;

                if (injuryVisit.Incidents.Count == 0)
                {
                    IRow ivBodyRow = sheet.CreateRow(rowCell + ivIndexBody);

                    ICell number = ivBodyRow.CreateCell(0);
                    number.SetCellValue("-");
                    number.CellStyle = style;

                    ICell time = ivBodyRow.CreateCell(1);
                    time.SetCellValue("-");
                    time.CellStyle = style;

                    ICell name = ivBodyRow.CreateCell(2);
                    name.SetCellValue("-");
                    name.CellStyle = style;

                    ICell grade = ivBodyRow.CreateCell(3);
                    grade.SetCellValue("-");
                    grade.CellStyle = style;

                    ICell location = ivBodyRow.CreateCell(4);
                    location.SetCellValue("-");
                    location.CellStyle = style;

                    ICell teacher = ivBodyRow.CreateCell(5);
                    teacher.SetCellValue("-");
                    teacher.CellStyle = style;

                    ICell notes = ivBodyRow.CreateCell(6);
                    notes.SetCellValue("-");
                    notes.CellStyle = style;

                    rowCell = rowCell + 2;
                }
                else
                {
                    int no = 1;

                    foreach (var iv in injuryVisit.Incidents)
                    {
                        IRow ivBodyRow = sheet.CreateRow(rowCell + ivIndexBody);

                        ICell number = ivBodyRow.CreateCell(0);
                        number.SetCellValue(no.ToString());
                        number.CellStyle = style;

                        ICell time = ivBodyRow.CreateCell(1);
                        time.SetCellValue($"{iv.Time.CheckIn} - {iv.Time.CheckOut}");
                        time.CellStyle = style;

                        ICell name = ivBodyRow.CreateCell(2);
                        name.SetCellValue(iv.Name);
                        name.CellStyle = style;

                        ICell grade = ivBodyRow.CreateCell(3);
                        grade.SetCellValue(iv.Grade.Description);
                        grade.CellStyle = style;

                        ICell location = ivBodyRow.CreateCell(4);
                        location.SetCellValue(iv.Location ?? "-");
                        location.CellStyle = style;

                        ICell teacher = ivBodyRow.CreateCell(5);
                        teacher.SetCellValue(iv.Teacher ?? "-");
                        teacher.CellStyle = style;

                        ICell notes = ivBodyRow.CreateCell(6);
                        notes.SetCellValue(iv.Notes ?? "-");
                        notes.CellStyle = style;

                        ivIndexBody++;
                        no++;
                    }

                    rowCell = (rowCell + ivIndexBody) + 1;
                }
                #endregion

                #region Exclude Injury Visit
                IRow excludeInjuryVisitTitle = sheet.CreateRow(rowCell);
                ICell excludeInjuryVisitTitleCell = excludeInjuryVisitTitle.CreateCell(0);
                excludeInjuryVisitTitleCell.SetCellValue($"Total clinic visitations (excluding injury): {excludeInjuryVisit.Summary.TotalOccurrence} occured (from {excludeInjuryVisit.Summary.UniqueIndividual} people)");
                excludeInjuryVisitTitleCell.CellStyle = titleStyle;
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowCell, rowCell, 0, 6));
                rowCell = rowCell + 2;

                var excludeInjuryVisitHeaderList = new string[]
                {
                    "No", "Time", "Name", "Grade", "Location", "Teacher in Charge", "Notes"
                };

                int excludeInjuryVisitIndexHeader = 0;

                IRow excludeInjuryVisitHeader = sheet.CreateRow(rowCell);

                foreach (var eivHeader in excludeInjuryVisitHeaderList)
                {
                    ICell excludeInjuryVisitCellHeader = excludeInjuryVisitHeader.CreateCell(excludeInjuryVisitIndexHeader);
                    excludeInjuryVisitCellHeader.SetCellValue(eivHeader);
                    excludeInjuryVisitCellHeader.CellStyle = style;
                    excludeInjuryVisitIndexHeader++;
                }

                rowCell++;

                int eivIndexBody = 0;

                if (excludeInjuryVisit.Incidents.Count == 0)
                {
                    IRow eivBodyRow = sheet.CreateRow(rowCell + eivIndexBody);

                    ICell number = eivBodyRow.CreateCell(0);
                    number.SetCellValue("-");
                    number.CellStyle = style;

                    ICell time = eivBodyRow.CreateCell(1);
                    time.SetCellValue("-");
                    time.CellStyle = style;

                    ICell name = eivBodyRow.CreateCell(2);
                    name.SetCellValue("-");
                    name.CellStyle = style;

                    ICell grade = eivBodyRow.CreateCell(3);
                    grade.SetCellValue("-");
                    grade.CellStyle = style;

                    ICell location = eivBodyRow.CreateCell(4);
                    location.SetCellValue("-");
                    location.CellStyle = style;

                    ICell teacher = eivBodyRow.CreateCell(5);
                    teacher.SetCellValue("-");
                    teacher.CellStyle = style;

                    ICell notes = eivBodyRow.CreateCell(6);
                    notes.SetCellValue("-");
                    notes.CellStyle = style;

                    rowCell = rowCell + 2;
                }
                else
                {
                    int no = 1;

                    foreach (var eiv in excludeInjuryVisit.Incidents)
                    {
                        IRow eivBodyRow = sheet.CreateRow(rowCell + eivIndexBody);

                        ICell number = eivBodyRow.CreateCell(0);
                        number.SetCellValue(no.ToString());
                        number.CellStyle = style;

                        ICell time = eivBodyRow.CreateCell(1);
                        time.SetCellValue($"{eiv.Time.CheckIn} - {eiv.Time.CheckOut}");
                        time.CellStyle = style;

                        ICell name = eivBodyRow.CreateCell(2);
                        name.SetCellValue(eiv.Name);
                        name.CellStyle = style;

                        ICell grade = eivBodyRow.CreateCell(3);
                        grade.SetCellValue(eiv.Grade.Description);
                        grade.CellStyle = style;

                        ICell location = eivBodyRow.CreateCell(4);
                        location.SetCellValue(eiv.Location ?? "-");
                        location.CellStyle = style;

                        ICell teacher = eivBodyRow.CreateCell(5);
                        teacher.SetCellValue(eiv.Teacher ?? "-");
                        teacher.CellStyle = style;

                        ICell notes = eivBodyRow.CreateCell(6);
                        notes.SetCellValue(eiv.Notes ?? "-");
                        notes.CellStyle = style;

                        eivIndexBody++;
                        no++;
                    }

                    rowCell = (rowCell + eivIndexBody) + 1;
                }
                #endregion

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
    }
}
