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
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryStaffListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;

        private static readonly string[] _columns = new[]
        {
            "IdUser",
            "Name",
            "StaffEmail"
        };

        public GetMedicalRecordEntryStaffListHandler(IStudentDbContext context, IMachineDateTime time)
        {
            _context = context;
            _time = time;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalRecordEntryStaffListRequest>
                (nameof(GetMedicalRecordEntryStaffListRequest.IdSchool));

            var response = new List<GetMedicalRecordEntryStaffListResponse>();
            DateTime today = _time.ServerTime;

            var getStaff = _context.Entity<MsStaff>()
                .Where(a => a.IdSchool == request.IdSchool
                    && (request.IdDesignation == null ? true : a.IdDesignation == request.IdDesignation))
                .ToList();

            var insertStaff = getStaff
                .Select(a => new GetMedicalRecordEntryStaffListResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.IdBinusian}#{today.ToString("ddMMyyyy")}"),
                        Description = a.IdBinusian
                    },
                    Name = NameUtil.GenerateFullName(a.FirstName, a.LastName),
                    Email = a.BinusianEmailAddress ?? a.PersonalEmailAddress ?? "-"
                })
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                insertStaff = insertStaff
                    .Where(a => a.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.IdBinusian.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            response.AddRange(insertStaff);

            response = request.OrderBy switch
            {
                "IdUser" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.IdBinusian.Description).ToList()
                    : response.OrderByDescending(a => a.IdBinusian.Description).ToList(),
                "Name" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Name).ToList()
                    : response.OrderByDescending(a => a.Name).ToList(),
                "StaffEmail" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Email).ToList()
                    : response.OrderByDescending(a => a.Email).ToList(),
                _ => response.OrderBy(a => a.Name).ToList()
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdBinusian).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
