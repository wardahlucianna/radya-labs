using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping
{
    public class GetListSubjectSelectionCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListSubjectSelectionCurriculumMappingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListSubjectSelectionCurriculumMappingRequest>(
                nameof(GetListSubjectSelectionCurriculumMappingRequest.IdAcademicYear));

            var getCurriculumGroupQuery = _dbContext.Entity<MsMappingCurriculumGrade>()
                .Where(x => x.Grade != null && x.Grade.MsLevel != null &&
                            x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
                            (string.IsNullOrWhiteSpace(param.IdGrade) || x.IdGrade == param.IdGrade))
                .Select(x => x.CurriculumGroup);

            var curricumGradeQuery = _dbContext.Entity<MsMappingCurriculumGrade>()
                .Where(x => getCurriculumGroupQuery.Contains(x.CurriculumGroup))
                .Select(x => new
                {
                    x.CurriculumGroup,
                    AcademicYearId = x.Grade.MsLevel.IdAcademicYear,
                    AcademicYearDescription = x.Grade.MsLevel.MsAcademicYear.Description,
                    CurriculumId = x.IdSubjectSelectionCurriculum,
                    CurriculumDescription = x.SubjectSelectionCurriculum.CurriculumName,
                    x.MinSubjectSelection,
                    x.MaxSubjectSelection,
                    CurriculumGrades = x.Grade.Description,
                    HasMappings = x.MappingCurriculumSubjectGroups.Any()
                });

            var res = await curricumGradeQuery
                .GroupBy(x => new
                {
                    x.CurriculumGroup,
                    x.AcademicYearId,
                    x.AcademicYearDescription,
                    x.CurriculumId,
                    x.CurriculumDescription,
                    x.MinSubjectSelection,
                    x.MaxSubjectSelection
                })
                .Select(g => new GetListSubjectSelectionCurriculumMappingResult
                {
                    CurriculumGroup = g.Key.CurriculumGroup,
                    AcademicYear = new ItemValueVm
                    {
                        Id = g.Key.AcademicYearId,
                        Description = g.Key.AcademicYearDescription
                    },
                    Curriculum = new ItemValueVm
                    {
                        Id = g.Key.CurriculumId,
                        Description = g.Key.CurriculumDescription
                    },
                    MinSubjectSelection = g.Key.MinSubjectSelection,
                    MaxSubjectSelection = g.Key.MaxSubjectSelection,
                    CurriculumGrades = string.Join(", ", g.Select(x => x.CurriculumGrades)),
                    CanDelete = g.All(x => !x.HasMappings)
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);
        }
    }
}
