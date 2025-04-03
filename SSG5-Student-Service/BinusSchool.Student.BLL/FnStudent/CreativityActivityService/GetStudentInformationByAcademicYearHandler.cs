using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetStudentInformationByAcademicYearHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;

        public GetStudentInformationByAcademicYearHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        private StatusOverallExperienceStudent GetStatusOverall(string IdStudent, List<string> IdAcademicYears)
        {
            var statusOverall = _dbContext.Entity<TrExperienceStudent>()
                                .Include(x => x.AcademicYear)
                                .Select(x => new
                                {
                                    IdStudent = x.IdStudent,
                                    StatusOverall = x.StatusOverall,
                                    AcademicYearCode = x.AcademicYear.Code,
                                    IdAcademicYear = x.IdAcademicYear
                                })
                                .Where(x => x.IdStudent == IdStudent && IdAcademicYears.Contains(x.IdAcademicYear))
                                .OrderByDescending(x => x.AcademicYearCode)
                                .FirstOrDefault();
                                
            if(statusOverall == null)
                return StatusOverallExperienceStudent.OnTrack;


            return statusOverall.StatusOverall;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentInformationByAcademicYearRequest>();

            var dataStudent = await _dbContext.Entity<MsStudent>()
                                .Where(x => x.Id == param.IdStudent)
                                .Select(e => new 
                                {
                                    IdStudent = e.Id,
                                    IdBinusian = e.IdBinusian,
                                    StudentName = e.FirstName+(e.MiddleName==null?"":" "+ e.MiddleName) + (e.LastName == null ? "" : " " + e.LastName)
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var dataHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e=>e.Student)
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Where(e => e.IdStudent == param.IdStudent && param.IdAcademicYears.Contains(e.Homeroom.Grade.MsLevel.MsAcademicYear.Id))
                        .Select(e => new 
                        {
                            StudentName = e.Student.FirstName+(e.Student.MiddleName==null?"":" "+ e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName), 
                            IdBinusian = e.Student.Id,
                            AcademicYear = e.Homeroom.Grade.MsLevel.MsAcademicYear,
                            Level = e.Homeroom.Grade.MsLevel,
                            Grade = e.Homeroom.Grade,
                            Semester = e.Homeroom.Semester,
                            Homeroom = e.Homeroom
                        })
                        .ToListAsync(CancellationToken);

            var result = new GetStudentInformationByAcademicYearResult
            {
                StudentName = dataStudent.StudentName, 
                IdBinusian = dataStudent.IdBinusian,
                AcademicYear = dataHomeroomStudent.Select(e => new ItemValueVm
                {
                    Id = e.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                    Description = e.Homeroom.Grade.MsLevel.MsAcademicYear.Description
                }).ToList(),
                Level = dataHomeroomStudent.Select(e => new ItemValueVm
                {
                    Id = e.Homeroom.Grade.MsLevel.Id,
                    Description = e.Homeroom.Grade.MsLevel.Description
                }).Distinct().ToList(),
                Grade = dataHomeroomStudent.Select(e => new ItemValueVm
                {
                    Id = e.Homeroom.Grade.Id,
                    Description = e.Homeroom.Grade.Description
                }).Distinct().ToList(),
                Semester = dataHomeroomStudent.Select(e => e.Homeroom.Semester).ToList(),
                Homeroom = dataHomeroomStudent.Select(e => new ItemValueVm
                {
                    Id = e.Homeroom.Id,
                    Description = e.Homeroom.Grade.Code + e.Homeroom.MsGradePathwayClassroom.Classroom.Code
                }).Distinct().ToList(),
                StatusOverall = GetStatusOverall(dataStudent.IdStudent,param.IdAcademicYears)
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
