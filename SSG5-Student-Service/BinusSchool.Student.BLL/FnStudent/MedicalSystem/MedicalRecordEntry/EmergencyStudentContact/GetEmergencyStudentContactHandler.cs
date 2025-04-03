using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact
{
    public class GetEmergencyStudentContactHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetEmergencyStudentContactHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var idStudent = MedicalDecryptionValidation.ValidateDecryptionData(param.Ids.FirstOrDefault());
            //var idStudent = param.Ids.FirstOrDefault();

            var emergencyStudentContact = await _dbContext.Entity<MsStudentParent>()
                .Include(x => x.Parent).ThenInclude(x => x.ParentRole)
                .Include(x => x.Student)
                .Where(x => x.IdStudent == idStudent)
                .Select(x => new GetEmergencyStudentContactResult
                {
                    ParentName = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                    ParentRole = x.Parent.ParentRole.ParentRoleNameEng,
                    PhoneNumber = GetNotEmptyContactInfo(x.Parent.MobilePhoneNumber1, x.Parent.MobilePhoneNumber2, x.Parent.MobilePhoneNumber3, x.Parent.ResidencePhoneNumber),
                    Email = GetNotEmptyContactInfo(x.Parent.PersonalEmailAddress, x.Parent.WorkEmailAddress),
                    PrimaryEmergencyContact = (x.Student.EmergencyContactRole == x.Parent.IdParentRole)
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(emergencyStudentContact as object);

        }

        private static string GetNotEmptyContactInfo(params string[] contactInfos)
        {
            return contactInfos.Where(x => !string.IsNullOrEmpty(x) && x != "-").FirstOrDefault();
        }
    }
}
