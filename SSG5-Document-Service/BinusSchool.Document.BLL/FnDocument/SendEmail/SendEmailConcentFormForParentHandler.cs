using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Document.FnDocument.SendEmail.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Document.FnDocument.SendEmail
{
    public class SendEmailConcentFormForParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        //private readonly IApiService<ISmtpEmail> _smtpEmailApi;
        private readonly ISendGrid _sendGridApi;

        public SendEmailConcentFormForParentHandler(
         IDocumentDbContext dbContext,
         IConfiguration configuration,
         ISendGrid sendGridApi
         )
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _sendGridApi = sendGridApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SendEmailConcentFormForParentRequest, SendEmailConcentFormForParentValidator>();

            //if (body.IdSchool != "3")
            //{
            //    return Request.CreateApiResult2(null, "OK", "Will be processed only for School Bekasi ");
            //}

            //FillConfiguration();
            //_smtpEmailApi.SetConfigurationFrom(ApiConfiguration);

            var From = _configuration.GetSection("EmailNotification:From").Get<string>();
            var SmtpServer = _configuration.GetSection("EmailNotification:SmtpServer").Get<string>();
            var Port = _configuration.GetSection("EmailNotification:Port").Get<int>();
            var Username = _configuration.GetSection("EmailNotification:Username").Get<string>();
            var Password = _configuration.GetSection("EmailNotification:Password").Get<string>();

            var studentResult = await _dbContext.Entity<TrBLPGroupStudent>()
                                                .Include(x => x.Student)
                                                     .ThenInclude(y => y.StudentParents)
                                                     .ThenInclude(y => y.Parent)
                                                .Where(a => a.IdAcademicYear == body.IdAcademicYear
                                                && a.Semester == body.Semester
                                                && a.IdBLPStatus == "1" //LF2FL
                                                && a.IdStudent == body.IdStudent)
                                                .FirstOrDefaultAsync();

            if (studentResult == null)
            {
                throw new BadRequestException("Failed - Student Status isn't LF2FL");
            }

            var SurveyPeriod = await _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(y => y.Grade)
                                                    .ThenInclude(y => y.SurveyPeriods)
                                                .Where(a => a.IdStudent == body.IdStudent
                                                && a.Homeroom.IdAcademicYear == body.IdAcademicYear
                                                && a.Homeroom.Semester == body.Semester
                                                && a.Homeroom.Grade.SurveyPeriods.Any(b => b.IdSurveyCategory == "1"))
                                                .Select(a => new {
                                                    IdSurveyPeriod = a.Homeroom.Grade.SurveyPeriods.OrderByDescending(b => b.StartDate).FirstOrDefault().Id
                                                })
                                                .ToListAsync();


            var AllowToFL2FLDesc = _dbContext.Entity<MsBLPEmail>()
                                 .Include(bem => bem.BLPSetting)
                                 .Include(bem => bem.RoleGroup)
                                 .Where(x => x.BLPSetting.IdSchool == body.IdSchool &&
                                             x.IdSurveyCategory == "1" &&
                                             x.BLPFinalStatus == BLPFinalStatus.Allowed &&
                                             x.RoleGroup.Code.ToUpper().Trim() == "PARENT")
                                 .FirstOrDefault();

            if (AllowToFL2FLDesc == null)
            {
                throw new BadRequestException("Failed to send email");
            }

            AllowToFL2FLDesc = FillEmailData(AllowToFL2FLDesc, body);


            var BLPAdditionalReceivers = _dbContext.Entity<MsBLPEmailAdditionalReceiver>()
                                            .Include(bear => bear.User)
                                            .Where(x => x.IdBLPEmail == AllowToFL2FLDesc.Id)
                                            .ToList();

            string additionalTo = "", additionalCC = "", additionalBCC = "";
            var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
            var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
            var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

            foreach (var emailParent in studentResult.Student.StudentParents)
            {
                var toParent = new SendSendGridEmailRequest_AddressBuilder()
                {
                    Address = emailParent.Parent.PersonalEmailAddress
                };

                additionalTo += emailParent.Parent.PersonalEmailAddress + ";";
                toList.Add(toParent);
            }

            //var to = new SendSendGridEmailRequest_AddressBuilder()
            //{
            //    Address = "sciputra@binus.edu"
            //};
            //additionalTo += "sciputra@binus.edu" + ";";
            //toList.Add(to);
                       
            foreach (var BLPAdditionalReceiver in BLPAdditionalReceivers)
            {
                var additionalReceiver = new SendSendGridEmailRequest_AddressBuilder()
                {
                    Address = BLPAdditionalReceiver.User.Email,
                    DisplayName = BLPAdditionalReceiver.User.DisplayName
                };
            
                if (BLPAdditionalReceiver.AddressType.ToUpper().Trim() == "CC")
                {
                    additionalCC += additionalReceiver.Address + ";";
                    ccList.Add(additionalReceiver);
                }

                if (BLPAdditionalReceiver.AddressType.ToUpper().Trim() == "BCC")
                {
                    additionalBCC += additionalReceiver.Address + ";";
                    bccList.Add(additionalReceiver);
                }
            }

            var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
            {
                //SmtpConfiguration = new SendSendGridEmailRequest_SmtpConfiguration
                //{
                //    HostServer = SmtpServer,
                //    Port = Port,
                //    FromAddress = From,
                //    FromDisplayName = Username,
                //    MailPassword = Password
                //},
                IdSchool = body.IdSchool,
                RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                {
                    ToList = toList
                },
                MessageContent = new SendSendGridEmailRequest_MessageContent
                {
                    Subject = "Concent Form Email For Parent",
                    BodyHtml = AllowToFL2FLDesc.Description
                }
            });

            var saveLog = new TrBLPEmailSentLog()
            {
                Id = Guid.NewGuid().ToString(),
                IdStudent = body.IdStudent,
                IdSurveyPeriod = SurveyPeriod.FirstOrDefault().IdSurveyPeriod,
                IdClearanceWeekPeriod = null,
                EmailSubject = AllowToFL2FLDesc.EmailSubject?.Trim(),
                HTMLDescription = AllowToFL2FLDesc.Description,
                PrimaryToAddress = additionalTo,
                AdditionalToAddress = additionalTo,
                AdditionalCCAddress = additionalCC,
                AdditionalBCCAddress = additionalBCC,
                ResendDate = null
            };

            _dbContext.Entity<TrBLPEmailSentLog>().Add(saveLog);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private MsBLPEmail FillEmailData(MsBLPEmail buildData, SendEmailConcentFormForParentRequest body)
        {
            var studentResult = _dbContext.Entity<MsHomeroomStudent>()
                                            .Include(x => x.Student)
                                            .Include(x => x.Homeroom)
                                                .ThenInclude(y => y.GradePathwayClassroom)
                                                .ThenInclude(y => y.Classroom)
                                            .Include(x => x.Homeroom).ThenInclude(y => y.Grade)
                                            .Where(a => a.IdStudent == body.IdStudent)
                                             .Select(a => new {                                                    
                                                   StudentName = (a.Student.FirstName == null ? "" : a.Student.FirstName + " ") + a.Student.LastName,
                                                   Class = a.Homeroom.Grade.Code + " " + a.Homeroom.GradePathwayClassroom.Classroom.Code
                                             }).FirstOrDefault();

            var container = GetContainerSasUri(24);
            var getDocument = GetDocument(buildData.FilePath, container);

            buildData.Description = buildData.Description
                                .Replace("{{StudentName}}", (studentResult?.StudentName ?? "{{StudentName}}"))
                                .Replace("{{StudentClass}}", (studentResult?.Class ?? "{{StudentClass}}"))                               
                                .Replace("{{DownloadLink}}", ((buildData.FilePath != null ? getDocument : null) ?? "{{DownloadLink}}"));

            return buildData;
        }

        public string GetContainerSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("clearance-form");

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
        }
        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Document:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Document:AccountStorage"]);
                return storageAccount;
            }

        }

        public static string GetDocument(string filename, string containerLink)
        {
            string url = containerLink.Replace("?", "/" + filename + "?");

            return url;
        }

    }
}
