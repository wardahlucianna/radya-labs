using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryOtherPatientListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;

        private static readonly string[] _columns = new[]
        {
            "IdUser",
            "Name",
            "BirthDate",
            "PhoneNumber"
        };

        public GetMedicalRecordEntryOtherPatientListHandler(IStudentDbContext context, IMachineDateTime time)
        {
            _context = context;
            _time = time;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalRecordEntryOtherPatientListRequest>
                (nameof(GetMedicalRecordEntryOtherPatientListRequest.IdSchool));

            var response = new List<GetMedicalRecordEntryOtherPatientListResponse>();
            DateTime today = _time.ServerTime;

            var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToList();

            var insertOtherPatient = getOtherPatient
                .Select(a => new GetMedicalRecordEntryOtherPatientListResponse
                {
                    IdUser = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.Id}#{today.ToString("ddMMyyyy")}"),
                        Description = a.Id
                    },
                    Name = a.MedicalOtherUsersName,
                    BirthDate = a.BirthDate,
                    PhoneNumber = a.PhoneNumber,
                })
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                insertOtherPatient = insertOtherPatient
                    .Where(a => a.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.IdUser.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.BirthDate.ToShortDateString().Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.PhoneNumber.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            response.AddRange(insertOtherPatient);

            response = request.OrderBy switch
            {
                "IdUser" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.IdUser.Description).ToList()
                    : response.OrderByDescending(a => a.IdUser.Description).ToList(),
                "Name" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Name).ToList()
                    : response.OrderByDescending(a => a.Name).ToList(),
                "BirthDate" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.BirthDate).ToList()
                    : response.OrderByDescending(a => a.BirthDate).ToList(),
                "PhoneNumber" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.PhoneNumber).ToList()
                    : response.OrderByDescending(a => a.PhoneNumber).ToList(),
                _ => response.OrderBy(a => a.Name).ToList()
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdUser).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
