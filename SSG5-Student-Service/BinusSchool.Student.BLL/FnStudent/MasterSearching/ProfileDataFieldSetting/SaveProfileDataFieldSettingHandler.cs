using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class SaveProfileDataFieldSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveProfileDataFieldSettingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SaveProfileDataFieldSettingRequest>();

            if (string.IsNullOrEmpty(request.IdBinusian))
                throw new BadRequestException("IdBinusian cannot be null");

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var checkDataField = await _dbContext.Entity<TrProfileDataFieldPrivilege>()
                        .Where(a => a.IdBinusian == request.IdBinusian)
                        .ToListAsync(CancellationToken);

                var addBinusianAccessField = request.IdProfileDataField
                    .Where(a => checkDataField.All(b => b.IdProfileDataField != a))
                    .Select(a => a)
                    .ToList();

                var deleteBinusianAccessField = checkDataField
                    .Where(a => request.IdProfileDataField.All(b => b != a.IdProfileDataField))
                    .Select(a => a.IdProfileDataField)
                    .ToList();

                // Delete Access
                foreach (var deleteField in deleteBinusianAccessField)
                {
                    var deleteDataField = checkDataField
                        .Where(a => a.IdProfileDataField == deleteField)
                        .SingleOrDefault();

                    _dbContext.Entity<TrProfileDataFieldPrivilege>().Remove(deleteDataField);
                }

                // Add Access
                foreach (var addField in addBinusianAccessField)
                {
                    var getProfileDataFieldGroup = await _dbContext.Entity<MsProfileDataField>()
                        .Where(a => a.Id == addField)
                        .FirstOrDefaultAsync(CancellationToken);

                    var insertNewAccess = new TrProfileDataFieldPrivilege()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBinusian = request.IdBinusian,
                        IdProfileDataField = addField,
                        IdProfileDataFieldGroup = getProfileDataFieldGroup.IdProfileDataFieldGroup
                    };

                    _dbContext.Entity<TrProfileDataFieldPrivilege>().Add(insertNewAccess);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2(code: HttpStatusCode.Created);

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
        }
    }
}
