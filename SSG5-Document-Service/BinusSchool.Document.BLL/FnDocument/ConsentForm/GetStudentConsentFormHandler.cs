using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.ConsentForm
{
    public class GetStudentConsentFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetStudentConsentFormHandler(IDocumentDbContext dbContext,
              IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentConsentFormRequest>(nameof(GetStudentConsentFormRequest.IdAcademicYear), nameof(GetStudentConsentFormRequest.Semester),  nameof(GetStudentConsentFormRequest.IdParent));

            var GetStudents = _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(y => y.GradePathwayClassroom)
                                        .ThenInclude(y => y.Classroom)
                                    .Include(x => x.Homeroom)
                                        .ThenInclude(y => y.Grade)
                                    .Include(x => x.Student)
                                        .ThenInclude(y => y.StudentParents)
                                    .Where(a => a.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                && a.Homeroom.Semester == param.Semester
                                                && a.Student.StudentParents.Any(b => b.IdParent == param.IdParent)
                                     )
                                    .Select(a => new
                                    {
                                        a.Homeroom.IdAcademicYear,
                                        a.Homeroom.Semester,
                                        StudentName = (a.Student.FirstName == null ? "" : a.Student.FirstName + " ") + a.Student.LastName,
                                        a.IdStudent,
                                        IdHomeroomStudent = a.Id,
                                        Class = a.Homeroom.Grade.Description + " " + a.Homeroom.GradePathwayClassroom.Classroom.Code,
                                        a.Homeroom.IdGrade
                                    })
                                    .ToList()
                                    .GroupJoin(_dbContext.Entity<TrBLPGroupStudent>().Include(x => x.BLPStatus).Include(x => x.BLPGroup),
                                        std => (std.IdAcademicYear, std.Semester, std.IdStudent),
                                        gs => (gs.IdAcademicYear, gs.Semester, gs.IdStudent),
                                        (std, gs) => new { std, gs })
                                    .SelectMany(
                                        x => x.gs.DefaultIfEmpty(),
                                        (x, y) => new { x.std, y })
                                    .Select(a => new GetStudentConsentFormResult()
                                    {
                                        IdAcademicYear = a.std.IdAcademicYear,
                                        Semester = a.std.Semester,
                                        IdStudent = a.std.IdStudent,
                                        StudentName = a.std.StudentName,
                                        IdHomeroomStudent = a.std.IdHomeroomStudent,
                                        IdGrade = a.std.IdGrade,
                                        Class = a.std.Class,
                                        BLPStatus = a.y?.BLPStatus?.BLPStatusName??"-",
                                        Group = a.y?.BLPGroup?.GroupName?? "-",
                                    })
                                    .ToList();

            return Request.CreateApiResult2(GetStudents as object);

        }
    }
}
