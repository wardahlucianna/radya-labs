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
    public class DeleteSubjectGroupSettingsCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public DeleteSubjectGroupSettingsCurriculumMappingHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<DeleteSubjectGroupSettingsCurriculumMappingRequest, DeleteSubjectGroupSettingsCurriculumMappingValidator>();

            await DeleteSubjectGroupSettingsCurriculumMapping(request);

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.NoContent);
        }

        public async Task DeleteSubjectGroupSettingsCurriculumMapping(DeleteSubjectGroupSettingsCurriculumMappingRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var mappingCurriculumSubjectGroups = await _context.Entity<MsMappingCurriculumSubjectGroup>()
                        .Where(a => request.IdMappingCurriculumSubjectGroups.Contains(a.Id))
                        .ToListAsync(CancellationToken);

                    mappingCurriculumSubjectGroups.ForEach(a => a.IsActive = false);

                    _context.Entity<MsMappingCurriculumSubjectGroup>().UpdateRange(mappingCurriculumSubjectGroups);

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
