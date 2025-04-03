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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class ChangeSubjectGroupSettingsCurriculumMappingStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public ChangeSubjectGroupSettingsCurriculumMappingStatusHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<ChangeSubjectGroupSettingsCurriculumMappingStatusRequest, ChangeSubjectGroupSettingsCurriculumMappingStatusValidator>();

            await ChangeSubjectGroupSettingsCurriculumMappingStatus(request);

            return Request.CreateApiResult2();
        }

        public async Task ChangeSubjectGroupSettingsCurriculumMappingStatus(ChangeSubjectGroupSettingsCurriculumMappingStatusRequest request)
        {
            using(_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var mappingCurriculumSubjectGroups = await _context.Entity<MsMappingCurriculumSubjectGroup>()
                        .Where(a => request.IdMappingCurriculumSubjectGroups.Contains(a.Id))
                        .ToListAsync(CancellationToken);

                    mappingCurriculumSubjectGroups.ForEach(a => a.ActiveStatus = request.ActiveStatus);

                    _context.Entity<MsMappingCurriculumSubjectGroup>().UpdateRange(mappingCurriculumSubjectGroups);

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch(Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception($"{ex.Message.ToString()}{Environment.NewLine}{ex.InnerException?.Message?.ToString() ?? ""}");
                }
            }
        }
    }
}
