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
using BinusSchool.Common.Utils;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Employee.FnStaff.StaffCertificationInformation
{
    public class StaffCertificationInformationHandler : FunctionsHttpCrudHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public StaffCertificationInformationHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetStaffCertificationInformationRequest>(nameof(GetStaffCertificationInformationRequest.IdBinusian));
            var predicate = PredicateBuilder.Create<TrStaffCertificationInformation>(x => x.IdBinusian == param.IdBinusian);

            var query = _dbContext.Entity<TrStaffCertificationInformation>()
                .Include(x => x.CertificationType)
                .Where(predicate);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetStaffCertificationInformationResult
                    {
                        Id = x.IdCertificationStaff,
                        Description = "Staff Certification Information",
                        IdCertificationStaff = x.IdCertificationStaff,
                        IdBinusian = x.IdBinusian,
                        IdCertificationType = new ItemValueVm
                        {
                            Id = x.IdCertificationType.ToString(),
                            Description = x.CertificationType.CertificationTypeDescriptionEng
                        },
                        CertificationNumber = x.CertificationNumber,
                        CertificationName = x.CertificationName,
                        CertificationYear = x.CertificationYear,
                        IssuedCertifInstitution = x.IssuedCertifInstitution,
                        CertificationExpDate = x.CertificationExpDate
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .Select(x => new GetStaffCertificationInformationResult
                    {
                        Id = x.IdCertificationStaff,
                        Description = "Staff Certification Information",
                        IdCertificationStaff = x.IdCertificationStaff,
                        IdBinusian = x.IdBinusian,
                        IdCertificationType = new ItemValueVm
                        {
                            Id = x.IdCertificationType.ToString(),
                            Description = x.CertificationType.CertificationTypeDescriptionEng
                        },
                        CertificationNumber = x.CertificationNumber,
                        CertificationName = x.CertificationName,
                        CertificationYear = x.CertificationYear,
                        IssuedCertifInstitution = x.IssuedCertifInstitution,
                        CertificationExpDate = x.CertificationExpDate
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdCertificationStaff).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            #region Add Staff Certification Information
            try
            {
                var body = await Request.ValidateBody<AddStaffCertificationInformationRequest, AddStaffCertificationInformationValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExist = await _dbContext.Entity<TrStaffCertificationInformation>()
                .Where(x => x.IdBinusian == body.IdBinusian && x.IdCertificationType == body.IdCertificationType && x.CertificationNumber == body.CertificationNumber /*&& x.IsActive == false*/)
                .FirstOrDefaultAsync(CancellationToken);

                if (isExist != null)
                    throw new BadRequestException($"{body.CertificationNumber} already exist");

                #region Skip Approval
                var staffCertificationInfo = new TrStaffCertificationInformation
                {
                    IdCertificationStaff = Guid.NewGuid().ToString(),
                    IdBinusian = body.IdBinusian,
                    IdCertificationType = body.IdCertificationType,
                    CertificationNumber = body.CertificationNumber,
                    CertificationName = body.CertificationName,
                    CertificationYear = body.CertificationYear,
                    IssuedCertifInstitution = body.IssuedCertifInstitution,
                    CertificationExpDate = body.CertificationExpDate,
                    UserIn = AuthInfo.UserId,
                    DateIn = DateTime.Now
                };

                _dbContext.Entity<TrStaffCertificationInformation>().Add(staffCertificationInfo);
                #endregion

                #region Using Approval
                ////var getdataType = isExist.GetType().ToString();
                //var staffCertificationInfoUpdate = new TrStaffInfoUpdate
                //{
                //    IdBinusian = body.IdBinusian,
                //    DateIn = DateTime.Now,
                //    TableName = "TrStaffCertificationInformation", //getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                //    FieldName = "Stsrc",
                //    UserUp = AuthInfo.UserId,
                //    Constraint1 = "action",
                //    Constraint2 = "",
                //    Constraint3 = "IdCertificationType",
                //    OldFieldValue = "false",
                //    CurrentFieldValue = "true",
                //    Constraint1Value = "Add",
                //    Constraint2Value = "",
                //    Constraint3Value = staffCertificationInfo.IdCertificationStaff,
                //    RequestedDate = DateTime.Now,
                //    //ApprovalDate = "",
                //    IdApprovalStatus = 4
                //    //Notes = ""
                //};
                //_dbContext.Entity<TrStaffInfoUpdate>().Add(staffCertificationInfoUpdate);
                #endregion

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                #region Set False to new Add Certificate
                //var isNewCerExist = await _dbContext.Entity<TrStaffCertificationInformation>()
                //.Where(x => x.IdCertificationStaff == staffCertificationInfo.IdCertificationStaff)
                //.FirstOrDefaultAsync();
                //if (isNewCerExist != null)
                //    isNewCerExist.IsActive = false;
                //    _dbContext.Entity<TrStaffCertificationInformation>().Update(isNewCerExist);
                //    await _dbContext.SaveChangesAsync(CancellationToken);
                #endregion

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

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            #region Update Staff Certification Information
            try
            {
                var body = await Request.ValidateBody<UpdateStaffCertificationInformationRequest, UpdateStaffCertificationInformationValidator>();
                var getdata = await _dbContext.Entity<TrStaffCertificationInformation>().Where(x => x.IdCertificationStaff == body.IdCertificationStaff).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Certification Staff With IDCertificationStaff"], "Id", body.IdCertificationStaff));

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Using Approval
                //var newBody = new SetStaffCertificationInformation
                //{
                //    IdCertificationType = new ItemValueVm
                //    {
                //        Id = body.IdCertificationType.ToString(),
                //        Description = body.CertificationTypeDescriptionEng
                //    },
                //    CertificationNumber = body.CertificationNumber,
                //    CertificationName = body.CertificationName,
                //    CertificationYear = body.CertificationYear,
                //    IssuedCertifInstitution = body.IssuedCertifInstitution,
                //    CertificationExpDate = body.CertificationExpDate
                //};

                //foreach (var prop in newBody.GetType().GetProperties())
                //{
                //    var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                //    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                //    var setVal = newBody.GetType().GetProperty(prop.Name).GetValue(newBody, null);
                //    var propType = prop.PropertyType;
                //    var getdataType = getdata.GetType().ToString();

                //    var staffCertificationInfoUpdate = new TrStaffInfoUpdate
                //    {
                //        IdBinusian = body.IdBinusian,
                //        DateIn = DateTime.Now,
                //        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                //        FieldName = prop.Name,
                //        UserIn = AuthInfo.UserId,
                //        //UserUp = AuthInfo.UserId,
                //        Constraint1 = "action",
                //        Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" : null,
                //        Constraint3 = "IdCertificationStaff",
                //        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                //        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                //        Constraint1Value = "Update",
                //        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? (setVal.GetType().GetProperty("Description").GetValue(setVal, null) == null ? null : setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString()) : null,
                //        Constraint3Value = getdata.IdCertificationStaff,
                //        RequestedDate = DateTime.Now,
                //        //ApprovalDate = "",
                //        IdApprovalStatus = 4,
                //        //Notes = ""
                //    };

                //    if (!(oldVal ?? "").Equals(newVal))
                //        _dbContext.Entity<TrStaffInfoUpdate>().Add(staffCertificationInfoUpdate);
                //}

                #endregion
                #region Skip Approval
                getdata.IdCertificationType = body.IdCertificationType;
                getdata.CertificationNumber = body.CertificationNumber;
                getdata.CertificationName = body.CertificationName;
                getdata.CertificationYear = body.CertificationYear;
                getdata.IssuedCertifInstitution = body.IssuedCertifInstitution;
                getdata.CertificationExpDate = body.CertificationExpDate;
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
