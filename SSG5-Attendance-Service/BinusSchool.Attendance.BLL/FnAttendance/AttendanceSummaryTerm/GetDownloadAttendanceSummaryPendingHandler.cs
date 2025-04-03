using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using BinusSchool.Common.Model.Enums;
using System.Linq;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Attendance.BLL.FnAttendance.Extensions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetDownloadAttendanceSummaryPendingHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;
        public GetDownloadAttendanceSummaryPendingHandler(IAttendanceDbContext AttendanceDbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
        {
            _dbContext = AttendanceDbContext;
            _attendanceSummaryTerm = AttendanceSummaryTerm;
            _redisCache = redisCache;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetDownloadAttendanceSummaryPendingRequest>();

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var AbsentTerms = redisMappingAttendance.Select(e => e.AbsentTerms).FirstOrDefault();


            var paramAttendancePending = new GetAttendanceSummaryPendingRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel,
                GetAll = true,
                Return = CollectionType.Lov,
                IdUser = param.IdUser,
                SelectedPosition = param.SelectedPosition,
            };

            var apiAttendanceSummaryPending = await _attendanceSummaryTerm.GetAttendanceSummaryPending(paramAttendancePending);
            var getAttendanceSummaryPending = apiAttendanceSummaryPending.IsSuccess? apiAttendanceSummaryPending.Payload:null;
            var dateRange = param.StartDate.ToString("dd MMM yyyy") + " - " + param.EndDate.ToString("dd MMM yyyy");
            var excelRecap = GenerateExcel(getAttendanceSummaryPending, AbsentTerms, dateRange);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Attendance_Summary_by_Student{DateTime.Now.Ticks}.xlsx"
            };
            return null;
        }
        private byte[] GenerateExcel(IEnumerable<GetAttendanceSummaryPendingResult> data, AbsentTerm absentTerms, string dateRange)
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailRequest>();

            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.SetFont(fontBold);

            var fontBoldHeader = workbook.CreateFont();
            fontBoldHeader.IsBold = true;
            fontBoldHeader.FontHeightInPoints = 20;

            var boldStyleHeader = workbook.CreateCellStyle();
            boldStyleHeader.SetFont(fontBoldHeader);

            #region header
            List<string> header = new List<string>();
            header.Add("No");

            if (absentTerms == AbsentTerm.Session)
            {
                header.Add("Homeroom");
                header.Add("Date");
            }
            else{
                header.Add("Date");
                header.Add("Homeroom");
            }
            

            if(absentTerms== AbsentTerm.Session)
                header.Add("Class ID");

            header.Add("Teacher");
            header.Add("Total Student");

            if (absentTerms == AbsentTerm.Session)
                header.Add("Session");
            #endregion

            var sheet = workbook.CreateSheet();
            int rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            var cellNo = rowHeader.CreateCell(0);
            cellNo.SetCellValue("Pending Attendance");
            cellNo.CellStyle = boldStyleHeader;
            rowIndex += 2;

            var i = 0;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(i);
            cellNo.SetCellValue("Date Range");
            i++;

            cellNo = rowHeader.CreateCell(i);
            cellNo.SetCellValue(dateRange);
            rowIndex += 2;

            i = 0;
            rowHeader = sheet.CreateRow(rowIndex);
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(i);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = boldStyle;
                i++;
            }
            rowIndex++;

            foreach (var itemData in data)
            {
                int startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(data.IndexOf(itemData)+1);
                startColumn++;

                if (absentTerms == AbsentTerm.Session)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Homeroom.Description);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Date.ToString("dd-MM-yyyy"));
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.ClassID);
                    startColumn++;
                }
                else
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Date.ToString("dd-MM-yyyy"));
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Homeroom.Description);
                    startColumn++;
                }

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Teacher.Description);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.TotalStudent);
                startColumn++;

                if (absentTerms == AbsentTerm.Session)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Session.SessionID);
                    startColumn++;
                }
                
                rowIndex++;
            }


            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }
}
