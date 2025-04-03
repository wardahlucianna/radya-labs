using System;
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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "semester", "code" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "homeroomStudent.homeroom.semester" }
        };
        
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentEnrollmentHomeroomHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentHomeroomRequest>(nameof(GetStudentEnrollmentHomeroomRequest.IdGrade));

            var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade);
            if (param.Semester.HasValue)
                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Semester == param.Semester);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(Convert.ToString(x.HomeroomStudent.Homeroom.Semester), param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Homeroom.Grade.Code, param.SearchPattern())
                    || EF.Functions.Like(x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code, param.SearchPattern()));

            var query = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .SearchByIds(param)
                .Where(predicate);
            
            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "code" => param.OrderType == OrderType.Asc
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
                    .Select(x => new ItemValueVm(x.HomeroomStudent.IdHomeroom, string.Format("{0}{1} {2}",
                        x.HomeroomStudent.Homeroom.Grade.Code,
                        x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                        "(Semester " + x.HomeroomStudent.Semester + ")")))
                    .Distinct()
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = query
                    .Select(x => new GetStudentEnrollmentHomeroomResult()
                    {
                        Id = x.HomeroomStudent.IdHomeroom,
                        Semester = x.HomeroomStudent.Semester,
                        Code = string.Format("{0} {1}",
                            x.HomeroomStudent.Homeroom.Grade.Code,
                            x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code),
                        Description = string.Format("{0} {1}",
                            x.HomeroomStudent.Homeroom.Grade.Description,
                            x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description)
                    })
                    .AsEnumerable()
                    .Distinct(new UniqueIdComparer<GetStudentEnrollmentHomeroomResult>())
                    .SetPagination(param)
                    .ToList();
            }
            
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.HomeroomStudent.IdHomeroom).Distinct().CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
