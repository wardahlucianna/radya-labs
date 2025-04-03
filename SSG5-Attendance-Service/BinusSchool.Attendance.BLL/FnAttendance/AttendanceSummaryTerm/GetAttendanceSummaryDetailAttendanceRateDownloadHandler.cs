using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailAttendanceRateDownloadHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailAttendanceRateDownloadHandler(IAttendanceDbContext AttendanceDbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
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

            var param = Request.ValidateParams<GetAttendanceSummaryDetailAttendanceRateRequest>();
            param.Return = CollectionType.Lov;
            param.GetAll = true;

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var listAttendaceSummaryAttendanceRate = _attendanceSummaryTerm.GetAttendanceSummaryDetailAttendanceRate(param).Result.Payload.ToList();

            var AbsentTerms = redisMappingAttendance.Select(e => e.AbsentTerms).FirstOrDefault();

            var excelRecap = GenerateExcel(listAttendaceSummaryAttendanceRate, AbsentTerms);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Attendance_Summary_Attendance_Rate_by_Student{DateTime.Now.Ticks}.xlsx"
            };
            return null;
        }
        private byte[] GenerateExcel(List<GetAttendanceSummaryDetailAttendanceRateResult> data, AbsentTerm AbsentTerms)
        {
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

            var sheet = workbook.CreateSheet();
            var rowHeader = sheet.CreateRow(0);

            #region header
            List<string> header = new List<string>();

            if (AbsentTerms == AbsentTerm.Day)
            {
                header.Add("Total Days to Date");
            }
            else
            {
                header.Add("Subject");
                header.Add("No. of Class Session to Date");
            }

            header.Add("Present");
            header.Add("Excused Absence");
            header.Add("Unxcused Absence");
            header.Add("Late");
            header.Add("Presence in Class");
            header.Add("Punctuality");
            #endregion

            var rowIndex = 0;
            int startColumn = 0;
            var cellNo = rowHeader.CreateCell(rowIndex);
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = boldStyle;
                startColumn++;
            }

            rowIndex++;
            foreach (var itemData in data)
            {
                startColumn = 0;

                rowHeader = sheet.CreateRow(rowIndex);

                if (AbsentTerms == AbsentTerm.Day)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.TotalDaysToDate);
                    startColumn++;
                }
                else
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Subject);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.ClassSessionToDate);
                    startColumn++;
                }

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Present);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.ExcusedAbsence);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.UnexcusedAbsence);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Late);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.PresenceInClass);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Punctuality);
                startColumn++;
                rowIndex++;
            }

            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }
}
