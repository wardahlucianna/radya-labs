using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate
{
    public class GetDetailSummaryByClassHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        private readonly IStudentHomeroomDetail _homeroomService;
        private readonly IClassroomMap _classroomMapService;

        public GetDetailSummaryByClassHandler(IStudentDbContext studentDbContext
            , IConfiguration configuration
            , IStudentHomeroomDetail homeroomService
            , IClassroomMap classroomMapService)
        {
            _dbContext = studentDbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _homeroomService = homeroomService;
            _classroomMapService = classroomMapService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailApprovalSummaryRequest>(nameof(GetDetailApprovalSummaryRequest.IdGrade),
                nameof(GetDetailApprovalSummaryRequest.IdClass), nameof(GetDetailApprovalSummaryRequest.IsParentUpdate));

            var idStudents = await _dbContext.Entity<TrStudentInfoUpdate>()
                .Where( x => x.IdApprovalStatus == _newEntryApproval && x.IsParentUpdate == param.IsParentUpdate)
                .Select(x => x.Constraint3Value)
                .Distinct()
                .ToListAsync();

            var nameStudent = await _dbContext.Entity<MsStudent>()
                .Select(x => new { x.Id, x.FirstName, x.LastName })
                .Distinct()
                .ToListAsync();

            List<string> Grades = new List<string>();
            Grades.Add(param.IdGrade);

            var listInfoApproval = await _dbContext.Entity<TrStudentInfoUpdate>()
                .Where(x => x.IdApprovalStatus == _newEntryApproval && x.IsParentUpdate == param.IsParentUpdate)
                .Select(x => new
                {
                    IdStudent = x.Constraint3Value,
                    IsParentUpdate = x.IsParentUpdate,
                    RequestUpdate = x.DateIn
                })
                .ToListAsync();

            var homerooms = Enumerable.Empty<GetGradeAndClassByStudentResult>();
            var homeroomsParam = new GetHomeroomByStudentRequest
            {
                Ids = idStudents,
                IdGrades = Grades
            };
            var homeroomsResult = await _homeroomService.GetGradeAndClassByStudents(homeroomsParam);
            homerooms = homeroomsResult.Payload;

            var crMapResult = await _classroomMapService.GetClassroomMappedsByGrade(new GetListGradePathwayClassRoomRequest
            {
                Ids = Grades
            });

            var templatesiswa = homeroomsResult.Payload.Where(x => x.Classroom.Id == param.IdClass)
                .Select(x => new
            {
                IdGrade = x.Grade.Id,
                Grade = x.Grade.Description,
                IdKelas = x.Classroom.Id,
                kelas = x.Classroom.Code,
                siswa = x.IdStudent
            }).ToList();

            var getNameStudent = templatesiswa
                .Join(nameStudent,
                templatesiswa => templatesiswa.siswa,
                nameStudent => nameStudent.Id,
                (templatesiswa, nameStudent) => new
                {
                    Grade = templatesiswa.Grade,
                    kelas = templatesiswa.kelas,
                    IdStudent = templatesiswa.siswa,
                    NamaStudent = nameStudent.FirstName + " " + nameStudent.LastName
                    //NamaStudent = (nameStudent.LastName.Contains(" ") == true ? nameStudent.FirstName + " " + nameStudent.LastName.Substring(0, nameStudent.LastName.LastIndexOf(' ')).TrimEnd() : nameStudent.FirstName.Trim())
                    //        + " " + (nameStudent.LastName.Contains(" ") == true ? nameStudent.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : nameStudent.LastName.Trim())

                }).ToList();

            var listStudentUpdate = listInfoApproval
                .Join(getNameStudent,
                listInfoApproval => listInfoApproval.IdStudent,
                getNameStudent => getNameStudent.IdStudent,
                (listInfoApproval, getNameStudent) => new
                {
                    Grade = getNameStudent.Grade,
                    kelas = getNameStudent.kelas,
                    IdStudent = getNameStudent.IdStudent,
                    NameStudent = getNameStudent.NamaStudent,
                    IsParent = listInfoApproval.IsParentUpdate,
                    RequestDate = listInfoApproval.RequestUpdate
                }).ToList();

            var summaryRequest = listStudentUpdate.GroupBy(x => new { x.IdStudent, x.kelas })
                .Select(g => new GetDetailApprovalSummaryResult
                {
                    IdStudent = g.First().IdStudent,
                    Grade = g.First().Grade,
                    Class = g.First().kelas,
                    StudentName = g.First().NameStudent,
                    RequestDate = g.First().RequestDate,
                    RequestApproval = g.Count()

                }).ToList();

            return Request.CreateApiResult2(summaryRequest as object);
        }
    }
}
