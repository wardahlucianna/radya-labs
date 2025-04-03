using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentDemographyReportTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentDemographyReportTypeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentDemographyReportTypeRequest>
                (nameof(GetStudentDemographyReportTypeRequest.IdSchool));

            var getStudentDemoReportType = await _dbContext.Entity<LtStudentDemoReportType>()
                .Where(a => a.IdSchool == param.IdSchool
                    && (string.IsNullOrWhiteSpace(param.Search) ? true : EF.Functions.Like(a.Description, param.SearchPattern())))
                .OrderBy(a => a.Description)
                .ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
                items = getStudentDemoReportType
                    .Select(a => new CodeWithIdVm
                    {
                        Id = a.Id,
                        Description = a.Description,
                        Code = a.Code
                    })
                    .ToList();
            else
                items = getStudentDemoReportType
                    .SetPagination(param)
                    .Select(a => new GetStudentDemographyReportTypeResult
                    {
                        Id = a.Id,
                        Description = a.Description,
                        Code = a.Code
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : getStudentDemoReportType.Select(a => a.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
