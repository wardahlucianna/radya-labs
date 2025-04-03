using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.School.FnSchool.GetActiveSemester;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.GetActiveSemester
{
    public class GetActiveSemesterHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetActiveSemesterHandler(
            ISchoolDbContext schoolDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetActiveSemesterRequest>();

            var query = _dbContext.Entity<MsPeriod>()
                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.SchoolID)
                    .Where(x => x.IdGrade == param.GradeID)
                    .OrderByDescending(x => x.StartDate)
                    .Select( x => new GetActiveSemesterResult
                    {
                        Semester = x.Semester
                    })
                    .FirstOrDefault();

            if(query.Semester == 1)
            {
                query.PreviousSemester = query.Semester + 1;
            }
            else
            {
                query.PreviousSemester = query.Semester - 1;
            }

            return Request.CreateApiResult2(query as object);
        }
    }
}
