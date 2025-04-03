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
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetStudentHomeroomHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentHomeroomRequest>();
            string[] _columns = { "FullName", "UserName", "BinusanId", "Grade", "Homeroom" };

            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);

            //filter
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.Homeroom.Semester == param.Semester);

            //if (!string.IsNullOrEmpty(param.Search))
            //    predicate = predicate.And(x => x.Student.IdBinusian.Contains(param.Search) || x.Student.use
            //    .User.DisplayName.Contains(param.Search));


            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         where Level.IdAcademicYear == param.IdAcademicYear
                         select new
                         {
                             IdStudent = Student.Id,
                             IdHomeroomStudent = HomeroomStudent.Id,
                             AcademicYear = AcademicYear.Description,
                             IdAcademicYear = AcademicYear.Id,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             IdLevel = Level.Id,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             IdHomerrom = Homeroom.Id,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             HomeroomCode = Grade.Code,
                             CodeClassroom = Classroom.Code,
                             CodeGrade = Grade.Code,
                             BinusanId = Student.IdBinusian,
                             FullName = User.DisplayName,
                             UserName = User.Username,
                         });

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester.ToString());
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomerrom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.BinusanId.Contains(param.Search) || (x.FullName).Contains(param.Search) || (x.UserName).Contains(param.Search));

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
                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusanId)
                        : query.OrderBy(x => x.BinusanId);
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

            query = query.OrderBy(e => e.Grade).ThenBy(e => e.CodeClassroom);

            IReadOnlyList<object> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetStudentHomeroomResult
                {
                    IdStudent = x.IdStudent,
                    FullName = x.FullName,
                    Username = x.UserName,
                    BinusanID = x.BinusanId,
                    Grade = x.Grade,
                    IdGrade = x.IdGrade,
                    Homeroom = x.Homeroom,
                    IdHomeroom = x.IdHomeroomStudent,
                    Level = x.Level,
                    IdLevel = x.IdLevel,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetStudentHomeroomResult
                {
                    IdStudent = x.IdStudent,
                    FullName = x.FullName,
                    Username = x.UserName,
                    BinusanID = x.BinusanId,
                    Grade = x.Grade,
                    IdGrade = x.IdGrade,
                    Homeroom = x.Homeroom,
                    IdHomeroom = x.IdHomeroomStudent,
                    Level = x.Level,
                    IdLevel = x.IdLevel,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.BinusanId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }



    }
}
