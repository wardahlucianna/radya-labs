using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffInfoUpdate;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Employee.FnStaff.StaffInfoUpdate
{
    public class UpdateStaffEducationHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;

        public UpdateStaffEducationHandler(IEmployeeDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var bodys = await Request.GetBody<IEnumerable<UpdateStaffInfoUpdate>>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var body in bodys)
                {
                    var getdata = await _dbContext.Entity<TrStaffEducationInformation>().Where(p => p.IdStaffEducation == body.Constraint3Value && p.IdBinusian == body.IdBinusian).FirstOrDefaultAsync();
                    if (getdata is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id Staff Education"], "Id", body.Constraint3Value));

                    var getdataStaffInfo = await _dbContext.Entity<TrStaffInfoUpdate>()
                                            .Where(p => p.Constraint3Value == body.Constraint3Value && p.IdBinusian == body.IdBinusian && p.TableName == body.TableName && p.FieldName == body.FieldName)
                                            .FirstOrDefaultAsync();
                    if (getdataStaffInfo is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id Staff Education"], "Id", body.Constraint3Value));

                    getdataStaffInfo.IdApprovalStatus = body.IdApprovalStatus;
                    getdataStaffInfo.Notes = body.Notes;
                    getdataStaffInfo.UserUp = AuthInfo.UserId;
                    getdataStaffInfo.DateUp = _dateTime.ServerTime;
                    getdataStaffInfo.ApprovalDate = _dateTime.ServerTime;

                    _dbContext.Entity<TrStaffInfoUpdate>().Update(getdataStaffInfo);
                    if (body.IdApprovalStatus == 1)
                    {
                        var propType = getdata.GetType().GetProperty(body.FieldName).PropertyType;
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(propType);
                        var convertedObject = converter.ConvertFromString(body.CurrentFieldValue);

                        var propertyInfo = getdata.GetType().GetProperty(body.FieldName);
                        propertyInfo.SetValue(getdata, convertedObject);
                        _dbContext.Entity<TrStaffEducationInformation>().Update(getdata);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
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
