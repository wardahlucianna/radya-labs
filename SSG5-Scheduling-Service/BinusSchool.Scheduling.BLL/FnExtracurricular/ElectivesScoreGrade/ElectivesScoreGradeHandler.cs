using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesScoreGrade;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.ElectivesObjective.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectivesScoreGrade
{
    public class ElectivesScoreGradeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ElectivesScoreGradeHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetElectivesScoreGradeRequest>(
                nameof(GetElectivesScoreGradeRequest.IdSchool),
                nameof(GetElectivesScoreGradeRequest.IdAcademicYear),
                nameof(GetElectivesScoreGradeRequest.IdExtracurricular)
                );

            var getExtracurricularScoreComp = await _dbContext.Entity<MsExtracurricularScoreCompMapping>()
                        .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                        .Where(x => x.ExtracurricularScoreCompCategory.IdAcademicYear == param.IdAcademicYear)
                        .Select(x => new {
                            IdExtracurricularScoreCalculationType = x.ExtracurricularScoreCompCategory.IdExtracurricularScoreCalculationType
                        })
                        .FirstOrDefaultAsync(CancellationToken);
            if (getExtracurricularScoreComp is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ElectivesScoreGrade"], "Id", param.IdSchool));

            IReadOnlyList<IItemValueVm> items;
            items = await _dbContext.Entity<MsExtracurricularScoreGrade>()
                                  .Where(x => x.IdSchool == param.IdSchool)
                                  .Where(x => x.IdExtracurricularScoreCalculationType == getExtracurricularScoreComp.IdExtracurricularScoreCalculationType)
                                  .Select(x => new GetElectivesScoreGradeResult()
                                  {
                                      Id = x.Id,
                                      IdSchool = x.IdSchool,
                                      MinScore = x.MinScore,
                                      MaxScore = x.MaxScore,
                                      Grade = x.Grade
                                  })
                                  .OrderByDescending(x => x.MaxScore)
                                  .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items);
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
