using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentWIthGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentEnrollmentWIthGradeHandler(ISchedulingDbContext dbContext,
             IApiService<IStudent> studentServices)
        {
            _dbContext = dbContext;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentWithGradeRequest>(new string[] 
            {
              nameof(GetStudentEnrollmentWithGradeRequest.IdGrade),
              nameof(GetStudentEnrollmentWithGradeRequest.IdAscTimetable)
            });

            var data = await _dbContext.Entity<TrAscTimetableEnrollment>()
                                 .Include(p=> p.AscTimetable)
                                 .Include(p => p.HomeroomStudentEnrollment.HomeroomStudent.Homeroom)
                                 .Where(p => p.IdAscTimetable == param.IdAscTimetable 
                                    && p.HomeroomStudentEnrollment.HomeroomStudent.Homeroom.IdGrade == param.IdGrade)
                                 .Select(p=> new GetStudentEnrollmentWithGradeResult 
                                 {
                                    IdStudent=p.HomeroomStudentEnrollment.HomeroomStudent.IdStudent,
                                 }).ToListAsync();

            if (data.Any()) 
            {
                var ListIdStudent = data.Select(p => p.IdStudent).ToList();
                var getStudent = _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Student)
                    .Include(x => x.StudentGradePathways)
                    .Where(x=> ListIdStudent.Contains(x.IdStudent))
                    .Select(x => new GetStudentByGradeResult
                    {
                        Id = x.Id,
                        StudentId = x.IdStudent,
                    })
                    .ToList();


                data.ForEach(p => 
                {
                    var get = getStudent.Where(e => e.StudentId == p.IdStudent).FirstOrDefault();
                    if (get !=null) 
                    {
                        p.StudentName = get.FullName;
                    }
                });
            }
            return Request.CreateApiResult2(data as object);
        }
    }
}
