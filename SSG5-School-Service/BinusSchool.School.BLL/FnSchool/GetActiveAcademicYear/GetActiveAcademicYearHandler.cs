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
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.GetActiveAcademicYear
{
    public class GetActiveAcademicYearHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetActiveAcademicYearHandler(
            ISchoolDbContext schoolDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<GetActiveAcademicYearRequest>();

            var getListPeriod = _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.SchoolID)
                    .ToList();

            var getListAcademicYear = getListPeriod
                    .Select(x => new 
                    {
                        AcademicYear = x.Grade.Level.AcademicYear.Description,
                        AcademicYearId = x.Grade.Level.AcademicYear.Id,
                        AcademicYearCode = x.Grade.Level.AcademicYear.Code,
                        Semester = x.Semester
                    })
                    .Distinct()
                    .OrderBy(x => x.AcademicYearCode)
                        .ThenBy(x => x.Semester)
                    .ToList();

            var query = getListPeriod
                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new GetActiveAcademicYearResult
                    {
                        AcademicYear = x.Grade.Level.AcademicYear.Description,
                        AcademicYearId = x.Grade.Level.AcademicYear.Id,
                        Semester = x.Semester
                    })
                    .FirstOrDefault();

            if(query == null)
            {
                query = getListPeriod
                    .OrderBy(x => x.StartDate)
                    .Select(x => new GetActiveAcademicYearResult
                    {
                        AcademicYear = x.Grade.Level.AcademicYear.Description,
                        AcademicYearId = x.Grade.Level.AcademicYear.Id,
                        Semester = x.Semester
                    })
                    .LastOrDefault();
            }

            var getPreviousAcademicYear = getListAcademicYear.TakeWhile(x => !x.AcademicYearId.Equals(query.AcademicYearId)).LastOrDefault();

            query.PreviousAcademicYear = getPreviousAcademicYear?.AcademicYear;
            query.PreviousAcademicYearId = getPreviousAcademicYear?.AcademicYearId;
            query.PreviousSemester = getPreviousAcademicYear?.Semester;

            return Request.CreateApiResult2(query as object);

        }
    }
}
