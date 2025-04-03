using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetTeacherVenueMappingV2Handler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;

        public GetTeacherVenueMappingV2Handler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetTeacherVenueMappingRequest, GetTeacherVenueMappingValidator>();
            #region GetStudent
            var InvitationBookingSettingDetail = await _dbContext.Entity<TrInvitationBookingSettingDetail>()
                .Include(e => e.InvitationBookingSetting).ThenInclude(e => e.AcademicYears)
                .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                .ToListAsync(CancellationToken);

            var InvitationBookingSettingUser = await _dbContext.Entity<TrInvitationBookingSettingUser>()
                .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting && body.IdHomeroomStudents.Contains(e.IdHomeroomStudent))
                .ToListAsync(CancellationToken);

            var IdSchool = InvitationBookingSettingDetail.Select(e => e.InvitationBookingSetting.AcademicYears.IdSchool).FirstOrDefault();

            List<GetHomeroomStudent> HomeroomStudents = new List<GetHomeroomStudent>();

            // if (InvitationBookingSettingDetail.Any())
            // {
            //     foreach (var item in InvitationBookingSettingDetail)
            //     {
            //         if (item.IdLevel != null && item.IdGrade != null && item.IdHomeroom != null)
            //         {
            //             var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
            //                 .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
            //                 .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade && e.IdHomeroom == item.IdHomeroom)
            //                 .Select(e => new GetHomeroomStudent
            //                 {
            //                     IdHomeroomStudent = e.Id,
            //                     IdLevel = e.Homeroom.Grade.IdLevel,
            //                     IdGrade = e.Homeroom.IdGrade,
            //                     IdHomeroom = e.IdHomeroom,
            //                     IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
            //                     IdStudent = e.IdStudent,
            //                     FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
            //                 })
            //                 .ToListAsync(CancellationToken);

            //             HomeroomStudents.AddRange(HomeroomStudent);
            //         }

            //         if (item.IdLevel != null && item.IdGrade != null && item.IdHomeroom == null)
            //         {
            //             var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
            //                 .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
            //                 .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade)
            //                  .Select(e => new GetHomeroomStudent
            //                  {
            //                      IdHomeroomStudent = e.Id,
            //                      IdLevel = e.Homeroom.Grade.IdLevel,
            //                      IdGrade = e.Homeroom.IdGrade,
            //                      IdHomeroom = e.IdHomeroom,
            //                      IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
            //                      IdStudent = e.IdStudent,
            //                      FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
            //                  })
            //                 .ToListAsync(CancellationToken);

            //             HomeroomStudents.AddRange(HomeroomStudent);
            //         }

            //         if (item.IdLevel != null && item.IdGrade == null && item.IdHomeroom == null)
            //         {
            //             var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
            //                 .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
            //                 .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel)
            //                  .Select(e => new GetHomeroomStudent
            //                  {
            //                      IdHomeroomStudent = e.Id,
            //                      IdLevel = e.Homeroom.Grade.IdLevel,
            //                      IdGrade = e.Homeroom.IdGrade,
            //                      IdHomeroom = e.IdHomeroom,
            //                      IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
            //                      IdStudent = e.IdStudent,
            //                      FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
            //                  })
            //                 .ToListAsync(CancellationToken);

            //             HomeroomStudents.AddRange(HomeroomStudent);
            //         }
            //     }
            // }

            if (InvitationBookingSettingUser.Any())
            {
                var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Where(e => InvitationBookingSettingUser.Select(x => x.IdHomeroomStudent).ToList().Contains(e.Id))
                             .Select(e => new GetHomeroomStudent
                             {
                                 IdHomeroomStudent = e.Id,
                                 IdLevel = e.Homeroom.Grade.IdLevel,
                                 IdGrade = e.Homeroom.IdGrade,
                                 IdHomeroom = e.IdHomeroom,
                                 IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                 IdStudent = e.IdStudent,
                                 FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
                             })
                            .ToListAsync(CancellationToken);

                HomeroomStudents.AddRange(HomeroomStudent);
            }

            var GetHomeroomStudents = HomeroomStudents.Where(e => body.IdHomeroomStudents.Contains(e.IdHomeroomStudent)).ToList();
            #endregion

            #region GetTeacher
            var InvitationBookingSettingVenue = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
               .Include(e => e.InvitationBookingSettingVenueDate)
               .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
               .Where(e => e.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == body.IdInvitationBookingSetting && e.InvitationBookingSettingVenueDate.DateInvitation.Contains(body.InvitationDate.ToString("yyyy-MM-dd")))
               .ToListAsync(CancellationToken);

            List<GetTeacherVenueMapping> UserTeacher = new List<GetTeacherVenueMapping>();

            //HomeroomTeacher
            var GetIdTeacherHomeroom = InvitationBookingSettingVenue
                                        .Where(e => e.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor || e.TeacherPosition.Position.Code == PositionConstant.CoTeacher)
                                        .Select(e => new { IdTeacherPosition = e.IdTeacherPosition, IdBinusian = e.IdUserTeacher })
                                        .ToList();

            var HomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Include(e => e.Staff)
                            .Include(e => e.TeacherPosition)
                            .AsEnumerable()
                            .Where(e => GetHomeroomStudents.Select(x => x.IdHomeroom).Contains(e.IdHomeroom)
                            && (e.Homeroom.Grade.Level.AcademicYear.IdSchool != "3") && GetIdTeacherHomeroom.Select(x => x.IdBinusian).Contains(e.IdBinusian) && GetIdTeacherHomeroom.Select(x => x.IdTeacherPosition).Contains(e.IdTeacherPosition))
                            .Select(e => new GetTeacherVenueMapping
                            {
                                IdUser = e.Staff.IdBinusian,
                                FullName = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                IdTeacherPosition = e.IdTeacherPosition,
                                TeacherPositionName = e.TeacherPosition.Description,
                                Description = e.Homeroom.Grade.Description + e.Homeroom.GradePathwayClassroom.Classroom.Description
                            })
                            .Distinct().ToList();
            UserTeacher.AddRange(HomeroomTeacher);

            //Subject Teacher
            var GetIdTeacherPosition = InvitationBookingSettingVenue.Where(e => e.TeacherPosition.Position.Code == PositionConstant.SubjectTeacher).Select(e => e.IdTeacherPosition).Distinct().ToList();
            var listTeacherPositionBySubject = await _dbContext.Entity<MsTeacherPosition>()
                                                .Include(e => e.Position)
                                               .Where(e => e.Position.Code == PositionConstant.SubjectTeacher && e.IdSchool == IdSchool && GetIdTeacherPosition.Contains(e.Id))
                                            .ToListAsync(CancellationToken);

            var GetHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                        .Include(e => e.HomeroomStudent)
                        .Where(e => GetHomeroomStudents.Select(e => e.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent))
                        .Distinct()
                       .ToListAsync(CancellationToken);

            var listExcludeSubject = _dbContext.Entity<TrInvitationBookingSettingExcludeSub>()
                .Where(x => x.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                .Select(e => e.IdSubject).ToList();

            if (listTeacherPositionBySubject.Any())
            {
                var GetSubjetTeacher = await _dbContext.Entity<MsLessonTeacher>()
                            .Include(e => e.Staff)
                            .Include(e => e.Lesson).ThenInclude(e => e.Subject).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                            .Include(e => e.Lesson).ThenInclude(e => e.AcademicYear)
                            .Where(x => GetHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList().Contains(x.IdLesson)
                                        && InvitationBookingSettingVenue.Select(e => e.IdUserTeacher).ToList().Contains(x.IdUser)
                                        && !listExcludeSubject.Contains(x.Lesson.IdSubject))
                            .Select(e => new GetTeacherVenueMapping
                            {
                                IdUser = e.Staff.IdBinusian,
                                FullName = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                //IdTeacherPosition = GetTeacherPositionBySubject.Id,
                                //TeacherPositionName = GetTeacherPositionBySubject.Description,
                                Description = e.Lesson.Subject.Description
                            })
                            .Distinct().ToListAsync(CancellationToken);

                foreach(var itemPosition in listTeacherPositionBySubject)
                {
                    foreach (var itemSubjectTeacher in GetSubjetTeacher)
                    {
                        var item = new GetTeacherVenueMapping
                        {
                            IdUser = itemSubjectTeacher.IdUser,
                            FullName = itemSubjectTeacher.FullName,
                            IdTeacherPosition = itemPosition.Id,
                            TeacherPositionName = itemPosition.Description,
                            Description = itemSubjectTeacher.Description,
                        };

                        UserTeacher.Add(item);
                    }
                }
            }

            //NonTeachig
            var getPositionByUser = _dbContext.Entity<TrNonTeachingLoad>()
                                       .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                       .Include(e => e.User)
                                       .AsEnumerable()
                                       .Where(x => GetHomeroomStudents.Select(e => e.IdAcademicYear).ToList().Contains(x.MsNonTeachingLoad.IdAcademicYear)
                                                && InvitationBookingSettingVenue.Select(e => e.IdUserTeacher).ToList().Contains(x.IdUser)
                                                && InvitationBookingSettingVenue.Select(e => e.IdTeacherPosition).ToList().Contains(x.MsNonTeachingLoad.TeacherPosition.Id)
                                                && x.Data != null
                                                )
                                       .Distinct()
                                       .ToList();

            List<GetUserGrade> idGrades = new List<GetUserGrade>();
            foreach (var itemHomeroomStudent in GetHomeroomStudents.Select(e => new { e.IdGrade, e.IdLevel }).Distinct().ToList())
            {
                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).Count() > 0)
                    {
                        var Principal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in Principal)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == itemHomeroomStudent.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                    Idlevel = x.IdLevel,
                                    Level = x.Level.Description,
                                    Description = x.Level.Description
                                }).Distinct().ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).Count() > 0)
                    {
                        var VicePrincipal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in VicePrincipal)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == itemHomeroomStudent.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                    Idlevel = x.IdLevel,
                                    Level = x.Level.Description,
                                    Description = x.Level.Description
                                }).Distinct().ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).Count() > 0)
                    {
                        var AffectiveCoordinator = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in AffectiveCoordinator)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == itemHomeroomStudent.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                    Idlevel = x.IdLevel,
                                    Level = x.Level.Description,
                                    Description = x.Level.Description
                                }).Distinct().ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead))
                {
                    var LevelHead = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList();
                    foreach (var item in LevelHead)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Grade", out var _GradeLH);
                        if (_GradeLH.Id == itemHomeroomStudent.IdGrade)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                Idlevel = x.IdLevel,
                                Level = x.Level.Description,
                                Description = x.Description
                            }).Distinct().ToListAsync(CancellationToken));
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead))
                {
                    //var listSubject = await _dbContext.Entity<MsSubject>()
                    //    .Include(e => e.Grade)
                    //    .Where(e => e.Grade.Level.IdAcademicYear == GetHomeroomStudents.Select(x => x.IdAcademicYear).FirstOrDefault())
                    //    .ToListAsync(CancellationToken);

                    var SubjectHead = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead).ToList();
                    foreach (var item in SubjectHead)
                    {
                        var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewSH.TryGetValue("Grade", out var _GradeSH);
                        _dataNewSH.TryGetValue("Department", out var _DepartementSH);
                        _dataNewSH.TryGetValue("Subject", out var _SubjectSH);

                        if (_GradeSH.Id == itemHomeroomStudent.IdGrade && _DepartementSH != null && !listExcludeSubject.Contains(_SubjectSH.Id))
                        {
                            //if (!listSubject.Where(e => e.Id == _SubjectSH.Id && e.IdGrade == itemHomeroomStudent.IdGrade && e.IdDepartment == _DepartementSH.Id && e.Grade.IdLevel == itemHomeroomStudent.IdLevel).Any())
                            //    continue;

                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                Idlevel = x.IdLevel,
                                Level = x.Level.Description,
                                Description = _DepartementSH.Description,
                            }).Distinct().ToListAsync(CancellationToken));
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.Principal)
                    && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.VicePrincipal)
                    && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.AffectiveCoordinator)
                    && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.LevelHead)
                    && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.SubjectHead)
                    )
                {
                    var Staff = getPositionByUser
                            .Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.Principal
                                        && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.VicePrincipal
                                        && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.AffectiveCoordinator
                                        && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.LevelHead
                                        && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.SubjectHead
                                ).Distinct().ToList();
                    foreach (var item in Staff)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                        _dataNewLH.TryGetValue("Subject", out var _SubjectLH);

                        if (_levelLH != null && _gradeLH == null)
                        {
                            if (_SubjectLH != null)
                            {
                                if (_levelLH.Id == itemHomeroomStudent.IdLevel && !listExcludeSubject.Contains(_SubjectLH.Id))
                                {
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                        TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                        Idlevel = x.IdLevel,
                                        Level = x.Level.Description,
                                        Description = x.Level.Description
                                    }).Distinct().ToListAsync(CancellationToken));
                                }
                            }
                            else
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                    Idlevel = x.IdLevel,
                                    Level = x.Level.Description,
                                    Description = x.Level.Description
                                }).Distinct().ToListAsync(CancellationToken));
                            }
                        }

                        if (_gradeLH != null)
                        {
                            if (_SubjectLH != null)
                            {
                                if (_gradeLH.Id == itemHomeroomStudent.IdGrade && !listExcludeSubject.Contains(_SubjectLH.Id))
                                {
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                        TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                        Idlevel = x.IdLevel,
                                        Level = x.Level.Description,
                                        Description = x.Level.Description
                                    }).Distinct().ToListAsync(CancellationToken));
                                }
                            }
                            else
                            {
                                if (_gradeLH.Id == itemHomeroomStudent.IdGrade)
                                {
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Include(e => e.Level).Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                        TeacherPositionName = item.MsNonTeachingLoad.TeacherPosition.Description,
                                        Idlevel = x.IdLevel,
                                        Level = x.Level.Description,
                                        Description = x.Level.Description
                                    }).Distinct().ToListAsync(CancellationToken));
                                }
                            }
                        }

                    }
                }
            }

            UserTeacher.AddRange(idGrades.Where(e => GetHomeroomStudents.Select(e => e.IdGrade).ToList().Contains(e.IdGrade))
                .Select(e => new GetTeacherVenueMapping
                {
                    IdUser = e.IdUser,
                    FullName = e.Fullname,
                    IdTeacherPosition = e.IdTeacherPosition,
                    TeacherPositionName = e.TeacherPositionName,
                    Description = e.Description,

                }).Distinct().ToList());

            var NewUserTeacher = UserTeacher.Where(e => InvitationBookingSettingVenue.Select(e => e.IdUserTeacher).ToList().Contains(e.IdUser)).Distinct().ToList();

            var listIdRole = InvitationBookingSettingVenue.Select(e => e.IdRole).ToList();
            var listIdTeacherPosition = InvitationBookingSettingVenue.Select(e => e.IdTeacherPosition).ToList();
            var listIdUser = InvitationBookingSettingVenue.Select(e => e.IdUserTeacher).ToList();
            var GetUserRole = await _dbContext.Entity<MsUserRole>()
                                  .Where(x => listIdRole.Contains(x.IdRole)
                                            && InvitationBookingSettingVenue.Select(e => e.IdUserTeacher).ToList().Contains(x.IdUser))
                                  .Select(e => e.IdUser)
                                  .Distinct()
                                  .ToListAsync(CancellationToken);

            List<GetTeacherVenueMapping> listNewUserTeacher = new List<GetTeacherVenueMapping>();
            foreach (var item in NewUserTeacher)
            {
                var exisiInvitationBookingSettingVenue = InvitationBookingSettingVenue.Where(e => e.IdUserTeacher == item.IdUser && e.IdTeacherPosition == item.IdTeacherPosition).Any();

                if (exisiInvitationBookingSettingVenue)
                {
                    listNewUserTeacher.Add(item);
                    //if (!listNewUserTeacher.Where(e => e.IdTeacherPosition == item.IdTeacherPosition && e.IdUser == item.IdUser).Any())
                    //    listNewUserTeacher.Add(item);
                }
            }

            var items = GetTeacher(listNewUserTeacher);

            items = items.OrderBy(x => x.Description).ToList();

            //var json = JsonConvert.SerializeObject(items.OrderBy(a => a.Description).ToList(), Formatting.Indented);

            #endregion
            return Request.CreateApiResult2(items as object);
        }

        public static List<ItemValueVm> GetTeacher(List<GetTeacherVenueMapping> DataTeacher)
        {
            var GetIdUser = DataTeacher.Select(e => e.IdUser).Distinct().ToList();
            List<ItemValueVm> Items = new List<ItemValueVm>();
            foreach (var ItemTeacher in GetIdUser)
            {
                var GetDataTeacherByIdUser = DataTeacher.Where(e => e.IdUser == ItemTeacher).ToList();
                var DataTeacherByIdUser = GetDataTeacherByIdUser.FirstOrDefault();
                if (DataTeacherByIdUser == null)
                    continue;

                var GetDataTeacherByIdUserByIdTeacherPosition = GetDataTeacherByIdUser
                    .Where(e => e.IdTeacherPosition == DataTeacherByIdUser.IdTeacherPosition)
                    .Select(e => new
                    {
                        Description = e.Description
                    }).Distinct().ToList();

                var Description = DataTeacherByIdUser.FullName + " - " + DataTeacherByIdUser.TeacherPositionName;

                var index = 0;
                foreach (var ItemDetail in GetDataTeacherByIdUserByIdTeacherPosition)
                {
                    Description += index == 0 ? " (" + ItemDetail.Description : ", " + ItemDetail.Description;
                    index++;
                }

                Description += GetDataTeacherByIdUserByIdTeacherPosition.Any() ? ")" : "";

                Items.Add(new ItemValueVm
                {
                    Id = ItemTeacher,
                    Description = Description,
                });
            }

            return Items;
        }
    }
}
