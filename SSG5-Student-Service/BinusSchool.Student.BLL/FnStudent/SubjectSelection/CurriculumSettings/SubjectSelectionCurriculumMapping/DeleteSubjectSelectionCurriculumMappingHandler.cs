using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping.Validator;
using BinusSchool.Student.DAL.Entities;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping
{
    public class DeleteSubjectSelectionCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteSubjectSelectionCurriculumMappingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteSubjectSelectionCurriculumMappingRequest, DeleteSubjectSelectionCurriculumMappingValidator>();

            var getCurriculumGroup = await _dbContext.Entity<MsMappingCurriculumGrade>()
                .Include(x => x.MappingCurriculumSubjectGroups)
                .Include(x => x.MappingCurriculumGradeSubLevels)
                .Where(x => param.CurriculumGroup.Contains(x.CurriculumGroup))
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var item in param.CurriculumGroup)
                {
                    //Check if the curriculum group is mapped
                    var curriculumGroup = getCurriculumGroup.Where(x => x.CurriculumGroup == item);

                    if (curriculumGroup.Count() == 0)
                    {
                        throw new Exception($"Curriculum Group doens't exists");
                    }

                    if (curriculumGroup.Any(x => x.MappingCurriculumSubjectGroups != null && x.MappingCurriculumSubjectGroups.Any()))
                    {
                        throw new Exception($"Curriculum Group has been mapped to subject group");
                    }

                    var subjectLevels = curriculumGroup.SelectMany(x => x.MappingCurriculumGradeSubLevels).ToList();
                    subjectLevels.ForEach(x => x.IsActive = false);

                    curriculumGroup.ForEach(x => x.IsActive = false);

                    _dbContext.Entity<MsMappingCurriculumGrade>().UpdateRange(curriculumGroup);
                    _dbContext.Entity<MsMappingCurriculumGradeSubLevel>().UpdateRange(subjectLevels);
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
