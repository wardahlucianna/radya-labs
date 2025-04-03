using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetStudentbyHomeromeStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentbyHomeromeStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetStudentbyHomeromeRequest>();

            var result = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                .Include(e=>e.Homeroom).ThenInclude(e=>e.GradePathway).ThenInclude(e=>e.MsGradePathwayClassrooms).ThenInclude(e=>e.Classroom)
                .Include(e=>e.Student)
                .Where(e=>e.IdHomeroom==param.IdHomeroom)
                .Select(e=>new GetStudentbyHomeromeResult
                {
                    IdHomeroomStudent=e.Id,
                    FullName=e.Student.FirstName+ e.Student.LastName,
                    Grade =e.Homeroom.Grade.Description,
                    Homeroom= string.Format("{0}{1}",
                                         e.Homeroom.Grade.Code,
                                         e.Homeroom.GradePathwayClassroom.Classroom.Code),
                    BunusanId = e.Student.Id
                })
                .ToListAsync(CancellationToken);





            return Request.CreateApiResult2(result as object);
        }
    }
}
