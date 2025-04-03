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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetCounselingServicesEntryByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetCounselingServicesEntryByStudentHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselingServicesEntryByStudentRequest>();
            string[] _columns = { "AcademicYear", "CounselingCategory", "CounselorName", "CounselingDate"};

            var predicate = PredicateBuilder.Create<TrCounselingServicesEntry>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdUserStudent);

            //filter
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.CounselingCategory.CounselingCategoryName.Contains(param.Search) || x.Counselor.User.DisplayName.Contains(param.Search));
            if (!string.IsNullOrEmpty(param.IdConselingCategory))
                predicate = predicate.And(x => x.IdCounselingCategory==param.IdConselingCategory);


            var query = _dbContext.Entity<TrCounselingServicesEntry>()
                        .Include(e => e.CounselingCategory)
                        .Include(e => e.Counselor)
                        .Include(e => e.AcademicYear)
                        .Where(predicate)
                        .Select(e => new
                        {
                            id = e.Id,
                            AcademicYear = e.AcademicYear.Description,
                            CounselingCategory = e.CounselingCategory.CounselingCategoryName,
                            CounselorName = e.Counselor.User.DisplayName,
                            CounselingDate = e.DateTime,
                        });

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "CounselingCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CounselingCategory)
                        : query.OrderBy(x => x.CounselingCategory);
                    break;
                case "CounselorName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CounselorName)
                        : query.OrderBy(x => x.CounselorName);
                    break;
                case "CounselingDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CounselingDate)
                        : query.OrderBy(x => x.CounselingDate);
                    break;
            };

            IReadOnlyList<object> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetCounselingServicesEntryByStudentResult
                {
                    Id = x.id,
                    AcademicYear = x.AcademicYear,
                    CounselingCategory = x.CounselingCategory,
                    CounselorName = x.CounselorName,
                    CounselingDate = x.CounselingDate,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetCounselingServicesEntryByStudentResult
                {
                    Id = x.id,
                    AcademicYear = x.AcademicYear,
                    CounselingCategory = x.CounselingCategory,
                    CounselorName = x.CounselorName,
                    CounselingDate = x.CounselingDate,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }




    }
}
