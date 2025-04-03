using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class DeleteCertificateTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteCertificateTemplateHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteCertificateTemplateRequest, DeleteCertificateTemplateValidator>();

            var data = await _dbContext.Entity<MsCertificateTemplate>().FindAsync(body.IdCertificateTemplate);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["CertificateTemplate"], "Id", body.IdCertificateTemplate));

            var dataDeclined = await _dbContext.Entity<MsCertificateTemplate>()
                .Where(x => x.Id == body.IdCertificateTemplate && x.ApprovalStatus == "Declined")
                .FirstOrDefaultAsync(CancellationToken);
            if (dataDeclined is null)
                throw new BadRequestException($"Certificate template can not delete, because certificate template on review/approved");

            data.IsActive = false;

            _dbContext.Entity<MsCertificateTemplate>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
