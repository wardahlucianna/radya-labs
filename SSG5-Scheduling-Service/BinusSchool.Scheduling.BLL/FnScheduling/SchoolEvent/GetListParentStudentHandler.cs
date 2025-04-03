using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetListParentStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListParentStudentRequest.IdAcademicYear),
        };
        private static readonly string[] _columns = { "ParentName", "ParentId", "StudentName", "StudentId", "Level", "Grade", "Homeroom" };
        private readonly ISchedulingDbContext _dbContext;

        public GetListParentStudentHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListParentStudentRequest>();
            var dataPeriode = _dbContext.Entity<MsPeriod>();

            #region Old code
            //var query = await (from s in _dbContext.Entity<MsStudent>()
            //                   join sg in _dbContext.Entity<MsStudentGrade>() on s.Id equals sg.IdStudent
            //                   join g in _dbContext.Entity<MsGrade>() on sg.IdGrade equals g.Id
            //                   join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
            //                   join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
            //                   join hs in _dbContext.Entity<MsHomeroomStudent>() on s.Id equals hs.IdStudent
            //                   join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
            //                   join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
            //                   join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
            //                   join sp in _dbContext.Entity<MsStudentParent>() on s.Id equals sp.IdStudent
            //                   join p in _dbContext.Entity<MsParent>() on sp.IdParent equals p.Id
            //                   where ay.Id == param.IdAcademicYear
            //                   select new GetListParentStudentResult
            //                   {
            //                       Id = p.Id,
            //                       ParentId = p.Id,
            //                       ParentName = NameUtil.GenerateFullName(p.FirstName, p.MiddleName, p.LastName),
            //                       StudentId = s.Id,
            //                       StudentName = NameUtil.GenerateFullName(s.FirstName, s.MiddleName, s.LastName),
            //                       IdLevel = l.Id,
            //                       Level = l.Description,
            //                       IdGrade = g.Id,
            //                       Grade = g.Description,
            //                       IdHomeroom = h.Id,
            //                       Homeroom = g.Code + c.Description
            //                   }).ToListAsync(CancellationToken);
            #endregion

            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.Student.HomeroomStudents.Any(y => y.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Student.FirstName.ToLower().Contains(param.Search.ToLower())
                || x.Student.MiddleName.ToLower().Contains(param.Search.ToLower())
                || x.Student.LastName.Contains(param.Search.ToLower())
                || x.IdStudent.ToLower().Contains(param.Search.ToLower()));

            var query = _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.Level)
                .Include(x => x.Student)
                    .ThenInclude(x => x.HomeroomStudents)
                        .ThenInclude(x => x.Homeroom)
                .Where(predicate);

            //ordering
            switch (param.OrderBy)
            {
                case "studentId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "studentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Student.FirstName)
                        : query.OrderBy(x => x.Student.FirstName);
                    break;
                case "level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade.Level.OrderNumber)
                        : query.OrderBy(x => x.Grade.Level.OrderNumber);
                    break;
                case "grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade.OrderNumber)
                        : query.OrderBy(x => x.Grade.OrderNumber);
                    break;
                case "homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade.OrderNumber).ThenByDescending(x => x.Student.HomeroomStudents
                            .First(y => y.Homeroom.IdAcademicYear == x.Grade.Level.IdAcademicYear).Homeroom.GradePathwayClassroom.Classroom.Code)
                        : query.OrderBy(x => x.Grade.OrderNumber).ThenBy(x => x.Student.HomeroomStudents
                            .First(y => y.Homeroom.IdAcademicYear == x.Grade.Level.IdAcademicYear).Homeroom.GradePathwayClassroom.Classroom.Code);
                    break;
            };

            var dataParentPagination = query
                .SetPagination(param)
                .Select(x => new GetListParentStudentResult
                {
                    ParentId = $"P{x.IdStudent}",
                    ParentName = $"Parent of {NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)}",
                    StudentId = x.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                    Level = new CodeWithIdVm
                    {
                        Id = x.Grade.IdLevel,
                        Code = x.Grade.Level.Code,
                        Description = x.Grade.Level.Description
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.IdGrade,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    ClassHomeroom = new ItemValueVm
                    {
                        Id = x.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == x.Grade.Level.IdAcademicYear).IdHomeroom,
                        Description = x.Grade.Code + x.Student.HomeroomStudents
                            .First(y => y.Homeroom.IdAcademicYear == x.Grade.Level.IdAcademicYear).Homeroom.GradePathwayClassroom.Classroom.Code,
                    }
                }).ToList();

            var count = param.CanCountWithoutFetchDb(dataParentPagination.Count)
                ? dataParentPagination.Count
                : query.Count();

            return Request.CreateApiResult2(dataParentPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
