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
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class UpdateCertificateTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateCertificateTemplateHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateCertificateTemplateRequest, UpdateCertificateTemplateValidator>();

            var dataCertificateTemplate = await _dbContext.Entity<MsCertificateTemplate>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (dataCertificateTemplate is null)
                throw new BadRequestException($"Certificate template not found");

            var cekApproved = await _dbContext.Entity<MsCertificateTemplate>()
                .Where(x => x.Id == body.Id && x.ApprovalStatus == "Declined")
                .FirstOrDefaultAsync(CancellationToken);
            if (cekApproved is null)
                throw new BadRequestException($"Certificate can not update, because certificate template on review/approved");
            var cekAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Include(x => x.School)
                .Where(x => x.Id == body.IdAcademicYear)
                .FirstOrDefaultAsync(CancellationToken);
            if (cekAcademicYear is null)
                throw new BadRequestException($"Academic year not found");

            var cekSignature1 = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == body.Signature1)
                .FirstOrDefaultAsync(CancellationToken);
            if (cekSignature1 is null)
                throw new BadRequestException($"User in signature 1 not found");

            if(body.Signature2 != null){
                var cekSignature2 = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == body.Signature2)
                .FirstOrDefaultAsync(CancellationToken);
                if (cekSignature2 is null)
                    throw new BadRequestException($"User in signature 2 not found");
            }

            var isNameExist = await _dbContext.Entity<MsCertificateTemplate>()
                .Where(x => x.IdAcademicYear  == body.IdAcademicYear && x.Name.ToLower() == body.Name.ToLower() && x.Id != body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (isNameExist != null)
                throw new BadRequestException($"{body.Name} already exists in this academic year");

            int countDescriptionDataFirst = body.Description.Split("{").Length;

            int countDescriptionDataLast = body.Description.Split("}").Length;

            if(body.Description.ToCharArray().Count(c => c == '{') < 3 || body.Description.ToCharArray().Count(c => c == '}') < 3)
                throw new BadRequestException($"Minimun 3 variable");

            if(body.Description.ToCharArray().Count(c => c == '{') > 3 || body.Description.ToCharArray().Count(c => c == '}') > 3)
                throw new BadRequestException($"Maximum 3 variable");

            if(!body.Description.Contains("InvolvementAwardName"))
                throw new BadRequestException($"Variable InvolvementAwardName is required");

            if(!body.Description.Contains("EventName"))
                throw new BadRequestException($"Variable EventName is required");

            if(!body.Description.Contains("EventDate"))
                throw new BadRequestException($"Variable EventDate is required");

            var dataPrincipal = await (from trntl in _dbContext.Entity<TrNonTeachingLoad>()
                                   join msntl in _dbContext.Entity<MsNonTeachingLoad>() on trntl.IdMsNonTeachingLoad equals msntl.Id
                                   join mstp in _dbContext.Entity<MsTeacherPosition>() on msntl.IdTeacherPosition equals mstp.Id
                                   join lp in _dbContext.Entity<LtPosition>() on mstp.IdPosition equals lp.Id
                                   join u in _dbContext.Entity<MsUser>() on trntl.IdUser equals u.Id
                                   join us in _dbContext.Entity<MsUserSchool>() on u.Id equals us.IdUser
                                   where us.IdSchool == cekAcademicYear.IdSchool && (lp.Code == "P" || lp.Code == "VP")
                                    
                                  select new MsUser
                                  {
                                      Id = u.Id,
                                      DisplayName = u.DisplayName
                                  }).FirstOrDefaultAsync(CancellationToken);
            
            if (dataPrincipal == null)
                throw new NotFoundException("User position Principal/Vice Principal not found");

            var data = _dbContext.Entity<MsCertificateTemplate>().Include(x => x.AcademicYear)
                .FirstOrDefault(x => x.Id == body.Id);
            
            data.IdAcademicYear = body.IdAcademicYear;
            data.Name = body.Name;
            data.IsUseBinusLogo = body.IsUseBinusLogo;
            data.Title = body.Title;
            data.SubTitle = body.SubTitle;
            data.Description = body.Description;
            data.Background = body.Background;
            data.Signature1 = body.Signature1;
            data.Signature1As = body.Signature1As;
            data.Signature2 = body.Signature2 != null ? body.Signature2 : null;
            data.Signature2As = body.Signature2 != null ? body.Signature2As : null;
            data.ApprovalStatus = "On Review";

            _dbContext.Entity<MsCertificateTemplate>().Update(data);

            var dataApprover = _dbContext.Entity<MsCertificateTemplateApprover>()
                .FirstOrDefault(x => x.IdCertificateTemplate == data.Id);   

            _dbContext.Entity<MsCertificateTemplateApprover>().Update(dataApprover);

            var certificateTemplateApprover = new MsCertificateTemplateApprover
            {
                Id = Guid.NewGuid().ToString(),
                State = 1,
                IdCertificateTemplate = data.Id,
                IdUser = dataPrincipal.Id
            };

            _dbContext.Entity<MsCertificateTemplateApprover>().Add(certificateTemplateApprover);

            var dataStaff = _dbContext.Entity<MsUser>()
                .FirstOrDefault(x => x.Id == data.UserIn);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await SendEmailAndNotif(data, dataPrincipal.DisplayName, dataStaff.DisplayName);

            return Request.CreateApiResult2();
        }

        public async Task SendEmailAndNotif(MsCertificateTemplate msCertificateTemplate, string NameUserApproval, string NameStaff)
        {
            if (!string.IsNullOrEmpty(NameUserApproval))
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
                paramTemplateNotification.Add("ApproverName", NameUserApproval);
                paramTemplateNotification.Add("AcademicYear", msCertificateTemplate.AcademicYear.Description);
                paramTemplateNotification.Add("TemplateName", msCertificateTemplate.Name);
                paramTemplateNotification.Add("CertificateTitle", msCertificateTemplate.Title);
                paramTemplateNotification.Add("TemplateSub-Title", msCertificateTemplate.SubTitle);
                paramTemplateNotification.Add("ApprovalStatus", msCertificateTemplate.ApprovalStatus);
                paramTemplateNotification.Add("IdCertificateTemplate", msCertificateTemplate.Id);
                paramTemplateNotification.Add("StaffName", NameStaff);

                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EM7")
                    {
                        IdRecipients = new[] { msCertificateTemplate.CertificateTemplateApprovers.First().IdUser },
                        KeyValues = paramTemplateNotification
                    });
                    collector.Add(message);
                }
            }
        }
    }
}
