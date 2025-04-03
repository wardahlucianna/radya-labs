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
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class GetCertificateTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetCertificateTemplateHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCertificateTemplateRequest>(nameof(GetCertificateTemplateRequest.UserId), nameof(GetCertificateTemplateRequest.IdAcademicYear), nameof(GetCertificateTemplateRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsCertificateTemplate>(x => true);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, $"%{param.Search}%")
                    || EF.Functions.Like(x.Title, $"%{param.Search}%")
                    || EF.Functions.Like(x.SubTitle, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsCertificateTemplate>()
                .Include(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Include(x => x.CertificateTemplateApprovers)
                .Where(predicate);

            if (param.IdAcademicYear != null)
                query = query.Where(x => x.IdAcademicYear == param.IdAcademicYear);

            if (param.ApprovalStatus != null)
                query = query.Where(x => x.ApprovalStatus == param.ApprovalStatus);

            switch (param.OrderBy)
            {
                case "templatename":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;

                case "certificatetitle":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Title)
                        : query.OrderBy(x => x.Title);
                    break;

                case "templatesubtitle":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SubTitle)
                        : query.OrderBy(x => x.SubTitle);
                    break;

                case "approvalstatus":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ApprovalStatus)
                        : query.OrderBy(x => x.ApprovalStatus);
                    break;
            };

            var msCertificateTemplates = await query
                    .ToListAsync(CancellationToken);

            var userRoles = await _dbContext.Entity<MsUserRole>().Where(x => x.IdUser == param.UserId).Select(x => x.IdRole).ToListAsync();

            var isPrincipal = await _dbContext.Entity<TrRolePosition>()
                .Include(x => x.TeacherPosition)
                    .ThenInclude(x => x.Position)
                .AnyAsync(x => userRoles.Contains(x.IdRole) && x.TeacherPosition.Position.Code == "P");

            var dataquery = msCertificateTemplates
                .Select(x => new
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear.Description,
                    TemplateName = x.Name,
                    CertificateTitle = x.Title,
                    TemplateSubtitle = x.SubTitle,
                    StatusApproval = x.ApprovalStatus,
                    CanApprove = x.ApprovalStatus.Contains("On Review") ? isPrincipal : false,
                    CanDelete = x.ApprovalStatus == "Declined" ? true : false,
                    SortApprovalSatatus = x.ApprovalStatus.Contains("On Review") == true ? 0 : 1,
                });

            if (param.OrderBy == null)
            {
                dataquery = dataquery.OrderBy(x => x.SortApprovalSatatus);
            }

            var certificateTemplates = dataquery
                .SetPagination(param)
                .Select(x => new GetCertificateTemplateResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    TemplateName = x.TemplateName,
                    CertificateTitle = x.CertificateTitle,
                    TemplateSubtitle = x.TemplateSubtitle,
                    StatusApproval = x.StatusApproval,
                    CanApprove = x.CanApprove,
                    CanDelete = x.CanDelete
                }).ToList();

            var count = param.CanCountWithoutFetchDb(certificateTemplates.ToList().Count)
                ? certificateTemplates.ToList().Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(certificateTemplates as object, param.CreatePaginationProperty(count));
        }
    }
}
