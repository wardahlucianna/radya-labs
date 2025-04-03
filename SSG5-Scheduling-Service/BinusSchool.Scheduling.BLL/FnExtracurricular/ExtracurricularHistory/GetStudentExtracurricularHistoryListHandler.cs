using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularHistory;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularHistory
{
    public class GetStudentExtracurricularHistoryListHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentExtracurricularHistoryListHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentExtracurricularHistoryListRequest>
                (nameof(GetStudentExtracurricularHistoryListRequest.IdStudent));

            var getStudentExtracurricularHistory = _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.Level.AcademicYear)
                .Include(a => a.Homeroom.GradePathwayClassroom.Classroom)
                .Where(a => a.IdStudent == param.IdStudent)
                .Join(_dbContext.Entity<MsExtracurricularParticipant>()
                    .Where(x => x.Extracurricular.Status == true),
                    hs => new { hs.IdStudent, hs.Semester, hs.Homeroom.Grade.Level.IdAcademicYear },
                    ep => new { ep.IdStudent, ep.Extracurricular.Semester, ep.Grade.Level.IdAcademicYear },
                    (hs, ep) => new { hs, ep })
                .Select(a => new
                {
                    IdAcademicYear = a.hs.Homeroom.Grade.Level.IdAcademicYear,
                    AYCode = a.hs.Homeroom.Grade.Level.AcademicYear.Description,
                    Semester = a.hs.Semester,
                    IdHomeroom = a.hs.IdHomeroom,
                    HomeroomCode = a.hs.Homeroom.Grade.Code + a.hs.Homeroom.GradePathwayClassroom.Classroom.Code,
                    IdStudent = a.hs.IdStudent,
                    StudentName = NameUtil.GenerateFullNameWithId(a.hs.IdStudent, a.hs.Student.FirstName, a.hs.Student.MiddleName, a.hs.Student.LastName),
                    IdExtracurricular = a.ep.IdExtracurricular,
                    ExtracurricularCode = a.ep.Extracurricular.Name.Trim(),
                    Status = a.ep.Status
                })
                .Where(a => (string.IsNullOrWhiteSpace(param.IdAcademicYear) ? true : a.IdAcademicYear == param.IdAcademicYear)
                    && (param.Semester == null ? true : a.Semester == param.Semester)
                    && a.Status == true)
                .OrderByDescending(a => a.AYCode)
                    .ThenBy(a => a.Semester)
                    .ThenBy(a => a.HomeroomCode)
                    .ThenBy(a => a.ExtracurricularCode)
                .ToList();

            var items = getStudentExtracurricularHistory.GroupBy(a => new
            {
                a.IdAcademicYear,
                a.AYCode,
                a.Semester,
                a.IdHomeroom,
                a.HomeroomCode,
                a.IdStudent,
                a.StudentName
            })
                .Select(b => new GetStudentExtracurricularHistoryListResult
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = b.Key.IdAcademicYear,
                        Description = b.Key.AYCode
                    },
                    Semester = b.Key.Semester,
                    Homeroom = new ItemValueVm
                    {
                        Id = b.Key.IdHomeroom,
                        Description = b.Key.HomeroomCode
                    },
                    Student = new ItemValueVm
                    {
                        Id = b.Key.IdStudent,
                        Description = b.Key.StudentName
                    },
                    Extracurricular = b.Select(c => new ItemValueVm
                    {
                        Id = c.IdExtracurricular,
                        Description = c.ExtracurricularCode
                    })
                    .ToList()
                })
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
