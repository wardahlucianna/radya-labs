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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class ExportExcelActiveUnsubmittedAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public ExportExcelActiveUnsubmittedAttendanceHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            //var param = Request.ValidateParams<GetExtracurricularStudentScoreRequest>(nameof(GetExtracurricularStudentScoreRequest.IdAcademicYear), nameof(GetExtracurricularStudentScoreRequest.Semester), nameof(GetExtracurricularStudentScoreRequest.IdExtracurricular));

            var param = await Request.GetBody<GetActiveUnsubmittedAttendanceRequest>();

            var School = _dbContext.Entity<MsSchool>()
                               .Where(a => a.Id == param.IdSchool)
                               .FirstOrDefault();
            #region Get Active AY
            var getActiveAcademicYear = await _dbContext.Entity<MsPeriod>()
                                        .Include(x => x.Grade)
                                        .ThenInclude(y => y.Level)
                                        .ThenInclude(z => z.AcademicYear)
                                        .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                        .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                                        .OrderByDescending(x => x.StartDate)
                                        .Select(x => new
                                        {
                                            AcademicYear = new ItemValueVm
                                            {
                                                Id = x.Grade.Level.AcademicYear.Id,
                                                Description = x.Grade.Level.AcademicYear.Description
                                            },
                                            AcademicYearCode = x.Grade.Level.AcademicYear.Code,
                                            Semester = x.Semester,
                                        })
                                        .FirstOrDefaultAsync();
            #endregion

            var idAcademicYear = getActiveAcademicYear.AcademicYear.Id;
            //var semester = getActiveAcademicYear.Semester;
            bool IsACOP = false;

            var CheckUserACOP = await _dbContext.Entity<MsUserRole>()
                            .Include(x => x.Role)
                            .Where(a => a.IdUser == param.IdUser
                            && a.Role.Code.ToLower().Contains("acop"))
                            .FirstOrDefaultAsync();

            if (CheckUserACOP != null)
            {
                IsACOP = true;
            }



            var getextracurricularIdList = _dbContext.Entity<MsExtracurricularSpvCoach>()
                            .Include(esc => esc.Extracurricular)
                            .Where(x =>
                                        //x.Extracurricular.Semester == semester &&
                                        x.Extracurricular.ShowAttendanceRC == true &&
                                        x.Extracurricular.Status == true
                                        );

            if(getextracurricularIdList.Where(x => x.IdBinusian == param.IdUser || IsACOP).ToList().Count() > 0)
            {
                getextracurricularIdList = getextracurricularIdList.Where(x => x.IdBinusian == param.IdUser || IsACOP);
            }

            var extracurricularIdList = getextracurricularIdList
                             .Join(_dbContext.Entity<TrExtracurricularGradeMapping>()
                                         .Include(x => x.Grade)
                                         .ThenInclude(x => x.Level)
                                         .ThenInclude(x => x.AcademicYear)
                                         .Where(x => x.Grade.Level.AcademicYear.Id == idAcademicYear),
                                     extracurricular => extracurricular.IdExtracurricular,
                                     grade => grade.IdExtracurricular,
                                     (extracurricular, grade) => extracurricular.IdExtracurricular)
                             .Select(x => x)
                             .Distinct()
                             .ToList();



            var attendanceSessionList = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                            .Include(ega => ega.ExtracurricularSession)
                                            .ThenInclude(es => es.Day)
                                            .Include(ega => ega.Extracurricular)
                                            .Where(x => extracurricularIdList.Contains(x.IdExtracurricular) &&
                                                        x.Date <= _dateTime.ServerTime.Date)
                                            .Join(_dbContext.Entity<MsExtracurricularSpvCoach>()
                                                        .Include(esc => esc.Staff)
                                                        .Include(y => y.ExtracurricularCoachStatus)
                                                        .Where(x => extracurricularIdList.Any(y => y == x.IdExtracurricular) &&
                                                                    (x.IsSpv == true || x.ExtracurricularCoachStatus.Code == "SPV"))
                                                        ,
                                                    session => session.IdExtracurricular,
                                                    supervisor => supervisor.IdExtracurricular,
                                                    (session, supervisor) => new
                                                    {
                                                        idExtracurricularGeneratedAtt = session.Id,
                                                        extracurricular = new NameValueVm
                                                        {
                                                            Id = session.Extracurricular.Id,
                                                            Name = session.Extracurricular.Name
                                                        },
                                                        supervisor = new NameValueVm
                                                        {
                                                            Id = supervisor.Staff.IdBinusian,
                                                            Name = NameUtil.GenerateFullName(supervisor.Staff.FirstName, supervisor.Staff.LastName)
                                                        },
                                                        day = session.ExtracurricularSession.Day.Description,
                                                        date = session.Date,
                                                        semester = session.Extracurricular.Semester
                                                    })
                                                    .OrderBy(x => x.extracurricular.Name)
                                            .Select(x => x)
                                            .Distinct()
                                            .ToList();

            var groupSessionList = attendanceSessionList
                .GroupBy(x => new
                {
                    x.extracurricular.Name,
                    x.extracurricular.Id,
                    x.semester,
                    x.day,
                    x.date,
                    x.idExtracurricularGeneratedAtt
                })
                .Select(x => new
                {
                    extracurricular = new NameValueVm
                    {
                        Id = x.Key.Id,
                        Name = x.Key.Name
                    },
                    x.Key.semester,
                    x.Key.day,
                    x.Key.date,
                    supervisorCoach = string.Join("; ", x.Select(x => x.supervisor.Name)),
                    x.Key.idExtracurricularGeneratedAtt
                })
                .Distinct().ToList();

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                .Include(x => x.Extracurricular)
                .Include(x => x.Student)
                .Where(x => groupSessionList.Select(x => x.extracurricular.Id).Contains(x.IdExtracurricular) && x.Status == true)
                .ToListAsync(CancellationToken);

            var studentAttendanceList = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                .Include(x => x.ExtracurricularGeneratedAtt)
                    .ThenInclude(x => x.Extracurricular)
                .Include(x => x.ExtracurricularStatusAtt)
                .Where(x => attendanceSessionList.Select(y => y.idExtracurricularGeneratedAtt).Contains(x.IdExtracurricularGeneratedAtt))
                .ToListAsync(CancellationToken);

            var getJoinGradeList = await _dbContext.Entity<MsExtracurricular>()
                .Include(x => x.ExtracurricularGradeMappings)
                .Where(x => extracurricularIdList.Contains(x.Id))
                .Where(x => x.Status == true)
                .Select(x => new
                {
                    joinGrade = string.Join(";", x.ExtracurricularGradeMappings.OrderBy(a => a.Grade.Code).Select(a => a.Grade.Description)),
                    IdExtracurricular = x.Id
                })
                .ToListAsync(CancellationToken);

            var joinStudentAttendance = from extracurricularParticipant in getExtracurricularParticipant
                                        join studentAttendance in studentAttendanceList
                                            on new { extracurricularParticipant.IdStudent, extracurricularParticipant.IdExtracurricular } equals new { studentAttendance.IdStudent, studentAttendance.ExtracurricularGeneratedAtt.IdExtracurricular } into studentList
                                        from studentData in studentList.DefaultIfEmpty()
                                        select new
                                        {
                                            IdStudent = extracurricularParticipant.IdStudent,
                                            Extracurricular = new NameValueVm
                                            {
                                                Name = extracurricularParticipant.Extracurricular.Name,
                                                Id = extracurricularParticipant.Extracurricular.Id,
                                            },
                                            Semester = extracurricularParticipant.Extracurricular.Semester,
                                            IdExtracurricularGeneratedAtt = studentData?.ExtracurricularGeneratedAtt?.Id,
                                            IdExtracurricularStatusAtt = studentData?.ExtracurricularStatusAtt?.Id,
                                            Date = studentData?.ExtracurricularGeneratedAtt?.Date
                                        };

            var studentDataList = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.GradePathwayClassroom)
                    .ThenInclude(x => x.Classroom)
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                .Include(x => x.Student)
                .Where(x => joinStudentAttendance.Select(y => y.IdStudent).Contains(x.IdStudent))
                .Where(x => x.Homeroom.IdAcademicYear == idAcademicYear)
                .ToListAsync(CancellationToken);

            var groupStudentAttendance = joinStudentAttendance
                .Join(studentDataList,
                    studentAttendance => new { studentAttendance.IdStudent, studentAttendance.Semester },
                    studentData => new { studentData.IdStudent, studentData.Semester },
                    (studentAttendance, studentData) => new { studentAttendance, studentData })
                .GroupBy(x => new
                {
                    x.studentData.IdStudent,
                    studentName = NameUtil.GenerateFullName(x.studentData.Student.FirstName, x.studentData.Student.MiddleName, x.studentData.Student.LastName),
                    classroom = x.studentData.Homeroom.Grade.Code + x.studentData.Homeroom.GradePathwayClassroom.Classroom.Code,
                    x.studentAttendance.Extracurricular.Name,
                    x.studentAttendance.Extracurricular.Id,
                    x.studentAttendance.Semester
                })
                .Select(x => new
                {
                    IdStudent = x.Key.IdStudent,
                    StudentName = x.Key.studentName,
                    Classroom = x.Key.classroom,
                    extracurricular = new NameValueVm
                    {
                        Id = x.Key.Id,
                        Name = x.Key.Name
                    },
                    x.Key.Semester,
                    StudentAttendance = x.Select(x => new
                    {
                        x.studentAttendance.IdExtracurricularGeneratedAtt,
                        x.studentAttendance.IdExtracurricularStatusAtt,
                        x.studentAttendance.Date
                    }).ToList()
                }).Distinct().ToList();

            var AttendanceResult = new ExportExcelActiveUnsubmittedAttendanceResult();
            AttendanceResult.SchoolDescription = School.Description;

            var studentAttendanceResultList = new List<ExportExcelActiveUnsubmittedAttendanceResultStudent>();

            foreach (var extracurricular in extracurricularIdList)
            {
                var filterGroupStudentAttendance = groupStudentAttendance
                    .Where(x => x.extracurricular.Id == extracurricular)
                    .Distinct().ToList();

                var extracurricularJoinGrade = getJoinGradeList.Where(x => x.IdExtracurricular == extracurricular).Select(x => x.joinGrade).FirstOrDefault();

                foreach (var studentAttendance in filterGroupStudentAttendance)
                {

                    var studentJoinDate = getExtracurricularParticipant
                        .Where(x => x.IdExtracurricular == extracurricular && x.IdStudent == studentAttendance.IdStudent).Select(x => x.JoinDate).FirstOrDefault();

                    var totalSession = attendanceSessionList
                        .Where(x => x.extracurricular.Id == extracurricular && x.date >= studentJoinDate)
                        .Select(x => x.idExtracurricularGeneratedAtt).Distinct().Count();

                    var studentAttendanceResult = new ExportExcelActiveUnsubmittedAttendanceResultStudent();
                    studentAttendanceResult.AcademicYear = getActiveAcademicYear.AcademicYearCode;
                    studentAttendanceResult.Semester = studentAttendance.Semester.ToString();
                    studentAttendanceResult.ElectiveName = studentAttendance.extracurricular.Name;
                    studentAttendanceResult.Grade = extracurricularJoinGrade;

                    var studentData = new NameValueVm()
                    {
                        Id = studentAttendance.IdStudent,
                        Name = studentAttendance.StudentName
                    };

                    studentAttendanceResult.Student = studentData;
                    studentAttendanceResult.Class = studentAttendance.Classroom;

                    var attendedSession = studentAttendance.StudentAttendance
                        .Where(x => x.IdExtracurricularGeneratedAtt != null && x.Date >= studentJoinDate)
                        .Distinct()
                        .Count();

                    double AttendancePercentage =
                        Math.Round(((double)attendedSession / (double)totalSession) * 100, 2);

                    studentAttendanceResult.AttendancePercentage = AttendancePercentage.ToString() + "%";

                    studentAttendanceResultList.Add(studentAttendanceResult);
                }
            }

            studentAttendanceResultList = studentAttendanceResultList
                .OrderBy(x => x.AcademicYear).ThenBy(x => x.Semester).ThenBy(x => x.ElectiveName).ThenBy(x => x.Grade).ThenBy(x => x.Student.Name)
                .ToList();

            AttendanceResult.StudentAttendanceList = studentAttendanceResultList;

            var unsubmittedAttendanceResultList = new List<ExportExcelActiveUnsubmittedAttendanceResultUnsubmitted>();

            foreach(var sessionData in groupSessionList)
            {
                var filterExtracurricularParticipant = getExtracurricularParticipant
                    .Where(x => x.IdExtracurricular == sessionData.extracurricular.Id)
                    .Where(x => x.JoinDate <= sessionData.date)
                    .ToList();

                var extracurricularJoinGrade = getJoinGradeList.Where(x => x.IdExtracurricular == sessionData.extracurricular.Id).Select(x => x.joinGrade).FirstOrDefault();

                var unsubmittedParticipantCount = filterExtracurricularParticipant
                            .GroupJoin(studentAttendanceList
                            .Where(x => x.IdExtracurricularGeneratedAtt == sessionData.idExtracurricularGeneratedAtt),
                                p => p.IdStudent,
                                a => a.IdStudent,
                                (p, a) => new { p, a })
                            .SelectMany(
                                x => x.a.DefaultIfEmpty(),
                                (participant, attendace) => new { participant, attendace })
                            .Where(x => x.attendace == null)
                            .Select(x => x.participant?.p.IdStudent)
                            .Distinct()
                            .Count();

                if (unsubmittedParticipantCount != 0)
                {
                    var submittedParticipantCount = filterExtracurricularParticipant
                            .GroupJoin(studentAttendanceList
                            .Where(x => x.IdExtracurricularGeneratedAtt == sessionData.idExtracurricularGeneratedAtt),
                                p => p.IdStudent,
                                a => a.IdStudent,
                                (p, a) => new { p, a })
                            .SelectMany(
                                x => x.a.DefaultIfEmpty(),
                                (participant, attendace) => new { participant, attendace })
                            .Where(x => x.attendace != null)
                            .Select(x => x.participant?.p.IdStudent)
                            .Distinct()
                            .Count();

                    var unsubmittedAttendanceResult = new ExportExcelActiveUnsubmittedAttendanceResultUnsubmitted();
                    unsubmittedAttendanceResult.AcademicYear = getActiveAcademicYear.AcademicYearCode;
                    unsubmittedAttendanceResult.Semester = sessionData.semester.ToString();
                    unsubmittedAttendanceResult.ElectiveName = sessionData.extracurricular.Name;
                    unsubmittedAttendanceResult.SupervisorCoach = sessionData.supervisorCoach;
                    unsubmittedAttendanceResult.Grade = extracurricularJoinGrade;
                    unsubmittedAttendanceResult.Day = sessionData.day;
                    unsubmittedAttendanceResult.Date = sessionData.date;
                    unsubmittedAttendanceResult.TotalEntry = submittedParticipantCount.ToString();
                    unsubmittedAttendanceResult.TotalParticipant = filterExtracurricularParticipant.Count().ToString();

                    unsubmittedAttendanceResultList.Add(unsubmittedAttendanceResult);
                }
            };;

            unsubmittedAttendanceResultList = unsubmittedAttendanceResultList
                .OrderBy(x => x.AcademicYear).ThenBy(x => x.Semester).ThenBy(x => x.ElectiveName).ThenBy(x => x.Grade).ThenBy(x => x.Date)
                .ToList();

            AttendanceResult.UnsubmittedAttendanceList = unsubmittedAttendanceResultList;
            AttendanceResult.TotalSupervisor = groupSessionList.Select(x => x.supervisorCoach).ToList().SelectMany(x => x.Split("; ")).Distinct().Count().ToString();

            var title = "UnSubmitted Attendance";

            if (studentAttendanceResultList != null || unsubmittedAttendanceResultList != null)
            {
                var generateExcelByte = GenerateExcel(AttendanceResult);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcelByte = GenerateBlankExcel(title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }


        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(2);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                //foreach (string field in fieldDataList)
                //{
                //    var Judul = row.CreateCell(fieldCount);
                //    Judul.SetCellValue(field);
                //    row.Cells[fieldCount].CellStyle = style;
                //    fieldCount++;
                //}

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
            }
        }

        public byte[] GenerateExcel(ExportExcelActiveUnsubmittedAttendanceResult Data)
        {
            var result = new byte[0];
            var studentData = Data.StudentAttendanceList;
            var unsubmittedData = Data.UnsubmittedAttendanceList;
            var School = Data.SchoolDescription;

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet StudentAttendance = workbook.CreateSheet("Student Attendance");

                int columnComponentCount = 8;

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.Alignment = HorizontalAlignment.Center;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

                //Create style for header
                ICellStyle dataStyle = workbook.CreateCellStyle();

                //Set border style 
                dataStyle.BorderBottom = BorderStyle.Thin;
                dataStyle.BorderLeft = BorderStyle.Thin;
                dataStyle.BorderRight = BorderStyle.Thin;
                dataStyle.BorderTop = BorderStyle.Thin;
                dataStyle.VerticalAlignment = VerticalAlignment.Center;
                dataStyle.Alignment = HorizontalAlignment.Left;
                dataStyle.WrapText = true;

                //Set font style
                IFont Datafont = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                Datafont.FontName = "Arial";
                Datafont.FontHeightInPoints = 12;
                dataStyle.SetFont(Datafont);

                //header 
                //IRow row = excelSheet.CreateRow(0);

                //Title 
                IRow row = StudentAttendance.CreateRow(0);
                var cellTitleRow = row.CreateCell(0);
                cellTitleRow.SetCellValue("Student Attendance");
                cellTitleRow.CellStyle = headerStyle;
                StudentAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, 7));

                row = StudentAttendance.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("School :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(School);
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 7; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                StudentAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 7));
                              

                row = StudentAttendance.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Total Supervisor :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(Data.TotalSupervisor);
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 7; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                StudentAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 7));

                row = StudentAttendance.CreateRow(row.RowNum + 1);

                //Body

                row = StudentAttendance.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Academic Year");
                row.Cells[0].CellStyle = headerStyle;
                row.CreateCell(1).SetCellValue("Semester");
                row.Cells[1].CellStyle = headerStyle;
                row.CreateCell(2).SetCellValue("Elective Name");
                row.Cells[2].CellStyle = headerStyle;
                row.CreateCell(3).SetCellValue("Grade");
                row.Cells[3].CellStyle = headerStyle;
                row.CreateCell(4).SetCellValue("Student ID");
                row.Cells[4].CellStyle = headerStyle;
                row.CreateCell(5).SetCellValue("Student Name");
                row.Cells[5].CellStyle = headerStyle;
                row.CreateCell(6).SetCellValue("Class");
                row.Cells[6].CellStyle = headerStyle;
                row.CreateCell(7).SetCellValue("Completeness Attendance Percentage");
                row.Cells[7].CellStyle = headerStyle;


                foreach (var item in studentData)
                {

                    row = StudentAttendance.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(item.AcademicYear);
                    row.Cells[0].CellStyle = dataStyle;
                    row.CreateCell(1).SetCellValue(item.Semester);
                    row.Cells[1].CellStyle = dataStyle;
                    row.CreateCell(2).SetCellValue(item.ElectiveName);
                    row.Cells[2].CellStyle = dataStyle;
                    row.CreateCell(3).SetCellValue(item.Grade);
                    row.Cells[3].CellStyle = dataStyle;
                    row.CreateCell(4).SetCellValue(item.Student.Id);
                    row.Cells[4].CellStyle = dataStyle;
                    row.CreateCell(5).SetCellValue(item.Student.Name);
                    row.Cells[5].CellStyle = dataStyle;
                    row.CreateCell(6).SetCellValue(item.Class);
                    row.Cells[6].CellStyle = dataStyle;
                    row.CreateCell(7).SetCellValue(item.AttendancePercentage);
                    row.Cells[7].CellStyle = dataStyle;

                }


                SetDynamicColumnWidthExcel(columnComponentCount, ref StudentAttendance);

                var columnComponentCount2 = 9;


                ISheet UnsubmittedAttendance = workbook.CreateSheet("Unsubmitted Attendance");
                //Title 
                IRow rowUnsubmitted = UnsubmittedAttendance.CreateRow(0);
                cellTitleRow = rowUnsubmitted.CreateCell(0);
                cellTitleRow.SetCellValue("Unsubmitted Attendance");
                cellTitleRow.CellStyle = headerStyle;
                UnsubmittedAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowUnsubmitted.RowNum, rowUnsubmitted.RowNum, 0, 8));

                rowUnsubmitted = UnsubmittedAttendance.CreateRow(rowUnsubmitted.RowNum + 1);
                rowUnsubmitted.CreateCell(0).SetCellValue("School :");
                rowUnsubmitted.Cells[0].CellStyle = dataStyle;
                rowUnsubmitted.CreateCell(1).SetCellValue(School);
                rowUnsubmitted.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 8; i++)
                {
                    rowUnsubmitted.CreateCell(i).SetCellValue("");
                    rowUnsubmitted.Cells[i].CellStyle = dataStyle;
                }
                UnsubmittedAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowUnsubmitted.RowNum, rowUnsubmitted.RowNum, 1, 8));


                rowUnsubmitted = UnsubmittedAttendance.CreateRow(rowUnsubmitted.RowNum + 1);
                rowUnsubmitted.CreateCell(0).SetCellValue("Total Supervisor :");
                rowUnsubmitted.Cells[0].CellStyle = dataStyle;
                rowUnsubmitted.CreateCell(1).SetCellValue(Data.TotalSupervisor);
                rowUnsubmitted.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 8; i++)
                {
                    rowUnsubmitted.CreateCell(i).SetCellValue("");
                    rowUnsubmitted.Cells[i].CellStyle = dataStyle;
                }
                UnsubmittedAttendance.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowUnsubmitted.RowNum, rowUnsubmitted.RowNum, 1, 8));

                rowUnsubmitted = UnsubmittedAttendance.CreateRow(rowUnsubmitted.RowNum + 1);

                //Body

                rowUnsubmitted = UnsubmittedAttendance.CreateRow(rowUnsubmitted.RowNum + 1);
                rowUnsubmitted.CreateCell(0).SetCellValue("Academic Year");
                rowUnsubmitted.Cells[0].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(1).SetCellValue("Semester");
                rowUnsubmitted.Cells[1].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(2).SetCellValue("Elective Name");
                rowUnsubmitted.Cells[2].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(3).SetCellValue("Grade");
                rowUnsubmitted.Cells[3].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(4).SetCellValue("Supervisor/Coach");
                rowUnsubmitted.Cells[4].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(5).SetCellValue("Day");
                rowUnsubmitted.Cells[5].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(6).SetCellValue("Date");
                rowUnsubmitted.Cells[6].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(7).SetCellValue("Total Participant");
                rowUnsubmitted.Cells[7].CellStyle = headerStyle;
                rowUnsubmitted.CreateCell(8).SetCellValue("Total Entry");
                rowUnsubmitted.Cells[8].CellStyle = headerStyle;


                foreach (var item in unsubmittedData)
                {

                    rowUnsubmitted = UnsubmittedAttendance.CreateRow(rowUnsubmitted.RowNum + 1);
                    rowUnsubmitted.CreateCell(0).SetCellValue(item.AcademicYear);
                    rowUnsubmitted.Cells[0].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(1).SetCellValue(item.Semester);
                    rowUnsubmitted.Cells[1].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(2).SetCellValue(item.ElectiveName);
                    rowUnsubmitted.Cells[2].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(3).SetCellValue(item.Grade);
                    rowUnsubmitted.Cells[3].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(4).SetCellValue(item.SupervisorCoach);
                    rowUnsubmitted.Cells[4].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(5).SetCellValue(item.Day);
                    rowUnsubmitted.Cells[5].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(6).SetCellValue(item.Date.ToString("dd/MM/yyyy"));
                    rowUnsubmitted.Cells[6].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(7).SetCellValue(item.TotalParticipant);
                    rowUnsubmitted.Cells[7].CellStyle = dataStyle;
                    rowUnsubmitted.CreateCell(8).SetCellValue(item.TotalEntry);
                    rowUnsubmitted.Cells[8].CellStyle = dataStyle;

                }

                SetDynamicColumnWidthExcel(columnComponentCount2, ref UnsubmittedAttendance);


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
