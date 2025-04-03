using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "id", "fullname", "username", "binusianid", "level", "grade", "homeroom" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "homeroomStudent.idUser" },
            { _columns[1], "homeroomStudent.student.fullname" }
        };

        private readonly ISchedulingDbContext _dbContext;

        public GetStudentEnrollmentStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentStudentRequest>();

            var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(x => 1 == 1);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.HomeroomStudent.IdHomeroom == param.IdHomeroom);
            if (param.IdSubjects?.Any() ?? false)
                predicate = predicate.And(x => param.IdSubjects.Contains(x.Lesson.IdSubject));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.HomeroomStudent.IdStudent, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Student.FirstName, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Homeroom.Grade.Code, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code, param.SearchPattern()));

            var query = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .SearchByIds(param)
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "homeroom" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.HomeroomStudent.Homeroom.Grade.Code)
                            .ThenBy(x => x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code)
                        : query.OrderByDescending(x => x.HomeroomStudent.Homeroom.Grade.Code)
                            .ThenByDescending(x => x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code),
                    _ => query.OrderByDynamic(param, _aliasColumns)
                };
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.HomeroomStudent.IdStudent, NameUtil.GenerateFullName(
                        x.HomeroomStudent.Student.FirstName,
                        x.HomeroomStudent.Student.MiddleName,
                        x.HomeroomStudent.Student.LastName)))
                    .Distinct()
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = (from q in query
                         join u in _dbContext.Entity<MsUser>() on q.HomeroomStudent.IdStudent equals u.Id
                         select new GetStudentEnrollmentStudentResult
                         {
                             Id = q.HomeroomStudent.IdStudent,
                             FirstName = q.HomeroomStudent.Student.FirstName,
                             MiddleName = q.HomeroomStudent.Student.MiddleName,
                             LastName = q.HomeroomStudent.Student.LastName,
                             FullName = q.HomeroomStudent.Student.MiddleName != null ? q.HomeroomStudent.Student.FirstName + " " + q.HomeroomStudent.Student.MiddleName : q.HomeroomStudent.Student.FirstName + " " + q.HomeroomStudent.Student.LastName,
                             UserName = u.Username,
                             IdHomeroomStudent = q.HomeroomStudent.Id,
                             Homeroom = string.Format("{0} {1}",
                                         q.HomeroomStudent.Homeroom.Grade.Code,
                                         q.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code),
                             BinusianId = q.HomeroomStudent.Student.Id,
                             Level = q.HomeroomStudent.Homeroom.Grade.Level.Description,
                             IdGrade = q.HomeroomStudent.Homeroom.Grade.Id,
                             Grade = q.HomeroomStudent.Homeroom.Grade.Description
                         })
                        .AsEnumerable()
                        .Distinct(new UniqueIdComparer<GetStudentEnrollmentStudentResult>())
                        .SetPagination(param)
                        .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.HomeroomStudent.IdStudent).Distinct().CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
