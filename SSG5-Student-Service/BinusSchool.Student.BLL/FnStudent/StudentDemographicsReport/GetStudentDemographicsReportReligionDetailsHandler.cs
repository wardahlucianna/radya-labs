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
    public class GetStudentDemographicsReportReligionDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentDemographicsReportReligionDetailsHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var columns = new[] { "Student", "Level", "Homeroom", "HomeroomTeacher", "Streaming", "StudentReligion", "ReligionSubject" };

            var param = await Request.GetBody<GetSDRReligionReportDetailsRequest>();

            var getStudentsQuery = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(a => a.Student.Religion)
                    .Include(a => a.Student.ReligionSubject)
                    .Include(a => a.Homeroom.Grade.MsLevel)
                    .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                    .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                && (a.Student.IdReligion == (param.IdReligion != null ? param.IdReligion : a.Student.IdReligion))
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


            var insertData = getData.Select(a => new GetSDRReligionReportDetailsResult
            {
                Student = new ItemValueVm()
                {
                    Id = a.Student.IdStudent,
                    Description = NameUtil.GenerateFullName(a.Student.Student.FirstName, a.Student.Student.LastName)
                },
                Level = new ItemValueVm
                {
                    Id = a.Student.Homeroom.Grade.IdLevel,
                    Description = a.Student.Homeroom.Grade.MsLevel.Description
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
                HomeroomTeacher = new ItemValueVm
                {
                    Id = a.Teacher?.IdBinusian ?? "-",
                    Description = string.IsNullOrWhiteSpace(NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? ""))
                         ? "-"
                         : NameUtil.GenerateFullName(a.Teacher?.Staff?.FirstName ?? "", a.Teacher?.Staff?.LastName ?? "")
                },
                Streaming = new ItemValueVm
                {
                    Id = a.Pathway?.IdPathway ?? "-",
                    Description = (a.Pathway?.Pathway?.Code ?? "-").ToLower() == "no pathway" ? "-" : (a.Pathway?.Pathway?.Code ?? "-")
                },
                StudentReligion = a.Student.Student.Religion.ReligionName,
                ReligionSubject = new ItemValueVm
                {
                    Id = a.Student.Student.IdReligionSubject,
                    Description = a.Student.Student.ReligionSubject.ReligionSubjectName
                }
            })
                .OrderBy(a => a.Grade.Description.Length)
                            .ThenBy(a => a.Grade.Description)
                            .ThenBy(a => a.Homeroom.Description.Length)
                            .ThenBy(a => a.Homeroom.Description)
                    .ThenBy(a => a.Student.Description)
                .ToList(); ;

            if (!string.IsNullOrEmpty(param.Search))
            {
                insertData = insertData.Where(x =>
                     (x.Student?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Level?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Homeroom?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.HomeroomTeacher?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.Streaming?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.StudentReligion ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     (x.ReligionSubject?.Description ?? "").IndexOf(param.Search, StringComparison.OrdinalIgnoreCase) >= 0
                 ).ToList();
            }

            insertData = param.OrderBy switch
            {
                "student" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.Student.Description).ToList()
                            : insertData.OrderByDescending(x => x.Student.Description).ToList(),
                "level" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.Level.Description).ToList()
                            : insertData.OrderByDescending(x => x.Level.Description).ToList(),
                "homeroom" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.Homeroom.Description).ToList()
                            : insertData.OrderByDescending(x => x.Homeroom.Description).ToList(),
                "homeroomTeacher" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.HomeroomTeacher.Description).ToList()
                            : insertData.OrderByDescending(x => x.HomeroomTeacher.Description).ToList(),
                "streaming" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.Streaming.Description).ToList()
                            : insertData.OrderByDescending(x => x.Streaming.Description).ToList(),
                "studentReligion" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.StudentReligion).ToList()
                            : insertData.OrderByDescending(x => x.StudentReligion).ToList(),
                "religionSubject" => param.OrderType == OrderType.Asc
                            ? insertData.OrderBy(x => x.ReligionSubject.Description).ToList()
                            : insertData.OrderByDescending(x => x.ReligionSubject.Description).ToList(),
                _ => insertData
            };

            if (param.GetAll == true)
            {
                return Request.CreateApiResult2(insertData as object);
            }
            else
            {
                var resultPagination = insertData.SetPagination(param).ToList();

                var count = param.CanCountWithoutFetchDb(resultPagination.Count)
                    ? resultPagination.Count
                    : insertData.Select(x => x.Student.Id).Count();

                return Request.CreateApiResult2(resultPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
