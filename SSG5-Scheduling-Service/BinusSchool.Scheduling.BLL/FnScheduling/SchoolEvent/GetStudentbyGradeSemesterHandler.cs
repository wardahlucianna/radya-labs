using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetStudentbyGradeSemesterHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentbyGradeSemesterHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetStudentbyGradeSemesterRequest>();


            var query = _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.MsGradePathwayClassrooms).ThenInclude(e => e.Classroom)
                .Include(e => e.Student)
                .Where(e => e.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear 
                    && e.Homeroom.Grade.IdLevel==param.IdLevel
                    && e.Homeroom.IdGrade==param.IdGrade
                    && e.Homeroom.Semester==param.Semester
                    );

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(e => e.IdHomeroom == param.IdHomeroom);

            var result = await query
                .Select(e => new GetStudentbyHomeromeResult
                {
                    IdHomeroomStudent = e.Id,
                    FullName = e.Student.FirstName + e.Student.LastName,
                    Grade = e.Homeroom.Grade.Description,
                    Homeroom = string.Format("{0}{1}",
                                         e.Homeroom.Grade.Code,
                                         e.Homeroom.GradePathwayClassroom.Classroom.Code),
                    BunusanId = e.Student.Id
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
