using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class GetDownloadAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceRecap _attendanceRecap;
        private readonly IAttendanceRecapService _attendanceRecapService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IMachineDateTime _machineDateTime;

        public GetDownloadAttendanceRecapHandler(IAttendanceDbContext dbContext, IAttendanceRecap attendanceRecap, IMachineDateTime machineDateTime, IAttendanceSummaryService attendanceSummaryService, IAttendanceRecapService attendanceRecapService)
        {
            _dbContext = dbContext;
            _attendanceRecap = attendanceRecap;
            _attendanceSummaryService = attendanceSummaryService;
            _attendanceRecapService = attendanceRecapService;
            _machineDateTime = machineDateTime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceRecapRequest>();
            param.Return = CollectionType.Lov;


            var dataAttendanceRecap = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Student)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.Grade)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.AcademicYear)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.GradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                        .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                        x.IdStudent == param.IdStudent &&
                        x.Homeroom.IdGrade == param.IdGrade &&
                        x.Homeroom.Grade.IdLevel == param.IdLevel &&
                        x.Homeroom.GradePathwayClassroom.IdClassroom == param.IdHomeroom)
                        .Select(x => new AttendanceRecapData
                        {
                            AcademicYear = x.Homeroom.AcademicYear.Description,
                            StudentName = $"{NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)}",
                            IdStudent = x.IdStudent,
                            IdClassroom = x.Homeroom.GradePathwayClassroom.IdClassroom,
                            Homeroom = $"{x.Homeroom.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}"
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            var getUnsubmitted = await GetUnsubmitted(param);
            var getPending = await GetAttendanceRecaps(param, "Pending");
            var getPresent = await GetAttendanceRecaps(param, "PR");
            var getLate = await GetAttendanceRecaps(param, "LT");
            var getExcused = await GetAttendanceRecaps(param, "Excused");
            var getUnexcused = await GetAttendanceRecaps(param, "Unexcused");

            dataAttendanceRecap.Unsubmitted = getUnsubmitted;
            dataAttendanceRecap.Pending = getPending;
            dataAttendanceRecap.Present = getPresent;
            dataAttendanceRecap.Late = getLate;
            dataAttendanceRecap.Excused = getExcused;
            dataAttendanceRecap.Unexcused = getUnexcused;


            var excelSubstitution = GenerateExcel(dataAttendanceRecap);

            return new FileContentResult(excelSubstitution, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Attendance_Recap_{DateTime.Now.Ticks}.xlsx"
            };
        }

        #region download excel
        private byte[] GenerateExcel(AttendanceRecapData data)
        {
            #region preparation
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var cellStyleAndBold = workbook.CreateCellStyle();
            cellStyleAndBold.BorderRight = BorderStyle.Thin;
            cellStyleAndBold.BorderLeft = BorderStyle.Thin;
            cellStyleAndBold.BorderBottom = BorderStyle.Thin;
            cellStyleAndBold.BorderTop = BorderStyle.Thin;
            cellStyleAndBold.SetFont(fontBold);
            cellStyleAndBold.Alignment = HorizontalAlignment.Center;

            int rowIndex = 9;
            int startColumn = 0;
            #endregion

            #region unsubmitted
            var sheet = workbook.CreateSheet("Unsubmitted");

            var rowDate = sheet.CreateRow(0);
            var cellDate = rowDate.CreateCell(0);
            var cellDateValue = rowDate.CreateCell(1);
            cellDate.SetCellValue("Get Data Time");
            cellDate.CellStyle = boldStyle;
            cellDateValue.SetCellValue(_machineDateTime.ServerTime.ToString("dd/MM/yyyy HH:mm:ss"));

            var rowAY = sheet.CreateRow(1);
            var cellAY = rowAY.CreateCell(0);
            var cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            var rowStudentName = sheet.CreateRow(2);
            var cellStudentName = rowStudentName.CreateCell(0);
            var cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            var rowStudentId = sheet.CreateRow(3);
            var cellStudentId = rowStudentId.CreateCell(0);
            var cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            var rowHomeroom = sheet.CreateRow(4);
            var cellHomeroom = rowHomeroom.CreateCell(0);
            var cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            var rowTabing = sheet.CreateRow(6);
            var cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Unsubmitted");
            cellTabing.CellStyle = boldStyle;

            var rowHeader = sheet.CreateRow(8);
            var cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            var cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            var cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Unsubmitted.Count() > 0)
            {
                foreach (var itemData in data.Unsubmitted)
                {
                    rowHeader = sheet.CreateRow(rowIndex);
                    sheet.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheet.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheet.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            rowIndex = 8;
            startColumn = 0;
            sheet.AutoSizeColumn(25);
            #endregion

            #region pending
            var sheetPending = workbook.CreateSheet("Pending");

            rowAY = sheetPending.CreateRow(0);
            cellAY = rowAY.CreateCell(0);
            cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            rowStudentName = sheetPending.CreateRow(1);
            cellStudentName = rowStudentName.CreateCell(0);
            cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            rowStudentId = sheetPending.CreateRow(2);
            cellStudentId = rowStudentId.CreateCell(0);
            cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            rowHomeroom = sheetPending.CreateRow(3);
            cellHomeroom = rowHomeroom.CreateCell(0);
            cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            rowTabing = sheetPending.CreateRow(5);
            cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Pending");
            cellTabing.CellStyle = boldStyle;

            rowHeader = sheetPending.CreateRow(7);
            cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Pending.Count() > 0)
            {
                foreach (var itemData in data.Pending)
                {
                    rowHeader = sheetPending.CreateRow(rowIndex);
                    sheetPending.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheetPending.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheetPending.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            rowIndex = 8;
            startColumn = 0;
            sheetPending.AutoSizeColumn(25);
            #endregion

            #region present
            var sheetPresent = workbook.CreateSheet("Present");

            rowAY = sheetPresent.CreateRow(0);
            cellAY = rowAY.CreateCell(0);
            cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            rowStudentName = sheetPresent.CreateRow(1);
            cellStudentName = rowStudentName.CreateCell(0);
            cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            rowStudentId = sheetPresent.CreateRow(2);
            cellStudentId = rowStudentId.CreateCell(0);
            cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            rowHomeroom = sheetPresent.CreateRow(3);
            cellHomeroom = rowHomeroom.CreateCell(0);
            cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            rowTabing = sheetPresent.CreateRow(5);
            cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Present");
            cellTabing.CellStyle = boldStyle;

            rowHeader = sheetPresent.CreateRow(7);
            cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Present.Count() > 0)
            {
                foreach (var itemData in data.Present)
                {
                    rowHeader = sheetPresent.CreateRow(rowIndex);
                    sheetPresent.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheetPresent.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheetPresent.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            rowIndex = 8;
            startColumn = 0;
            sheetPresent.AutoSizeColumn(25);
            #endregion

            #region late
            var sheetLate = workbook.CreateSheet("Late");

            rowAY = sheetLate.CreateRow(0);
            cellAY = rowAY.CreateCell(0);
            cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            rowStudentName = sheetLate.CreateRow(1);
            cellStudentName = rowStudentName.CreateCell(0);
            cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            rowStudentId = sheetLate.CreateRow(2);
            cellStudentId = rowStudentId.CreateCell(0);
            cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            rowHomeroom = sheetLate.CreateRow(3);
            cellHomeroom = rowHomeroom.CreateCell(0);
            cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            rowTabing = sheetLate.CreateRow(5);
            cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Late");
            cellTabing.CellStyle = boldStyle;

            rowHeader = sheetLate.CreateRow(7);
            cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Late.Count() > 0)
            {
                foreach (var itemData in data.Late)
                {
                    rowHeader = sheetLate.CreateRow(rowIndex);
                    sheetLate.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheetLate.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheetLate.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            rowIndex = 8;
            startColumn = 0;
            sheetLate.AutoSizeColumn(25);
            #endregion

            #region excused
            var sheetExcused = workbook.CreateSheet("Excused Absence");

            rowAY = sheetExcused.CreateRow(0);
            cellAY = rowAY.CreateCell(0);
            cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            rowStudentName = sheetExcused.CreateRow(1);
            cellStudentName = rowStudentName.CreateCell(0);
            cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            rowStudentId = sheetExcused.CreateRow(2);
            cellStudentId = rowStudentId.CreateCell(0);
            cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            rowHomeroom = sheetExcused.CreateRow(3);
            cellHomeroom = rowHomeroom.CreateCell(0);
            cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            rowTabing = sheetExcused.CreateRow(5);
            cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Excused Absence");
            cellTabing.CellStyle = boldStyle;

            rowHeader = sheetExcused.CreateRow(7);
            cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Excused.Count() > 0)
            {
                foreach (var itemData in data.Excused)
                {
                    rowHeader = sheetExcused.CreateRow(rowIndex);
                    sheetExcused.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheetExcused.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheetExcused.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            rowIndex = 8;
            startColumn = 0;
            sheetExcused.AutoSizeColumn(25);
            #endregion

            #region unexcused
            var sheetUnexcused = workbook.CreateSheet("Unexcused Absence");

            rowAY = sheetUnexcused.CreateRow(0);
            cellAY = rowAY.CreateCell(0);
            cellAYValue = rowAY.CreateCell(1);
            cellAY.SetCellValue("Academic Year");
            cellAY.CellStyle = boldStyle;
            cellAYValue.SetCellValue(data.AcademicYear);

            rowStudentName = sheetUnexcused.CreateRow(1);
            cellStudentName = rowStudentName.CreateCell(0);
            cellStudentNameValue = rowStudentName.CreateCell(1);
            cellStudentName.SetCellValue("Student Name");
            cellStudentName.CellStyle = boldStyle;
            cellStudentNameValue.SetCellValue(data.StudentName);

            rowStudentId = sheetUnexcused.CreateRow(2);
            cellStudentId = rowStudentId.CreateCell(0);
            cellStudentIdValue = rowStudentId.CreateCell(1);
            cellStudentId.SetCellValue("Student Id");
            cellStudentId.CellStyle = boldStyle;
            cellStudentIdValue.SetCellValue(data.IdStudent);

            rowHomeroom = sheetUnexcused.CreateRow(3);
            cellHomeroom = rowHomeroom.CreateCell(0);
            cellHomeroomValue = rowHomeroom.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue(data.Homeroom);

            rowTabing = sheetUnexcused.CreateRow(5);
            cellTabing = rowTabing.CreateCell(0);
            cellTabing.SetCellValue("Unexcused Absence");
            cellTabing.CellStyle = boldStyle;

            rowHeader = sheetUnexcused.CreateRow(7);
            cellDateData = rowHeader.CreateCell(0);
            cellDateData.SetCellValue("Date");
            cellDateData.CellStyle = cellStyleAndBold;

            cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = cellStyleAndBold;

            if (data.Unexcused.Count() > 0)
            {
                foreach (var itemData in data.Unexcused)
                {
                    rowHeader = sheetUnexcused.CreateRow(rowIndex);
                    sheetUnexcused.AutoSizeColumn(0);
                    cellDateData = rowHeader.CreateCell(0);
                    cellDateData.SetCellValue(itemData.Date.HasValue ? itemData.Date.Value.ToString("dd/MM/yyyy") : "");
                    cellDateData.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheet.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.Homeroom);
                    cellClassID.CellStyle = cellStyle;

                    cellSession = rowHeader.CreateCell(2);
                    sheet.AutoSizeColumn(2);
                    cellSession.SetCellValue(itemData.Session);
                    cellSession.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }
            rowIndex = 8;
            startColumn = 0;
            sheetUnexcused.AutoSizeColumn(25);
            #endregion

            using var ms = new MemoryStream();
            //ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
        #endregion

        #region unsubmitted
        public async Task<List<GetDataDetailAttendanceRecapResult>> GetUnsubmitted(GetDetailAttendanceRecapRequest param)
        {
            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear && e.IdLevel == param.IdLevel)
                .Select(e => new
                {
                    e.Id,
                    e.IdLevel,
                    e.AbsentTerms,
                    e.IsNeedValidation,
                    e.IsUseWorkhabit,
                    e.IsUseDueToLateness,
                })
                .FirstOrDefaultAsync(CancellationToken);
            if (mappingAttendance is null)
                throw new Exception("Data mapping is missing");

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>().Include(x => x.Homeroom)
                .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdStudent)
                .Select(x => x.Id)
                .ToListAsync(CancellationToken);

            var msHomeroomStudentEnrollment = await _attendanceRecapService.GetHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, param.IdStudent, CancellationToken);
            var trHomeroomStudentEnrollment = await _attendanceRecapService.GetTrHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, param.IdStudent, CancellationToken);
            var period = await GetPeriodAsync(param.IdAcademicYear, param.IdLevel, CancellationToken);

            var homeroomStudentEnrollment = msHomeroomStudentEnrollment.Union(trHomeroomStudentEnrollment);

            if (period == null)
            {
                throw new BadRequestException("Period not found !");
            }

            var startDate = period.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
            var endDate = period.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;

            var lessons = homeroomStudentEnrollment.Select(x => x.IdLesson).Distinct().ToList();

            var scheduleLessons = await _attendanceRecapService.GetScheduleLessonAsync(param.IdAcademicYear, param.IdLevel, lessons, startDate, endDate, CancellationToken);

            if (!scheduleLessons.Any())
            {
                return new List<GetDataDetailAttendanceRecapResult>();
            }

            var listMappingSemesterGradeIdClassroomAndIdHomeroom =
                new List<(
                    int Semester,
                    string IdGrade,
                    string IdClassroom,
                    string ClassroomCode,
                    string IdHomeroom,
                    string GradeCode)>();

            var homeroomQueryable = _dbContext.Entity<MsHomeroom>()
                .AsNoTracking()
                .Where(e => e.IdGrade == param.IdGrade && e.Grade.Level.Id == param.IdLevel)
                .AsQueryable();


            var homerooms = await homeroomQueryable
                .Select(e => new
                {
                    IdHomeroom = e.Id,
                    IdClassroom = e.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.GradePathwayClassroom.Classroom.Code,
                    ClassroomDesc = e.GradePathwayClassroom.Classroom.Description,
                    e.Semester,
                    e.IdGrade,
                    GradeCode = e.Grade.Code
                })
                .ToListAsync(CancellationToken);

            if (!homerooms.Any())
                throw new Exception("Invalid grade");

            foreach (var item in homerooms)
            {
                listMappingSemesterGradeIdClassroomAndIdHomeroom.Add((
                    item.Semester,
                    item.IdGrade,
                    item.IdClassroom,
                    item.ClassroomCode,
                    item.IdHomeroom,
                    item.GradeCode));
            }

            var dict = new Dictionary<(int Semester, string IdGrade, string IdHomeroom), List<StudentEnrollmentDto2>>();
            foreach (var item in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                dict.Add((item.Semester, item.IdGrade, item.IdHomeroom),
                    await _attendanceSummaryService.GetStudentEnrolledAsync(
                        item.IdHomeroom,
                        DateTime.MinValue, CancellationToken));
            var listIdStudent = dict.SelectMany(e => e.Value).Where(x => x.IdStudent == param.IdStudent).Select(e => e.IdStudent).Distinct().ToArray();
            var studentStatuses =
                await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear,
                    CancellationToken);

            var groupedAttendanceEntries = await GetAttendanceEntriesGroupedAsync(
                scheduleLessons
                    .Select(e => e.Id)
                    .ToList(), param.IdStudent, CancellationToken);

            var querySchedule = _dbContext.Entity<MsSchedule>()
                .Include(e => e.User)
                .Include(e => e.Lesson)
                .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                querySchedule = querySchedule.Where(e => e.Lesson.Grade.IdLevel == param.IdLevel);

            var listSchedule = await querySchedule
                .GroupBy(e => new RedisAttendanceSummaryScheduleResult
                {
                    IdLesson = e.IdLesson,
                    Teacher = new RedisAttendanceSummaryTeacher
                    {
                        IdUser = e.IdUser,
                        FirstName = e.User.FirstName,
                        LastName = e.User.LastName
                    },
                    IdWeek = e.IdWeek,
                    IdDay = e.IdDay,
                    IdSession = e.IdSession,
                })
                .Select(e => e.Key)
                .ToListAsync(CancellationToken);

            var lists = new List<GetAttendanceSummaryUnsubmitedResult>();

            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                var finalScheduleLessons = scheduleLessons
                    .GroupBy(e => new
                    {
                        e.ClassID,
                        e.Subject.Description
                    })
                    .ToDictionary(e => e.Key, y => y.ToList());

                foreach (var scheduleLesson in finalScheduleLessons)
                {
                    foreach (var item2 in scheduleLesson.Value)
                    {
                        var teacher = listSchedule
                            .FirstOrDefault(e => e.IdLesson == item2.IdLesson &&
                                                 e.IdSession == item2.Session.Id &&
                                                 e.IdWeek == item2.IdWeek &&
                                                 e.IdDay == item2.IdDay);
                        if (teacher is null)
                            continue;

                        foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                        {
                            if (!(homeroom.Semester == item2.Semester &&
                                  item2.IdGrade == homeroom.IdGrade &&
                                  item2.LessonPathwayResults.Any(e => e.IdHomeroom == homeroom.IdHomeroom)))
                                continue;

                            if (!string.IsNullOrWhiteSpace(param.IdHomeroom) && param.IdHomeroom != homeroom.IdClassroom)
                                continue;

                            var vm = new GetAttendanceSummaryUnsubmitedResult
                            {
                                Date = item2.ScheduleDate,
                                ClassID = item2.ClassID,
                                Teacher = new ItemValueVm
                                {
                                    Id = teacher?.Teacher.IdUser,
                                    Description = teacher is null
                                        ? string.Empty
                                        : NameUtil.GenerateFullName(teacher.Teacher.FirstName,
                                            teacher.Teacher.LastName) ?? string.Empty
                                },
                                Homeroom = new ItemValueVm
                                {
                                    Id = homeroom.IdHomeroom ?? string.Empty,
                                    Description = homeroom.GradeCode + homeroom.ClassroomCode ?? string.Empty
                                },
                                SubjectId = item2.Subject.SubjectID,
                                Session = new RedisAttendanceSummarySession
                                {
                                    Id = item2.Session.Id,
                                    Name = item2.Session.Name,
                                    SessionID = item2.Session.SessionID
                                }
                            };
                            vm.ListStudent = new List<string>();

                            if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                                continue;

                            var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                            // get all attendance entry by id schedule lesson
                            if (!groupedAttendanceEntries.ContainsKey(item2.Id))
                            {
                                //loop every student enrolled
                                foreach (var studentEnrolled in listStudentEnrolled)
                                {
                                    //logic student status
                                    var studentStatus = studentStatuses
                                        .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
                                    //student status is null or not active, skipped
                                    if (studentStatus is null || studentStatus.IsActive == false)
                                        continue;

                                    var passed = false;
                                    //logic moving
                                    foreach (var current in studentEnrolled.Items)
                                    {
                                        if (passed)
                                            break;

                                        if (string.IsNullOrWhiteSpace(current.IdLesson))
                                            continue;

                                        if (current.Ignored || current.IdLesson != item2.IdLesson)
                                            continue;

                                        //when current id lesson is same as above and the date of the current moving still satisfied
                                        //then set to passed, other than that will be excluded
                                        if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                            item2.ScheduleDate.Date < current.EndDt.Date)
                                            passed = true;
                                    }

                                    if (!passed)
                                        continue;

                                    vm.TotalStudent++;
                                    vm.ListStudent.Add(studentEnrolled.IdStudent);
                                }
                            }
                            else
                            {
                                var attendanceEntries = groupedAttendanceEntries[item2.Id];

                                //loop every student enrolled
                                foreach (var studentEnrolled in listStudentEnrolled)
                                {
                                    //logic student status
                                    var studentStatus = studentStatuses
                                        .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
                                    //student status is null or not active, skipped
                                    if (studentStatus is null || studentStatus.IsActive == false)
                                        continue;

                                    var passed = false;
                                    //logic moving
                                    foreach (var current in studentEnrolled.Items)
                                    {
                                        if (passed)
                                            break;

                                        if (string.IsNullOrWhiteSpace(current.IdLesson))
                                            continue;

                                        if (current.Ignored || current.IdLesson != item2.IdLesson)
                                            continue;

                                        //when current id lesson is same as above and the date of the current moving still satisfied
                                        //then set to passed, other than that will be excluded
                                        if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                            item2.ScheduleDate.Date < current.EndDt.Date)
                                            passed = true;
                                    }

                                    if (!passed)
                                        continue;
                                    var attendanceEntry = attendanceEntries.Where(e =>
                                            e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                        .OrderByDescending(e => e.DateIn)
                                        .FirstOrDefault();

                                    if (attendanceEntry is null)
                                    {
                                        vm.TotalStudent++;
                                        vm.ListStudent.Add(studentEnrolled.IdStudent);
                                    }
                                }
                            }

                            if (vm.TotalStudent > 0)
                                lists.Add(vm);
                        }
                    }
                }
            }
            else
            {
                var finalScheduleLessons = scheduleLessons
                    .GroupBy(e => new
                    {
                        e.ScheduleDate
                    })
                    .ToDictionary(e => e.Key, y => y.First());

                foreach (var scheduleLesson in finalScheduleLessons)
                {
                    var item2 = scheduleLesson.Value;

                    var teacher = listSchedule
                        .FirstOrDefault(e => e.IdLesson == item2.IdLesson &&
                                             e.IdSession == item2.Session.Id &&
                                             e.IdWeek == item2.IdWeek &&
                                             e.IdDay == item2.IdDay);
                    if (teacher is null)
                        continue;

                    foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                    {
                        if (!(homeroom.Semester == item2.Semester &&
                              item2.IdGrade == homeroom.IdGrade &&
                              item2.LessonPathwayResults.Any(e => e.IdHomeroom == homeroom.IdHomeroom)))
                            continue;

                        var vm = new GetAttendanceSummaryUnsubmitedResult
                        {
                            Date = item2.ScheduleDate,
                            ClassID = item2.ClassID,
                            Teacher = new ItemValueVm
                            {
                                Id = teacher?.Teacher.IdUser,
                                Description = teacher is null
                                    ? string.Empty
                                    : NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName) ??
                                      string.Empty
                            },
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.IdHomeroom ?? string.Empty,
                                Description = homeroom.GradeCode + homeroom.ClassroomCode ?? string.Empty
                            },
                            SubjectId = item2.Subject.SubjectID,
                            Session = new RedisAttendanceSummarySession
                            {
                                Id = item2.Session.Id,
                                Name = item2.Session.Name,
                                SessionID = item2.Session.SessionID
                            }
                        };
                        vm.ListStudent = new List<string>();

                        if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                            continue;

                        var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                        // get all attendance entry by id schedule lesson
                        if (!groupedAttendanceEntries.ContainsKey(item2.Id))
                        {
                            //loop every student enrolled
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                //logic student status
                                var studentStatus = studentStatuses
                                   .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
                                //student status is null or not active, skipped
                                if (studentStatus is null || studentStatus.IsActive == false)
                                    continue;

                                var passed = false;
                                //logic moving
                                foreach (var current in studentEnrolled.Items)
                                {
                                    if (passed)
                                        break;

                                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                                        continue;

                                    if (current.Ignored || current.IdLesson != item2.IdLesson)
                                        continue;

                                    //when current id lesson is same as above and the date of the current moving still satisfied
                                    //then set to passed, other than that will be excluded
                                    if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                        item2.ScheduleDate.Date < current.EndDt.Date)
                                        passed = true;
                                }

                                if (!passed)
                                    continue;

                                vm.TotalStudent++;
                                vm.ListStudent.Add(studentEnrolled.IdStudent);
                            }
                        }
                        else
                        {
                            var attendanceEntries = groupedAttendanceEntries[item2.Id];

                            //loop every student enrolled
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                //logic student status
                                var studentStatus = studentStatuses
                                    .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
                                //student status is null or not active, skipped
                                if (studentStatus is null || studentStatus.IsActive == false)
                                    continue;

                                var passed = false;
                                //logic moving
                                foreach (var current in studentEnrolled.Items)
                                {
                                    if (passed)
                                        break;

                                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                                        continue;

                                    if (current.Ignored || current.IdLesson != item2.IdLesson)
                                        continue;

                                    //when current id lesson is same as above and the date of the current moving still satisfied
                                    //then set to passed, other than that will be excluded
                                    if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                        item2.ScheduleDate.Date < current.EndDt.Date)
                                        passed = true;
                                }

                                if (!passed)
                                    continue;
                                var attendanceEntry = attendanceEntries.Where(e =>
                                        e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                    .OrderByDescending(e => e.DateIn)
                                    .FirstOrDefault();

                                if (attendanceEntry is null)
                                {
                                    vm.TotalStudent++;
                                    vm.ListStudent.Add(studentEnrolled.IdStudent);
                                }
                            }
                        }

                        if (vm.TotalStudent > 0)
                            lists.Add(vm);
                    }
                }
            }

            var result = lists.Select(x => new GetDataDetailAttendanceRecapResult
            {
                Date = x.Date,
                ClassId = x.ClassID,
                Homeroom = x.Homeroom.Description,
                Session = x.Session.Name
            }).ToList();

            return result;
        }
        #endregion

        #region pending, present, late, excused, unexcused
        public async Task<List<GetDataDetailAttendanceRecapResult>> GetAttendanceRecaps(GetDetailAttendanceRecapRequest param, string attendanceCode)
        {
            //ms mapping attendance
            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear && e.IdLevel == param.IdLevel)
                .Select(e => new
                {
                    e.Id,
                    e.IdLevel,
                    e.AbsentTerms,
                    e.IsNeedValidation,
                    e.IsUseWorkhabit,
                    e.IsUseDueToLateness,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (mappingAttendance is null)
                throw new Exception("Data mapping is missing");

            //Get Student Status
            var listIdStudent = new List<string> { param.IdStudent }.ToArray();
            var studentStatuses = await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear, CancellationToken);

            // Get Student Enrollment
            var getStudentEnrollment = await _attendanceSummaryService.GetStudentEnrolledByStudentAsync(param.IdAcademicYear,
                        param.IdStudent,
                        DateTime.MinValue, CancellationToken);

            var period = await GetPeriodAsync(param.IdAcademicYear, param.IdLevel, CancellationToken);

            if (period == null)
            {
                throw new BadRequestException("Period not found !");
            }

            var startDate = period.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
            var endDate = period.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;

            // Get Entry Student
            var getAttendanceEntries = await GetAttendanceEntryByStudentAsync(param.IdAcademicYear, param.IdStudent, startDate, endDate, attendanceCode, CancellationToken);
            if (!getAttendanceEntries.Any())
                return new List<GetDataDetailAttendanceRecapResult>();

            var listAttendanceEntry = new List<AttendanceEntryNewResult>();
            if (attendanceCode.ToLower() == "excused" || attendanceCode.ToLower() == "unexcused")
            {
                listAttendanceEntry = getAttendanceEntries
                                       .Where(e => e.Attendance.AbsenceCategory.HasValue
                                                && e.Status == AttendanceEntryStatus.Submitted
                                            )
                                        .ToList();

            }
            else
            {
                listAttendanceEntry = getAttendanceEntries.Where(e => e.Status == AttendanceEntryStatus.Submitted).ToList();
            }

            //start logic
            var listUaEa = new List<GetDataDetailAttendanceRecapResult>();
            foreach (var itemAttendanceEntry in listAttendanceEntry)
            {
                //filter student status
                if (!studentStatuses.Any(e => e.StartDt.Date <= itemAttendanceEntry.ScheduleDate.Date && e.EndDt >= itemAttendanceEntry.ScheduleDate.Date && e.IdStudent == param.IdStudent))
                    continue;

                var studentEnrolled = getStudentEnrollment.Where(e => e.IdHomeroomStudent == itemAttendanceEntry.IdHomeroomStudent).FirstOrDefault();

                var passed = false;
                //logic moving
                foreach (var current in studentEnrolled.Items)
                {
                    if (passed)
                        break;

                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                        continue;

                    if (current.Ignored || current.IdLesson != itemAttendanceEntry.IdLesson)
                        continue;

                    //when current id lesson is same as above and the date of the current moving still satisfied
                    //then set to passed, other than that will be excluded
                    if (current.StartDt.Date <= itemAttendanceEntry.ScheduleDate.Date &&
                        itemAttendanceEntry.ScheduleDate.Date < current.EndDt.Date)
                        passed = true;
                }

                if (!passed)
                    continue;

                var newUaEa = new GetDataDetailAttendanceRecapResult
                {
                    Date = itemAttendanceEntry.ScheduleDate,
                    Session = itemAttendanceEntry.Session.SessionID,
                    ClassId = itemAttendanceEntry.ClassID,
                    Homeroom = itemAttendanceEntry.Homeroom,
                    AbsenceCategory = itemAttendanceEntry.Attendance.AbsenceCategory
                };

                listUaEa.Add(newUaEa);
            }

            var queryUaEa = listUaEa.Distinct();

            if (attendanceCode.ToLower() == "excused" || attendanceCode.ToLower() == "unexcused")
            {
                queryUaEa = queryUaEa.Where(e => e.AbsenceCategory == (attendanceCode.ToLower() == "excused"? AbsenceCategory.Excused : AbsenceCategory.Unexcused));
            }

            queryUaEa = mappingAttendance.AbsentTerms == AbsentTerm.Session
                ? queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Session = e.Session,
                                ClassId = e.ClassId,
                                Homeroom = e.Homeroom
                            })
                            .Select(e => new GetDataDetailAttendanceRecapResult
                            {
                                Date = e.Key.Date,
                                Session = e.Key.Session,
                                ClassId = e.Key.ClassId,
                                Homeroom = e.Key.Homeroom
                            }).OrderBy(e => e.Date).Distinct()
                : queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Homeroom = e.Homeroom
                            })
                            .Select(e => new GetDataDetailAttendanceRecapResult
                            {
                                Date = e.Key.Date,
                                Homeroom = e.Key.Homeroom
                            }).OrderBy(e => e.Date).Distinct();

            return queryUaEa.ToList();
        }
        #endregion

        #region data source
        public async Task<List<AttendanceEntryNewResult>> GetAttendanceEntryByStudentAsync(string idAcademicYear, string idStudent, DateTime startDate, DateTime endDate, 
            string attendaceCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(idStudent))
                throw new InvalidOperationException();

            var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => e.ScheduleLesson.IdAcademicYear == idAcademicYear && e.HomeroomStudent.IdStudent == idStudent &&
                            e.ScheduleLesson.ScheduleDate.Date >= startDate && e.ScheduleLesson.ScheduleDate.Date <= endDate &&
                            e.ScheduleLesson.IsGenerated == true);

            if (!string.IsNullOrEmpty(attendaceCode))
            {
                if (attendaceCode.ToLower() == "pending")
                {
                    queryAttendanceEntry = queryAttendanceEntry.Where(x => x.Status == AttendanceEntryStatus.Pending);
                }
                else if(attendaceCode.ToLower() == "lt" || attendaceCode.ToLower() == "pr")
                {
                    queryAttendanceEntry = queryAttendanceEntry.Where(x => x.AttendanceMappingAttendance.Attendance.Code == attendaceCode);
                }
            }

            var results = await queryAttendanceEntry
                .GroupBy(e => new AttendanceEntryNewResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ScheduleDate = e.ScheduleLesson.ScheduleDate,
                    IdLesson = e.ScheduleLesson.IdLesson,
                    ClassID = e.ScheduleLesson.ClassID,
                    IdGrade = e.ScheduleLesson.IdGrade,
                    IdAcademicYear = e.ScheduleLesson.IdAcademicYear,
                    IdLevel = e.ScheduleLesson.IdLevel,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    Attendance = new AttendanceSummaryAttendanceResult
                    {
                        Id = e.AttendanceMappingAttendance.Attendance.Id,
                        Code = e.AttendanceMappingAttendance.Attendance.Code,
                        Description = e.AttendanceMappingAttendance.Attendance.Description,
                        AbsenceCategory = e.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                        ExcusedAbsenceCategory = e.AttendanceMappingAttendance.Attendance.ExcusedAbsenceCategory
                    },
                    Session = new AttendanceSummarySessionResult
                    {
                        Id = e.ScheduleLesson.Session.Id,
                        Name = e.ScheduleLesson.Session.Name,
                        SessionID = e.ScheduleLesson.Session.SessionID.ToString()
                    },
                    Status = e.Status,
                    GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                    Classroom = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        Description = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Code = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    },
                    IdHomeroom = e.HomeroomStudent.Homeroom.Id,
                    Homeroom = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
        }
        public async Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesGroupedAsync(
            List<string> idSchedules, string idStudent, CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(x => x.HomeroomStudent)
                .Where(e => idSchedules.Contains(e.IdScheduleLesson) && e.HomeroomStudent.IdStudent == idStudent)
                .Select(e => new AttendanceEntryResult
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    //PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    DateIn = e.DateIn.Value,
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.IdScheduleLesson).ToDictionary(g => g.Key, g => g.ToList());
        }
        public async Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsPeriod>()
                .AsNoTracking()
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (!string.IsNullOrWhiteSpace(idLevel))
                queryable = queryable.Where(e => e.Grade.IdLevel == idLevel);

            queryable = queryable.Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear);

            var listPeriod = await queryable
                .GroupBy(e => new PeriodResult
                {
                    IdPeriod = e.Id,
                    IdGrade = e.IdGrade,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Semester = e.Semester,
                    IdLevel = e.Grade.IdLevel,
                    AttendanceStartDate = e.AttendanceStartDate,
                    AttendanceEndDate = e.AttendanceEndDate
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listPeriod;
        }
        #endregion

    }

    #region class data
    public class AttendanceRecapData
    {
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string IdStudent { get; set; }
        public string IdClassroom { get; set; }
        public string Homeroom { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Unsubmitted { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Pending { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Present { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Late { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Unexcused { get; set; }
        public List<GetDataDetailAttendanceRecapResult> Excused { get; set; }
    }
    #endregion

    #region batch data
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            return enumerable.BatchCore(batchSize);
        }

        private static IEnumerable<IEnumerable<T>> BatchCore<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            var c = 0;
            var batch = new List<T>();
            foreach (var item in enumerable)
            {
                batch.Add(item);
                if (++c % batchSize == 0)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }
            if (batch.Count != 0)
            {
                yield return batch;
            }
        }
    }
    #endregion
}
