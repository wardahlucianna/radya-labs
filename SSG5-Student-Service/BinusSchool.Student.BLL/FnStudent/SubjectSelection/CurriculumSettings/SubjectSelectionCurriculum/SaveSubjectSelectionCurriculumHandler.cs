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
    public class SaveSubjectSelectionCurriculumHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveSubjectSelectionCurriculumHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveSubjectSelectionCurriculumRequest, SaveSubjectSelectionCurriculumValidator>();

            var isCurriculumNameExists = await _dbContext.Entity<LtSubjectSelectionCurriculum>()
                .Where(x => x.CurriculumName == param.CurriculumName &&
                            x.IdSchool == param.IdSchool &&
                            (string.IsNullOrWhiteSpace(param.IdSubjectSelectionCurriculum) || x.Id != param.IdSubjectSelectionCurriculum))
                .AnyAsync(CancellationToken);

            if (isCurriculumNameExists)
            {
                throw new Exception($"Curriculum Name {param.CurriculumName} exists");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                //Update
                if (!string.IsNullOrWhiteSpace(param.IdSubjectSelectionCurriculum))
                {
                    var subjectSelectionCurriculumData = await _dbContext.Entity<LtSubjectSelectionCurriculum>()
                        .Where(x => x.Id == param.IdSubjectSelectionCurriculum && x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (subjectSelectionCurriculumData == null)
                    {
                        throw new Exception("Subject Selection Curriculum doesn't exists");
                    }

                    subjectSelectionCurriculumData.CurriculumName = param.CurriculumName;

                    _dbContext.Entity<LtSubjectSelectionCurriculum>().Update(subjectSelectionCurriculumData);
                }
                //Create
                else
                {
                    var subjectSelectionCurriculumData = new LtSubjectSelectionCurriculum
                    {
                        Id = Guid.NewGuid().ToString(),
                        CurriculumName = param.CurriculumName,
                        IdSchool = param.IdSchool
                    };

                    _dbContext.Entity<LtSubjectSelectionCurriculum>().Add(subjectSelectionCurriculumData);
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
