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
    public class SaveSubjectGroupSettingsSubjectSelectionGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public SaveSubjectGroupSettingsSubjectSelectionGroupHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveSubjectGroupSettingsSubjectSelectionGroupRequest, SaveSubjectGroupSettingsSubjectSelectionGroupValidator>();

            await SaveSubjectGroupSettingsSubjectSelectionGroup(request);

            return Request.CreateApiResult2();
        }

        public async Task SaveSubjectGroupSettingsSubjectSelectionGroup(SaveSubjectGroupSettingsSubjectSelectionGroupRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                if (string.IsNullOrEmpty(request.IdSubjectSelectionGroup))
                {
                    try
                    {
                        var subjectSelectionGroups = await _context.Entity<LtSubjectSelectionGroup>()
                            .Where(a => a.IdSchool == request.IdSchool)
                            .ToListAsync(CancellationToken);

                        bool similarityCheck = subjectSelectionGroups
                            .Any(a => a.SubjectSelectionGroupName.Trim().Equals(request.Description.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (similarityCheck)
                            throw new BadRequestException($"{request.Description} is already exist.");

                        var insert = new LtSubjectSelectionGroup
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSchool = request.IdSchool,
                            SubjectSelectionGroupName = request.Description.Trim(),
                            ActiveStatus = true,
                        };

                        _context.Entity<LtSubjectSelectionGroup>().Add(insert);

                        await _context.SaveChangesAsync(CancellationToken);
                        await _transaction.CommitAsync(CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _transaction?.RollbackAsync(CancellationToken);
                        throw new Exception($"{ex.Message.ToString()}{Environment.NewLine}{ex.InnerException?.Message?.ToString() ?? ""}");
                    }
                }
                else
                {
                    try
                    {
                        var subjectSelectionGroups = await _context.Entity<LtSubjectSelectionGroup>()
                            .Where(a => a.IdSchool == request.IdSchool
                                && a.Id != request.IdSubjectSelectionGroup)
                            .ToListAsync(CancellationToken);

                        bool similarityCheck = subjectSelectionGroups
                            .Any(a => a.SubjectSelectionGroupName.Trim().Equals(request.Description.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (similarityCheck)
                            throw new BadRequestException($"{request.Description} is already exist.");

                        var update = await _context.Entity<LtSubjectSelectionGroup>()
                            .Where(a => a.Id == request.IdSubjectSelectionGroup)
                            .FirstOrDefaultAsync(CancellationToken);

                        update.SubjectSelectionGroupName = request.Description.Trim();

                        _context.Entity<LtSubjectSelectionGroup>().Update(update);

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
}
