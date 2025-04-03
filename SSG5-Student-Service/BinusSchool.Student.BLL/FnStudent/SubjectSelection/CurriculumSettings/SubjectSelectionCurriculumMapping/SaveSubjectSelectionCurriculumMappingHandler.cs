using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.SubjectLearningContinuumSettings;
using BinusSchool.Persistence.StudentDb.Abstractions;
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
    public class SaveSubjectSelectionCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveSubjectSelectionCurriculumMappingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveSubjectSelectionCurriculumMappingRequest, SaveSubjectSelectionCurriculumMappingValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                //Grade cannot be mapped into the same curriculum
                var isGradeMapped = await _dbContext.Entity<MsMappingCurriculumGrade>()
                    .Where(x => x.IdSubjectSelectionCurriculum == param.IdSubjectSelectionCurriculum &&
                                param.IdGrade.Contains(x.IdGrade) &&
                                (string.IsNullOrWhiteSpace(param.CurriculumGroup) || param.CurriculumGroup != x.CurriculumGroup))
                    .AnyAsync(CancellationToken);

                if (isGradeMapped)
                {
                    throw new Exception("Grade cannot be mapped into the same curriculum");
                }

                //Validate Max <= Min
                if (param.MinSubjectSelection > param.MaxSubjectSelection)
                {
                    throw new Exception("Minimal subject value must be smaller than maximal subject value");
                }

                var totalSubjectLevelsMin = 0;

                foreach (var item in param.SubjectLevels)
                {
                    totalSubjectLevelsMin += item.MinRange ?? 0;
                    if (item.MinRange > item.MaxRange)
                    {
                        throw new Exception($"Subject level minimal value must be smaller than maximal value");
                    }
                }

                if (totalSubjectLevelsMin > param.MinSubjectSelection)
                {
                    throw new Exception($"Total range of minimal subject levels ({totalSubjectLevelsMin}) must be less than minimal subject value ({param.MinSubjectSelection})");
                }

                //Create
                if (string.IsNullOrWhiteSpace(param.CurriculumGroup))
                {
                    var curriculumGroupId = Guid.NewGuid().ToString();
                    foreach (var item in param.IdGrade)
                    {
                        var curriculumGrade = new MsMappingCurriculumGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdGrade = item,
                            IdSubjectSelectionCurriculum = param.IdSubjectSelectionCurriculum,
                            Description = param.Description,
                            CurriculumGroup = curriculumGroupId,
                            MaxSubjectSelection = param.MaxSubjectSelection,
                            MinSubjectSelection = param.MinSubjectSelection,
                        };

                        _dbContext.Entity<MsMappingCurriculumGrade>().Add(curriculumGrade);

                        foreach (var subjectLevel in param.SubjectLevels)
                        {
                            if (subjectLevel.MaxRange == null && subjectLevel.MinRange == null)
                            {
                                continue;
                            }

                            var subjectLevelData = new MsMappingCurriculumGradeSubLevel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSubjectLevel = subjectLevel.IdSubjectLevel,
                                IdMappingCurriculumGrade = curriculumGrade.Id,
                                MinRange = subjectLevel.MinRange,
                                MaxRange = subjectLevel.MaxRange
                            };

                            _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().Add(subjectLevelData);
                        }
                    }
                }
                //Update
                else
                {
                    var curriculumGroup = await _dbContext.Entity<MsMappingCurriculumGrade>()
                        .Include(x => x.MappingCurriculumGradeSubLevels)
                        .Include(x => x.MappingCurriculumSubjectGroups)
                        .Where(x => x.CurriculumGroup == param.CurriculumGroup &&
                                    x.IdSubjectSelectionCurriculum == param.IdSubjectSelectionCurriculum)
                        .ToListAsync(CancellationToken);

                    if (curriculumGroup == null || !curriculumGroup.Any())
                    {
                        throw new Exception("Curriculum Group doesn't exists");
                    }

                    //Add new grade
                    var newGrade = param.IdGrade.Except(curriculumGroup.Select(x => x.IdGrade)).ToList();
                    foreach (var item in newGrade)
                    {
                        var curriculumGrade = new MsMappingCurriculumGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdGrade = item,
                            IdSubjectSelectionCurriculum = param.IdSubjectSelectionCurriculum,
                            Description = param.Description,
                            CurriculumGroup = param.CurriculumGroup,
                            MaxSubjectSelection = param.MaxSubjectSelection,
                            MinSubjectSelection = param.MinSubjectSelection,
                        };

                        _dbContext.Entity<MsMappingCurriculumGrade>().Add(curriculumGrade);

                        foreach (var subjectLevel in param.SubjectLevels)
                        {
                            if (subjectLevel.MaxRange == null && subjectLevel.MinRange == null)
                            {
                                continue;
                            }

                            var subjectLevelData = new MsMappingCurriculumGradeSubLevel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSubjectLevel = subjectLevel.IdSubjectLevel,
                                IdMappingCurriculumGrade = curriculumGrade.Id,
                                MinRange = subjectLevel.MinRange,
                                MaxRange = subjectLevel.MaxRange
                            };

                            _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().Add(subjectLevelData);
                        }
                    }

                    //Remove grade
                    var removeGrade = curriculumGroup.Where(x => !param.IdGrade.Contains(x.IdGrade)).ToList();
                    foreach (var item in removeGrade)
                    {
                        //Check if grade is used
                        if (item.MappingCurriculumSubjectGroups.Any())
                        {
                            throw new Exception("Grade is used in mapping curriculum subject group");
                        }

                        //Remove subject level
                        var subjectLevels = item.MappingCurriculumGradeSubLevels.ToList();
                        subjectLevels.ForEach(x => x.IsActive = false);

                        //Remove grade
                        item.IsActive = false;

                        _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().UpdateRange(subjectLevels);
                        _dbContext.Entity<MsMappingCurriculumGrade>().Update(item);
                    }

                    //Update grade
                    var updateGrade = curriculumGroup.Where(x => param.IdGrade.Contains(x.IdGrade)).ToList();
                    foreach (var item in updateGrade)
                    {
                        item.Description = param.Description;
                        item.MaxSubjectSelection = param.MaxSubjectSelection;
                        item.MinSubjectSelection = param.MinSubjectSelection;

                        _dbContext.Entity<MsMappingCurriculumGrade>().Update(item);

                        var removeSubjectLevels = item.MappingCurriculumGradeSubLevels
                            .Where(x => !param.SubjectLevels.Select(y => y.IdSubjectLevel).Contains(x.IdSubjectLevel))
                            .ToList();

                        var newSubjectLevels = param.SubjectLevels
                            .Where(x => !item.MappingCurriculumGradeSubLevels.Select(y => y.IdSubjectLevel).Contains(x.IdSubjectLevel))
                            .ToList();

                        var updateSubjectLevels = item.MappingCurriculumGradeSubLevels
                            .Where(x => param.SubjectLevels.Select(y => y.IdSubjectLevel).Contains(x.IdSubjectLevel))
                            .ToList();

                        //Remove subject level that doesn't exists
                        removeSubjectLevels.ForEach(x => x.IsActive = false);

                        _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().UpdateRange(removeSubjectLevels);

                        //Add new subject level
                        foreach (var subjectLevel in newSubjectLevels)
                        {
                            if (subjectLevel.MaxRange == null && subjectLevel.MinRange == null)
                            {
                                continue;
                            }

                            var subjectLevelData = new MsMappingCurriculumGradeSubLevel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSubjectLevel = subjectLevel.IdSubjectLevel,
                                IdMappingCurriculumGrade = item.Id,
                                MinRange = subjectLevel.MinRange,
                                MaxRange = subjectLevel.MaxRange
                            };

                            _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().Add(subjectLevelData);
                        }

                        //Update subject level
                        foreach (var subjectLevel in updateSubjectLevels)
                        {
                            var subjectLevelData = param.SubjectLevels
                                .FirstOrDefault(x => x.IdSubjectLevel == subjectLevel.IdSubjectLevel);

                            if (subjectLevelData.MinRange == null && subjectLevelData.MaxRange == null)
                            {
                                subjectLevel.IsActive = false;
                            }
                            else
                            {
                                subjectLevel.MinRange = subjectLevelData.MinRange;
                                subjectLevel.MaxRange = subjectLevelData.MaxRange;
                            }

                            _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().Update(subjectLevel);
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
