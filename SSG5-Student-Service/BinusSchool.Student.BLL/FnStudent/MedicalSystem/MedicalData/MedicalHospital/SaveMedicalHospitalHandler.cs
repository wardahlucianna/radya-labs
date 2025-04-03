using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital
{
    public class SaveMedicalHospitalHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveMedicalHospitalHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalHospitalRequest, SaveMedicalHospitalValidator>();

            var validEmail = StringUtil.IsValidEmailAddress(param.HospitalEmail);

            Regex regex = new Regex(@"^\d+$");

            if (!validEmail || !regex.IsMatch(param.HospitalPhone))
            {
                throw new Exception("Invalid Input");
            }   
            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.IdHospital == null)
                {
                    var newData = new MsMedicalHospital
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        HospitalName = param.HospitalName,
                        HospitalAddress = param.HospitalAddress,
                        HospitalPhoneNumber = param.HospitalPhone,
                        HospitalEmail = param.HospitalEmail
                    };

                    _dbContext.Entity<MsMedicalHospital>().Add(newData);
                }
                else
                {
                    var editData = await _dbContext.Entity<MsMedicalHospital>()
                        .Where(x => x.Id == param.IdHospital && x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Hospital Not Found");

                    editData.HospitalName = param.HospitalName;
                    editData.HospitalAddress = param.HospitalAddress;
                    editData.HospitalPhoneNumber = param.HospitalPhone;
                    editData.HospitalEmail = param.HospitalEmail;

                    _dbContext.Entity<MsMedicalHospital>().Update(editData);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                await _transaction.RollbackAsync(CancellationToken);
                throw ex;
            }

            return Request.CreateApiResult2();
        }
    }
}
