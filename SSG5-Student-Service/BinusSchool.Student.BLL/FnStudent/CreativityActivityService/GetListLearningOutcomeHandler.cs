using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListLearningOutcomeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListLearningOutcomeHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListLearningOutcomeRequest>();

            var query = await _dbContext.Entity<MsLearningOutcome>()
                .Include(x => x.AcademicYear)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .Select(x => new
                {
                    Id = x.Id,
                    Code = x.Id,
                    Description = x.LearningOutcomeName,
                    AcademicYear = new CodeWithIdVm(x.IdAcademicYear,x.AcademicYear.Code,x.AcademicYear.Description),
                    Order = x.Order
                })
                .OrderBy(x => x.Order)
                .ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items;

            items = query
                .Select(x => new GetListLearningOutcomeResult()
                {
                    Id = x.Id,
                    Code = x.Id,
                    Description = x.Description,
                    AcademicYear = x.AcademicYear,
                    Order = x.Order
                })
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
