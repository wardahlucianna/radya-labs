using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping.Validator;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping
{
    public class CopySubjectSelectionCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopySubjectSelectionCurriculumMappingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CopySubjectSelectionCurriculumMappingRequest, CopySubjectSelectionCurriculumMappingValidator>();

            #region Retrieve Current and Previous Academic Year
            var currentAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == param.CurrentAcademicYear)
                .Select(x => new { x.Id, x.Code, x.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (currentAcademicYear == null)
            {
                throw new Exception("Current Academic Year not found.");
            }

            int previousYearCode = int.Parse(currentAcademicYear.Code) - 1;

            var previousAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Code == previousYearCode.ToString() && x.IdSchool == currentAcademicYear.IdSchool)
                .Select(x => new { x.Id, x.Code, x.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (previousAcademicYear == null)
                throw new Exception("Previous Academic Year not found.");
            #endregion

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getCurriculumGroup = await _dbContext.Entity<MsMappingCurriculumGrade>()
                .Include(x => x.MappingCurriculumGradeSubLevels)
                .Include(x => x.Grade).ThenInclude(x => x.MsLevel)
                .Where(x => x.Grade.MsLevel.IdAcademicYear == previousAcademicYear.Id ||
                            x.Grade.MsLevel.IdAcademicYear == currentAcademicYear.Id)
                .ToListAsync(CancellationToken);

                var prevCurriculumGroup = getCurriculumGroup.Where(x => x.Grade.MsLevel.IdAcademicYear == previousAcademicYear.Id).ToList();
                var currCurriculumGroup = getCurriculumGroup.Where(x => x.Grade.MsLevel.IdAcademicYear == currentAcademicYear.Id).ToList();

                // There must be not any curriculum group in the current academic year
                if (currCurriculumGroup.Any())
                {
                    throw new Exception("Curriculum Group already exists in the current academic year");
                }

                // There must be at least one curriculum group in the previous academic year
                if (!prevCurriculumGroup.Any())
                {
                    throw new Exception("Curriculum Group not found in the previous academic year");
                }

                // Copy Curriculum Group
                var getGrades = await _dbContext.Entity<MsGrade>()
                    .Where(x => x.MsLevel.IdAcademicYear == currentAcademicYear.Id)
                    .ToListAsync(CancellationToken);

                var groupPrevCurriculumGroup = prevCurriculumGroup
                    .GroupBy(x => new
                    {
                        x.CurriculumGroup,
                        x.MinSubjectSelection,
                        x.MaxSubjectSelection,
                        x.Description,
                        x.IdSubjectSelectionCurriculum
                    })
                    .Select(x => new
                    {
                        x.Key.Description,
                        x.Key.CurriculumGroup,
                        x.Key.MinSubjectSelection,
                        x.Key.MaxSubjectSelection,
                        x.Key.IdSubjectSelectionCurriculum,
                        GradeSubLevels = x.Select(y => new
                        {
                            y.Grade,
                            y.MappingCurriculumGradeSubLevels
                        })
                    }).ToList();

                foreach (var prevCurriculum in groupPrevCurriculumGroup)
                {
                    var curriculumGroupId = Guid.NewGuid().ToString();

                    foreach (var curriculumGrade in prevCurriculum.GradeSubLevels)
                    {
                        var grade = getGrades.FirstOrDefault(x => x.Code == curriculumGrade.Grade.Code);

                        if (grade == null)
                        {
                            continue;
                        }

                        var curriculum = new MsMappingCurriculumGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSubjectSelectionCurriculum = prevCurriculum.IdSubjectSelectionCurriculum,
                            Description = prevCurriculum.Description,
                            MinSubjectSelection = prevCurriculum.MinSubjectSelection,
                            MaxSubjectSelection = prevCurriculum.MaxSubjectSelection,
                            CurriculumGroup = curriculumGroupId,
                            IdGrade = grade.Id
                        };

                        _dbContext.Entity<MsMappingCurriculumGrade>().Add(curriculum);

                        foreach (var subLevels in curriculumGrade.MappingCurriculumGradeSubLevels)
                        {
                            var subLevel = new MsMappingCurriculumGradeSubLevel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdMappingCurriculumGrade = curriculum.Id,
                                IdSubjectLevel = subLevels.IdSubjectLevel,
                                MinRange = subLevels.MinRange,
                                MaxRange = subLevels.MaxRange
                            };

                            _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().Add(subLevel);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }
    }
}
