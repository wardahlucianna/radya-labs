using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherByAssignment
{
    public class GetTeacherByGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;

        public GetTeacherByGradeHandler(ITeachingDbContext teachingDbContext)
        {
            _teachingDbContext = teachingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            // var result = new List<GetTimeTableByUserResult>();
            var param = await Request.GetBody<GetTeacherByGradeRequest>();

            // var data = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
            // .Include(p => p.Childs)
            // .Include(p => p.TimetablePrefDetails)
            // .ThenInclude(p => p.TeachingLoads)
            // .Where(p => p.IsActive && p.IdParent == null)
            // .ToListAsync(CancellationToken);

            var data2 = await _teachingDbContext.Entity<TrTeachingLoad>()
            .Include(x => x.TimetablePrefDetail)
            .Where(x => x.TimetablePrefDetail != null)
            .ToListAsync(CancellationToken);

            var idSubjectCombs = data2.Select(x => x.TimetablePrefDetail.IdTimetablePrefHeader).Distinct();
            var subjectCombs = await _teachingDbContext.Entity<MsSubjectCombination>()
                .Where(x 
                    => idSubjectCombs.Contains(x.Id)
                    && x.Subject.Grade.Level.IdAcademicYear == param.AcademicYearId)
                .Select(x => new { x.Id, Grade = new ItemValueVm(x.Subject.IdGrade, x.Subject.Grade.Description) })
                .ToListAsync(CancellationToken);

            var retVal =
            (
                from _dataTeachingLoad in data2
                join _dataSubjectCombination in subjectCombs on _dataTeachingLoad.TimetablePrefDetail.IdTimetablePrefHeader equals _dataSubjectCombination.Id
                select new GetTeacherByGradeResult
                {
                    BinusianId = _dataTeachingLoad.IdUser,
                    Grade = _dataSubjectCombination.Grade.Description,
                }
                ).Where(x => x.Grade == param.Grade)
                .GroupBy(x => x.BinusianId)
                .Select(x => x.First())
                .ToList();

            return Request.CreateApiResult2(retVal as object, null);
        }

        //public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        //{
        //    HashSet<TKey> seenKeys = new HashSet<TKey>();
        //    foreach (TSource element in source)
        //    {
        //        if (seenKeys.Add(keySelector(element)))
        //        {
        //            yield return element;
        //        }
        //    }
        //}

    }
}
