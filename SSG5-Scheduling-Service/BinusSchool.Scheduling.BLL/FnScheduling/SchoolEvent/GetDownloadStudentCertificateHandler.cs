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
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDownloadStudentCertificateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDownloadStudentCertificateHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
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

        private DataStaff GetDataStaff(string IdStaff)
        {
            if (IdStaff == null)
            {
                return null;
            }
            else
            {
                var dataStaff = (from a in _dbContext.Entity<MsUser>()
                                join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                join sc in _dbContext.Entity<MsSchool>() on us.IdSchool equals sc.Id
                                join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                where a.Id == IdStaff
                                select new DataStaff
                                {
                                    Id = s.IdBinusian,
                                    Fullname = s.FirstName + " " + s.LastName,
                                    BinusianID = s.IdBinusian,
                                    SchoolName = sc.Name,
                                    SchoolLogo = "https://bssschoolstorage.blob.core.windows.net/school-logo/BinusSchool_Serpong.png"
                                }).FirstOrDefault();
                // var dataStaff = _dbContext.Entity<MsStaff>()
                //                     .Select(x => new DataStaff
                //                     {
                //                         Id = x.IdBinusian,
                //                         Fullname = x.FirstName + " " + x.LastName,
                //                         BinusianID = x.IdBinusian
                //                     })
                //                     .Where(x => x.Id == IdStaff)
                //                     .FirstOrDefault();

                return dataStaff;
            }
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDownloadStudentCertificateRequest>(nameof(GetDownloadStudentCertificateRequest.IdEventActivityAward));

            var checkDataEventActivityAward = _dbContext.Entity<TrEventActivityAward>().Where(x => x.Id == param.IdEventActivityAward).FirstOrDefault();

            // if(checkDataEventActivityAward == null)
            //     throw new BadRequestException($"Data School Event Involvement/Award not found");
            if(checkDataEventActivityAward != null)
            {
                var predicate = PredicateBuilder.Create<TrEventActivityAward>(x => true);


                var query = _dbContext.Entity<TrEventActivityAward>()
                    .Include(x => x.Award)
                    .Include(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.School)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Activity)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.CertificateTemplate)
                                .ThenInclude(x => x.User1)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.CertificateTemplate)
                                .ThenInclude(x => x.User2)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.EventDetails)
                    .Where(predicate).Where(x => x.Id == param.IdEventActivityAward);

                var dataAward = await query
                        .SingleOrDefaultAsync(CancellationToken);

                var result = new GetDownloadStudentCertificateResult
                    {
                        Id = dataAward.Id,
                        TemplateName =  !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ?  dataAward.EventActivity.Event.CertificateTemplate.Name : null,
                        CertificateTitle = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Title : null,
                        TemplateSubtitle = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.SubTitle : null,
                        Description = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Description : null,
                        StudentName = dataAward.HomeroomStudent.Student.FirstName + " " + dataAward.HomeroomStudent.Student.LastName,
                        AwardName = dataAward.Award.Description,
                        EventName = dataAward.EventActivity.Event.Name,
                        EventDate = dataAward.EventActivity.Event.EventDetails.First().StartDate,
                        SchoolName = dataAward.HomeroomStudent.Student.School.Name,
                        Background = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Background : null,
                        Signature1 = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Signature1 != null ? new CodeWithIdVm
                        {
                            Id = dataAward.EventActivity.Event.CertificateTemplate.Signature1,
                            Description = GetSignature1Name(dataAward.EventActivity.Event.CertificateTemplate),
                        } : null : null,
                        Signature1As = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Signature1 != null ? dataAward.EventActivity.Event.CertificateTemplate.Signature1As : null : null,
                        Signature2 = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Signature2 != null ? new CodeWithIdVm
                        {
                            Id = dataAward.EventActivity.Event.CertificateTemplate.Signature2,
                            Description = GetSignature2Name(dataAward.EventActivity.Event.CertificateTemplate),
                        } : null : null,
                        Signature2As = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.Signature2 != null ? dataAward.EventActivity.Event.CertificateTemplate.Signature2As : null : null,
                        IsUseBinusLogo = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.IsUseBinusLogo : false,
                        LinkSchoolBinusLogo = !string.IsNullOrEmpty(dataAward.EventActivity.Event.IdCertificateTemplate) ? dataAward.EventActivity.Event.CertificateTemplate.IsUseBinusLogo == true ? dataAward.HomeroomStudent.Student.School.Logo : null : null,
                        UrlCertificate = dataAward.Url,
                        OriginalFilename = dataAward.OriginalFilename
                    };

                    return Request.CreateApiResult2(result as object);
            }

            var checkDataEventActivityAwardTeacher = _dbContext.Entity<TrEventActivityAwardTeacher>().Where(x => x.Id == param.IdEventActivityAward).FirstOrDefault();

            if(checkDataEventActivityAwardTeacher == null)
                throw new BadRequestException($"Data School Event Involvement/Award not found");

            var predicateTeacher = PredicateBuilder.Create<TrEventActivityAwardTeacher>(x => true);


            var queryTeacher = _dbContext.Entity<TrEventActivityAwardTeacher>()
                .Include(x => x.Award)
                .Include(x => x.Staff)
                .Include(x => x.EventActivity)
                    .ThenInclude(x => x.Activity)
                .Include(x => x.EventActivity)
                    .ThenInclude(x => x.Event)
                        .ThenInclude(x => x.CertificateTemplate)
                            .ThenInclude(x => x.User1)
                .Include(x => x.EventActivity)
                    .ThenInclude(x => x.Event)
                        .ThenInclude(x => x.CertificateTemplate)
                            .ThenInclude(x => x.User2)
                .Include(x => x.EventActivity)
                    .ThenInclude(x => x.Event)
                        .ThenInclude(x => x.EventDetails)
                .Where(predicateTeacher).Where(x => x.Id == param.IdEventActivityAward);

            var dataAwardTeacher = await queryTeacher
                    .SingleOrDefaultAsync(CancellationToken);

            var resultTeacher = new GetDownloadStudentCertificateResult
                {
                    Id = dataAwardTeacher.Id,
                    TemplateName =  !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ?  dataAwardTeacher.EventActivity.Event.CertificateTemplate.Name : null,
                    CertificateTitle = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Title : null,
                    TemplateSubtitle = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.SubTitle : null,
                    Description = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Description : null,
                    StudentName = GetDataStaff(dataAwardTeacher.IdStaff).Fullname,
                    AwardName = dataAwardTeacher.Award.Description,
                    EventName = dataAwardTeacher.EventActivity.Event.Name,
                    EventDate = dataAwardTeacher.EventActivity.Event.EventDetails.First().StartDate,
                    SchoolName = GetDataStaff(dataAwardTeacher.IdStaff).SchoolName,
                    Background = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Background : null,
                    Signature1 = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature1 != null ? new CodeWithIdVm
                    {
                        Id = dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature1,
                        Description = GetSignature1Name(dataAwardTeacher.EventActivity.Event.CertificateTemplate),
                    } : null : null,
                    Signature1As = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature1 != null ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature1As : null : null,
                    Signature2 = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature2 != null ? new CodeWithIdVm
                    {
                        Id = dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature2,
                        Description = GetSignature2Name(dataAwardTeacher.EventActivity.Event.CertificateTemplate),
                    } : null : null,
                    Signature2As = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature2 != null ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.Signature2As : null : null,
                    IsUseBinusLogo = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.IsUseBinusLogo : false,
                    LinkSchoolBinusLogo = !string.IsNullOrEmpty(dataAwardTeacher.EventActivity.Event.IdCertificateTemplate) ? dataAwardTeacher.EventActivity.Event.CertificateTemplate.IsUseBinusLogo == true ? GetDataStaff(dataAwardTeacher.IdStaff).SchoolLogo : null : null,
                    UrlCertificate = dataAwardTeacher.Url,
                    OriginalFilename = dataAwardTeacher.OriginalFilename
                };

                return Request.CreateApiResult2(resultTeacher as object);

        }
    }
}
