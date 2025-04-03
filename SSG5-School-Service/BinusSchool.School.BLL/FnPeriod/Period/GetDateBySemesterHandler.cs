using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class GetDateBySemesterHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetDateBySemesterHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var data = new GetDateBySemesterResult();
            var param = Request.ValidateParams<GetDateBySemesterRequest>(nameof(GetDateBySemesterRequest.IdGrade), nameof(GetDateBySemesterRequest.Semester));
            
            var query = await _dbContext.Entity<MsPeriod>()
                            .Where(x => x.IdGrade == param.IdGrade && x.Semester == param.Semester).OrderBy(x=>x.StartDate).ToListAsync();

            if (query.Count == 0)
                return Request.CreateApiResult2(data as object);

            data = new GetDateBySemesterResult { StartDate = query.Select(x => x.StartDate).First(), EndDate = query.Select(x => x.EndDate).Last() };

            return Request.CreateApiResult2(data as object);
        }
    }
}
