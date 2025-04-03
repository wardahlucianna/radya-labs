using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using System.Text.RegularExpressions;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor
{
    public class SaveMedicalDoctorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveMedicalDoctorHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalDoctorRequest, SaveMedicalDoctorValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (!StringUtil.IsValidEmailAddress(param.DoctorEmail))
                {
                    throw new BadRequestException("Invalid email format");
                }

                if (param.IdMedicalDoctor == null)
                {
                    var insertData = new MsMedicalDoctor
                    {
                        Id = Guid.NewGuid().ToString(),
                        DoctorName = param.DoctorName,
                        DoctorAddress = param.DoctorAddress,
                        DoctorEmail = param.DoctorEmail,
                        DoctorPhoneNumber = param.DoctorPhoneNumber,
                        IdMedicalHospital = param.IdMedicalHospital,
                        IdSchool = param.IdSchool
                    };

                    _dbContext.Entity<MsMedicalDoctor>().Add(insertData);
                }
                else
                {
                    var updateData = await _dbContext.Entity<MsMedicalDoctor>()
                        .Where(x => x.Id == param.IdMedicalDoctor && x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical doctor not found");

                    updateData.DoctorName = param.DoctorName;
                    updateData.DoctorAddress = param.DoctorAddress;
                    updateData.DoctorEmail = param.DoctorEmail;
                    updateData.DoctorPhoneNumber = param.DoctorPhoneNumber;
                    updateData.IdMedicalHospital = param.IdMedicalHospital;

                    _dbContext.Entity<MsMedicalDoctor>().Update(updateData);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
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

            return Request.CreateApiResult2();
        }
    }
}
