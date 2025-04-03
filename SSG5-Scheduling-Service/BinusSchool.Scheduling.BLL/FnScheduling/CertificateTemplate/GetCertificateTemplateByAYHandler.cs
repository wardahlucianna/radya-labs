using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class GetCertificateTemplateByAYHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetCertificateTemplateByAYHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCertificateTemplateByAYRequest>(nameof(GetCertificateTemplateByAYRequest.IdAcademicYear));

            var predicate = PredicateBuilder.Create<MsCertificateTemplate>(x => true);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsCertificateTemplate>()
                .Include(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Where(predicate);

            query = query.Where(x => x.ApprovalStatus == "Approved");

            if (param.IdAcademicYear != null)
                query = query.Where(x => x.IdAcademicYear  == param.IdAcademicYear);

            switch(param.OrderBy)
            {
                case "templatename":
                    query = param.OrderType == OrderType.Desc 
                        ? query.OrderByDescending(x => x.Name) 
                        : query.OrderBy(x => x.Name);
                    break;
            };
            
            var msCertificateTemplates = await query
                    .ToListAsync(CancellationToken);

            var certificateTemplates = msCertificateTemplates
                .SetPagination(param)
                .Select(x => new GetCertificateTemplateByAYResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear.Description,
                    TemplateName = x.Name,
                }).ToList();

            var count = param.CanCountWithoutFetchDb(certificateTemplates.ToList().Count) 
                ? certificateTemplates.ToList().Count 
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(certificateTemplates as object, param.CreatePaginationProperty(count));
        }
    }
}
