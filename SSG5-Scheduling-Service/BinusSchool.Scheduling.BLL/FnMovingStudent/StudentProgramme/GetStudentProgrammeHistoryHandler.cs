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
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentProgrammeHistoryHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentProgrammeHistoryRequest>();
            var predicate = PredicateBuilder.Create<HTrStudentProgramme>(x => true);
            string[] _columns = { "studentId", "studentName", "newProgramme", "oldProgramme", "effectiveDate" };

            var query = _dbContext.Entity<HTrStudentProgramme>()
                        .Include(e => e.Student)
                        .Where(e => e.IdStudent==param.IdStudent)
                        .Select(e => new GetStudentProgrammeHistoryResult
                        {
                            idStudent = e.IdStudent,
                            studentName = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.LastName),
                            effectiveDate = e.StartDate,
                            programmeOld = e.ProgrammeOld.GetDescription(),
                            programmeNew = e.ProgrammeNew.GetDescription()
                        });

            switch (param.OrderBy)
            {
                case "studentId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.idStudent)
                        : query.OrderBy(x => x.idStudent);
                    break;

                //case "studentName":
                //    query = param.OrderType == OrderType.Desc
                //        ? query.OrderByDescending(x => x.studentName)
                //        : query.OrderBy(x => x.studentName);
                //    break;
                case "oldProgramme":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.programmeOld)
                        : query.OrderBy(x => x.programmeOld);
                    break;
                case "newProgramme":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.programmeNew)
                        : query.OrderBy(x => x.programmeNew);
                    break;
                case "effectiveDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.effectiveDate)
                        : query.OrderBy(x => x.effectiveDate);
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
