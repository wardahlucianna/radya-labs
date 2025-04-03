using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using ClosedXML.Excel;
using System.Net.Http;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Common.Functions.Handler;
using HandlebarsDotNet;
using FluentEmail.Core.Models;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary.Notification
{
    public class ATD4Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<UserPositionVm> _userPositions;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD4Notification> _logger;
        private readonly IMachineDateTime _machineDateTime;
        private readonly MemoryStream _memoryStream;
        public ATD4Notification(INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            IConfiguration configuration,
            ILogger<ATD4Notification> logger,
            IMachineDateTime machineDateTime,
            IDictionary<string, object> notificationData) :
             base("ATD4", notificationManager, configuration, logger)
        {
            _dbContext = new AttendanceDbContext(options); ;
            _logger = logger;
            _machineDateTime = machineDateTime;
            _notificationData = notificationData;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = false
                }
            };

            return Task.CompletedTask;
        }
        protected override async Task Prepare()
        {
            try
            {
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                _notificationData = new Dictionary<string, object>
                {
                    { "hostUrl", hostUrl }
                };
                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;
                var currentAcademicYearBySchool = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == IdSchool)
               .Where(x => _machineDateTime.ServerTime.Date >= x.StartDate.Date)
               .Where(x => _machineDateTime.ServerTime.Date <= x.EndDate.Date)
               .GroupBy(x => x.Grade.Level.AcademicYear.Id)
               .Select(x => new
               {
                   IdAcademicyear = x.Key,
                   AttendanceStartDate = x.Min(y => y.AttendanceStartDate)
               })
               .FirstOrDefaultAsync(CancellationToken);

                if (currentAcademicYearBySchool == null)
                    return;
                #region Find User P , VP And Level Head
                var userByPosition = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.NonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
                .Where(x => x.NonTeachingLoad.IdAcademicYear == currentAcademicYearBySchool.IdAcademicyear)
                .Where(x => (x.NonTeachingLoad.TeacherPosition.LtPosition.Code == PositionConstant.Principal
                || x.NonTeachingLoad.TeacherPosition.LtPosition.Code == PositionConstant.VicePrincipal
                || x.NonTeachingLoad.TeacherPosition.LtPosition.Code == PositionConstant.LevelHead))
                .Select(x => new
                {
                    x.IdUser,
                    x.Data,
                    x.NonTeachingLoad.TeacherPosition.LtPosition.Code
                }).ToListAsync(CancellationToken);
                var idUsers = userByPosition.Select(x => x.IdUser).ToList();
                var users =
                    (
                        from _user in userByPosition
                        join _staff in _dbContext.Entity<MsStaff>() on _user.IdUser equals _staff.IdBinusian
                        select new
                        {
                            _user.IdUser,
                            _user.Data,
                            _user.Code,
                            _staff.BinusianEmailAddress,
                            _staff.FirstName,
                            _staff.LastName
                        }
                   ).ToList();
                _userPositions = users.GroupBy(x => new
                {
                    x.IdUser,
                    x.Code,
                    x.BinusianEmailAddress,
                    x.FirstName,
                    x.LastName
                })
                .Select(x => new UserPositionVm
                {
                    IdUser = x.Key.IdUser,
                    Code = x.Key.Code,
                    EmailAddress = x.Key.BinusianEmailAddress,
                    FirstName = x.Key.FirstName,
                    LastName = x.Key.LastName,
                    Data = x.Select(y => y.Data).ToList()
                })
                .ToList();
                IdUserRecipients = users.GroupBy(x => new
                {
                    x.IdUser,
                    x.Code,
                    x.BinusianEmailAddress,
                    x.FirstName,
                    x.LastName
                })
                .Select(x => x.Key.IdUser)
                .ToList();
                #endregion
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        private class UserPositionVm
        {
            public string IdUser { get; set; }
            public string Code { get; set; }
            public string EmailAddress { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<string> Data { get; set; }
        }


        public void Dispose()
        {
            (_dbContext as AttendanceDbContext)?.Dispose();
        }

        protected override async Task SendPushNotification()
        {
            if (IdUserRecipients is null)
                _logger.LogInformation($"Skip sending notification. No Id User Recipients");

            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new
                {
                    x.IdUser,
                    x.FirebaseToken
                }
                )
                .ToListAsync(CancellationToken);
            var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
            var compilePush = Handlebars.Compile(NotificationTemplate.Push);
            //Summary Attendance for this {{period}} has been sent to your email
            _notificationData["period"] = "today";
            var emailTitle = compileTitle(_notificationData);

            foreach (var item in _userPositions)
            {
                var tokenByUser = tokens.Where(x => x.IdUser == item.IdUser).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;
                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = compileTitle(_notificationData),
                        Body = compilePush(_notificationData),
                    },
                    Tokens = tokenByUser,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                // send push notification
                await NotificationManager.SendPushNotification(message);
            }
        }

        protected override async Task SendEmailNotification()
        {
            if (IdUserRecipients is null)
                _logger.LogInformation($"Skip sending notification. No Id User Recipients");
            var sendEmailTasks = new List<Task>();
            var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).Select(x => new
            {
                x.Name,
                x.Logo
            }
            ).FirstOrDefaultAsync(CancellationToken);
            MemoryStream memoryLogo = new MemoryStream();
            if (!string.IsNullOrWhiteSpace(school.Logo))
            {
                var client = new HttpClient();
                var response = await client.GetAsync(school.Logo);
                var myByteArray = await response.Content.ReadAsByteArrayAsync();
                memoryLogo.Write(myByteArray, 0, myByteArray.Length);
            }
            foreach (var item in _userPositions)
            {
                var resultExcel = new MemoryStream();
                var resuleExcel2 = new byte[0];
                var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
                List<string> idLevels = new List<string>();
                if (item.Code == PositionConstant.Principal)
                {
                    foreach (var data in item.Data)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevels.Add(_levelLH.Id);
                    }
                    predicateHomeroom = predicateHomeroom.And(x => idLevels.Contains(x.Grade.IdLevel));
                }

                if (item.Code == PositionConstant.VicePrincipal)
                {
                    foreach (var data in item.Data)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevels.Add(_levelLH.Id);
                    }
                    predicateHomeroom = predicateHomeroom.And(x => idLevels.Contains(x.Grade.IdLevel));
                }

                if (item.Code == PositionConstant.LevelHead)
                {
                    foreach (var data in item.Data)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                        idLevels.Add(_gradeLH.Id);
                    }
                    predicateHomeroom = predicateHomeroom.And(x => idLevels.Contains(x.IdGrade));
                }


                var homerooms = await _dbContext.Entity<MsHomeroom>()
                                     .Include(x => x.Grade).ThenInclude(x => x.Level)
                                     .Where(predicateHomeroom)
                                     .Select(x => new
                                     {
                                         x.Id,
                                         x.Grade.IdLevel
                                     })
                                     .ToListAsync(CancellationToken);

                var idHomerooms = homerooms.Select(x => x.Id).Distinct().ToList();

                var allSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                      .Include(x => x.GeneratedScheduleStudent)
                                       .ThenInclude(x => x.Student)
                                           .ThenInclude(x => x.StudentGrades)
                                       .Include(x => x.AttendanceEntries)
                                            .ThenInclude(x => x.AttendanceMappingAttendance)
                                                .ThenInclude(x => x.Attendance)
                                       .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.Level)
                                                .ThenInclude(x => x.AcademicYear)
                                                    .ThenInclude(x => x.School)
                                       .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                      .Where(x =>
                                            x.IsGenerated
                                           && _machineDateTime.ServerTime.Date.AddDays(-4) == x.ScheduleDate.Date
                                           && idHomerooms.Contains(x.IdHomeroom)
                                           )
                                      .ToListAsync(CancellationToken);
                if (allSchedules == null)
                    continue;

                #region Sheet 1
                var table11 = allSchedules
                    .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.OrderNumber)
                        .ThenBy(x => x.Homeroom.Grade.Level.OrderNumber)
                            .ThenBy(x => x.Homeroom.Grade.OrderNumber)
                                .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
                                    .ThenBy(x => x.GeneratedScheduleStudent.IdStudent)
                    .Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted))
                    .Select(x => new
                    {
                        idStudent = x.GeneratedScheduleStudent.IdStudent,
                        nameStudent = x.GeneratedScheduleStudent.Student.FirstName + x.GeneratedScheduleStudent.Student.LastName,
                        homeroom = x.HomeroomName
                    }).ToList();

                var table12 = allSchedules
                    .OrderBy(x => x.IdUser)
                        .ThenBy(x => Convert.ToInt32(x.SessionID))
                    .Where(x => !x.AttendanceEntries.Any())
                    .GroupBy(x => new
                    {
                        x.IdUser,
                        x.TeacherName,
                        x.SessionID
                    })
                    .Select(x => new
                    {
                        x.Key.IdUser,
                        x.Key.TeacherName,
                        x.Key.SessionID
                    }).ToList();
                #endregion
                #region Sheet 2
                var table21 = allSchedules
                    .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.OrderNumber)
                        .ThenBy(x => x.Homeroom.Grade.Level.OrderNumber)
                            .ThenBy(x => x.Homeroom.Grade.OrderNumber)
                                .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
                                    .ThenBy(x => x.GeneratedScheduleStudent.IdStudent)
                    .GroupBy(x => new
                    {
                        x.GeneratedScheduleStudent.IdStudent,
                        x.HomeroomName,
                        x.GeneratedScheduleStudent.Student.FirstName,
                        x.GeneratedScheduleStudent.Student.LastName
                    })
                    .Select(x => new
                    {
                        x.Key.IdStudent,
                        nameStudent = x.Key.FirstName + x.Key.LastName,
                        homeroom = x.Key.HomeroomName,
                        tappingTime = "",
                        Session1 = x.Where(p => p.SessionID == "1").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session2 = x.Where(p => p.SessionID == "2").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session3 = x.Where(p => p.SessionID == "3").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session4 = x.Where(p => p.SessionID == "4").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session5 = x.Where(p => p.SessionID == "5").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session6 = x.Where(p => p.SessionID == "6").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session7 = x.Where(p => p.SessionID == "7").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session8 = x.Where(p => p.SessionID == "8").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session9 = x.Where(p => p.SessionID == "9").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session10 = x.Where(p => p.SessionID == "10").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session11 = x.Where(p => p.SessionID == "11").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        Session12 = x.Where(p => p.SessionID == "12").Max(s => s.AttendanceEntries.FirstOrDefault()?.AttendanceMappingAttendance?.Attendance?.Code),
                        TotalPresent = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present && z.Status == AttendanceEntryStatus.Submitted)),
                        TotalAbsent = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)),
                        TotalLate = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)),
                    }).ToList();
                #endregion
                #region Sheet 3
                var table31 = allSchedules
                    .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.OrderNumber)
                        .ThenBy(x => x.Homeroom.Grade.Level.OrderNumber)
                            .ThenBy(x => x.Homeroom.Grade.OrderNumber)
                                .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
                                    .ThenBy(x => x.GeneratedScheduleStudent.IdStudent)
                    .Where(x => (
                    x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AbsenceCategory.HasValue ? y.AttendanceMappingAttendance.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused : false)
                    || x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present)))
                    .Select(x => new
                    {
                        idStudent = x.GeneratedScheduleStudent.IdStudent,
                        nameStudent = x.GeneratedScheduleStudent.Student.FirstName + x.GeneratedScheduleStudent.Student.LastName,
                        homeroom = x.HomeroomName,
                        classId = x.ClassID,
                        sesion = x.SessionID,
                        status = x.AttendanceEntries.FirstOrDefault().Status,
                        teacher = x.TeacherName,
                        attendance = x.AttendanceEntries.FirstOrDefault().AttendanceMappingAttendance.Attendance.Description
                    }).ToList();
                #endregion
                #region Sheet 4
                var table42 = allSchedules
                    .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.OrderNumber)
                        .ThenBy(x => x.Homeroom.Grade.Level.OrderNumber)
                            .ThenBy(x => x.Homeroom.Grade.OrderNumber)
                                .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
                .GroupBy(x => new
                {
                    x.Homeroom.Grade.Code
                })
                .Select(x => new
                {
                    Grade = "Grade " + x.Key.Code,
                    TotalSessions = x.Count()
                })
                .ToList();
                #endregion
                //var table3 = null;
                using (var ms = new MemoryStream())
                {
                    #region Workbook
                    var workbook = new XLWorkbook();
                    var sheet1 = workbook.Worksheets.Add("UA Students");
                    var sheet2 = workbook.Worksheets.Add("UA & Present Students");
                    var sheet3 = workbook.Worksheets.Add("UA & Present - Details");
                    var sheet4 = workbook.Worksheets.Add("Summary");
                    #endregion
                    #region Sheet 1
                    if (!string.IsNullOrEmpty(school.Logo))
                    {
                        var imageSheet1 = sheet1.AddPicture(memoryLogo)
                            .MoveTo(sheet1.Cell("A1"))
                            .Scale(0.05d);
                    }
                    var header1 = sheet1.Range("C1:E2");
                    header1.SetValue("List of UA Students");
                    header1.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header1.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header1.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header1.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header1.Style.Font.FontName = "Calibri";
                    header1.Style.Font.FontSize = 20;
                    header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header1.Style.Alignment.WrapText = true;
                    header1.Style.Font.Bold = true;
                    header1.Merge();


                    var header11 = sheet1.Range("C3:E3");
                    header11.SetValue($"as at {_machineDateTime.ServerTime.Date.ToString("dd MMMM yyyy HH:mm")}");
                    header11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header11.Merge();

                    var header2 = sheet1.Range("G1:J3");
                    header2.SetValue("Teachers who have not filled Attendance");
                    header2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header2.Style.Font.FontName = "Calibri";
                    header2.Style.Font.FontSize = 20;
                    header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header2.Style.Alignment.WrapText = true;
                    header1.Style.Font.Bold = true;
                    header2.Merge();

                    var header3 = sheet1.Range("L1:O3");
                    header3.SetValue("Students who did not Tap In");
                    header3.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header3.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header3.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header3.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header3.Style.Font.FontName = "Calibri";
                    header3.Style.Font.FontSize = 20;
                    header3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header3.Style.Alignment.WrapText = true;
                    header1.Style.Font.Bold = true;
                    header3.Merge();
                    #region Table 1
                    var column1Table1 = sheet1.Cell("A6").Value = "No";
                    var column2Table1 = sheet1.Cell("B6").Value = "Student ID";
                    var column3Table1 = sheet1.Cell("C6").Value = "Student Name";
                    var column4Table1 = sheet1.Cell("D6").Value = "Class";

                    var rangeHeaderTable1 = sheet1.Range("A6:D6");
                    rangeHeaderTable1.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1.Style.Font.FontName = "Calibri";
                    rangeHeaderTable1.Style.Font.FontSize = 11;
                    rangeHeaderTable1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeHeaderTable1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeHeaderTable1.Style.Alignment.WrapText = true;
                    rangeHeaderTable1.Style.Font.Bold = true;

                    int rowTable1 = 7;
                    int counterDataTable1 = 1;
                    foreach (var t11 in table11)
                    {
                        sheet1.Cell($"A{rowTable1}").Value = counterDataTable1;
                        sheet1.Cell($"B{rowTable1}").Value = t11.idStudent;
                        sheet1.Cell($"C{rowTable1}").Value = t11.nameStudent;
                        sheet1.Cell($"D{rowTable1}").Value = t11.homeroom;
                        rowTable1++;
                        counterDataTable1++;
                    }
                    var rangeCecllTable1 = sheet1.Range($"A7:D{rowTable1 - 1}");
                    rangeCecllTable1.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeCecllTable1.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeCecllTable1.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeCecllTable1.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeCecllTable1.Style.Font.FontName = "Calibri";
                    rangeCecllTable1.Style.Font.FontSize = 11;
                    rangeCecllTable1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeCecllTable1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    #endregion
                    #region Table 2
                    var column1Table2 = sheet1.Cell("G6").Value = "No";

                    var column2Table2 = sheet1.Cell("H6").Value = "Teacher ID";

                    var column3Table2 = sheet1.Cell("I6").Value = "Teacher Name";

                    var column4Table2 = sheet1.Cell("J6").Value = "Session";

                    var rangeHeaderTable2 = sheet1.Range("G6:J6");
                    rangeHeaderTable2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable2.Style.Font.FontName = "Calibri";
                    rangeHeaderTable2.Style.Font.FontSize = 11;
                    rangeHeaderTable2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeHeaderTable2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeHeaderTable2.Style.Alignment.WrapText = true;
                    rangeHeaderTable2.Style.Font.Bold = true;

                    int rowTable2 = 7;
                    int counterDataTable2 = 1;
                    foreach (var t11 in table12)
                    {
                        sheet1.Cell($"G{rowTable2}").Value = counterDataTable2;
                        sheet1.Cell($"H{rowTable2}").Value = t11.IdUser;
                        sheet1.Cell($"I{rowTable2}").Value = t11.TeacherName;
                        sheet1.Cell($"J{rowTable2}").Value = t11.SessionID;
                        rowTable2++;
                        counterDataTable2++;
                    }
                    var rangeCellTable2 = sheet1.Range($"G7:J{rowTable2 - 1}");
                    rangeCellTable2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2.Style.Font.FontName = "Calibri";
                    rangeCellTable2.Style.Font.FontSize = 11;
                    rangeCellTable2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeCellTable2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    #endregion
                    #endregion
                    #region Sheet 2
                    if (!string.IsNullOrEmpty(school.Logo))
                    {
                        var imageSheet2 = sheet2.AddPicture(memoryLogo)
                            .MoveTo(sheet2.Cell("A1"))
                            .Scale(0.05d);
                    }
                    var header1Sheet2 = sheet2.Range("G1:M2");
                    header1Sheet2.SetValue("List of Students with UA & Present");
                    header1Sheet2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header1Sheet2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header1Sheet2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header1Sheet2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header1Sheet2.Style.Font.FontName = "Calibri";
                    header1Sheet2.Style.Font.FontSize = 20;
                    header1Sheet2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header1Sheet2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header1Sheet2.Style.Alignment.WrapText = true;
                    header1Sheet2.Style.Font.Bold = true;
                    header1Sheet2.Merge();

                    var header2Sheet2 = sheet2.Range("G3:M3");
                    header2Sheet2.SetValue($"as at {_machineDateTime.ServerTime.Date.ToString("dd MMMM yyyy HH:mm")}");
                    header2Sheet2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header2Sheet2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header2Sheet2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header2Sheet2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header2Sheet2.Style.Font.FontName = "Calibri";
                    header2Sheet2.Style.Font.FontSize = 20;
                    header2Sheet2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header2Sheet2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header2Sheet2.Style.Alignment.WrapText = true;
                    header2Sheet2.Style.Font.Bold = true;
                    header2Sheet2.Merge();

                    var column1Table1Sheet2 = sheet2.Range("A6:A7").SetValue("No");
                    column1Table1Sheet2.Merge();
                    var column2Table1Sheet2 = sheet2.Range("B6:B7").SetValue("Student ID");
                    column2Table1Sheet2.Merge();
                    var column3Table1Sheet2 = sheet2.Range("C6:C7").SetValue("Student Name");
                    column3Table1Sheet2.Merge();
                    var column4Table1Sheet2 = sheet2.Range("D6:D7").SetValue("Class");
                    column4Table1Sheet2.Merge();
                    var column5Table1Sheet2 = sheet2.Range("E6:E7").SetValue("Tapping Time");
                    column5Table1Sheet2.Merge();
                    var column6Table1Sheet2 = sheet2.Range("F6:Q6").SetValue("Session");
                    column6Table1Sheet2.Merge();

                    var column7Table1Sheet2 = sheet2.Range("R6:R7").SetValue("Total Present");
                    column7Table1Sheet2.Merge();
                    var column8Table1Sheet2 = sheet2.Range("S6:S7").SetValue("Total Absent");
                    column8Table1Sheet2.Merge();
                    var column9Table1Sheet2 = sheet2.Range("T6:T7").SetValue("Total Late");
                    column9Table1Sheet2.Merge();

                    int startColumnSession = 6;
                    for (int i = 1; i <= 12; i++)
                    {
                        var session = sheet2.Cell(7, startColumnSession).Value = i;
                        startColumnSession++;
                    }

                    var header3Sheet2 = sheet2.Range("A6:T7");
                    header3Sheet2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header3Sheet2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header3Sheet2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header3Sheet2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header3Sheet2.Style.Font.FontName = "Calibri";
                    header3Sheet2.Style.Font.FontSize = 11;
                    header3Sheet2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header3Sheet2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header3Sheet2.Style.Alignment.WrapText = true;
                    header3Sheet2.Style.Font.Bold = true;

                    int rowTable1Sheet2 = 8;
                    int counterDataTable1Sheet2 = 1;
                    foreach (var t21 in table21)
                    {
                        sheet2.Cell($"A{rowTable1Sheet2}").Value = counterDataTable1Sheet2;
                        sheet2.Cell($"B{rowTable1Sheet2}").Value = t21.IdStudent;
                        sheet2.Cell($"C{rowTable1Sheet2}").Value = t21.nameStudent;
                        sheet2.Cell($"D{rowTable1Sheet2}").Value = t21.homeroom;
                        sheet2.Cell($"E{rowTable1Sheet2}").Value = t21.tappingTime;
                        sheet2.Cell($"F{rowTable1Sheet2}").Value = t21.Session1;
                        sheet2.Cell($"G{rowTable1Sheet2}").Value = t21.Session2;
                        sheet2.Cell($"H{rowTable1Sheet2}").Value = t21.Session3;
                        sheet2.Cell($"I{rowTable1Sheet2}").Value = t21.Session4;
                        sheet2.Cell($"J{rowTable1Sheet2}").Value = t21.Session5;
                        sheet2.Cell($"K{rowTable1Sheet2}").Value = t21.Session6;
                        sheet2.Cell($"L{rowTable1Sheet2}").Value = t21.Session7;
                        sheet2.Cell($"M{rowTable1Sheet2}").Value = t21.Session8;
                        sheet2.Cell($"N{rowTable1Sheet2}").Value = t21.Session9;
                        sheet2.Cell($"O{rowTable1Sheet2}").Value = t21.Session10;
                        sheet2.Cell($"P{rowTable1Sheet2}").Value = t21.Session11;
                        sheet2.Cell($"Q{rowTable1Sheet2}").Value = t21.Session12;
                        sheet2.Cell($"R{rowTable1Sheet2}").Value = t21.TotalPresent;
                        sheet2.Cell($"S{rowTable1Sheet2}").Value = t21.TotalAbsent;
                        sheet2.Cell($"T{rowTable1Sheet2}").Value = t21.TotalLate;
                        rowTable1Sheet2++;
                        counterDataTable1Sheet2++;
                    }
                    var rangeCellTable1Sheet2 = sheet2.Range($"A8:T{rowTable1Sheet2 - 1}");
                    rangeCellTable1Sheet2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet2.Style.Font.FontName = "Calibri";
                    rangeCellTable1Sheet2.Style.Font.FontSize = 11;
                    rangeCellTable1Sheet2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeCellTable1Sheet2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    #endregion
                    #region Sheet 3
                    if (!string.IsNullOrEmpty(school.Logo))
                    {
                        var imageSheet3 = sheet3.AddPicture(memoryLogo)
                            .MoveTo(sheet3.Cell("A1"))
                            .Scale(0.05d);
                    }
                    #region Table 1
                    var header1Sheet3 = sheet3.Range("C1:H2");
                    header1Sheet3.SetValue("List of Students with UA & Present - Details");
                    header1Sheet3.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header1Sheet3.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header1Sheet3.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header1Sheet3.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header1Sheet3.Style.Font.FontName = "Calibri";
                    header1Sheet3.Style.Font.FontSize = 20;
                    header1Sheet3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header1Sheet3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header1Sheet3.Style.Alignment.WrapText = true;
                    header1Sheet3.Style.Font.Bold = true;
                    header1Sheet3.Merge();

                    var header1Sheet31 = sheet3.Range("C3:H3");
                    header11.SetValue($"as at {_machineDateTime.ServerTime.Date.ToString("dd MMMM yyyy HH:mm")}");
                    header11.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header11.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header11.Merge();

                    var column1Table1Sheet3 = sheet3.Cell("A6").Value = "No";
                    var column2Table1Sheet3 = sheet3.Cell("B6").Value = "Student ID";
                    var column3Table1Sheet3 = sheet3.Cell("C6").Value = "Student Name";
                    var column4Table1Sheet3 = sheet3.Cell("D6").Value = "Class";
                    var column5Table1Sheet3 = sheet3.Cell("E6").Value = "Class ID";
                    var column6Table1Sheet3 = sheet3.Cell("F6").Value = "Session";
                    var column7Table1Sheet3 = sheet3.Cell("G6").Value = "Status";
                    var column8Table1Sheet3 = sheet3.Cell("H6").Value = "Teacher";

                    int rowTable1Sheet3 = 7;
                    int counterDataTable1Sheet3 = 1;
                    foreach (var t31 in table31)
                    {
                        sheet3.Cell($"A{rowTable1Sheet3}").Value = counterDataTable1Sheet3;
                        sheet3.Cell($"B{rowTable1Sheet3}").Value = t31.idStudent;
                        sheet3.Cell($"C{rowTable1Sheet3}").Value = t31.nameStudent;
                        sheet3.Cell($"D{rowTable1Sheet3}").Value = t31.homeroom;
                        sheet3.Cell($"E{rowTable1Sheet3}").Value = t31.classId;
                        sheet3.Cell($"F{rowTable1Sheet3}").Value = t31.sesion;
                        sheet3.Cell($"G{rowTable1Sheet3}").Value = t31.attendance;
                        sheet3.Cell($"H{rowTable1Sheet3}").Value = t31.teacher;
                        rowTable1Sheet3++;
                        counterDataTable1Sheet3++;
                    }
                    var rangeCellTable1Sheet3 = sheet3.Range($"A7:H{rowTable1Sheet3 - 1}");
                    rangeCellTable1Sheet3.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet3.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet3.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet3.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeCellTable1Sheet3.Style.Font.FontName = "Calibri";
                    rangeCellTable1Sheet3.Style.Font.FontSize = 11;
                    rangeCellTable1Sheet3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeCellTable1Sheet3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    #endregion
                    #endregion
                    #region Sheet 4
                    if (!string.IsNullOrEmpty(school.Logo))
                    {
                        var imageSheet2 = sheet4.AddPicture(memoryLogo)
                            .MoveTo(sheet4.Cell("A1"))
                            .Scale(0.05d);
                    }
                    var header1Sheet4 = sheet4.Range("A1:J3");
                    header1Sheet4.SetValue("Summary");
                    header1Sheet4.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    header1Sheet4.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    header1Sheet4.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    header1Sheet4.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    header1Sheet4.Style.Font.FontName = "Calibri";
                    header1Sheet4.Style.Font.FontSize = 28;
                    header1Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header1Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header1Sheet4.Style.Alignment.WrapText = true;
                    header1Sheet4.Style.Font.Bold = true;
                    header1Sheet4.Merge();
                    #region Table 1
                    var column1Table1Sheet4 = sheet4.Cell("A6").Value = "Grade Level";
                    var column2Table1Sheet4 = sheet4.Cell("B6").Value = "Class";
                    var column3Table1Sheet4 = sheet4.Cell("C6").Value = "Student Name";
                    var column4Table1Sheet4 = sheet4.Cell("D6").Value = "Class ID";
                    var column5Table1Sheet4 = sheet4.Cell("E6").Value = "Session";

                    var rangeHeaderTable1Sheet4 = sheet4.Range("A6:E6");
                    rangeHeaderTable1Sheet4.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1Sheet4.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1Sheet4.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1Sheet4.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeHeaderTable1Sheet4.Style.Font.FontName = "Calibri";
                    rangeHeaderTable1Sheet4.Style.Font.FontSize = 11;
                    rangeHeaderTable1Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeHeaderTable1Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeHeaderTable1Sheet4.Style.Alignment.WrapText = true;
                    rangeHeaderTable1Sheet4.Style.Font.Bold = true;
                    rangeHeaderTable1Sheet4.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
                    #endregion
                    #region Table 2

                    var header2Sheet4 = sheet4.Range("G6:J6");
                    header2Sheet4.SetValue("Unexcused Absences");
                    header2Sheet4.Style.Font.FontName = "Calibri";
                    header2Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header2Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header2Sheet4.Style.Alignment.WrapText = true;
                    header2Sheet4.Merge();
                    var header3Sheet4 = sheet4.Range("G7:J7");
                    header3Sheet4.SetValue("Summary Data");
                    header3Sheet4.Style.Font.FontName = "Calibri";
                    header3Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header3Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header3Sheet4.Style.Alignment.WrapText = true;
                    header3Sheet4.Merge();

                    var header4Sheet4 = sheet4.Range("G8:J8");
                    header4Sheet4.SetValue($"{_machineDateTime.ServerTime.Date.ToString("dddd, dd-MMMM-yyyy HH:mm")}");
                    header4Sheet4.Style.Font.FontName = "Calibri";
                    header4Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    header4Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    header4Sheet4.Style.Alignment.WrapText = true;
                    header4Sheet4.Style.Font.Bold = true;
                    header4Sheet4.Merge();

                    var column1Table2Sheet4 = sheet4.Cell("G9").Value = "Grade Level";
                    var column2Table2Sheet4 = sheet4.Cell("H9").Value = "Total Sessions";
                    var column3Table2Sheet4 = sheet4.Cell("I9").Value = "";

                    int rowTable2Sheet4 = 10;
                    int counterDataTable2Sheet4 = 1;
                    foreach (var t42 in table42)
                    {
                        sheet4.Cell($"G{rowTable2Sheet4}").Value = t42.Grade;
                        sheet4.Cell($"H{rowTable2Sheet4}").Value = t42.TotalSessions;
                        sheet4.Cell($"I{rowTable2Sheet4}").Value = "Sessions";
                        rowTable2Sheet4++;
                        counterDataTable2Sheet4++;
                    }
                    var rangeCellTable2Sheet4 = sheet1.Range($"G10:I{rowTable2Sheet4}");
                    rangeCellTable2Sheet4.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2Sheet4.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2Sheet4.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2Sheet4.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rangeCellTable2Sheet4.Style.Font.FontName = "Calibri";
                    rangeCellTable2Sheet4.Style.Font.FontSize = 11;
                    rangeCellTable2Sheet4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeCellTable2Sheet4.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    var SummaryColumn2Table2Sheet4 = sheet4.Cell($"H{rowTable2Sheet4}").FormulaA1 = $"SUM(H10:H{rowTable2Sheet4 - 1})";
                    #endregion
                    #endregion
                    workbook.CalculateMode = XLCalculateMode.Auto;
                    sheet1.SheetView.FreezeRows(6);
                    sheet1.Columns().AdjustToContents();

                    //sheet2.Columns().AdjustToContents();
                    sheet2.SheetView.FreezeColumns(3);
                    sheet2.SheetView.FreezeRows(7);

                    sheet3.Columns().AdjustToContents();
                    sheet3.SheetView.FreezeRows(6);

                    sheet4.Columns().AdjustToContents();
                    sheet4.SheetView.FreezeRows(6);
                    ms.Position = 0;
                    ms.Flush();
                    workbook.SaveAs(ms);
                    resuleExcel2 = ms.ToArray();
                    System.IO.File.WriteAllBytes($@"C:\Work\BSSDEX\script-migration\test-freeze-{Guid.NewGuid()}.xlsx", resuleExcel2);
                }
                var compileSubject = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Email);
                _notificationData["period"] = "today";
                _notificationData["receiverName"] = $"{item.FirstName} {item.LastName}";
                _notificationData["time"] = _machineDateTime.ServerTime.Date.ToString("HH:mm");
                Stream stream = new MemoryStream(resuleExcel2);
                //FluentEmail.Core.Models.Attachment attachment = new FluentEmail.Core.Models.Attachment
                //{
                //    Filename = $"Summary Attendance at {_machineDateTime.ServerTime.Date.ToString("dd MMM yyyy HH:mm")}",
                //    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                //    Data = stream,
                //    IsInline = true,
                //};
                //var message = new EmailData
                //{
                //    Subject = compileSubject(_notificationData),
                //    Body = compileBody(_notificationData),
                //    IsHtml = NotificationTemplate.EmailIsHtml,
                //    ToAddresses = new[] { new Address(item.EmailAddress) }.ToList(),
                //    Attachments = new [] { attachment }
                //};
                //sendEmailTasks.Add(NotificationManager.SendSmtp(message));
                var message = new SendGridMessage
                {
                    Subject = compileSubject(_notificationData),
                    Personalizations = new List<Personalization>
                            {
                                new Personalization
                                {
                                    Tos = new List<EmailAddress>()
                                    {
                                        new EmailAddress
                                        {
                                            Email = item.EmailAddress
                                        }
                                    },
                                    Ccs = null
                                }
                            }
                };
                message.AddAttachment($"Summary Attendance at {_machineDateTime.ServerTime.Date.ToString("dd MMM yyyy HH:mm")}", Convert.ToBase64String(resuleExcel2), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attachment");
                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = compileBody(_notificationData);
                else
                    message.PlainTextContent = compileBody(_notificationData); ;
                sendEmailTasks.Add(NotificationManager.SendEmail(message));
                // send email
            }
            await Task.WhenAll(sendEmailTasks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
            var compileBody = Handlebars.Compile(NotificationTemplate.Push);
            //Summary Attendance for this {{period}} has been sent to your email
            _notificationData["period"] = "today";
            var emailTitle = compileTitle(_notificationData);

            foreach (var item in _userPositions)
            {
                var notification = CreateNotificationHistory(
                    idUserRecipients,
                    isBlast,
                    compileTitle(_notificationData),
                    compileBody(_notificationData));

                await NotificationManager.SaveNotification(notification);
            }
        }
    }
}
