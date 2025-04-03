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
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetMoveStudentEnrollmentHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetMoveStudentEnrollmentHistoryHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMoveStudentEnrollmentHistoryRequest>();
            var predicate = PredicateBuilder.Create<TrHomeroomStudentEnrollment>(x => x.IdHomeroomStudent == param.idHomeroomStudent && x.IsShowHistory);
            string[] _columns = { "newSubjectName", "newSubjectLevel", "oldSubjectName", "oldSubjectLevel", "effectiveDate", "note" };

            var query = _dbContext.Entity<TrHomeroomStudentEnrollment>()
                        .Include(e => e.SubjectLevelNew)
                        .Include(e => e.SubjectLevelOld)
                        .Include(e => e.SubjectNew)
                        .Include(e => e.LessonNew).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                        .Include(e => e.SubjectOld)
                        .Include(e => e.LessonOld).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                        .Where(predicate)
                        .Select(e => new GetMoveStudentEnrollmentHistoryResult
                        {
                            newSubjectLevel = e.IsDelete ? "-" : e.SubjectLevelNew.Description,
                            oldSubjectLevel = e.SubjectLevelOld.Description,
                            newSubjectName = e.IsDelete
                                                ?"-"
                                                : e.SubjectNew.Description + " - " + e.LessonNew.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.FirstName == null ? e.Staff.LastName.Trim() : e.Staff.FirstName.Trim() + " " + e.Staff.LastName.Trim()).FirstOrDefault() + " - " + e.LessonNew.ClassIdGenerated,
                            oldSubjectName = e.SubjectOld.Description + " - " + e.LessonOld.LessonTeachers.Where(x => x.IsPrimary).Select(e => e.Staff.FirstName == null ? e.Staff.LastName.Trim() : e.Staff.FirstName.Trim() + " " + e.Staff.LastName.Trim()).FirstOrDefault() + " - " + e.LessonOld.ClassIdGenerated,
                            effectiveDate = e.StartDate,
                            note = e.Note
                        }).Distinct();

            switch (param.OrderBy)
            {
                case "newSubjectName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.newSubjectName)
                        : query.OrderBy(x => x.newSubjectName);
                    break;

                case "newSubjectLevel":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.newSubjectLevel)
                        : query.OrderBy(x => x.newSubjectLevel);
                    break;
                case "oldSubjectName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.oldSubjectName)
                        : query.OrderBy(x => x.oldSubjectName);
                    break;
                case "oldSubjectLevel":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.oldSubjectLevel)
                        : query.OrderBy(x => x.oldSubjectLevel);
                    break;
                case "effectiveDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.effectiveDate)
                        : query.OrderBy(x => x.effectiveDate);
                    break;
                case "note":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.note)
                        : query.OrderBy(x => x.note);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
