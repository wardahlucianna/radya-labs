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
    public class SetCertificateTemplateApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SetCertificateTemplateApprovalHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetCertificateTemplateApprovalRequest, SetCertificateTemplateApprovalValidator>();

            var user = _dbContext.Entity<MsUser>()
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == body.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var role = user.UserRoles.Where(x => x.IdUser == body.UserId).FirstOrDefault();
            if (role == null)
                throw new NotFoundException("Role not found");

            var certificateTemplate = _dbContext.Entity<MsCertificateTemplate>()
                .Include(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Where(x => x.Id == body.Id)
                .FirstOrDefault();
            if (certificateTemplate == null)
                throw new NotFoundException("Certificate Template not found");

            var userRoles = await _dbContext.Entity<MsUserRole>().Where(x => x.IdUser == body.UserId).Select(x => x.IdRole).ToListAsync();

            var isPrincipal = await _dbContext.Entity<TrRolePosition>()
                .Include(x => x.TeacherPosition)
                    .ThenInclude(x => x.Position)
                .AnyAsync(x => userRoles.Contains(x.IdRole) && x.TeacherPosition.Position.Code == "P");

            if (!isPrincipal)
                throw new NotFoundException("The user not allowed to approved/reject");

            var dataHsCertificateTemplateApprover = _dbContext.Entity<HMsCertificateTemplateApprover>()
                .Where(x => x.IdCertificateTemplate == certificateTemplate.Id)
                .FirstOrDefault();

            var dataPrincipal = await (from trntl in _dbContext.Entity<TrNonTeachingLoad>()
                                       join msntl in _dbContext.Entity<MsNonTeachingLoad>() on trntl.IdMsNonTeachingLoad equals msntl.Id
                                       join mstp in _dbContext.Entity<MsTeacherPosition>() on msntl.IdTeacherPosition equals mstp.Id
                                       join lp in _dbContext.Entity<LtPosition>() on mstp.IdPosition equals lp.Id
                                       join u in _dbContext.Entity<MsUser>() on trntl.IdUser equals u.Id
                                       join us in _dbContext.Entity<MsUserSchool>() on u.Id equals us.IdUser
                                       where us.IdSchool == certificateTemplate.AcademicYear.School.Id && (lp.Code == "P" || lp.Code == "VP")

                                       select new MsUser
                                       {
                                           Id = u.Id,
                                           DisplayName = u.DisplayName
                                       }).FirstOrDefaultAsync(CancellationToken);

            if (dataPrincipal == null)
                throw new NotFoundException("User position Principal/Vice Principal not found");

            if (body.IsApproved == false)
            {
                var historyCertificateTemplateApprover = new HMsCertificateTemplateApprover
                {
                    IdHMsCertificateTemplateApprover = Guid.NewGuid().ToString(),
                    State = 1,
                    IdCertificateTemplate = certificateTemplate.Id,
                    IdUser = dataPrincipal.Id
                };

                _dbContext.Entity<HMsCertificateTemplateApprover>().Add(historyCertificateTemplateApprover);
            }

            certificateTemplate.ApprovalStatus = body.IsApproved ? "Approved" : "Declined";

            _dbContext.Entity<MsCertificateTemplate>().Update(certificateTemplate);

            if (dataHsCertificateTemplateApprover != null)
            {
                dataHsCertificateTemplateApprover.Reason = body.Reason != null ? body.Reason : null;
                dataHsCertificateTemplateApprover.IsApproved = body.IsApproved;

                _dbContext.Entity<HMsCertificateTemplateApprover>().Update(dataHsCertificateTemplateApprover);
            }
            else
            {
                var historyCertificateTemplateApprover = new HMsCertificateTemplateApprover
                {
                    IdHMsCertificateTemplateApprover = Guid.NewGuid().ToString(),
                    State = 1,
                    IdCertificateTemplate = certificateTemplate.Id,
                    IdUser = dataPrincipal.Id
                };

                _dbContext.Entity<HMsCertificateTemplateApprover>().Add(historyCertificateTemplateApprover);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await SendEmailAndNotif(certificateTemplate, dataPrincipal.DisplayName, body.IsApproved);

            return Request.CreateApiResult2();
        }

        public async Task SendEmailAndNotif(MsCertificateTemplate msCertificateTemplate, string NameUserApproval, bool IsApproved)
        {
            if (!string.IsNullOrEmpty(NameUserApproval))
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>
                {
                    { "ApproverName", NameUserApproval },
                    { "AcademicYear", msCertificateTemplate.AcademicYear.Description },
                    { "TemplateName", msCertificateTemplate.Name },
                    { "CertificateTitle", msCertificateTemplate.Title },
                    { "TemplateSub-Title", msCertificateTemplate.SubTitle },
                    { "ApprovalStatus", msCertificateTemplate.ApprovalStatus }
                };

                if (IsApproved == true)
                {
                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EM8")
                        {
                            IdRecipients = new[] { msCertificateTemplate.UserIn },
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
                else
                {
                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EM9")
                        {
                            IdRecipients = new[] { msCertificateTemplate.UserIn },
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }
    }
}
