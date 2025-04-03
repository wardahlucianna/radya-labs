using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentCertification
{
    public class GetAcadYearByStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetAcadYearByStudentRequest.IdStudent),
        };
        private readonly ISchedulingDbContext _dbContext;

        public GetAcadYearByStudentHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAcadYearByStudentRequest>();
            var predicate = PredicateBuilder.Create<MsStudent>(x => true);
            var gradeByStudent = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                .IgnoreQueryFilters()
                .Where(x => x.IdStudent == param.IdStudent)
                .Select(x => x.IdGrade)
                .ToListAsync();
            var data = await _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Include(x => x.MsGrades)
                    .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                .IgnoreQueryFilters()
                .Where(x => x.MsGrades.Any(x => gradeByStudent.Any(y=> y == x.Id)))
                .OrderBy(x => x.AcademicYear.Code.Length).ThenBy(x => x.AcademicYear.Code)
                .Select(x => new GetAcadYearByStudentResult2
                {
                    Level = new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                    },
                    Grades = x.MsGrades.Where(x=>gradeByStudent.Any(y=>y == x.Id)).Select(y => new GradeByStudent
                    {
                        Id = y.Id,
                        Code = y.Code,
                        Description = y.Description,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = y.Level.IdAcademicYear,
                            Code = y.Level.AcademicYear.Code,
                            Description = y.Level.AcademicYear.Description
                        }
                    }).ToList()
                }).ToListAsync();
            return Request.CreateApiResult2(data as object);
        }
    }
}
