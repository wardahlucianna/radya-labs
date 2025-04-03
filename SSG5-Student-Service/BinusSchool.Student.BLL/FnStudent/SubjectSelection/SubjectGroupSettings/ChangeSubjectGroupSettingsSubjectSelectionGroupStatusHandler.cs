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
    public class ChangeSubjectGroupSettingsSubjectSelectionGroupStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public ChangeSubjectGroupSettingsSubjectSelectionGroupStatusHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<ChangeSubjectGroupSettingsSubjectSelectionGroupStatusRequest, ChangeSubjectGroupSettingsSubjectSelectionGroupStatusValidator>();

            await ChangeSubjectGroupSettingsSubjectSelectionGroupStatus(request);

            return Request.CreateApiResult2();
        }

        public async Task ChangeSubjectGroupSettingsSubjectSelectionGroupStatus(ChangeSubjectGroupSettingsSubjectSelectionGroupStatusRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var subjectSelectionGroup = await _context.Entity<LtSubjectSelectionGroup>()
                        .Where(a => a.Id == request.IdSubjectSelectionGroup)
                        .FirstOrDefaultAsync(CancellationToken);

                    subjectSelectionGroup.ActiveStatus = request.ActiveStatus;

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
