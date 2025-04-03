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
    public class DetailCertificateTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DetailCertificateTemplateHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        private string GetSignature1Name(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature1 == null) return "-";
            return msCertificateTemplate.User1.DisplayName;
        }

        private string GetSignature2Name(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature2 == null) return "-";
            return msCertificateTemplate.User2.DisplayName;
        }

        private string GetHsCertificateTemplate(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.HistoryCertificateTemplateApprovers == null) return null;
            return msCertificateTemplate.HistoryCertificateTemplateApprovers.Where(x => x.DateUp != null).FirstOrDefault().Reason;
        }

        private string GetHsCertificateTemplateUserApprover(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.HistoryCertificateTemplateApprovers == null) return null;
            return msCertificateTemplate.HistoryCertificateTemplateApprovers.Where(x => x.DateUp != null).FirstOrDefault().User.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailCertificateTemplateRequest>(nameof(DetailCertificateTemplateRequest.UserId),nameof(DetailCertificateTemplateRequest.IdCertificateTemplate));
            var certificateTemplates = await GetDetailCertificateTemplate(param);

            return Request.CreateApiResult2(certificateTemplates as object);
        }

        public async Task<DetailCertificateTemplateResult> GetDetailCertificateTemplate(DetailCertificateTemplateRequest param)
        {
            var predicate = PredicateBuilder.Create<MsCertificateTemplate>(x => true);

            if(!string.IsNullOrWhiteSpace(param.IdCertificateTemplate))
                predicate = predicate.And(x => x.Id == param.IdCertificateTemplate);

            var msCertificateTemplates = await _dbContext.Entity<MsCertificateTemplate>()
                .Include(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Include(x => x.User1)
                .Include(x => x.User2)
                .Include(x => x.CertificateTemplateApprovers)
                .Include(x => x.HistoryCertificateTemplateApprovers)
                    .ThenInclude(x => x.User)
                .Where(predicate)
                .FirstOrDefaultAsync(CancellationToken);

            if(msCertificateTemplates == null)
                throw new BadRequestException($"Certificate template not found");
            
            var certificateTemplates = new DetailCertificateTemplateResult
                {
                    DateCreated = msCertificateTemplates.DateIn,
                    Id = msCertificateTemplates.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = msCertificateTemplates.AcademicYear.Id,
                        Description = msCertificateTemplates.AcademicYear.Description,
                    },
                    TemplateName = msCertificateTemplates.Name,
                    CertificateTitle = msCertificateTemplates.Title,
                    TemplateSubtitle = msCertificateTemplates.SubTitle,
                    StatusApproval = msCertificateTemplates.ApprovalStatus,
                    IsUseBinusLogo = msCertificateTemplates.IsUseBinusLogo,
                    LinkBinusLogo = msCertificateTemplates.AcademicYear.School.Logo,
                    Description = msCertificateTemplates.Description,
                    Reason = msCertificateTemplates.ApprovalStatus == "Declined" ?  GetHsCertificateTemplate(msCertificateTemplates) : null,
                    UserApproverName = msCertificateTemplates.ApprovalStatus == "Declined" ? GetHsCertificateTemplateUserApprover(msCertificateTemplates) : null,
                    Background = msCertificateTemplates.Background,
                    Signature1 = msCertificateTemplates.Signature1 != null ? new CodeWithIdVm
                    {
                        Id = msCertificateTemplates.Signature1,
                        Description = GetSignature1Name(msCertificateTemplates),
                    } : null,
                    Signature1As = msCertificateTemplates.Signature1As,
                    Signature2 = msCertificateTemplates.Signature2 != null ? new CodeWithIdVm
                    {
                        Id = msCertificateTemplates.Signature2,
                        Description = msCertificateTemplates.Signature2 != null ? GetSignature2Name(msCertificateTemplates) : null,
                    } : null,
                    Signature2As = msCertificateTemplates.Signature2As,
                    CanApprove = msCertificateTemplates.ApprovalStatus == "On Review" && msCertificateTemplates.CertificateTemplateApprovers.Any(y => y.IdUser == param.UserId),
                    CanEdit = msCertificateTemplates.ApprovalStatus == "Declined",
                    CanDelete = msCertificateTemplates.ApprovalStatus == "Declined"
                };

            return certificateTemplates;
        }
    }
}
