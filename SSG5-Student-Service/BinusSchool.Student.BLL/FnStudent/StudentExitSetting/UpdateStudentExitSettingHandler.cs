using System;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.StudentExitSetting
{
    public class UpdateStudentExitSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _studentDbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;

        public UpdateStudentExitSettingHandler(IStudentDbContext studentDbContext, IMachineDateTime dateTime)
        {
            _studentDbContext = studentDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateStudentExitSettingRequest, UpdateStudentExitSettingValidator>();

                _transaction = await _studentDbContext.BeginTransactionAsync(CancellationToken);
                foreach (var item in body.Request)
                {
                    var queryStudentExitSetting = await _studentDbContext.Entity<MsStudentExitSetting>()
                        .FirstOrDefaultAsync(x => x.IdHomeroomStudent == item.IdHomeroomStudent);
                    if (queryStudentExitSetting == null)
                    {
                        var newStudentExitSetting = new MsStudentExitSetting
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = item.IdHomeroomStudent,
                            IsExit = item.IsExit,
                            IdAcademicYear = item.AcademicYear,
                        };

                        await _studentDbContext.Entity<MsStudentExitSetting>().AddAsync(newStudentExitSetting, CancellationToken);
                    }
                    else
                    {
                        if (queryStudentExitSetting.IsExit != item.IsExit)
                        {
                            queryStudentExitSetting.IsExit = item.IsExit;
                            _studentDbContext.Entity<MsStudentExitSetting>().Update(queryStudentExitSetting);
                        }
                    }
                }

                await _studentDbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
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
