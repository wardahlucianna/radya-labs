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
    public class GetListLearningOutcomeForSACHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListLearningOutcomeForSACHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListLearningOutcomeForSACRequest>(
                            nameof(GetListLearningOutcomeForSACRequest.IdAcademicYear)
                        );

            var query = await _dbContext.Entity<MsMappingLearningOutcome>()
                .Where(x => x.IdAcademicyear == param.IdAcademicYear)
                .Select(x => new
                {
                    IdLearningOutcome = x.LearningOutcome.Id,
                    LearningOutcomeName = x.LearningOutcome.LearningOutcomeName
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            var result = query.Select(x => new ItemValueVm
            {
                Id = x.IdLearningOutcome,
                Description = x.LearningOutcomeName
            })
            .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
