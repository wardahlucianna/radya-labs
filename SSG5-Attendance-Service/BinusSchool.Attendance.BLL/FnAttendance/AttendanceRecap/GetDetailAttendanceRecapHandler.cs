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
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class GetDetailAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetDetailAttendanceRecapHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceRecapRequest>();

            var result = await _dbContext.Entity<TrAttendanceSummaryTerm>()
                        .Include(x => x.Grade)
                        .Include(x => x.Level)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.GradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                        .Include(x => x.Student)
                        .Include(x => x.AcademicYear)
                        .Where(x => x.Homeroom.GradePathwayClassroom.Classroom.Id == param.IdHomeroom && x.IdStudent == param.IdStudent)
                        .Select(x => new GetDetailHeaderAttendanceRecapResult
                        {
                            IdAcademicYear = x.IdAcademicYear,
                            AcademicYear = x.AcademicYear.Description,
                            IdStudent = x.IdStudent,
                            StudentName = $"{NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)}",
                            IdHomeroom = x.Homeroom.GradePathwayClassroom.Classroom.Id,
                            Homeroom = $"{x.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                            IdLevel = x.IdLevel,
                            IdGrade = x.IdGrade,
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
