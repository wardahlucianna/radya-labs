using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
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
    public class GetStudentByAppointmentBookingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentByAppointmentBookingHandler(ISchedulingDbContext AppointmentBookingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = AppointmentBookingDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentByAppointmentBookingRequest>();
            var childrens = new List<GetChildResult>();

            var InvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
              .Where(e => e.Id  == param.IdInvitationBookingSetting)
              .FirstOrDefaultAsync(CancellationToken);

            if (InvitationBookingSetting == null)
                throw new BadRequestException($"Id Invitation booking setting : {param.IdInvitationBookingSetting} is not found");

            var GetPeriod = await _dbContext.Entity<MsPeriod>()
                .Include(e=>e.Grade).ThenInclude(e=>e.Level)
              .Where(e => e.Grade.Level.IdAcademicYear == InvitationBookingSetting.IdAcademicYear && (_dateTime.ServerTime >= e.StartDate.Date && _dateTime.ServerTime <= e.EndDate.Date))
              .FirstOrDefaultAsync(CancellationToken);

            #region get all student
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
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade && e.IdHomeroom == item.IdHomeroom && e.Semester== GetPeriod.Semester)
                            .Select(e => new GetHomeroomStudent
                            {
                                IdHomeroomStudent = e.Id,
                                IdLevel = e.Homeroom.Grade.IdLevel,
                                IdGrade = e.Homeroom.IdGrade,
                                IdHomeroom = e.IdHomeroom,
                                IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                IdStudent = e.IdStudent,
                                FullName = e.Student.FirstName + (e.Student.MiddleName==null?"":" "+ e.Student.FirstName)+ (e.Student.LastName==null?"":" "+ e.Student.LastName),
                                Grade = e.Homeroom.Grade.Description,
                                Level = e.Homeroom.Grade.Level.Description,
                                Homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                            })
                            .ToListAsync(CancellationToken);

                        HomeroomStudents.AddRange(HomeroomStudent);
                    }

                    if (item.IdLevel != null && item.IdGrade != null && item.IdHomeroom == null)
                    {
                        var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.HomeroomStudentEnrollments)
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Homeroom.IdGrade == item.IdGrade && e.Semester == GetPeriod.Semester)
                             .Select(e => new GetHomeroomStudent
                             {
                                 IdHomeroomStudent = e.Id,
                                 IdLevel = e.Homeroom.Grade.IdLevel,
                                 IdGrade = e.Homeroom.IdGrade,
                                 IdHomeroom = e.IdHomeroom,
                                 IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                 IdStudent = e.IdStudent,
                                 FullName = (e.Student.FirstName == null ? "" : "" + e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                                 Grade = e.Homeroom.Grade.Description,
                                 Level = e.Homeroom.Grade.Level.Description,
                                 Homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                             })
                            .ToListAsync(CancellationToken);

                        HomeroomStudents.AddRange(HomeroomStudent);
                    }

                    if (item.IdLevel != null && item.IdGrade == null && item.IdHomeroom == null)
                    {
                        var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.Homeroom.Grade.IdLevel == item.IdLevel && e.Semester == GetPeriod.Semester)
                             .Select(e => new GetHomeroomStudent
                             {
                                 IdHomeroomStudent = e.Id,
                                 IdLevel = e.Homeroom.Grade.IdLevel,
                                 IdGrade = e.Homeroom.IdGrade,
                                 IdHomeroom = e.IdHomeroom,
                                 IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                 IdStudent = e.IdStudent,
                                 FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                                 Grade = e.Homeroom.Grade.Description,
                                 Level = e.Homeroom.Grade.Level.Description,
                                 Homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                             })
                            .ToListAsync(CancellationToken);

                        HomeroomStudents.AddRange(HomeroomStudent);
                    }
                }
            }

            if(InvitationBookingSetting.InvitationType == InvitationType.Personal)
            {
                if (InvitationBookingSettingUser.Any())
                {
                    var HomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Select(e => new GetHomeroomStudent
                                {
                                    IdHomeroomStudent = e.Id,
                                    IdLevel = e.Homeroom.Grade.IdLevel,
                                    IdGrade = e.Homeroom.IdGrade,
                                    IdHomeroom = e.IdHomeroom,
                                    IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                    IdStudent = e.IdStudent,
                                    FullName = e.Student.FirstName + (e.Student.MiddleName == null ? "" : " " + e.Student.FirstName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                                    Grade = e.Homeroom.Grade.Description,
                                    Level = e.Homeroom.Grade.Level.Description,
                                    Homeroom = e.Homeroom.Grade.Code // + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                })
                                .ToListAsync(CancellationToken);

                    HomeroomStudents.AddRange(HomeroomStudent);
                }
            }
            
            #endregion

            #region sibling
            if (param.Role == RoleConstant.Parent)
            {
                var dataUser = await _dbContext.Entity<MsUser>().Where(x => x.Id == param.IdParent).Select(x => new { x.Username, x.DisplayName }).FirstOrDefaultAsync(CancellationToken);
                var idStudent = dataUser.Username.Substring(1);

                var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                      .Where(x => x.IdStudent == idStudent)
                                      .FirstOrDefaultAsync(CancellationToken);

                var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
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

                HomeroomStudents = HomeroomStudents.Where(e=>childrens.Select(x=>x.Id).ToList().Contains(e.IdStudent)).Distinct().ToList();
            }
            #endregion


            List<GetHomeroomTeacher> UserTeacher = new List<GetHomeroomTeacher>();
            if (!string.IsNullOrEmpty(param.IdUserTeacher))
            {
                //HomeroomTeacher
                var HomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Staff)
                                .Where(e => e.IdBinusian==param.IdUserTeacher)
                                .Select(e => new GetHomeroomTeacher
                                {
                                    IdGrade = e.Homeroom.IdGrade,
                                    IdLevel = e.Homeroom.Grade.IdLevel,
                                    IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                    IdHomeroom = e.IdHomeroom
                                })
                                .Distinct().ToListAsync(CancellationToken);
                UserTeacher.AddRange(HomeroomTeacher);

                //Subject Teacher
                var GetIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                                .Where(x => x.IdUser == param.IdUserTeacher)
                                .Select(e => e.IdLesson)
                                .ToListAsync(CancellationToken);

                var getHomeroomTeachers = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                            .Where(e => GetIdLesson.Contains(e.IdLesson))
                            .Select(e => new GetHomeroomTeacher
                            {
                                IdGrade = e.HomeroomStudent.Homeroom.IdGrade,
                                IdLevel = e.HomeroomStudent.Homeroom.Grade.IdLevel,
                                IdAcademicYear = e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                IdHomeroom = e.HomeroomStudent.IdHomeroom
                            })
                           .Distinct().ToListAsync(CancellationToken);

                UserTeacher.AddRange(getHomeroomTeachers);

                HomeroomStudents = HomeroomStudents.Where(e => UserTeacher.Select(e => e.IdHomeroom).ToList().Contains(e.IdHomeroom)).ToList();
            }

            var Items = HomeroomStudents
                .Where(e => InvitationBookingSettingUser.Select(x => x.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent))
                .GroupBy(e => new
                {
                    e.IdLevel,
                    e.IdGrade,
                    e.IdAcademicYear,
                    e.IdStudent,
                    e.FullName,
                    e.Grade,
                    e.Level,
                }).Select(e=>e.Key).ToList();

            return Request.CreateApiResult2(Items as object);
        }


    }
}
