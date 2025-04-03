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
    public class GetStudentNationalityDemographyDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentNationalityDemographyDetailHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentNationalityDemographyDetailRequest>();

            var items = await StudentNationalityDetail(new GetStudentNationalityDemographyDetailRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                ViewCategoryType = param.ViewCategoryType,
                Grade = param.Grade,
                Homeroom = param.Homeroom,
                IdCountry = param.IdCountry,
                Gender = param.Gender
            });

            return Request.CreateApiResult2(items as object);
        }

        public async Task<GetStudentNationalityDemographyDetailResult> StudentNationalityDetail(GetStudentNationalityDemographyDetailRequest param)
        {
            var retVal = new GetStudentNationalityDemographyDetailResult();

            var getStudentsQuery = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(a => a.Student.Country)
                    .Include(a => a.Student.Nationality)
                    .Include(a => a.Homeroom.Grade.MsLevel)
                    .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                    .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                && (a.Student.IdCountry == (param.IdCountry != null ? param.IdCountry : a.Student.IdCountry))
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


            var InsertItems = new GetStudentNationalityDemographyDetailResult
            {
                ListData = getData.Select(a => new DataList
                {
                    Student = new NameValueVm()
                    {
                        Id = a.Student.IdStudent,
                        Name = NameUtil.GenerateFullName(a.Student.Student.FirstName, a.Student.Student.LastName)
                    },
                    Grade = a.Student.Homeroom.Grade.Description,
                    Homeroom = a.Student.Homeroom.Grade.Code + a.Student.Homeroom.MsGradePathwayClassroom.Classroom.Description,
                    Pathway = (a.Pathway?.Pathway?.Code ?? "-").ToLower() == "no pathway" ? "-" : (a.Pathway?.Pathway?.Code ?? "-"),
                    NationalityType = a.Student.Student.Nationality.NationalityName,
                    NationalityCountry = a.Student.Student.Country.CountryName,
                    HomeroomTeacher = string.IsNullOrWhiteSpace(NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? ""))
                                    ? "-"
                                    : NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? "")
                })
                        .OrderBy(a => a.Grade.Length)
                            .ThenBy(a => a.Grade)
                            .ThenBy(a => a.Homeroom.Length)
                            .ThenBy(a => a.Homeroom)
                            .ThenBy(a => a.Student.Name)
                        .Distinct()
                        .ToList()
            };

            retVal.Category = param.ViewCategoryType == "grade" ? "grade" : "homeroom";
            retVal.ListData = InsertItems.ListData;

            return retVal;
        }
    }
}
