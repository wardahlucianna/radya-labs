using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
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
    public class GetUserForUserVenueHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;

        public GetUserForUserVenueHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        private CodeWithIdVm GetUserRole(string IdUser)
        {
            if(IdUser == null)
                return null;

            var dataUserRole = _dbContext.Entity<MsUserRole>()
                            .Include(x => x.Role)
                            .Where(x => x.IdUser == IdUser)
                            .Select(x => new CodeWithIdVm
                            {
                                Id = x.IdRole,
                                Code = x.Role.Code,
                                Description = x.Role.Description
                            })
                            .FirstOrDefault();

            return dataUserRole;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserForUserVenueRequest>(nameof(GetUserForUserVenueRequest.IdInvitationBookingSetting));

            var InvitationBookingSettingDetail = await _dbContext.Entity<TrInvitationBookingSettingDetail>()
                          .Where(e => e.IdInvitationBookingSetting == param.IdInvitationBookingSetting)
                          .ToListAsync(CancellationToken);

            var InvitationBookingSettingUser = await _dbContext.Entity<TrInvitationBookingSettingUser>()
                .Where(e => e.IdInvitationBookingSetting == param.IdInvitationBookingSetting)
                .ToListAsync(CancellationToken);

            List<GetHomeroomStudent> HomeroomStudents = new List<GetHomeroomStudent>();

            if (InvitationBookingSettingDetail.Any())
            {
                foreach (var item in InvitationBookingSettingDetail)
                {
                    if (item.IdLevel != null && item.IdGrade != null && item.IdHomeroom != null)
                    {
                        var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade && e.IdHomeroom == item.IdHomeroom)
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

                    if (item.IdLevel != null && item.IdGrade != null && item.IdHomeroom == null)
                    {
                        var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade)
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

                    if (item.IdLevel != null && item.IdGrade == null && item.IdHomeroom == null)
                    {
                        var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel)
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
                }
            }

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

            var Items = new List<ItemValueVm>();
                #region getStudent
                var DataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                           .Include(e => e.Student)
                           .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                           .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                           .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                           .Where(e => HomeroomStudents.Select(y => y.IdHomeroomStudent).ToList().Contains(e.Id))
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
                // var InvitationBookingSettingVenue = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                // .Include(e => e.InvitationBookingSettingVenueDate)
                // .Where(e => e.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == param.IdInvitationBookingSetting)
                // .Select(e => e.IdUserTeacher)
                // .ToListAsync(CancellationToken);

                List<GetUserForUserVenueResult> UserTeacher = new List<GetUserForUserVenueResult>();

                //HomeroomTeacher
                var HomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Staff)
                                .Where(e => DataStudent.Select(x => x.IdHomeroom).Contains(e.IdHomeroom))
                                .Select(e => new GetUserForUserVenueResult
                                {
                                    Id = e.Staff.IdBinusian,
                                    Description = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                    BinusianID = e.Staff.IdBinusian,
                                    Fullname = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                    Username = e.Staff.IdBinusian,
                                    Role = e.Staff.IdBinusian,
                                    Position = e.TeacherPosition.Description,
                                    CodePosition = e.TeacherPosition.Position.Id,
                                    IdUser = e.IdBinusian,
                                })
                                .Distinct().ToListAsync(CancellationToken);
                UserTeacher.AddRange(HomeroomTeacher);

            //Subject Teacher

            var codeSubjectTeacher = await _dbContext.Entity<LtPosition>()
                           .Where(e => e.Description== "Subject Teacher")
                          .FirstOrDefaultAsync(CancellationToken);

            var GetHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent)
                            .Where(e => DataStudent.Select(e => e.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent))
                           .ToListAsync(CancellationToken);

                var GetSubjetTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                .Include(e => e.Staff)
                                .Where(x => GetHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList().Contains(x.IdLesson))
                                .Select(e => new GetUserForUserVenueResult
                                {
                                    Id = e.Staff.IdBinusian,
                                    Description = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                    BinusianID = e.Staff.IdBinusian,
                                    Fullname = e.Staff.FirstName + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName),
                                    Username = e.Staff.IdBinusian,
                                    Role = e.Staff.IdBinusian,
                                    Position = "Subject Teacher",
                                    CodePosition = codeSubjectTeacher.Id,
                                    IdUser = e.IdUser,
                                })
                                .Distinct().ToListAsync(CancellationToken);
                UserTeacher.AddRange(GetSubjetTeacher);

                //NonTeachig
                var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                           .Include(e => e.MsNonTeachingLoad).ThenInclude(e=>e.TeacherPosition).ThenInclude(e=>e.Position)
                                           .Include(e=>e.User)
                                           .Where(x => DataStudent.Select(e => e.IdAcademicYear).ToList().Contains(x.MsNonTeachingLoad.IdAcademicYear))
                                           .ToListAsync(CancellationToken);

                List<GetUserForUserVenueResult> idGrades = new List<GetUserForUserVenueResult>();
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
                                    .Select(x => new GetUserForUserVenueResult
                                    {
                                        Id = item.IdUser,
                                        IdGrade = x.Id,
                                        IdUser = item.User.Id,
                                        Description = item.User.DisplayName,
                                        BinusianID = item.IdUser,
                                        Fullname = item.User.DisplayName,
                                        Username = item.User.Username,
                                        Role = item.IdUser,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Position.Id,
                                        Position = item.MsNonTeachingLoad.TeacherPosition.Description
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
                                    .Select(x => new GetUserForUserVenueResult
                                    {
                                        Id = item.IdUser,
                                        IdGrade = x.Id,
                                        IdUser = item.User.Id,
                                        Description = item.User.DisplayName,
                                        BinusianID = item.IdUser,
                                        Fullname = item.User.DisplayName,
                                        Username = item.User.Username,
                                        Role = item.IdUser,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Position.Code,
                                        Position = item.MsNonTeachingLoad.TeacherPosition.Id
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
                                    .Select(x => new GetUserForUserVenueResult
                                    {
                                        Id = item.IdUser,
                                        IdGrade = x.Id,
                                        IdUser = item.User.Id,
                                        Description = item.User.DisplayName,
                                        BinusianID = item.IdUser,
                                        Fullname = item.User.DisplayName,
                                        Username = item.User.Username,
                                        Role = item.IdUser,
                                        CodePosition = item.MsNonTeachingLoad.TeacherPosition.Position.Code,
                                        Position = item.MsNonTeachingLoad.TeacherPosition.Id
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
                                .Select(x => new GetUserForUserVenueResult
                                {
                                    Id = item.IdUser,
                                    IdGrade = x.Id,
                                    IdUser = item.User.Id,
                                    Description = item.User.DisplayName,
                                    BinusianID = item.IdUser,
                                    Fullname = item.User.DisplayName,
                                    Username = item.User.Username,
                                    Role = item.IdUser,
                                    CodePosition = item.MsNonTeachingLoad.TeacherPosition.Position.Code,
                                    Position = item.MsNonTeachingLoad.TeacherPosition.Id
                                }).ToListAsync(CancellationToken));
                            }
                        }
                    }
                }


                UserTeacher.AddRange(idGrades.Where(e => DataStudent.Select(e => e.IdGrade).ToList().Contains(e.IdGrade))
                    .Select(e => new GetUserForUserVenueResult
                    {
                        Id = e.IdUser,
                        Description = e.Fullname,
                        BinusianID = e.IdUser,
                        Fullname = e.Fullname,
                        Username = e.Username,
                        Role = e.Role,
                        CodePosition = e.CodePosition,
                        Position = e.Position,
                        IdUser = e.IdUser
                    }).Distinct().ToList());
                #endregion

                var dataListUserTeacher = UserTeacher
                .Select(x => new GetUserForUserVenueResult
                {
                    Id = x.IdUser,
                    Description = x.Fullname,
                    BinusianID = x.IdUser,
                    Fullname = x.Fullname,
                    Username = x.Username,
                    IdRole = GetUserRole(x.Role) != null ? GetUserRole(x.Role).Id : null,
                    Role = GetUserRole(x.Role) != null ? GetUserRole(x.Role).Description : null,
                    CodePosition = x.CodePosition,
                    Position = x.Position,
                    IdUser = x.IdUser
                })
                .Distinct().ToList();

                if (!string.IsNullOrEmpty(param.Search))
                    dataListUserTeacher = dataListUserTeacher.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                
                if(!string.IsNullOrEmpty(param.IdRole))
                    dataListUserTeacher = dataListUserTeacher.Where(x => x.IdRole == param.IdRole).ToList();

                if(!string.IsNullOrEmpty(param.CodePosition))
                    dataListUserTeacher = dataListUserTeacher.Where(x => x.CodePosition == param.CodePosition).ToList();

                switch(param.OrderBy)
                {
                    case "fullname":
                        dataListUserTeacher = param.OrderType == OrderType.Desc 
                            ? dataListUserTeacher.OrderByDescending(x => x.Fullname).ToList()
                            : dataListUserTeacher.OrderBy(x => x.Fullname).ToList();
                        break;
                    case "binusianid":
                        dataListUserTeacher = param.OrderType == OrderType.Desc 
                            ? dataListUserTeacher.OrderByDescending(x => x.BinusianID).ToList()
                            : dataListUserTeacher.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataListUserTeacher = param.OrderType == OrderType.Desc 
                            ? dataListUserTeacher.OrderByDescending(x => x.Username).ToList()
                            : dataListUserTeacher.OrderBy(x => x.Username).ToList();
                        break;
                };

                var dataUserPagination = dataListUserTeacher
                .SetPagination(param)
                .Select(x => new GetUserForUserVenueResult
                {
                    Id = x.IdUser,
                    Description = x.Fullname,
                    BinusianID = x.IdUser,
                    Fullname = x.Fullname,
                    Username = x.Username,
                    IdRole = x.IdRole,
                    Role = x.Role,
                    CodePosition = x.CodePosition,
                    Position = x.Position,
                    IdUser = x.IdUser
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                ? dataUserPagination.Count 
                : dataListUserTeacher.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
        }
    }
}
