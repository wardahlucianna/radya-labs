using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class GetListMappingLearningOutcomeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMappingLearningOutcomeHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListMappingLearningOutcomeRequest>(
                    nameof(GetListMappingLearningOutcomeRequest.IdAcademicYear)
                );

            var data = await _dbContext.Entity<MsMappingLearningOutcome>()
                .Where(x => x.IdAcademicyear == param.IdAcademicYear)
                .Include(x => x.LearningOutcome)
                .Select(x => new
                {
                    IdMappingLearningOutcome = x.Id,
                    Description = x.LearningOutcome.LearningOutcomeName,
                    OrderNo = x.LearningOutcome.Order
                })
                .Distinct()
                .OrderBy(x => x.OrderNo)
                .ToListAsync(CancellationToken);

            var result = data
                .Select(x => new ItemValueVm
                {
                    Id = x.IdMappingLearningOutcome,
                    Description = x.Description
                })
                .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
