using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class GetMedicalTappingSystemPatientListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IPeriod _period;
        private readonly IMachineDateTime _time;

        private static readonly string[] _columns = new[]
        {
            "IdBinusian",
            "Name",
            "SchoolLevel",
            "Grade",
            "Homeroom",
            "IsCheckIn"
        };

        public GetMedicalTappingSystemPatientListHandler(IStudentDbContext context, IPeriod period, IMachineDateTime time)
        {
            _context = context;
            _period = period;
            _time = time;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalTappingSystemPatientListRequest>
                (nameof(GetMedicalTappingSystemPatientListRequest.IdSchool));

            var response = new List<GetMedicalTappingSystemPatientListResponse>();
            DateTime time = _time.ServerTime;

            var getActiveAY = await _period.GetCurrenctAcademicYear(new CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var getTrMedicalRecord = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdSchool == request.IdSchool
                    && a.CheckInDateTime.Date == time.Date)
                .ToList();

            #region Student Data
            var getStudent = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Where(a => a.Semester == getActiveAY.Payload.Semester
                    && a.Homeroom.Semester == getActiveAY.Payload.Semester
                    && a.Homeroom.Grade.MsLevel.IdAcademicYear == getActiveAY.Payload.Id)
            .ToList();

            var insertStudent = getStudent
                .Select(a => new GetMedicalTappingSystemPatientListResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.Student.Id}#{time.ToString("ddMMyyyy")}"),
                        Description = a.Student.Id
                    },
                    Name = NameUtil.GenerateFullName(a.Student.FirstName, a.Student.LastName),
                    SchoolLevel = new CodeWithIdVm
                    {
                        Id = a.Homeroom.Grade.IdLevel,
                        Description = a.Homeroom.Grade.MsLevel.Description,
                        Code = a.Homeroom.Grade.MsLevel.Code,
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = a.Homeroom.IdGrade,
                        Description = a.Homeroom.Grade.Description,
                        Code = a.Homeroom.Grade.Code,
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = a.IdHomeroom,
                        Description = a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    },
                    IsCheckedIn = getTrMedicalRecord
                        .Where(b => b.IdUser == a.IdStudent)
                        .OrderByDescending(b => b.CheckInDateTime)
                        .FirstOrDefault() is var record && record != null && record.CheckInDateTime != null && record.CheckOutDateTime == null,
                    Mode = "student"
                });

            response.AddRange(insertStudent);
            #endregion

            #region Staff Data
            var getStaff = _context.Entity<MsStaff>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToList();

            var insertStaff = getStaff
                .Select(a => new GetMedicalTappingSystemPatientListResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.IdBinusian}#{time.ToString("ddMMyyyy")}"),
                        Description = a.IdBinusian
                    },
                    Name = NameUtil.GenerateFullName(a.FirstName, a.LastName),
                    SchoolLevel = new CodeWithIdVm(),
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm(),
                    IsCheckedIn = getTrMedicalRecord
                        .Where(b => b.IdUser == a.IdBinusian)
                        .OrderByDescending(b => b.CheckInDateTime)
                        .FirstOrDefault() is var record && record != null && record.CheckInDateTime != null && record.CheckOutDateTime == null,
                    Mode = "staff"
                });

            response.AddRange(insertStaff);
            #endregion

            #region Other Patient
            var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToList();

            var insertOtherPatient = getOtherPatient
                .Select(a => new GetMedicalTappingSystemPatientListResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.Id}#{time.ToString("ddMMyyyy")}"),
                        Description = a.Id
                    },
                    Name = a.MedicalOtherUsersName,
                    SchoolLevel = new CodeWithIdVm(),
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm(),
                    IsCheckedIn = getTrMedicalRecord
                        .Where(b => b.IdUser == a.Id)
                        .OrderByDescending(b => b.CheckInDateTime)
                        .FirstOrDefault() is var record && record != null && record.CheckInDateTime != null && record.CheckOutDateTime == null,
                    Mode = "other"
                });

            response.AddRange(insertOtherPatient);
            #endregion

            response = request.OrderBy switch
            {
                "IdBinusian" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.IdBinusian.Description).ToList()
                    : response.OrderByDescending(a => a.IdBinusian.Description).ToList(),
                "Name" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.Name).ToList()
                    : response.OrderByDescending(a => a.Name).ToList(),
                "SchoolLevel" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.SchoolLevel.Description).ToList()
                    : response.OrderByDescending(a => a.SchoolLevel.Description).ToList(),
                "Grade" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.Grade.Description).ToList()
                    : response.OrderByDescending(a => a.Grade.Description).ToList(),
                "Homeroom" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.Homeroom.Description).ToList()
                    : response.OrderByDescending(a => a.Homeroom.Description).ToList(),
                "IsCheckIn" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.IsCheckedIn).ToList()
                    : response.OrderByDescending(a => a.IsCheckedIn).ToList(),
                _ => response.OrderBy(a => a.Name).ToList()
            };

            if (!string.IsNullOrEmpty(request.Search))
                response = response
                    .Where(a => a.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.IdBinusian.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || (a.SchoolLevel.Description == null ? false : a.SchoolLevel.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                        || (a.Grade.Description == null ? false : a.Grade.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                        || (a.Homeroom.Description == null ? false : a.Homeroom.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdBinusian).Count();

            object items;

            if (request.Return == Common.Model.Enums.CollectionType.Lov)
            {
                items = response
                    .Select(a => new CodeWithIdVm
                    {
                        Id = a.IdBinusian.Id,
                        Code = a.Mode,
                        Description = $"{a.IdBinusian.Description} - {a.Name}"
                    })
                    .ToList();
            }
            else
            {
                items = response
                    .SetPagination(request)
                    .Select(a => new GetMedicalTappingSystemPatientListResponse
                    {
                        IdBinusian = a.IdBinusian,
                        Name = a.Name,
                        SchoolLevel = a.SchoolLevel,
                        Grade = a.Grade,
                        Homeroom = a.Homeroom,
                        IsCheckedIn = a.IsCheckedIn,
                        Mode = a.Mode,
                    })
                    .ToList();
            }

            var result = items is IReadOnlyList<IItemValueVm> itemValues
                ? Request.CreateApiResult2(itemValues as object)
                : Request.CreateApiResult2(items as object, request.CreatePaginationProperty(count).AddColumnProperty(_columns));

            return result;
        }
    }
}
