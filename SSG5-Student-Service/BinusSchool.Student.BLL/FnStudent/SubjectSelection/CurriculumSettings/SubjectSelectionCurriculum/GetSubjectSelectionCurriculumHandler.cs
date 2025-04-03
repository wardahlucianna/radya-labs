using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum
{
    public class GetSubjectSelectionCurriculumHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetSubjectSelectionCurriculumHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectSelectionCurriculumRequest>(nameof(GetSubjectSelectionCurriculumRequest.IdSchool));

            var getSubjectSelectionCurriculum = await _dbContext.Entity<LtSubjectSelectionCurriculum>()
                .Include(x => x.MappingCurriculumGrades)
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetSubjectSelectionCurriculumResult
                {
                    IdSubjectSelectionCurriculum = x.Id,
                    CurriculumName = x.CurriculumName,
                    CanDelete = !x.MappingCurriculumGrades.Any()
                }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(getSubjectSelectionCurriculum as object);
        }
    }
}
