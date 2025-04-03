using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Employee.FnStaff.StaffCertificationInformation;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using BinusSchool.Employee.FnStaff.StaffCertificationInformation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Employee.FnStaff.StaffCertificationInformation
{
    public class DeleteStaffCertificationInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public DeleteStaffCertificationInformationHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            #region Update Staff Certification Information
            try
            {
                var body = await Request.ValidateBody<DeleteStaffCertificationInformationRequest, DeleteStaffCertificationInformationValidator>();
                var getdata = await _dbContext.Entity<TrStaffCertificationInformation>().Where(x => x.IdCertificationStaff == body.IdCertificationStaff).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Certification Staff With IDCertificationStaff"], "Id", body.IdCertificationStaff));

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Using Approval
                //var getdataType = getdata.GetType().ToString();

                //var staffCertificationInfoUpdate = new TrStaffInfoUpdate
                //{
                //    IdBinusian = body.IdBinusian,
                //    DateIn = DateTime.Now,
                //    TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                //    FieldName = "Stsrc",
                //    UserIn = AuthInfo.UserId,
                //    Constraint1 = "action",
                //    Constraint2 = "",
                //    Constraint3 = "IdCertificationType",
                //    OldFieldValue = "true",
                //    CurrentFieldValue = "false",
                //    Constraint1Value = "Delete",
                //    Constraint2Value = "",
                //    Constraint3Value = body.IdCertificationStaff,
                //    RequestedDate = DateTime.Now,
                //    //ApprovalDate = "",
                //    IdApprovalStatus = 4,
                //    //Notes = ""
                //};
                //_dbContext.Entity<TrStaffInfoUpdate>().Add(staffCertificationInfoUpdate);
                #endregion

                #region Skip Approval
                if (getdata != null)
                    getdata.IsActive = false;
                _dbContext.Entity<TrStaffCertificationInformation>().Update(getdata);
                #endregion 

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
            #endregion
        }
    }
}
