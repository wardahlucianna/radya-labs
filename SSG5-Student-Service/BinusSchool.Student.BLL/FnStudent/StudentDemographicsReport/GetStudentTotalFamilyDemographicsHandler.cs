using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentTotalFamilyDemographicsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentTotalFamilyDemographicsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentTotalFamilyDemographicsRequest>();

            var items = new List<GetStudentTotalFamilyDemographicsResult>();

            var getHomeroomStudents = _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel.MsAcademicYear)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                    && (param.Level == null ? true : param.Level.Any(b => b == a.Homeroom.Grade.IdLevel))
                    && (param.Grade == null ? true : param.Grade.Any(b => b == a.Homeroom.IdGrade))
                    && (param.Semester == null ? true : a.Semester == param.Semester))
                .ToList();

            if (param.Semester != null)
            {
                var getSiblingGroup = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(a => getHomeroomStudents.Select(b => b.IdStudent).Any(b => b == a.IdStudent))
                    .ToListAsync(CancellationToken);

                var getAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                    .Where(a => a.Id == param.IdAcademicYear)
                    .FirstOrDefaultAsync(CancellationToken);

                var result = new GetStudentTotalFamilyDemographicsResult
                {
                    Semester = param.Semester,
                    AcademicYear = new ItemValueVm
                    {
                        Id = getAcademicYear.Id,
                        Description = getAcademicYear.Description
                    },
                    TotalFamily = getSiblingGroup.Count()
                };

                items.Add(result);
            }
            else
            {
                for (int i = 1; i <= 2; i++)
                {
                    var homeroomStudent = getHomeroomStudents.Where(a => a.Semester == i).ToList();

                    var getSiblingGroup = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(a => homeroomStudent.Select(b => b.IdStudent).Any(b => b == a.IdStudent))
                    .ToListAsync(CancellationToken);

                    var getAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                        .Where(a => a.Id == param.IdAcademicYear)
                        .FirstOrDefaultAsync(CancellationToken);

                    var result = new GetStudentTotalFamilyDemographicsResult
                    {
                        Semester = i,
                        AcademicYear = new ItemValueVm
                        {
                            Id = getAcademicYear.Id,
                            Description = getAcademicYear.Description
                        },
                        TotalFamily = getSiblingGroup.Count()
                    };

                    items.Add(result);
                }
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
