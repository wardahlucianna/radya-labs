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
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class GetUnmappedStudentStatusByAYHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetUnmappedStudentStatusByAYHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnmappedStudentStatusByAYRequest>(
                                nameof(GetUnmappedStudentStatusByAYRequest.IdAcademicYear)
                            );
            
            var paramAcademicYearDetail = await _dbContext.Entity<MsAcademicYear>()
                                        .Where(x => x.Id == param.IdAcademicYear)
                                        .FirstOrDefaultAsync(CancellationToken);

            var intParamPrevAY = int.Parse(paramAcademicYearDetail.Code) - 1;
            var paramPrevAYString = intParamPrevAY.ToString();

            var excludedStudents = _dbContext.Entity<TrStudentStatus>()
                .Where(tss => (tss.AcademicYear.Code == paramPrevAYString || tss.AcademicYear.Code == param.IdAcademicYear)
                    && tss.IsActive)
                .Join(_dbContext.Entity<MsStudent>().Where(ms2 => ms2.IsActive && ms2.IdSchool == paramAcademicYearDetail.IdSchool),
                    tss => tss.IdStudent,
                    ms2 => ms2.Id,
                    (tss, ms2) => ms2.Id)
                .Distinct();

            var predicate = PredicateBuilder.Create<MsStudent>
                (ms => ms.IdSchool == paramAcademicYearDetail.IdSchool
                && ms.IdStudentStatus == 1
                && ms.IsActive == true
                && !excludedStudents.Contains(ms.Id));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(
                        (string.IsNullOrWhiteSpace(x.FirstName) ? "" : x.FirstName) +
                        (string.IsNullOrWhiteSpace(x.MiddleName) ? "" : x.MiddleName) +
                        (string.IsNullOrWhiteSpace(x.LastName) ? "" : x.LastName)
                        , param.SearchPattern())
                    || EF.Functions.Like(x.Id, param.SearchPattern())
                    );

            var query = _dbContext.Entity<MsStudent>()
                .Where(predicate)
                .OrderBy(ms => ms.FirstName)
                    .ThenBy(ms => ms.MiddleName)
                    .ThenBy(ms => ms.LastName);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                    .Select(ms => new ItemValueVm(ms.Id, NameUtil.GenerateFullName(ms.FirstName, ms.MiddleName, ms.LastName)))
                    .ToList();
            }
            else
            {
                items = query
                    .SetPagination(param)
                    .Select(ms => new GetUnmappedStudentStatusByAYResult
                    {
                        Id = ms.Id,
                        Description = NameUtil.GenerateFullName(ms.FirstName, ms.MiddleName, ms.LastName)
                    })
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count())
                ? items.Count()
                : query.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
