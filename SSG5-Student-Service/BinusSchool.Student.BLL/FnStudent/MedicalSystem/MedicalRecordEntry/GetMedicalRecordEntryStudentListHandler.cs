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
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryStudentListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;

        private static readonly string[] _columns = new[]
        {
            "IdUser",
            "Name",
            "Level",
            "Grade",
            "Homeroom"
        };

        public GetMedicalRecordEntryStudentListHandler(IStudentDbContext context, IMachineDateTime time)
        {
            _context = context;
            _time = time;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalRecordEntryStudentListRequest>
                (nameof(GetMedicalRecordEntryStudentListRequest.IdAcademicYear),
                 nameof(GetMedicalRecordEntryStudentListRequest.Semester));

            var response = new List<GetMedicalRecordEntryStudentListResponse>();

            var getStudent = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == request.IdAcademicYear
                    && a.Semester == request.Semester
                    && a.Homeroom.Semester == request.Semester
                    && (string.IsNullOrEmpty(request.IdLevel) ? true : a.Homeroom.Grade.IdLevel == request.IdLevel)
                    && (string.IsNullOrEmpty(request.IdGrade) ? true : a.Homeroom.IdGrade == request.IdGrade)
                    && (string.IsNullOrEmpty(request.IdHomeroom) ? true : a.IdHomeroom == request.IdHomeroom))
                .ToList();

            var insertStudent = getStudent
                .Select(a => new GetMedicalRecordEntryStudentListResponse
                {
                    Id = AESCBCEncryptionUtil.EncryptBase64Url($"{a.Student.Id}#{_time.ServerTime.ToString("ddMMyyyy")}"),
                    IdStudent = a.Student.Id,
                    Name = NameUtil.GenerateFullName(a.Student.FirstName, a.Student.LastName),
                    Level = new CodeWithIdVm
                    {
                        Id = a.Homeroom.Grade.MsLevel.Id,
                        Description = a.Homeroom.Grade.MsLevel.Description,
                        Code = a.Homeroom.Grade.MsLevel.Code
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = a.Homeroom.Grade.Id,
                        Description = a.Homeroom.Grade.Description,
                        Code = a.Homeroom.Grade.Code
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = a.Homeroom.Id,
                        Description = a.Homeroom.Grade.Code + " " + a.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    }
                }).ToList();

            if (!string.IsNullOrEmpty(request.Search))
            {
                insertStudent = insertStudent
                    .Where(a => a.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.IdStudent.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Level.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Grade.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Homeroom.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            response.AddRange(insertStudent);

            response = request.OrderBy switch
            {
                "IdUser" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.IdStudent).ToList()
                    : response.OrderByDescending(a => a.IdStudent).ToList(),
                "Name" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Name).ToList()
                    : response.OrderByDescending(a => a.Name).ToList(),
                "Level" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Level.Description).ToList()
                    : response.OrderByDescending(a => a.Level.Description).ToList(),
                "Grade" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Grade.Description).ToList()
                    : response.OrderByDescending(a => a.Grade.Description).ToList(),
                "Homeroom" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Homeroom.Description).ToList()
                    : response.OrderByDescending(a => a.Homeroom.Description).ToList(),
                _ => response.OrderBy(a => a.Name).ToList()
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdStudent).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
