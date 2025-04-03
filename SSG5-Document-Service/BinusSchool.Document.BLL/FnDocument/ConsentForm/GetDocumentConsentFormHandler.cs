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
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Document.FnDocument.ConsentForm
{
    public class GetDocumentConsentFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;


        public GetDocumentConsentFormHandler(
         IDocumentDbContext dbContext,
         IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;           
        }
        protected override async Task<ApiErrorResult<object>> Handler()    
        {
            var param = Request.ValidateParams<GetDocumentConsentFormRequest>(nameof(GetDocumentConsentFormRequest.IdSchool));

            var GetBLP = await _dbContext.Entity<MsBLPEmail>()
                              .Include(bem => bem.BLPSetting)
                              .Where(x => x.BLPSetting.IdSchool == param.IdSchool &&
                                          x.IdSurveyCategory == "1" &&
                                          x.BLPFinalStatus == BLPFinalStatus.Allowed)
                              .FirstOrDefaultAsync();

            if (GetBLP == null)
            {
                throw new BadRequestException("Failed to send email");
            }

            var container = GetContainerSasUri(1);
            var getDocument = GetDocument(GetBLP.FilePath, container);
            GetDocumentConsentFormResult ReturnResult = new GetDocumentConsentFormResult();
            ReturnResult.DocumentLink = getDocument;

            return Request.CreateApiResult2(ReturnResult as object);
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
