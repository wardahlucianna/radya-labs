using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetEntryMeritDemeritStudentByFreezeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetEntryMeritDemeritStudentByFreezeHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEntryMeritDemeritStudentByFreezeRequest>();
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>
            (
                x => x.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) &&
                x.Student.StudentGrades.Any(e => e.Grade.IdLevel == param.IdLevel) &&
                x.Student.StudentGrades.Any(e => e.IdGrade == param.IdGrade)
            );

            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Merit", "Demerit", "LevelOfInfraction", "Sunction", "LastUpdate" };

            var StudentStatus = await _dbContext.Entity<TrStudentStatus>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYear && (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date
                    || (x.StartDate < param.Date.Date
                        ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                        : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .Select(x => x.IdStudent)
                .ToListAsync();

            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         //join StudentStatus in _dbContext.Entity<TrStudentStatus>() on Student.Id equals StudentStatus.IdStudent
                         //join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Freeze in _dbContext.Entity<MsStudentFreezeMeritDemerit>() on HomeroomStudent.Id equals Freeze.IdHomeroomStudent into JoinedFreeze
                         from Freeze in JoinedFreeze.DefaultIfEmpty()
                         where Level.IdAcademicYear == param.IdAcademicYear
                          && Grade.Id == param.IdGrade
                          && Level.Id == param.IdLevel
                          && !StudentStatus.Contains(HomeroomStudent.IdStudent)
                         select new
                         {
                             IdHomeroomStudent = HomeroomStudent.Id,
                             IdHomeroom = Homeroom.Id,
                             FullName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                             UserName = Student.Id,
                             IdBinusan = Student.IdBinusian,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             IdLevel = Level.Id,
                             Semester = Homeroom.Semester.ToString(),
                             GradeCode = Grade.Code,
                             Classroom = Classroom.Code,
                             Homeroom = new CodeWithIdVm
                             {
                                 Id = Homeroom.Id,
                                 Code = Grade.Code,
                                 Description = Grade.Code + Classroom.Code,
                             },
                             IsFreeze = Freeze == null ? false : Freeze.IsFreeze,
                         })
                         .OrderBy(e => e.GradeCode).ThenBy(e => e.Classroom)
                         .Where(e => e.IsFreeze == false);

            if (!string.IsNullOrEmpty(param.Semester))
                query = query.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdBinusan.Contains(param.Search) || x.UserName.Contains(param.Search) || x.FullName.Contains(param.Search));
            if (param.ExcludeStudents != null && param.ExcludeStudents.Count > 0)
                query = query.Where(x => !param.ExcludeStudents.Contains(x.IdBinusan));
            //ordering
            switch (param.OrderBy)
            {
                case "FullName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.FullName)
                        : query.OrderBy(x => x.FullName);
                    break;
                case "UserName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UserName)
                        : query.OrderBy(x => x.UserName);
                    break;
                case "IdBinusan":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdBinusan)
                        : query.OrderBy(x => x.IdBinusan);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
            };

            IReadOnlyList<GetEntryMeritDemeritStudentByFreezeResult> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                  .Select(x => new GetEntryMeritDemeritStudentByFreezeResult
                  {
                      IdHomeroomStudent = x.IdHomeroomStudent,
                      FullName = x.FullName,
                      UserName = x.UserName,
                      IdBinusan = x.IdBinusan,
                      Grade = x.Grade,
                      Homeroom = x.Homeroom
                  })
                  .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                   .Select(x => new GetEntryMeritDemeritStudentByFreezeResult
                   {
                       IdHomeroomStudent = x.IdHomeroomStudent,
                       FullName = x.FullName,
                       UserName = x.UserName,
                       IdBinusan = x.IdBinusan,
                       Grade = x.Grade,
                       Homeroom = x.Homeroom
                   })
                   .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdHomeroomStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));


        }
    }
}
