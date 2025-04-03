using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingDateHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;

        public GetAppointmentBookingDateHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetAppointmentBookingDateRequest, GetAppointmentBookingDateValidator>();

            var Items = new List<ItemValueVm>();
            if (body.Role == RoleConstant.Teacher)
            {
                Items = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting && e.IdUserTeacher == body.IdUser)
                .Select(e => new ItemValueVm
                {
                    Id = e.DateInvitation.Date.ToString(),
                    Description = e.DateInvitation.Date.ToString("dd MMMM yyyy"),
                })
                .Distinct().ToListAsync(CancellationToken);
            }
            else if(body.Role == RoleConstant.Parent || body.Role == RoleConstant.Staff)
            {
                #region getStudent
                var DataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                           .Include(e => e.Student)
                           .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                           .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                           .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                           .Where(e => body.IdStudentHomerooms.Contains(e.Id))
                           .Select(e => new 
                           {
                               IdAcademicYear = e.Homeroom.AcademicYear.Id,
                               IdLevel = e.Homeroom.Grade.Level.Id,
                               IdGrade = e.Homeroom.Grade.Id,
                               IdHomeroom = e.IdHomeroom,
                               IdHomeroomStudent = e.Id
                           }).ToListAsync(CancellationToken);

                #endregion

                #region GetTeacher
                var InvitationBookingSettingVenue = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                .Include(e => e.InvitationBookingSettingVenueDate)
                .Where(e => e.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                .Select(e => e.IdUserTeacher)
                .ToListAsync(CancellationToken);

                List<ItemValueVm> UserTeacher = new List<ItemValueVm>();

                //HomeroomTeacher
                var HomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Staff)
                                .Where(e => DataStudent.Select(x => x.IdHomeroom).Contains(e.IdHomeroom) && InvitationBookingSettingVenue.Contains(e.IdBinusian))
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Staff.IdBinusian,
                                    Description = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName)
                                })
                                .ToListAsync(CancellationToken);
                UserTeacher.AddRange(HomeroomTeacher);

                //Subject Teacher

                var GetHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent)
                            .Where(e => DataStudent.Select(e => e.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent))
                           .ToListAsync(CancellationToken);

                var GetSubjetTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                .Include(e => e.Staff)
                                .Where(x => GetHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList().Contains(x.IdLesson) && InvitationBookingSettingVenue.Contains(x.IdUser))
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Staff.IdBinusian,
                                    Description = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName)
                                })
                                .ToListAsync(CancellationToken);
                UserTeacher.AddRange(GetSubjetTeacher);

                //NonTeachig
                var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                           .Include(e => e.MsNonTeachingLoad).ThenInclude(e=>e.TeacherPosition).ThenInclude(e=>e.Position)
                                           .Include(e=>e.User)
                                           .Where(x => DataStudent.Select(e => e.IdAcademicYear).ToList().Contains(x.MsNonTeachingLoad.IdAcademicYear))
                                           .ToListAsync(CancellationToken);

                List<GetUserGrade> idGrades = new List<GetUserGrade>();
                foreach (var itemHomeroomStudent in DataStudent.Select(e => new { IdGrade= e.IdGrade, IdLevel = e.IdLevel }).Distinct().ToList())
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
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToListAsync(CancellationToken));
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
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToListAsync(CancellationToken));
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
                                    idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToListAsync(CancellationToken));
                                }
                            }
                        }
                    }

                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList() != null)
                    {
                        var LevelHead = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Grade", out var _GradeLH);
                            if (_GradeLH.Id == itemHomeroomStudent.IdGrade)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                }).ToListAsync(CancellationToken));
                            }
                        }
                    }

                    if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.Principal)
                           && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.VicePrincipal)
                           && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.AffectiveCoordinator)
                           && getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.LevelHead)
                           )
                    {
                        var Staff = getPositionByUser
                                .Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.Principal
                                            && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.VicePrincipal
                                            && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.AffectiveCoordinator
                                            && x.MsNonTeachingLoad.TeacherPosition.Position.Code != PositionConstant.LevelHead
                                    ).ToList();
                        foreach (var item in Staff)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == itemHomeroomStudent.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                }).Distinct().ToListAsync(CancellationToken));
                        }
                    }
                }


                UserTeacher.AddRange(idGrades.Where(e => DataStudent.Select(e => e.IdGrade).ToList().Contains(e.IdGrade))
                    .Select(e => new ItemValueVm
                    {
                        Id = e.IdUser,
                        Description = e.Fullname
                    }).Distinct().ToList());

                if (!string.IsNullOrEmpty(body.IdUserTeacher))
                    UserTeacher = UserTeacher.Where(x => x.Id == body.IdUserTeacher).ToList();
                #endregion

                Items = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
               .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting & UserTeacher.Select(f=>f.Id).ToList().Contains(e.IdUserTeacher))
               .Select(e => new ItemValueVm
               {
                   Id = e.DateInvitation.Date.ToString(),
                   Description = e.DateInvitation.Date.ToString("dd MMMM yyyy"),

               })
               .Distinct().ToListAsync(CancellationToken);
            }

            return Request.CreateApiResult2(Items as object);
        }
    }
}
