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
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetDownloadAttendanceSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;
        public GetDownloadAttendanceSummaryHandler(IAttendanceDbContext AttendanceDbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
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

            var param = Request.ValidateParams<GetAttendanceSummaryDetailRequest>();
            param.GetAll = true;

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendance = await AttendanceSummaryRedisCacheHandler.GetAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                            .Where(e => e.Id == param.IdAcademicYear)
                            .Select(e => e.IdSchool)
                            .FirstOrDefaultAsync(CancellationToken);

            var AbsentTerms = redisMappingAttendance.Select(e => e.AbsentTerms).FirstOrDefault();

            var isGroupEa = !redisAttendance
                            .Where(e => e.ExcusedAbsenceCategory != null)
                            .Any();

            var resultAttendance = _attendanceSummaryTerm.GetAttendanceSummaryDetail(param).Result.Payload;
            resultAttendance = resultAttendance.OrderBy(x => x.Student.Name).ToList();
            var excelRecap = GenerateExcel(resultAttendance, idSchool, AbsentTerms, isGroupEa);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Attendance_Summary_by_Student{DateTime.Now.Ticks}.xlsx"
            };
            return null;
        }
        private byte[] GenerateExcel(IEnumerable<GetAttendanceSummaryDetailResult> data, string idSchool, AbsentTerm AbsentTerms, bool isGroupEa)
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

            var sheet = workbook.CreateSheet();
            var rowHeader = sheet.CreateRow(0);

            #region header
            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID - Name");
            header.Add("Class");
            header.Add("Attendance Rate (%)");

            if (AbsentTerms == AbsentTerm.Session)
                header.Add("Total Class Session To Date");
            else
                header.Add("Total Day To Date");

            header.Add("Total UA");

            if (isGroupEa)
                header.Add("Total EA");
            else
            {
                header.Add("Assign By School");
                header.Add("Personal");
            }

            header.Add("Total Presence in Class to Date");
            header.Add("Total Absence to Date (UA + EA)");

            var workhabit = data.Select(e => e.Workhabits).FirstOrDefault();
            foreach (var itemWorkhabit in workhabit)
            {
                header.Add(itemWorkhabit.Code);
            }
            #endregion



            var i = 0;
            var cellNo = rowHeader.CreateCell(i);
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(i);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = boldStyle;
                i++;
            }

            int rowIndex = 1;
            foreach (var itemData in data)
            {
                int startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(rowIndex);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"{itemData.Student.Id} - {itemData.Student.Name}");
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.Homeroom.Name);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"{itemData.AttendanceRate}%");
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.ClassSession);
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.UnexcusedAbsent);
                startColumn++;

                if (isGroupEa)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemData.ExcusedAbsence.Select(e => e.Count).Sum());
                    startColumn++;
                }
                else
                {
                    foreach(var itemExcusedAbsence in itemData.ExcusedAbsence.OrderBy(x => x.Category).ToList())
                    {
                        cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.SetCellValue(itemExcusedAbsence.Count);
                        startColumn++;
                    }
                }
                
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"{itemData.PresenceRate}%");
                startColumn++;

                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemData.UnexcusedAbsent + itemData.ExcusedAbsence.Select(e => e.Count).Sum());
                startColumn++;

                foreach (var itemWorkhabit in itemData.Workhabits)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemWorkhabit.Count);
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
