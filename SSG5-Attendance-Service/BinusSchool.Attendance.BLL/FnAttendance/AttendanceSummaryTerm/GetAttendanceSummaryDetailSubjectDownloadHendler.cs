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
    public class GetAttendanceSummaryDetailSubjectDownloadHendler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailSubjectDownloadHendler(IAttendanceDbContext AttendanceDbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
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

            var param = Request.ValidateParams<GetAttendanceSummaryDetailSubjectRequest>();
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

            var listAttendaceSummarySubject = _attendanceSummaryTerm.GetAttendanceSummaryDetailSubject(param).Result.Payload.ToList();

            var IsNeedValidation = redisMappingAttendance
                .Select(e => e.IsNeedValidation)
                .FirstOrDefault();

            var excelRecap = GenerateExcel(listAttendaceSummarySubject, IsNeedValidation);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Attendance_Summary_by_Subject{DateTime.Now.Ticks}.xlsx"
            };
            return null;
        }
        private byte[] GenerateExcel(List<GetAttendanceSummaryDetailSubjectResult> data, bool IsNeedValidation)
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
            header.Add("Homeroom");
            header.Add("Class Id - Subject");
            header.Add("Teacher's Name");
            header.Add("Unsubmitted");

            if (IsNeedValidation)
                header.Add("Pending");
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

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Homeroom);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.ClassIdSubject);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.TeacherName);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Unsubmited);
                startColumn++;

                if (IsNeedValidation)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.Pending);
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
