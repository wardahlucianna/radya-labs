//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BinusSchool.Common.Constants;
//using BinusSchool.Common.Exceptions;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Handler;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Common.Utils;
//using BinusSchool.Data.Abstractions;
//using BinusSchool.Data.Api.Extensions;
//using BinusSchool.Data.Api.User.FnUser;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
//using BinusSchool.Domain.Extensions;
//using BinusSchool.Persistence.Extensions;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class GetCalendarEvent2DetailHandler : FunctionsHttpSingleHandler
//    {
//        private readonly ISchedulingDbContext _dbContext;
//        private readonly IApiService<IRole> _roleService;

//        public GetCalendarEvent2DetailHandler(ISchedulingDbContext dbContext, IApiService<IRole> roleService)
//        {
//            _dbContext = dbContext;
//            _roleService = roleService;
//        }

//        protected override async Task<ApiErrorResult<object>> Handler()
//        {
//            if (!KeyValues.TryGetValue("id", out var id))
//                throw new ArgumentNullException(nameof(id));

//            var result = await _dbContext.Entity<MsEvent>()
//                .Where(x => x.Id == (string)id)
//                .Select(x => new GetCalendarEvent2DetailResult
//                {
//                    Id = x.Id,
//                    Name = x.Name,
//                    Dates = x.EventDetails.OrderBy(y => y.StartDate).Select(y => new DateTimeRange
//                    {
//                        Start = y.StartDate,
//                        End = y.EndDate
//                    }),
//                    // EventType = new CalendarEventTypeVm
//                    // {
//                    //     Id = x.IdEventType,
//                    //     Code = x.EventType.Code,
//                    //     Description = x.EventType.Description,
//                    //     Color = x.EventType.Color
//                    // },
//                    Audit = x.GetRawAuditResult2(),
//                    Role = new CodeWithIdVm(null, x.EventIntendedFor.IntendedFor),
//                    Option = x.EventIntendedFor.Option,
//                    Acadyear = new CodeWithIdVm(x.IdAcademicYear, x.AcademicYear.Code, x.AcademicYear.Description),
//                    Code = x.IsShowOnCalendarAcademic.ToString() // store temporary show calendar academic
//                })
//                .FirstOrDefaultAsync(CancellationToken);

//            if (result is null)
//                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", id));

//            // get intended for
//            if (result.Role.Code == RoleConstant.Teacher)
//            {
//                var forTeacher = await _dbContext.Entity<MsEventIntendedFor>()
//                    .If(result.Option == EventOptionType.Grade, x => x
//                        .Include(y => y.EventIntendedForGrades).ThenInclude(y => y.Grade))
//                    .If(result.Option == EventOptionType.Department, x => x
//                        .Include(y => y.EventIntendedForDepartments).ThenInclude(y => y.Department))
//                    .If(result.Option == EventOptionType.Subject, x => x
//                        .Include(y => y.EventIntendedForGradeSubjects).ThenInclude(y => y.EventIntendedForSubjects).ThenInclude(y => y.Subject)
//                        .Include(y => y.EventIntendedForGradeSubjects).ThenInclude(y => y.Grade).ThenInclude(y => y.Level))
//                    .FirstOrDefaultAsync(x => x.Id == result.Id, CancellationToken);

//                result.ForTeacher = new CalendarEvent2DetailForTeacher();
//                if (result.Option == EventOptionType.Grade)
//                {
//                    result.ForTeacher.Grades = forTeacher.EventIntendedForGrades
//                        .Select(x => new CodeWithIdVm(x.IdGrade, x.Grade.Code, x.Grade.Description));
//                }
//                else if (result.Option == EventOptionType.Department)
//                {
//                    result.ForTeacher.Departments = forTeacher.EventIntendedForDepartments
//                        .Select(x => new CodeWithIdVm(x.IdDepartment, x.Department.Code, x.Department.Description));
//                }
//                else if (result.Option == EventOptionType.Subject)
//                {
//                    result.ForTeacher.Subjects = forTeacher.EventIntendedForGradeSubjects
//                        .Select(x => new CalendarEvent2DetailSubject
//                        {
//                            Level = new CodeWithIdVm(x.Grade.IdLevel, x.Grade.Level.Code, x.Grade.Level.Description),
//                            Grade = new CodeWithIdVm(x.IdGrade, x.Grade.Code, x.Grade.Description),
//                            Subjects = x.EventIntendedForSubjects
//                                .Select(y => new CodeWithIdVm(y.IdSubject, y.Subject.Code, y.Subject.Description))
//                        });
//                }
//            }
//            else if (result.Role.Code == RoleConstant.Student)
//            {
//                var forStudent = await _dbContext.Entity<MsEventIntendedFor>()
//                    .If(result.Option == EventOptionType.Personal, x => x
//                        .Include(y => y.EventIntendedForPersonalStudents).ThenInclude(y => y.Student))
//                    .If(result.Option == EventOptionType.Subject, x => x
//                        .Include(y => y.EventIntendedForSubjectStudents).ThenInclude(y => y.Subject).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
//                        .Include(y => y.EventIntendedForSubjectStudents).ThenInclude(y => y.Subject).ThenInclude(y => y.HomeroomStudentEnrollments)
//                            .ThenInclude(y => y.HomeroomStudent).ThenInclude(y => y.Homeroom).ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom))
//                    .If(result.Option == EventOptionType.Grade, x => x
//                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom).ThenInclude(y => y.Grade).ThenInclude(y => y.Level)
//                        .Include(y => y.EventIntendedForGradeStudents).ThenInclude(y => y.Homeroom)
//                            .ThenInclude(y => y.GradePathwayClassroom).ThenInclude(y => y.Classroom))
//                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents).ThenInclude(x=> x.User)
//                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
//                    .FirstOrDefaultAsync(x => x.Id == result.Id, CancellationToken);

