using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class GetPeriodByDateHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetPeriodByDateHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPeriodByDateRequest>(nameof(GetPeriodByDateRequest.Date), nameof(GetPeriodByDateRequest.IdSchool));

            var res = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.MsLevel)
                    .ThenInclude(x => x.MsAcademicYear)
                .Where(x => x.StartDate <= param.Date && x.EndDate >= param.Date)
                .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                .OrderByDescending(x => x.Grade.MsLevel.MsAcademicYear.Code).ThenByDescending(x => x.Semester)
                .Select(x => new GetPeriodByDateResult
                {
                    IdAcademicYear = x.Grade.MsLevel.IdAcademicYear,
                    Semester = x.Semester
                })
                .FirstOrDefaultAsync();

            return Request.CreateApiResult2(res as object);
        }

    }
}
