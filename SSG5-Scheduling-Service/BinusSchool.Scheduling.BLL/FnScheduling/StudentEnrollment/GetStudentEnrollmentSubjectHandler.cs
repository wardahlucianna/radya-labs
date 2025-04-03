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
    public class GetStudentEnrollmentSubjectHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "code", "description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "lesson.subject.code" },
            { _columns[1], "lesson.subject.description" }
        };
        
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentEnrollmentSubjectHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentSubjectRequest>(nameof(GetStudentEnrollmentSubjectRequest.IdHomeroom));
            var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(x => x.HomeroomStudent.IdHomeroom == param.IdHomeroom);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Lesson.Subject.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Lesson.Subject.Description, param.SearchPattern()));
                        
            var query = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns);
            
            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Lesson.IdSubject, x.Lesson.Subject.Description))
                    .Distinct()
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = query
                    .Select(x => new GetStudentEnrollmentSubjectResult()
                    {
                        Id = x.Lesson.IdSubject,
                        IdLesson = x.IdLesson,
                        Code = x.Lesson.Subject.Code,
                        Description = x.Lesson.Subject.Description
                    })
                    .AsEnumerable()
                    .Distinct(new UniqueIdComparer<GetStudentEnrollmentSubjectResult>())
                    .SetPagination(param)
                    .ToList();
            }
            
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Lesson.IdSubject).Distinct().CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));    
        }
    }
}
