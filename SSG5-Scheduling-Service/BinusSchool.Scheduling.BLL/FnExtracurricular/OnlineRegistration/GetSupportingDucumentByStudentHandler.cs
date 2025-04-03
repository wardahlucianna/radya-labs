using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetSupportingDucumentByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        public GetSupportingDucumentByStudentHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSupportingDucumentByStudentRequest>(nameof(GetSupportingDucumentByStudentRequest.IdStudent));

            var getPeriodActive = await _dbContext.Entity<MsPeriod>()
                                  .Include(x => x.Grade)
                                  .ThenInclude(y => y.StudentGrades)
                                  .Where(a => a.Grade.StudentGrades.Any(b => b.IdStudent == param.IdStudent)
                                  && _dateTime.ServerTime >= a.StartDate
                                  && _dateTime.ServerTime <= a.EndDate)
                                  .Select(a => new {
                                      a.Grade.Level.IdAcademicYear,
                                      a.Semester,
                                      a.Grade
                                  })
                                  .ToListAsync(CancellationToken);


            if ((getPeriodActive?.Count() ?? 0) == 0)             
                throw new BadRequestException("Period Active has not found");         

            var ReturnResult = await _dbContext.Entity<MsExtracurricularSupportDoc>()
                              .Include(x => x.ExtracurricularSupportDocGrades)
                                  .ThenInclude(y => y.Grade)
                                  .ThenInclude(y => y.Level)
                           .Where(x => x.ExtracurricularSupportDocGrades.Any(a => a.IdGrade == getPeriodActive.FirstOrDefault().Grade.Id)
                           && x.Status == true
                           && (param.ShowToStudent == true ? x.ShowToStudent : x.ShowToParent) == true
                           )
                           .Select(x => new GetSupportingDucumentByStudentResult
                           {
                               IdExtracurricularSupportDoc = x.Id,
                               FileName = (x.FileName != null || x.FileName != "" ? x.FileName : null),
                               DocumentName = x.Name,
                               Grade = getPeriodActive.Select(a => new ItemValueVm() { Id = a.Grade.Id, Description = a.Grade.Description }).FirstOrDefault(),
                               Status = x.Status

                           }).ToListAsync(CancellationToken);


            if (ReturnResult?.Count() > 0)
            {
                var container = GetContainerSasUri(1);
                ReturnResult.ForEach(a => a.DocumentLink = (a.FileName != null ? GetDocument(a.FileName, container) : null));

            }


            return Request.CreateApiResult2(ReturnResult as object);
        }

        public string GetContainerSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("extracurricular-sup-doc");

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
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Scheduling:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Scheduling:AccountStorage"]);
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
