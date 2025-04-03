using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GetGradeCodeByIDForAscTimetableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetGradeCodeByIDForAscTimetableHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeCodeByIDForAscTimetableRequest>(nameof(GetGradeCodeByIDForAscTimetableRequest.IdGrade));
            var gradeCode = await _dbContext.Entity<MsGrade>()
                                      .Where(p => param.IdGrade.Any(x => x == p.Id))
                                      .Select(p => new CodeWithIdVm 
                                      {
                                        Id=p.Id,
                                        Code=p.Code,
                                        Description=p.Description,
                                      }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(gradeCode as object);
        }
    }
}
