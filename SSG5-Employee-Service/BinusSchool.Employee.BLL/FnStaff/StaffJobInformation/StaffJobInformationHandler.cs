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
using BinusSchool.Data.Model.Employee.FnStaff.StaffJobInformation;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using BinusSchool.Employee.FnStaff.StaffJobInformation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Employee.FnStaff.StaffJobInformation
{
    public class StaffJobInformationHandler : FunctionsHttpCrudHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public StaffJobInformationHandler(IEmployeeDbContext employeeDbContext)
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

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            #region Update Staff Job Information
            try
            {
                var body = await Request.ValidateBody<UpdateStaffJobInformationRequest, UpdateStaffJobInformationValidator>();

                var getdata = await _dbContext.Entity<MsStaffJobInformation>().Include(x => x.Staff).Where(x => x.IdBinusian == body.IdBinusian).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Staff Job Information With Id Binusian"], "Id", body.IdBinusian));

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Using Approval
                //var newBody = new SetStaffJobInformation
                //{
                //    IdEmployeeStatus = new ItemValueVm
                //    {
                //        Id = body.IdEmployeeStatus,
                //        Description = body.IdEmployeeStatusDesc
                //    },
                //    IdPTKType = new ItemValueVm
                //    {
                //        Id = body.IdPTKType,
                //        Description = body.IdPTKTypeDesc
                //    },
                //    IdExpSpecialTreatments = new ItemValueVm
                //    {
                //        Id = body.IdExpSpecialTreatments,
                //        Description = body.IdExpSpecialTreatmentsDesc
                //    },
                //    IdLabSkillsLevel = new ItemValueVm
                //    {
                //        Id = body.IdLabSkillsLevel,
                //        Description = body.IdLabSkillsLevelDesc
                //    },
                //    IdIsyaratLevel = new ItemValueVm
                //    {
                //        Id = body.IdIsyaratLevel,
                //        Description = body.IdIsyaratLevelDesc
                //    },
                //    IdBrailleExpLevel = new ItemValueVm
                //    {
                //        Id = body.IdBrailleExpLevel,
                //        Description = body.IdBrailleExpLevelDesc
                //    },
                //    SubjectSpecialization = body.SubjectSpecialization,
                //    TeacherDurationWeek = body.TeacherDurationWeek,
                //    NUPTK = body.NUPTK,
                //    NoSrtKontrak = body.NoSrtKontrak,
                //    NoIndukGuruKontrak = body.NoIndukGuruKontrak,
                //    IsPrincipalLicensed = body.IsPrincipalLicensed,
                //    AdditionalTaskNotes = body.AdditionalTaskNotes
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
                //        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? (setVal.GetType().GetProperty("Description").GetValue(setVal, null) == null ? null : setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString()) : null,
                //        Constraint3Value = getdata.Staff.IdSchool == null ? null : getdata.Staff.IdSchool,
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
                getdata.IdEmployeeStatus = body.IdEmployeeStatus;
                getdata.IdPTKType = body.IdPTKType;
                getdata.IdExpSpecialTreatments = body.IdExpSpecialTreatments;
                getdata.IdLabSkillsLevel = body.IdLabSkillsLevel;
                getdata.IdIsyaratLevel = body.IdIsyaratLevel;
                getdata.IdBrailleExpLevel = body.IdBrailleExpLevel;
                getdata.SubjectSpecialization = body.SubjectSpecialization;
                getdata.TeacherDurationWeek = body.TeacherDurationWeek;
                getdata.NoSrtPengangkatan = body.NoSrtPengangkatan;
                getdata.TglSrtPengangkatan = body.TglSrtPengangkatan;
                getdata.NUPTK = body.NUPTK;
                getdata.NoSrtKontrak = body.NoSrtKontrak;
                getdata.NoIndukGuruKontrak = body.NoIndukGuruKontrak;
                getdata.IsPrincipalLicensed = body.IsPrincipalLicensed;
                getdata.AdditionalTaskNotes = body.AdditionalTaskNotes;
                getdata.TglSrtKontrakKerja = body.TglSrtKontrakKerja;
                _dbContext.Entity<MsStaffJobInformation>().Update(getdata);
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
