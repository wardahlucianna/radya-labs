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
    public class GetReportStudentToGcByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetReportStudentToGcByStudentHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetReportStudentToGcByStudentRequest>();
            string[] _columns = { "ReportedBy", "ReportStudentDate", "ReportStudentNote" };

            var predicate = PredicateBuilder.Create<TrGcReportStudent>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdUserStudent);

            //filter
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Note.Contains(param.Search) || x.UserReport.DisplayName.Contains(param.Search));


            var query = _dbContext.Entity<TrGcReportStudent>()
                        .Include(e => e.Student)
                        .Include(e => e.UserReport)
                        .Include(e => e.AcademicYear)
                        .Where(predicate)
                        .Select(e => new
                        {
                            id = e.Id,
                            ReportedBy = e.UserReport.DisplayName,
                            ReportStudentDate = e.Date,
                            ReportStudentNote = e.Note,

                        });

            //ordering
            switch (param.OrderBy)
            {
                case "ReportedBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportedBy)
                        : query.OrderBy(x => x.ReportedBy);
                    break;
                case "ReportStudentDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentDate)
                        : query.OrderBy(x => x.ReportStudentDate);
                    break;
                case "ReportStudentNote":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ReportStudentNote)
                        : query.OrderBy(x => x.ReportStudentNote);
                    break;

            };

            IReadOnlyList<object> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcByStudentResult
                {
                    Id = x.id,
                    ReportedBy = x.ReportedBy,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetReportStudentToGcByStudentResult
                {
                    Id = x.id,
                    ReportedBy = x.ReportedBy,
                    ReportStudentDate = x.ReportStudentDate,
                    ReportStudentNote = x.ReportStudentNote,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
