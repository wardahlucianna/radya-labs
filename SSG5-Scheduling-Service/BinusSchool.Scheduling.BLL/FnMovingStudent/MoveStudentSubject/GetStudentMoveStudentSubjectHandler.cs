using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Common.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetStudentMoveStudentSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentMoveStudentSubjectHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentMoveStudentSubjectRequest>();

            string[] _columns = { "FullName", "BinusianId", "Level", "Grade", "Homeroom"};

            var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(x => x.IsActive);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(e => e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(e => e.HomeroomStudent.Homeroom.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdLessonOld))
                predicate = predicate.And(e => e.IdLesson == param.IdLessonOld);

            var listHomeroomStudentEnroolment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                    .Where(predicate)
                                                    .GroupBy(e => new
                                                    {
                                                        IdHomeroomStudent = e.IdHomeroomStudent,
                                                        IdStudent = e.HomeroomStudent.IdStudent,
                                                        FirstName = e.HomeroomStudent.Student.FirstName,
                                                        MiddleName = e.HomeroomStudent.Student.MiddleName,
                                                        LastName = e.HomeroomStudent.Student.LastName,
                                                        LevelCode = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                                                        GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                                        ClassrommCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                                                    })
                                                    .Select(e => new 
                                                    {
                                                        IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                                        IdStudent = e.Key.IdStudent,
                                                        FirstName = e.Key.FirstName,
                                                        MiddleName = e.Key.MiddleName,
                                                        LastName = e.Key.LastName,
                                                        Level = e.Key.LevelCode,
                                                        Grade = e.Key.GradeCode,
                                                        Homeroom = e.Key.GradeCode+e.Key.ClassrommCode
                                                    })
                                                    .ToListAsync(CancellationToken);

            var query = listHomeroomStudentEnroolment
                        .Select(e => new GetStudentMoveStudentSubjectResult
                        {
                            IdHomeroomStudent = e.IdHomeroomStudent,
                            IdStudent = e.IdStudent,
                            FullName = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName),
                            Level = e.Level,
                            Grade = e.Grade,
                            Homeroom = e.Homeroom
                        });


            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(e => e.FullName.ToLower().Contains(param.Search) || e.IdStudent.ToLower().Contains(param.Search));

            switch (param.OrderBy)
            {
                case "FullName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.FullName)
                        : query.OrderBy(x => x.FullName);
                    break;

                case "BinusianId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
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

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                 items = query
                    .ToList();
            }
            else
            {
                items = query
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
