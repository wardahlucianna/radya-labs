using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum.Validator;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum
{
    public class DeleteSubjectSelectionCurriculumHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteSubjectSelectionCurriculumHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteSubjectSelectionCurriculumRequest, DeleteSubjectSelectionCurriculumValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var subjectSelectionCurriculumData = await _dbContext.Entity<LtSubjectSelectionCurriculum>()
                    .Include(x => x.MappingCurriculumGrades)
                    .Where(x => x.Id == param.IdSubjectSelectionCurriculum)
                    .FirstOrDefaultAsync(CancellationToken);

                if (subjectSelectionCurriculumData.MappingCurriculumGrades.Any())
                {
                    throw new Exception("Curriculum is already mapped with grades");
                }

                subjectSelectionCurriculumData.IsActive = false;

                _dbContext.Entity<LtSubjectSelectionCurriculum>().Update(subjectSelectionCurriculumData);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                _transaction?.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }
    }
}
