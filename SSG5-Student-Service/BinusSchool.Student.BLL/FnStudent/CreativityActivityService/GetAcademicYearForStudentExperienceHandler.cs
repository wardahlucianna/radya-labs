using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetAcademicYearForStudentExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetAcademicYearForStudentExperienceHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAcademicYearForStudentExperienceRequest>();

            var predicate = PredicateBuilder.Create<MsStudent>(x => true);
            var gradeByStudent = await _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Grade)
                .Include(x => x.Student)
                .IgnoreQueryFilters()
                .Where(x => x.IdStudent == param.IdStudent && (x.Grade.Code.Contains("11") || x.Grade.Code.Contains("12")))
                .Select(x => x.IdGrade)
                .ToListAsync();
            var data = await _dbContext.Entity<MsLevel>()
                .Include(x => x.MsAcademicYear)
                .Include(x => x.MsGrades)
                    .ThenInclude(x => x.MsLevel)
                        .ThenInclude(x => x.MsAcademicYear)
                .IgnoreQueryFilters()
                .Where(x => x.MsGrades.Any(x => gradeByStudent.Any(y=> y == x.Id)))
                .OrderBy(x => x.MsAcademicYear.Code.Length).ThenBy(x => x.MsAcademicYear.Code)
                .Select(x => new GetAcademicYearForStudentExperienceResult
                {
                    Id = x.MsAcademicYear.Id,
                    Code = x.MsAcademicYear.Code,
                    Description = x.MsAcademicYear.Description
                })
                .Distinct()
                .ToListAsync();
            return Request.CreateApiResult2(data as object);
        }
    }
}