//                result.ForStudent = new CalendarEvent2DetailForStudent();
//                if (result.Option == EventOptionType.Personal)
//                {
//                    result.ForStudent.Students = forStudent.EventIntendedForPersonalStudents
//                        .Select(x => new NameValueVm
//                        {
//                            Id = x.IdStudent,
//                            Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
//                        });
//                }
//                else if (result.Option == EventOptionType.Subject)
//                {
//                    result.ForStudent.Subjects = forStudent.EventIntendedForSubjectStudents
//                        .GroupBy(x => x.Subject.Grade)
//                        .Select(x => new CalendarEvent2DetailSubject
//                        {
//                            Level = new CodeWithIdVm(x.Key.IdLevel, x.Key.Level.Code, x.Key.Level.Description),
//                            Grade = new CodeWithIdVm(x.Key.Id, x.Key.Code, x.Key.Description),
//                            Homeroom = new CalendarEvent2DetailHomeroom
//                            {
//                                Id = x.First().Subject.HomeroomStudentEnrollments.First().HomeroomStudent.IdHomeroom,
//                                Code = x.First().Subject.HomeroomStudentEnrollments.First().HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
//                                Description = x.First().Subject.HomeroomStudentEnrollments.First().HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
//                                Semester = x.First().Subject.HomeroomStudentEnrollments.First().HomeroomStudent.Homeroom.Semester
//                            },
//                            Subjects = x.Select(y => new CodeWithIdVm(y.IdSubject, y.Subject.Code, y.Subject.Description))
//                        });
//                }
//                else if (result.Option == EventOptionType.Grade)
//                {
//                    result.ForStudent.Grades = forStudent.EventIntendedForGradeStudents
//                        .GroupBy(x => x.Homeroom.IdGrade)
//                        .Select(x => new CalendarEvent2DetailForStudent.CalendarEvent2DetailForStudentGrade
//                        {
//                            Id = x.Key,
//                            Code = x.First().Homeroom.Grade.Code,
//                            Description = x.First().Homeroom.Grade.Description,
//                            Level = x.Select(y => new CodeWithIdVm
//                            {
//                                Id = y.Homeroom.Grade.Level.Id,
//                                Code = y.Homeroom.Grade.Level.Code,
//                                Description = y.Homeroom.Grade.Level.Description
//                            }).First(),
//                            Homerooms = x.Select(y => new CalendarEvent2DetailHomeroom
//                            {
//                                Id = y.IdHomeroom,
//                                Code = y.Homeroom.GradePathwayClassroom.Classroom.Code,
//                                Description = y.Homeroom.GradePathwayClassroom.Classroom.Description,
//                                Semester = y.Homeroom.Semester
//                            })
//                        });
//                }

//                var attStudent = forStudent.EventIntendedForAttendanceStudents.First();
//                result.ForStudent.AttendanceOption = attStudent.Type;
//                result.ForStudent.SetAttendanceEntry = attStudent.IsSetAttendance;
                
//                // use temporary show calendar academic
//                result.ForStudent.ShowOnCalendarAcademic = bool.Parse(result.Code);

//                if (result.ForStudent.SetAttendanceEntry)
//                {
//                    var attPic = attStudent.EventIntendedForAtdPICStudents.First();
//                    result.ForStudent.PicAttendance = attPic.Type;

//                    if (new[] { EventIntendedForAttendancePICStudent.UserStaff, EventIntendedForAttendancePICStudent.UserTeacher }.Contains(attPic.Type))
//                        result.ForStudent.UserPic = new NameValueVm(attPic.IdUser, attPic.User.DisplayName);

//                    result.ForStudent.RepeatAttendanceCheck = attStudent.IsRepeat;
//                    result.ForStudent.AttendanceCheckDates = attStudent.EventIntendedForAtdCheckStudents
//                        //.GroupBy(x => (x.StartDate, x.EndDate))
//                        .GroupBy(x => (x.StartDate))
//                        .OrderBy(x => x.Key)
//                        .Select(x => new CalendarEvent2ForStudentAttendanceDate
//                        {
//                            //Date = new DateTimeRange(x.Key.StartDate, x.Key.EndDate),
//                            StartDate = x.Key.Date,
//                            AttendanceChecks = x.OrderBy(y => y.Time).Select(y => new CalendarEvent2ForStudentAttendanceCheck
//                            {
//                                Name = y.CheckName,
//                                TimeInMinute = (int)y.Time.TotalMinutes,
//                                IsMandatory = y.IsPrimary
//                            })
//                        });
//                }
//            }

//            // fill role
//            FillConfiguration();
//            var roleResult = await _roleService
//                .SetConfigurationFrom(ApiConfiguration)
//                .Execute.GetRoleGroupByCode(new IdCollection
//                {
//                    Ids = new[] { result.Role.Code }
//                });
//            result.Role.Id = roleResult.Payload?.FirstOrDefault()?.Id;
//            result.Role.Description = roleResult.Payload?.FirstOrDefault()?.Description;

//            // reset temporary show calendar academic
//            result.Code = null;

//            return Request.CreateApiResult2(result as object);
//        }
//    }
//}
