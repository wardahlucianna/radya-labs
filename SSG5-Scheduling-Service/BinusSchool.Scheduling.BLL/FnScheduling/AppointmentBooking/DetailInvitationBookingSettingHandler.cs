using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class DetailInvitationBookingSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DetailInvitationBookingSettingHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        private CodeWithIdVm GetGrade(string IdGrade)
        {
            if(IdGrade == null)
                return null;

            var dataGrade = _dbContext.Entity<MsGrade>().Where(x => x.Id == IdGrade)
                            .Select(x => new CodeWithIdVm
                            {
                                Id = x.Id,
                                Code = x.Code,
                                Description = x.Description
                            })
                            .FirstOrDefault();

            return dataGrade;
        }

        private DataStudent GetDataStudent(string IdHomeroomStudent)
        {
            if (IdHomeroomStudent == null)
            {
                return null;
            }
            else
            {
                var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(x => x.Student)
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.Level)
                                    .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                    .Select(x => new DataStudent
                                    {
                                        Id = x.Id,
                                        Code = x.Student.Id,
                                        Description = x.Student.FirstName,
                                        FullName = x.Student.FirstName + " " + x.Student.LastName,
                                        Username = x.Student.Id,
                                        BinusianId = x.Student.Id,
                                        Level = x.Homeroom.Grade.Level.Description,
                                        Grade = x.Homeroom.Grade.Description,
                                        Homeroom = x.Homeroom.Grade.Code + x.Homeroom.GradePathwayClassroom.Classroom.Code
                                    })
                                    .Where(x => x.Id == IdHomeroomStudent)
                                    .FirstOrDefault();
                return dataStudent;
            }
        }

        private DataUserTeacher GetDataUserTeacher(string IdUser, string IdVenue , string IdRole, string IdTeacherPosition, string IdInvitationBookingSettingVenueDtl)
        {
            if (IdUser == null)
            {
                return null;
            }
            else
            {
                var dataRole = _dbContext.Entity<LtRole>();
                var dataTeacherPosition = _dbContext.Entity<MsTeacherPosition>();
                var dataUser = _dbContext.Entity<MsUser>()
                                    .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                                    .Include(x => x.NonTeachingLoads)
                                        .ThenInclude(x => x.MsNonTeachingLoad)
                                            .ThenInclude(x => x.TeacherPosition)
                                                .ThenInclude(x => x.Position)
                                    .Select(x => new DataUserTeacher
                                    {
                                        Id = x.Id,
                                        Code = x.Username,
                                        Description = x.DisplayName,
                                        IdRole = IdRole == null ? x.UserRoles.OrderByDescending(y => y.Role.Description).Select(y => y.Role.Id).FirstOrDefault() : IdRole,
                                        Role = IdRole == null ? x.UserRoles.OrderByDescending(y => y.Role.Description).Select(y => y.Role.Description).FirstOrDefault() != null ? x.UserRoles.OrderByDescending(y => y.Role.Description).Select(y => y.Role.Description).FirstOrDefault() : null : dataRole.Where(x => x.Id == IdRole).Select(x => x.Description).FirstOrDefault(),
                                        IdTeacherPosition = IdTeacherPosition == null ? x.NonTeachingLoads.Select(y => y.MsNonTeachingLoad.TeacherPosition.Position.Id).FirstOrDefault() : IdTeacherPosition,
                                        Position = IdTeacherPosition == null ? x.NonTeachingLoads.Select(y => y.MsNonTeachingLoad.TeacherPosition.Description).FirstOrDefault() != null ?x.NonTeachingLoads.Select(y => y.MsNonTeachingLoad.TeacherPosition.Description).FirstOrDefault() : "Subject Teacher" : dataTeacherPosition.Where(x => x.Id == IdTeacherPosition).Select(x => x.Description).FirstOrDefault(),
                                        Venue = GetDataVenue(IdVenue),
                                        IdInvitationBookingSettingVenueDtl = IdInvitationBookingSettingVenueDtl

                                    })
                                    .Where(x => x.Id == IdUser)
                                    .FirstOrDefault();
                return dataUser;
            }
        }

        private CodeWithIdVm GetDataVenue(string IdVenue)
        {
            if (IdVenue == null)
            {
                return null;
            }
            else
            {
                var dataVenue = _dbContext.Entity<MsVenue>()
                                    .Select(x => new CodeWithIdVm
                                    {
                                        Id = x.Id,
                                        Code = x.Code,
                                        Description = x.Description
                                    })
                                    .Where(x => x.Id == IdVenue)
                                    .FirstOrDefault();

                return dataVenue;
            }
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailInvitationBookingSettingRequest>(nameof(DetailInvitationBookingSettingRequest.IdInvitationBookingSetting));
            var invitationBookingSetting = await GetDetailInvitationBookingSetting(param);

            return Request.CreateApiResult2(invitationBookingSetting as object);
        }

        public async Task<DetailInvitationBookingSettingResult> GetDetailInvitationBookingSetting(DetailInvitationBookingSettingRequest param)
        {
            var predicate = PredicateBuilder.Create<TrInvitationBookingSetting>(x => true);

            predicate = predicate.And(x => x.Id == param.IdInvitationBookingSetting);

            var data = await _dbContext.Entity<TrInvitationBookingSetting>()
                .Include(x => x.AcademicYears)
                .Include(x => x.InvitationBookingSettingDetails)
                    .ThenInclude(x => x.Grade)
                .Include(x => x.InvitationBookingSettingDetails)
                    .ThenInclude(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                .Include(x => x.InvitationBookingSettingDetails)
                    .ThenInclude(x => x.Homeroom)
                        .ThenInclude(e => e.GradePathwayClassroom)
                            .ThenInclude(e => e.Classroom)
                // .ThenInclude(x => x.GradePathways)
                //     .ThenInclude(x => x.MsGradePathwayClassrooms)
                //         .ThenInclude(x => x.Classroom)
                // .Include(x => x.InvitationBookingSettingUsers)
                .Include(x => x.InvitationBookingSettingVenueDates)
                    .ThenInclude(x => x.InvitationBookingSettingVenueDtl)
                .Include(x => x.InvitationBookingSettingQuotas)
                .Include(x => x.InvBookingSettingRoleParticipants)
                    .ThenInclude(x => x.TeacherPosition)
                .Include(x => x.InvBookingSettingExcludeSub)
                    .ThenInclude(x => x.Subject)
                .Where(predicate)
                .FirstOrDefaultAsync(CancellationToken);

            var dataSettingUsers = await _dbContext.Entity<TrInvitationBookingSettingUser>().Where(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting).ToListAsync(CancellationToken);

            if(data == null)
                throw new BadRequestException($"Invitation booking setting not found");

            var dataRole = _dbContext.Entity<LtRole>();
            var dataGrade = _dbContext.Entity<MsGrade>();
            
            var invitationBookingSettings = new DetailInvitationBookingSettingResult
                {
                    StepWizard = data.StepWizard,
                    GeneralInfoDetail = new GeneralInfoDetail
                    {
                        AcademicYear = new CodeWithIdVm(data.AcademicYears.Id,data.AcademicYears.Code,data.AcademicYears.Description),
                        InvitationName = data.InvitationName,
                        InvitationStartDate = data.InvitationStartDate,
                        InvitationEndDate = data.InvitationEndDate,
                        InvitationType = data.InvitationType,
                        ParentBookingStartDate = data.ParentBookingStartDate,
                        ParentBookingEndDate = data.ParentBookingEndDate,
                        StaffBookingStartDate = data.StaffBookingStartDate != null ? data.StaffBookingStartDate : null,
                        StaffBookingEndDate = data.StaffBookingEndDate != null ? data.StaffBookingEndDate : null,
                        SchedulingSiblingSameTime = data.SchedulingSiblingSameTime,
                        FootNote = data.FootNote,
                        Grade = data.InvitationBookingSettingDetails != null ? data.InvitationBookingSettingDetails.Select(x => new CodeWithIdVm
                        {
                            Id = x.IdGrade,
                            Code = x.Grade.Code,
                            Description = x.Grade.Description
                        }).ToList() : null,
                        Homeroom = data.InvitationBookingSettingDetails != null ? data.InvitationBookingSettingDetails.Select(x => new CodeWithIdVm
                        {
                            Id = x.IdHomeroom,
                            Code = x.IdHomeroom,
                            Description = x.IdHomeroom != null ?  $"{x.Homeroom.Grade.Description.Trim()} ({x.Homeroom.Grade.Code.Trim()}{x.Homeroom.GradePathwayClassroom.Classroom.Code.Trim()})" : null
                        }).ToList()
                        : null,
                        PersonalStudent = data.InvitationType == InvitationType.Personal? dataSettingUsers != null ? dataSettingUsers.Select(x => new DataStudent
                        {
                            Id = x.Id,
                            Code = GetDataStudent(x.IdHomeroomStudent).Code,
                            Description = GetDataStudent(x.IdHomeroomStudent).Description,
                            FullName = GetDataStudent(x.IdHomeroomStudent).FullName,
                            Username= GetDataStudent(x.IdHomeroomStudent).Username,
                            BinusianId = GetDataStudent(x.IdHomeroomStudent).BinusianId,
                            Level = GetDataStudent(x.IdHomeroomStudent).Level,
                            Grade  = GetDataStudent(x.IdHomeroomStudent).Grade,
                            Homeroom = GetDataStudent(x.IdHomeroomStudent).Homeroom,
                        }).ToList() : null : null,
                        RoleParticipants = data.InvBookingSettingRoleParticipants.Select(x => new DetailRoleParticipant
                        {
                            Role = new ItemValueVm(
                                        x.IdRole != null ? x.IdRole : "ALL"
                                        ,x.IdRole != null ? dataRole.Where(e => e.Id == x.IdRole).Select(e => e.Description).First() : "ALL"
                                        ),
                            TeacherPosition = new DetailTeacherPosition{
                                Id = x.IdTeacherPosition != null ? x.TeacherPosition.Id : "ALL",
                                Code = x.IdTeacherPosition != null ? x.TeacherPosition.Code : "ALL",
                                Description = x.IdTeacherPosition != null ? x.TeacherPosition.Description : "ALL",
                                IdPosition = x.IdTeacherPosition != null ? x.TeacherPosition.IdPosition : "ALL",
                                IdTeacherPosition = x.IdTeacherPosition != null ? x.TeacherPosition.Id : "ALL"
                            }
                        }).ToList(),
                        ExcludeSubjects = data.InvBookingSettingExcludeSub.GroupBy(x => x.IdGrade).Select(x => new DetailExcludeSubject
                        {
                            Grade = new ItemValueVm(x.Key, dataGrade.Where(e => e.Id == x.Key).Select(e => e.Description).First()),
                            Subject = x.Select(e => new ItemValueVm(e.IdSubject, e.Subject.Description)).ToList()
                        }).ToList()
                    },
                    UserVenueMappingDetail = data.InvitationBookingSettingVenueDates != null ? data.InvitationBookingSettingVenueDates.Select(uvm => new UserVenueMappingDetail
                    {
                        IdInvitationBookingSettingVenueDate = uvm.Id,
                        IdInvitationBookingSetting = uvm.IdInvitationBookingSetting,
                        DateInvitationExact = uvm.DateInvitationExact,
                        InvitationDate = uvm.DateInvitation.Split("||").ToList(),
                        UserTeacher = uvm.InvitationBookingSettingVenueDtl
                        .Select(
                            y => GetDataUserTeacher(y.IdUserTeacher,y.IdVenue,y.IdRole,y.IdTeacherPosition, y.Id)
                        ).ToList()
                        // Venue = GetDataVenue(uvm.InvitationBookingSettingVenueDtl.Select(y => y.IdVenue).FirstOrDefault())
                    }).ToList() : null,
                    SettingQuotaDurationDetail = data.InvitationBookingSettingQuotas != null ? data.InvitationBookingSettingQuotas.Select(z => new SettingQuotaDurationDetail
                    {
                        IdInvitationBookingSettingQuota = z.Id,
                        SettingType = z.SettingType,
                        Grade = z.IdGrade != null ? GetGrade(z.IdGrade) : null,
                        DateInvitation = z.DateInvitation,
                        StartTime = z.StartTime,
                        EndTime = z.EndTime,
                        BreakBetweenSession = z.BreakBetweenSession != null ? z.BreakBetweenSession : null,
                        QuotaSlot = z.QuotaSlot,
                        Duration = z.Duration
                    }).ToList() : null
                };

            return invitationBookingSettings;
        }
    }
}
