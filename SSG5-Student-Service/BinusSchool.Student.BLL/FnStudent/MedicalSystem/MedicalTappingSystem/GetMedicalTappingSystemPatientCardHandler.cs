using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Attendance;
using BinusSchool.Persistence.StudentDb.Entities.Employee;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class GetMedicalTappingSystemPatientCardHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;
        private readonly UpdateMedicalTappingSystemPatientStatusHandler _tapping;

        public GetMedicalTappingSystemPatientCardHandler(IStudentDbContext context, IMachineDateTime time, UpdateMedicalTappingSystemPatientStatusHandler tapping)
        {
            _context = context;
            _time = time;
            _tapping = tapping;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalTappingSystemPatientCardRequest>
                (nameof(GetMedicalTappingSystemPatientCardRequest.TagId));

            DateTime date = _time.ServerTime;

            var getBinusianId = _context.Entity<MsCard>()
                .Where(a => a.CardID == request.TagId)
                .FirstOrDefault();

            if (getBinusianId == null)
                throw new BadRequestException("User not registered");

            var getMedicalRecord = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdUser == getBinusianId.BinusianID
                    && a.CheckInDateTime.Date == date.Date)
                .OrderByDescending(a => a.CheckInDateTime)
                .FirstOrDefault();

            bool isValid = true;
            if (getMedicalRecord == null)
                isValid = false;
            else
                isValid = getMedicalRecord.CheckInDateTime != null && getMedicalRecord.CheckOutDateTime == null;

            #region School
            var getStudent = _context.Entity<MsStudent>()
                .Where(a => a.Id == getBinusianId.BinusianID)
                .Select(a => a.IdSchool)
                .FirstOrDefault();

            var getStaff = _context.Entity<MsStaff>()
                .Where(a => a.IdBinusian == getBinusianId.BinusianID)
                .Select(a => a.IdSchool)
                .FirstOrDefault();

            var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                .Where(a => a.Id == getBinusianId.BinusianID)
                .Select(a => a.IdSchool)
                .FirstOrDefault();

            var idSchool = getStudent ?? getStaff ?? getOtherPatient ?? throw new BadRequestException("User not registered on any school");
            #endregion

            var response = await _tapping.UpdateMedicalTappingSystemPatientStatus(new UpdateMedicalTappingSystemPatientStatusRequest
            {
                IdSchool = idSchool,
                IdBinusian = AESCBCEncryptionUtil.EncryptBase64Url($"{getBinusianId.BinusianID}#{date.ToString("ddMMyyyy")}"),
                Status = isValid == true ? 0 : 1 // 0 checkout, 1 checkin
            });

            return Request.CreateApiResult2(response as object);
        }
    }
}
