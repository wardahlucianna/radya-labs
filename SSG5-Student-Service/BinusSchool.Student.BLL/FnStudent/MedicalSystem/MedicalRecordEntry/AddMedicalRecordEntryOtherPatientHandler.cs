using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class AddMedicalRecordEntryOtherPatientHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public AddMedicalRecordEntryOtherPatientHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<AddMedicalRecordEntryOtherPatientRequest, AddMedicalRecordEntryOtherPatientValidator>();

            using(_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                    .Where(a => a.IdSchool == request.IdSchool)
                    .Select(a => a.Id)
                    .ToList();

                    var highestOrderNumber = 0;

                    if (getOtherPatient.Any())
                    {
                        highestOrderNumber = getOtherPatient
                            .Select(a => int.Parse(a.Split('_').Last().Substring(2)))
                            .OrderByDescending(a => a)
                            .FirstOrDefault();
                    }

                    int newNumber = highestOrderNumber + 1;

                    string formattedIncrement = newNumber.ToString("D4");

                    var insertOtherPatient = new MsMedicalOtherUsers()
                    {
                        Id = $"{request.IdSchool}_OT{formattedIncrement}",
                        MedicalOtherUsersName = request.Name,
                        BirthDate = request.BirthDate.Date,
                        PhoneNumber = request.PhoneNumber,
                        IdSchool = request.IdSchool,
                    };

                    _context.Entity<MsMedicalOtherUsers>().Add(insertOtherPatient);

                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.Created);
        }
    }
}
