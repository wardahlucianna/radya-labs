using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingByUserHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAppointmentBookingByUserHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime DateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = DateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAppointmentBookingByUserRequest>();

            List<GetAppointmentBookingByUserResult> Items = new List<GetAppointmentBookingByUserResult>();
            if (param.Role == RoleConstant.Parent)
            {
                var dataUser = await _dbContext.Entity<MsUser>().Where(x => x.Id == param.IdUser).Select(x => new { x.Username, x.DisplayName }).FirstOrDefaultAsync(CancellationToken);
                var idStudent = dataUser.Username.Substring(1);

                var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                           .Where(x => x.IdStudent == idStudent)
                                           .FirstOrDefaultAsync(CancellationToken);

                var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(x => x.IdStudent == idStudent).Select(x => x.Id).FirstOrDefaultAsync(CancellationToken);
                var childrens = new List<GetChildResult>();
                if (sibligGroup != null)
                {
                    var siblingStudent = await _dbContext.Entity<MsSiblingGroup>().Where(x => x.Id == sibligGroup).Select(x => x.IdStudent).ToListAsync(CancellationToken);
                    childrens = await _dbContext.Entity<MsStudent>()
                                    .Where(x => siblingStudent.Any(y => y == x.Id))
                                    .Select(x => new GetChildResult
                                    {
                                        Id = x.Id,
                                        Name = x.FirstName,
                                        Order = 2,
                                        Role = RoleConstant.Student,
                                        IdSchool = x.IdSchool,
                                    }).ToListAsync(CancellationToken);
                }
                else
                {
                    childrens = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Student)
                                .Where(x => x.IdParent == dataStudentParent.IdParent)
                                .Select(x => new GetChildResult
                                {
                                    Id = x.Student.Id,
                                    Name = x.Student.FirstName,
                                    Order = 2,
                                    Role = RoleConstant.Student,
                                    IdSchool = x.Student.IdSchool,
                                }).ToListAsync(CancellationToken);
                }

                List<GetAllChildDateilByParentResult> ListChild = new List<GetAllChildDateilByParentResult>();
                foreach (var item in childrens)
                {
                    var GetSemeterAy = await _dbContext.Entity<MsPeriod>()
                           .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                           .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.Level.AcademicYear.IdSchool == item.IdSchool)
                           .Select(e => new { IdAcademicYear = e.Grade.Level.IdAcademicYear })
                           .Distinct().FirstOrDefaultAsync(CancellationToken);

                    var result = await _dbContext.Entity<MsHomeroomStudent>()
                           .Include(e => e.Student)
                           .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                           .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                           .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                           .Where(e => e.IdStudent == item.Id
                           && e.Homeroom.AcademicYear.Id == GetSemeterAy.IdAcademicYear
                           )
                           .Select(e => new GetAllChildDateilByParentResult
                           {
                               StudentName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                               IdBinusan = e.Student.Id,
                               AcademicYear = new ItemValueVm
                               {
                                   Id = e.Homeroom.AcademicYear.Id,
                                   Description = e.Homeroom.AcademicYear.Description
                               },
                               Level = new ItemValueVm
                               {
                                   Id = e.Homeroom.Grade.Level.Id,
                                   Description = e.Homeroom.Grade.Level.Description
                               },
                               Grade = new ItemValueVm
                               {
                                   Id = e.Homeroom.Grade.Id,
                                   Description = e.Homeroom.Grade.Description
                               },
                               Semester = e.Homeroom.Semester,
                               Homeroom = new ItemValueVm
                               {
                                   Id = e.Homeroom.Id,
                                   Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                               },
                               IdHomeroomStudent = e.Id
                           }).ToListAsync(CancellationToken);

                    ListChild.AddRange(result);
                }

                var InvitationBookingSettingDetail = await _dbContext.Entity<TrInvitationBookingSettingDetail>()
                    .Include(e => e.InvitationBookingSetting)
                .Where(e => e.InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear && (_dateTime.ServerTime >= e.InvitationBookingSetting.ParentBookingStartDate && _dateTime.ServerTime <= e.InvitationBookingSetting.ParentBookingEndDate))

                .ToListAsync(CancellationToken);

                var InvitationBookingSettingUser = await _dbContext.Entity<TrInvitationBookingSettingUser>()
                   .Include(e => e.InvitationBookingSetting)
                    .Where(e => e.InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear && (_dateTime.ServerTime >= e.InvitationBookingSetting.ParentBookingStartDate && _dateTime.ServerTime <= e.InvitationBookingSetting.ParentBookingEndDate))
                    .ToListAsync(CancellationToken);

                if (InvitationBookingSettingDetail.Any())
                {
                    if (InvitationBookingSettingDetail.Where(e => e.IdLevel != null && e.IdGrade != null && e.IdHomeroom != null).Any())
                    {
                        var InvitationBookingSettingDetailByHomeroom = InvitationBookingSettingDetail
                                .Where(e => ListChild.Select(x => x.Homeroom.Id).ToList().Contains(e.IdHomeroom) && e.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published)
                            .Select(e => new GetAppointmentBookingByUserResult
                            {
                                IdInvitationBookingSetting = e.IdInvitationBookingSetting,
                                InvitationBookingName = e.InvitationBookingSetting.InvitationName,
                                IsSchedulingSiblingSameTime = e.InvitationBookingSetting.SchedulingSiblingSameTime
                            })
                            .ToList();

                        Items.AddRange(InvitationBookingSettingDetailByHomeroom.Where(e => !Items.Select(e => e.IdInvitationBookingSetting).ToList().Contains(e.IdInvitationBookingSetting)));
                    }

                    if (InvitationBookingSettingDetail.Where(e => e.IdLevel != null && e.IdGrade != null && e.IdHomeroom == null).Any())
                    {
                        var InvitationBookingSettingDetailByGrade = InvitationBookingSettingDetail
                              .Where(e => ListChild.Select(x => x.Grade.Id).ToList().Contains(e.IdGrade) && e.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published)
                          .Select(e => new GetAppointmentBookingByUserResult
                          {
                              IdInvitationBookingSetting = e.IdInvitationBookingSetting,
                              InvitationBookingName = e.InvitationBookingSetting.InvitationName,
                              IsSchedulingSiblingSameTime = e.InvitationBookingSetting.SchedulingSiblingSameTime
                          })
                          .ToList();

                        Items.AddRange(InvitationBookingSettingDetailByGrade.Where(e => !Items.Select(e => e.IdInvitationBookingSetting).ToList().Contains(e.IdInvitationBookingSetting)));
                    }

                    if (InvitationBookingSettingDetail.Where(e => e.IdLevel != null && e.IdGrade == null && e.IdHomeroom == null).Any())
                    {
                        var InvitationBookingSettingDetailByLevel = InvitationBookingSettingDetail
                               .Where(e => ListChild.Select(x => x.Level.Id).ToList().Contains(e.IdLevel) && e.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published)
                           .Select(e => new GetAppointmentBookingByUserResult
                           {
                               IdInvitationBookingSetting = e.IdInvitationBookingSetting,
                               InvitationBookingName = e.InvitationBookingSetting.InvitationName,
                               IsSchedulingSiblingSameTime = e.InvitationBookingSetting.SchedulingSiblingSameTime
                           })
                           .ToList();

                        Items.AddRange(InvitationBookingSettingDetailByLevel.Where(e => !Items.Select(e => e.IdInvitationBookingSetting).ToList().Contains(e.IdInvitationBookingSetting)));
                    }
                }

                if (InvitationBookingSettingUser.Any())
                {
                    var InvitationBookingSettingUserByHomeroomStudent = InvitationBookingSettingUser
                                .Where(e => ListChild.Select(x => x.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent) && e.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published)
                            .Select(e => new GetAppointmentBookingByUserResult
                            {
                                IdInvitationBookingSetting = e.IdInvitationBookingSetting,
                                InvitationBookingName = e.InvitationBookingSetting.InvitationName,
                                IsSchedulingSiblingSameTime = e.InvitationBookingSetting.SchedulingSiblingSameTime
                            })
                            .ToList();

                    Items.AddRange(InvitationBookingSettingUserByHomeroomStudent.Where(e => !Items.Select(e => e.IdInvitationBookingSetting).ToList().Contains(e.IdInvitationBookingSetting)));
                }
            }
            else if (param.Role == RoleConstant.Staff)
            {
                Items = await _dbContext.Entity<TrInvitationBookingSetting>()
                    .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.Status == StatusInvitationBookingSetting.Published && (_dateTime.ServerTime >= e.StaffBookingStartDate && _dateTime.ServerTime <= e.StaffBookingEndDate))
                .Select(e => new GetAppointmentBookingByUserResult
                {
                    IdInvitationBookingSetting = e.Id,
                    InvitationBookingName = e.InvitationName,
                    IsSchedulingSiblingSameTime = e.SchedulingSiblingSameTime
                })
                .ToListAsync(CancellationToken);
            }
            else if (param.Role == RoleConstant.Teacher)
            {
                Items = await _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                .Include(e => e.InvitationBookingSettingVenueDate).ThenInclude(e => e.InvitationBookingSetting)
                // .Where(e => e.IdUserTeacher == param.IdUser && e.InvitationBookingSettingVenueDate.InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear && e.InvitationBookingSettingVenueDate.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published && (_dateTime.ServerTime >= e.InvitationBookingSettingVenueDate.InvitationBookingSetting.ParentBookingStartDate && _dateTime.ServerTime <= e.InvitationBookingSettingVenueDate.InvitationBookingSetting.ParentBookingEndDate)) //comment because have feedback teacher get from staff periode
                .Where(e => e.IdUserTeacher == param.IdUser && e.InvitationBookingSettingVenueDate.InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear && e.InvitationBookingSettingVenueDate.InvitationBookingSetting.Status == StatusInvitationBookingSetting.Published && (_dateTime.ServerTime >= e.InvitationBookingSettingVenueDate.InvitationBookingSetting.StaffBookingStartDate && _dateTime.ServerTime <= e.InvitationBookingSettingVenueDate.InvitationBookingSetting.StaffBookingEndDate))
                .Select(e => new GetAppointmentBookingByUserResult
                {
                    IdInvitationBookingSetting = e.InvitationBookingSettingVenueDate.IdInvitationBookingSetting,
                    InvitationBookingName = e.InvitationBookingSettingVenueDate.InvitationBookingSetting.InvitationName,
                    IsSchedulingSiblingSameTime = e.InvitationBookingSettingVenueDate.InvitationBookingSetting.SchedulingSiblingSameTime
                })
                .Distinct().ToListAsync(CancellationToken);
            }


            return Request.CreateApiResult2(Items as object);
        }
    }
}
