using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentGenderDemographyDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentGenderDemographyDetailHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentGenderDemographyDetailRequest>();

            var result = await StudentGenderDetail(new GetStudentGenderDemographyDetailRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                Grade = param.Grade,
                Homeroom = param.Homeroom,
                ViewCategoryType = param.ViewCategoryType,
                Gender = param.Gender
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetStudentGenderDemographyDetailResult>> StudentGenderDetail(GetStudentGenderDemographyDetailRequest param)
        {
            var retVal = new List<GetStudentGenderDemographyDetailResult>();

            var getStudentsQuery = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(a => a.Student.Country)
                    .Include(a => a.Student.Nationality)
                    .Include(a => a.Homeroom.Grade.MsLevel)
                    .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                    .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                && a.Semester == param.Semester
                                && a.Homeroom.Semester == param.Semester
                                && (a.Student.Gender == (param.Gender != null ? param.Gender : a.Student.Gender)));

            if (param.ViewCategoryType == "grade")
                getStudentsQuery = getStudentsQuery.Where(a => param.Grade == null ? true : param.Grade.Any(b => b == a.Homeroom.IdGrade));
            else if (param.ViewCategoryType == "homeroom")
                getStudentsQuery = getStudentsQuery.Where(a => param.Homeroom == null ? true : param.Homeroom.Any(b => b == a.IdHomeroom));

            var getStudents = getStudentsQuery.ToList();

            var pathways = await _dbContext.Entity<MsStudentGradePathway>()
                .Include(a => a.Pathway)
                .Include(a => a.StudentGrade)
                .Where(a => getStudents.Select(b => b.IdStudent).Any(b => b == a.StudentGrade.IdStudent)
                    && getStudents.Select(b => b.Homeroom.IdGrade).Any(b => b == a.StudentGrade.IdGrade))
                .ToListAsync(CancellationToken);

            var teachers = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(a => a.Staff)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                    && a.TeacherPosition.Position.Code != "COT"
                    && getStudents.Select(b => b.IdHomeroom).Any(b => b == a.IdHomeroom))
                .ToListAsync(CancellationToken);

            var getData = from homeroomStudent in getStudents
                          join pathway in pathways
                              on new { homeroomStudent.IdStudent, homeroomStudent.Homeroom.IdGrade }
                              equals new { pathway.StudentGrade.IdStudent, pathway.StudentGrade.IdGrade } into pathwayGroup
                          from pathway in pathwayGroup.DefaultIfEmpty()
                          join teacher in teachers
                              on homeroomStudent.IdHomeroom equals teacher.IdHomeroom into teacherGroup
                          from teacher in teacherGroup.DefaultIfEmpty()
                          select new
                          {
                              Student = homeroomStudent,
                              Pathway = pathway,
                              Teacher = teacher
                          };


            var insertItems = getData.Select(a => new GetStudentGenderDemographyDetailResult
            {
                Student = new NameValueVm
                {
                    Id = a.Student.IdStudent,
                    Name = NameUtil.GenerateFullName(a.Student.Student.FirstName, a.Student.Student.LastName)
                },
                Grade = new ItemValueVm
                {
                    Id = a.Student.Homeroom.IdGrade,
                    Description = a.Student.Homeroom.Grade.Description
                },
                Homeroom = new ItemValueVm
                {
                    Id = a.Student.IdHomeroom,
                    Description = a.Student.Homeroom.Grade.Code + a.Student.Homeroom.MsGradePathwayClassroom.Classroom.Description
                },
                Teacher = new NameValueVm
                {
                    Id = a.Teacher?.IdBinusian ?? "-",
                    Name = string.IsNullOrWhiteSpace(NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? ""))
                         ? "-"
                         : NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? "")
                },
                Streaming = new ItemValueVm
                {
                    Id = a.Pathway?.IdPathway ?? "-",
                    Description = (a.Pathway?.Pathway?.Code ?? "-").ToLower() == "no pathway" ? "-" : (a.Pathway?.Pathway?.Code ?? "-")
                },
                Gender = a.Student.Student.Gender
            })
                .OrderBy(a => a.Grade.Description.Length)
                            .ThenBy(a => a.Grade.Description)
                            .ThenBy(a => a.Homeroom.Description.Length)
                            .ThenBy(a => a.Homeroom.Description)
                    .ThenBy(a => a.Student.Name)
                .ToList();

            retVal.AddRange(insertItems);

            return retVal;
        }
    }
}
