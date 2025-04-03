using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetActiveStudentsGradeByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetActiveStudentsGradeByStudentHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetActiveStudentsGradeByStudentRequest, GetActiveStudentsGradeByStudentValidator>();

            var resultList = new List<GetActiveStudentsGradeByStudentResult>();

            foreach (var idStudent in param.IdStudent)
            {
                #region Unused code
                //var IdSchool = await _dbContext.Entity<MsStudent>()
                //            .Where(x => x.Id == idStudent)
                //            .Select(x => x.IdSchool)
                //            .FirstOrDefaultAsync();

                //var GetAcademicYear = await _dbContext.Entity<MsPeriod>()
                //                    .Include(x => x.Grade)
                //                    .ThenInclude(y => y.Level)
                //                    .ThenInclude(z => z.AcademicYear)
                //                    .Where(x => _dateTime.ServerTime >= x.StartDate &&
                //                                _dateTime.ServerTime <= x.EndDate &&
                //                                x.Grade.Level.AcademicYear.IdSchool == IdSchool)
                //                    .OrderByDescending(x => x.StartDate)
                //                    .Select(x => new
                //                    {
                //                        AcademicYear = new ItemValueVm
                //                        {
                //                            Id = x.Grade.Level.AcademicYear.Id,
                //                            Description = x.Grade.Level.AcademicYear.Description
                //                        },
                //                        Semester = x.Semester,
                //                    })
                //                    .FirstOrDefaultAsync();

                //var result = await _dbContext.Entity<MsHomeroomStudent>()
                //        .Include(x => x.Homeroom)
                //        .ThenInclude(x => x.Grade)
                //        .ThenInclude(x => x.Level)
                //        .ThenInclude(x => x.AcademicYear)
                //        .ThenInclude(x => x.School)
                //    .Where(x => x.IdStudent == idStudent && x.Semester == GetAcademicYear.Semester && x.Homeroom.IdAcademicYear == GetAcademicYear.AcademicYear.Id)
                //    .Select(x => new GetActiveStudentsGradeByStudentResult
                //    {
                //        StudentId = idStudent,
                //        SchoolId = x.Homeroom.AcademicYear.IdSchool,
                //        AcadYear = new CodeWithIdVm()
                //        {
                //            Id = x.Homeroom.AcademicYear.Id,
                //            Code = x.Homeroom.AcademicYear.Code,
                //            Description = x.Homeroom.AcademicYear.Description,
                //        },
                //        Semester = x.Semester,
                //        Level = new CodeWithIdVm()
                //        {
                //            Id = x.Homeroom.Grade.Level.Id,
                //            Code = x.Homeroom.Grade.Level.Code,
                //            Description = x.Homeroom.Grade.Level.Description,
                //        },
                //        Grade = new CodeWithIdVm()
                //        {
                //            Id = x.Homeroom.Grade.Id,
                //            Code = x.Homeroom.Grade.Code,
                //            Description = x.Homeroom.Grade.Description,
                //        }
                //    })
                //    .FirstOrDefaultAsync();
                #endregion

                var result = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(hs => hs.Student)
                        .Include(hs => hs.Homeroom)
                        .ThenInclude(h => h.GradePathwayClassroom)
                        .ThenInclude(gpc => gpc.Classroom)
                        .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                        .ThenInclude(x => x.School)
                    .Where(x => x.IdStudent == idStudent)
                    .Join(_dbContext.Entity<MsPeriod>()
                            .Where(x => _dateTime.ServerTime >= x.StartDate &&
                                        _dateTime.ServerTime <= x.EndDate)
                            .OrderByDescending(x => x.StartDate),
                            homeroom => new { gradeId = homeroom.Homeroom.Grade.Id, homeroom.Semester },
                            period => new { gradeId = period.IdGrade, period.Semester },
                            (homeroom, period) => new GetActiveStudentsGradeByStudentResult
                            {
                                School = new ItemValueVm()
                                {
                                    Id = homeroom.Homeroom.AcademicYear.School.Id,
                                    Description = homeroom.Homeroom.AcademicYear.School.Description,
                                },
                                AcadYear = new CodeWithIdVm()
                                {
                                    Id = homeroom.Homeroom.AcademicYear.Id,
                                    Code = homeroom.Homeroom.AcademicYear.Code,
                                    Description = homeroom.Homeroom.AcademicYear.Description,
                                },
                                Level = new CodeWithIdVm()
                                {
                                    Id = homeroom.Homeroom.Grade.Level.Id,
                                    Code = homeroom.Homeroom.Grade.Level.Code,
                                    Description = homeroom.Homeroom.Grade.Level.Description,
                                },
                                Grade = new CodeWithIdVm()
                                {
                                    Id = homeroom.Homeroom.Grade.Id,
                                    Code = homeroom.Homeroom.Grade.Code,
                                    Description = homeroom.Homeroom.Grade.Description,
                                },
                                Semester = homeroom.Semester,
                                Student = new NameValueVm
                                {
                                    Id = homeroom.Student.Id,
                                    Name = NameUtil.GenerateFullName(homeroom.Student.FirstName, homeroom.Student.LastName)
                                },
                                Homeroom = new NameValueVm
                                {
                                    Id = homeroom.Homeroom.Id,
                                    Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code)
                                },
                                IdHomeroomStudent = homeroom.Id
                            })
                    .Distinct()
                    .FirstOrDefaultAsync();

                if (result != null)
                    resultList.Add(result);
            }

            return Request.CreateApiResult2(resultList as object);
        }
    }
}
