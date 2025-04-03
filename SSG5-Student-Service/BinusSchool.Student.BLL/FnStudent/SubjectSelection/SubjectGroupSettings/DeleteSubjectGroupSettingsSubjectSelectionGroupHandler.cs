using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings.Validator;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class DeleteSubjectGroupSettingsSubjectSelectionGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public DeleteSubjectGroupSettingsSubjectSelectionGroupHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<DeleteSubjectGroupSettingsSubjectSelectionGroupRequest, DeleteSubjectGroupSettingsSubjectSelectionGroupValidator>();

            await DeleteSubjectGroupSettingsSubjectSelectionGroup(request);
            
            return Request.CreateApiResult2();
        }

        public async Task DeleteSubjectGroupSettingsSubjectSelectionGroup(DeleteSubjectGroupSettingsSubjectSelectionGroupRequest request)
        {
            using (_transaction =  await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    bool alreadyUsed = _context.Entity<MsMappingCurriculumSubjectGroup>()
                        .Where(a => a.IdSubjectSelectionGroup ==  request.IdSubjectSelectionGroup)
                        .Any();

                    if (alreadyUsed)
                        throw new BadRequestException($"Cannot delete if Subject Group has been mapped.");

                    var subjectSelectionGroup = await _context.Entity<LtSubjectSelectionGroup>()
                        .Where(a => a.Id == request.IdSubjectSelectionGroup)
                        .FirstOrDefaultAsync(CancellationToken);

                    subjectSelectionGroup.IsActive = false;

                    _context.Entity<LtSubjectSelectionGroup>().Update(subjectSelectionGroup);

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    _transaction?.RollbackAsync(CancellationToken);
                    throw new Exception($"{ex.Message.ToString()}{Environment.NewLine}{ex.InnerException?.Message?.ToString() ?? ""}");
                }
            }
        }
    }
}
