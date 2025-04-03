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
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAllChildDateilByParentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAllChildDateilByParentHandler(ISchedulingDbContext AppointmentBookingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = AppointmentBookingDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllChildDateilByParentRequest>();
            var childrens = new List<GetChildResult>();
            var sibligGroup = "";
            var idStudent = param.IdStudent;
            MsStudentParent dataStudentParent = default;

            if (param.Role== RoleConstant.Parent)
            {
                var dataUser = await _dbContext.Entity<MsUser>().Where(x => x.Id == param.IdParent).Select(x => new { x.Username, x.DisplayName }).FirstOrDefaultAsync(CancellationToken);
                idStudent = dataUser.Username.Substring(1);
            }

            dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                       .Where(x => x.IdStudent == idStudent)
                                       .FirstOrDefaultAsync(CancellationToken);

            sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                .Where(x => x.IdStudent == idStudent).Select(x => x.Id).FirstOrDefaultAsync(CancellationToken);

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

            if (!param.IsSiblingSameTime)
            {
                childrens = childrens.Where(e => e.Id == param.IdStudent).ToList();
            }
            else
            {
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
                                    FullName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
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
                                     FullName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
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
                                     FullName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
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
                                     FullName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName)
                                 })
                                .ToListAsync(CancellationToken);

                    HomeroomStudents.AddRange(HomeroomStudent);
                }

                childrens = childrens.Where(e => HomeroomStudents.Select(f=>f.IdStudent).ToList().Contains(e.Id)).ToList();
            }


            List<GetAllChildDateilByParentResult> ListChild = new List<GetAllChildDateilByParentResult>();
            foreach (var item in childrens)
            {
                var GetSemeterAy = await _dbContext.Entity<MsPeriod>()
                       .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                       .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.Level.AcademicYear.IdSchool == item.IdSchool)
                       .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.Level.IdAcademicYear })
                       .Distinct().FirstOrDefaultAsync(CancellationToken);

                var result = await _dbContext.Entity<MsHomeroomStudent>()
                       .Include(e => e.Student)
                       .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                       .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                       .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                       .Where(e => e.IdStudent == item.Id && e.Homeroom.AcademicYear.Id == GetSemeterAy.IdAcademicYear && e.Homeroom.Semester == GetSemeterAy.Semester)
                       .Select(e => new GetAllChildDateilByParentResult
                       {
                           StudentName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
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
                       }).FirstOrDefaultAsync(CancellationToken);

                ListChild.Add(result);
            }

            return Request.CreateApiResult2(ListChild as object);
        }
    }
}
