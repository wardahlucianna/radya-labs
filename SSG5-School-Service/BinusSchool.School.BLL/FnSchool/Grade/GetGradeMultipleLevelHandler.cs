using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GetGradeMultipleLevelHandlerHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetGradeMultipleLevelHandlerHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeMultipleLevelRequest>(nameof(GetGradeMultipleLevelRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsGrade>(x => x.Level.AcademicYear.School.Id == param.IdSchool);


            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcadyear);

            if (param.IdLevel != null)
            {
                predicate = predicate.And(x => param.IdLevel.Any(p => p == x.IdLevel));
            }

            var gradeMultipleLevel = await _dbContext.Entity<MsGrade>()
                .Where(predicate)
                .Select(x => new GetGradeMultipleLevelResult
                {
                    IdLevel = x.IdLevel,
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description
                })
                .OrderBy(x => x.IdLevel)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return Request.CreateApiResult2(gradeMultipleLevel as object);
        }
    }
}
