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
using BinusSchool.Data.Model.Employee.FnStaff.Teacher;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using BinusSchool.Employee.FnStaff.Teacher.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Employee.FnStaff.Teacher
{
    public class UpdateExpatriateFormalitiesHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public UpdateExpatriateFormalitiesHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            #region Update Staff Expatriate Formalities
            try
            {
                var body = await Request.ValidateBody<UpdateExpatriateFormalitiesRequest, UpdateExpatriateFormalitiesValidator>();
                var getdata = await _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == body.IdBinusian).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Staff Expatriate Formalities With Id Binusian"], "Id", body.IdBinusian));

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Using Approval
                //var newBody = new SetExpatriateFormalities
                //{
                //    IdKITASStatus = new ItemValueVm
                //    {
                //        Id = body.IdKITASStatus,
                //        Description = body.IdKITASStatusDesc
                //    },
                //    IdIMTASchoolLevel = new ItemValueVm
                //    {
                //        Id = body.IdIMTASchoolLevel,
                //        Description = body.IdIMTASchoolLevelDesc
                //    },
                //    IdIMTAMajorAssignPosition = new ItemValueVm
                //    {
                //        Id = body.IdIMTAMajorAssignPosition,
                //        Description = body.IdIMTAMajorAssignPositionDesc
                //    },
                //    KITASNumber = body.KITASNumber,
                //    KITASSponsor = body.KITASSponsor,
                //    KITASExpDate = body.KITASExpDate,
                //    IMTANumber = body.IMTANumber,
                //    IMTAExpDate = body.IMTAExpDate
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
                //        Constraint3 = "IdSchool",
                //        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                //        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                //        Constraint1Value = "Update",
                //        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? (setVal.GetType().GetProperty("Description").GetValue(setVal, null) == null ? null : setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString()  ): null,
                //        Constraint3Value = getdata.IdSchool,
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
                getdata.IdKITASStatus = body.IdKITASStatus;
                getdata.IdIMTASchoolLevel = body.IdIMTASchoolLevel;
                getdata.IdIMTAMajorAssignPosition = body.IdIMTAMajorAssignPosition;
                getdata.KITASNumber = body.KITASNumber;
                getdata.KITASSponsor = body.KITASSponsor;
                getdata.KITASExpDate = body.KITASExpDate;
                getdata.IMTANumber = body.IMTANumber;
                getdata.IMTAExpDate = body.IMTAExpDate;

                _dbContext.Entity<MsStaff>().Update(getdata);
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
