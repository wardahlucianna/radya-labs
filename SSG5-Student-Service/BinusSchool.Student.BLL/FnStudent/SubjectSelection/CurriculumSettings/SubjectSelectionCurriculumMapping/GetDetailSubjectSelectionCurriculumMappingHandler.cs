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
    public class GetDetailSubjectSelectionCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailSubjectSelectionCurriculumMappingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailSubjectSelectionCurriculumMappingRequest>(
                nameof(GetDetailSubjectSelectionCurriculumMappingRequest.CurriculumGroup));

            var res = await _dbContext.Entity<MsMappingCurriculumGrade>()
                .Where(x => x.CurriculumGroup == param.CurriculumGroup)
                .Select(x => new
                {
                    x.IdSubjectSelectionCurriculum,
                    SubjectSelectionCurriculumDescription = x.SubjectSelectionCurriculum.CurriculumName,
                    x.Description,
                    x.MinSubjectSelection,
                    x.MaxSubjectSelection,
                    CurriculumGrades = new
                    {
                        x.IdGrade,
                        GradeDescription = x.Grade.Description,
                        IsDisabled = x.MappingCurriculumSubjectGroups.Any()
                    },
                    SubjectLevels = x.MappingCurriculumGradeSubLevels.Select(y => new
                    {
                        y.IdSubjectLevel,
                        y.SubjectLevel.Description,
                        y.MinRange,
                        y.MaxRange
                    }).Distinct()
                })
                .GroupBy(x => new
                {
                    x.IdSubjectSelectionCurriculum,
                    x.SubjectSelectionCurriculumDescription,
                    x.Description,
                    x.MinSubjectSelection,
                    x.MaxSubjectSelection
                })
                .Select(g => new GetDetailSubjectSelectionCurriculumMappingResult
                {
                    SubjectSelectionCurriculum = new ItemValueVm
                    {
                        Id = g.Key.IdSubjectSelectionCurriculum,
                        Description = g.Key.SubjectSelectionCurriculumDescription
                    },
                    Description = g.Key.Description,
                    MinSubjectSelection = g.Key.MinSubjectSelection,
                    MaxSubjectSelection = g.Key.MaxSubjectSelection,
                    CanUpdate = true,
                    CurriculumGrades = g.Select(y => new GetDetailSubjectSelectionCurriculumMappingResult_CurriculumGrade
                    {
                        Grade = new ItemValueVm
                        {
                            Id = y.CurriculumGrades.IdGrade,
                            Description = y.CurriculumGrades.GradeDescription,
                        },
                        IsDisabled = y.CurriculumGrades.IsDisabled
                    }).ToList(),
                    SubjectLevels = g.SelectMany(y => y.SubjectLevels)
                        .GroupBy(y => new
                        {
                            y.IdSubjectLevel,
                            y.Description,
                            y.MaxRange,
                            y.MinRange
                        })
                        .Select(y => new GetDetailSubjectSelectionCurriculumMappingResult_SubjectLevel
                        {
                            SubjectLevel = new ItemValueVm
                            {
                                Id = y.Key.IdSubjectLevel,
                                Description = y.Key.Description
                            },
                            MaxRange = y.Key.MaxRange,
                            MinRange = y.Key.MinRange
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);
        }
    }
}

