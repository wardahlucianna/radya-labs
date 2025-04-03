using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model.Abstractions;
using Microsoft.AspNetCore.Http;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.BlendedLearningProgram
{
    public class UploadFileBLPHandler : FunctionsHttpCrudHandler
    {
        private static readonly string[] _allowedFiles = new[] { ".png", ".jpg", ".jpeg", ".pdf" };

        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public UploadFileBLPHandler(IDocumentDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> idRespondent)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var param = await Request.GetBodyForm<UploadFileBLPRequest>();
            try
            {
                string containerName = "";
                string fileName = "";

                if (string.IsNullOrEmpty(param.IdStudent))
                    throw new BadRequestException(null);

                var student = await _dbContext.Entity<MsStudent>()
                        .Where(x => x.Id == param.IdStudent)
                        .SingleOrDefaultAsync(CancellationToken);
                if (student is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["IdStudent"], "Not Exist", "IdStudent"));

                var getStudentData = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Student)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.GradePathwayClassroom)
                            .ThenInclude(x => x.Classroom)
                        .Include(x => x.Homeroom)
                            .ThenInclude(x => x.Grade)
                        .Where(x => x.IdStudent == student.Id)
                        .FirstOrDefaultAsync(CancellationToken);

                //var getHomeroom = (getStudentData?.Homeroom?.Grade?.Code == null ? "" : getStudentData.Homeroom.Grade.Code) + " "
                //    + (getStudentData.Homeroom?.GradePathwayClassroom?.Classroom?.Code == null ? "" : getStudentData.Homeroom.GradePathwayClassroom.Classroom.Code);

                var getGrade = getStudentData?.Homeroom?.Grade?.Description == null ? "" : getStudentData.Homeroom.Grade.Code;

                containerName = "ccform-doc/" + student.IdSchool + "/" + getGrade + "/" + param.IdStudent; //Sementara sampai grade aja

                var file = Request.Form.Files.FirstOrDefault();
                if (file is null || file.Length == 0)
                    throw new BadRequestException("Document file not found");

                var fileInfo = new FileInfo(file.FileName);
                if (!_allowedFiles.Any(x => x == fileInfo.Extension))
                    throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedFiles)}");

                await UploadFile(file, containerName, out var blob, out fileName);

                var result = new UploadFileBLPRResult
                {
                    IsUpdate = param.IsUpdate,
                    IdStudent = param.IdStudent,
                    IdSurveyPeriod = param.IdSurveyPeriod,
                    IdClearanceWeekPeriod = param.IdClearanceWeekPeriod == null ? null : param.IdClearanceWeekPeriod,
                    IdSurveyQuestionMapping = param.IdSurveyQuestionMapping,
                    IdSurveyAnswerMapping = param.IdSurveyAnswerMapping,
                    FileName = containerName + "/"+ fileName
                };

                //var updateSurveyStudentAnswer = await _dbContext.Entity<TrSurveyStudentAnswer>()
                //    .Where(x => x.IdSurveyAnswerMapping == param.IdSurveyAnswerMapping)
                //    .Where(x => x.IdSurveyQuestionMapping == param.IdSurveyQuestionMapping)
                //    .Where(x => x.IdRespondent == param.IdRespondent)
                //    .FirstOrDefaultAsync(CancellationToken);

                //if (updateSurveyStudentAnswer == null)
                //{
                //    // Insert DB
                //    var AddSurveyStudentAnswer = new TrSurveyStudentAnswer
                //    {
                //        Id = Guid.NewGuid().ToString(),
                //        IdSurveyAnswerMapping = param.IdSurveyAnswerMapping,
                //        IdSurveyQuestionMapping = param.IdSurveyQuestionMapping,
                //        IdRespondent = param.IdRespondent,
                //        FilePath = fileName,
                //    };
                //    _dbContext.Entity<TrSurveyStudentAnswer>().Add(AddSurveyStudentAnswer);
                //}
                //else
                //{
                //    // Update DB
                //    updateSurveyStudentAnswer.FilePath = fileName;
                //    _dbContext.Entity<TrSurveyStudentAnswer>().Update(updateSurveyStudentAnswer);
                //}

                var oldFileName = "";
                //updateSurveyStudentAnswer.FilePath = fileName;

                // remove old file
                //await RemoveFileIfExists(oldFileName, containerName);

                //await _dbContext.SaveChangesAsync();
                //await _transaction.CommitAsync(CancellationToken);
                //return Request.CreateApiResult2();

                return Request.CreateApiResult2(result as object);
            }
            catch (Exception ex)
            {
                //var getAllHistoryAnswer = await _dbContext.Entity<HTrSurveyStudentAnswer>()
                //        .Where(x => x.IdRespondent == param.IdRespondent)
                //        .ToListAsync(CancellationToken);

                //var getAllAnswer = await _dbContext.Entity<TrSurveyStudentAnswer>()
                //    .Where(x => x.IdRespondent == param.IdRespondent)
                //    .ToListAsync(CancellationToken);

                //var getrespondent = await _dbContext.Entity<MsRespondent>()
                //        .Where(x => x.Id == param.IdRespondent)
                //        .ToListAsync(CancellationToken);

                //if (!param.IsUpdate)
                //{
                //    if (getAllAnswer != null)
                //        _dbContext.Entity<TrSurveyStudentAnswer>().RemoveRange(getAllAnswer);
                //    _dbContext.Entity<MsRespondent>().RemoveRange(getrespondent);
                //}
                //else
                //{
                //    if (getAllAnswer != null)
                //        _dbContext.Entity<TrSurveyStudentAnswer>().RemoveRange(getAllAnswer);

                //    var newHistory = new List<TrSurveyStudentAnswer>();

                //    foreach (var data in getAllHistoryAnswer)
                //    {
                //        var history = new TrSurveyStudentAnswer
                //        {
                //            Id = data.IdHTrSurveyStudentAnswer,
                //            IdRespondent = data.IdRespondent,
                //            IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                //            IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                //            IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                //            Description = data.Description,
                //            FilePath = data.FilePath
                //        };
                //        newHistory.Add(history);
                //    }

                //    if (getAllHistoryAnswer != null)
                //        _dbContext.Entity<TrSurveyStudentAnswer>().AddRange(newHistory);
                //    //_dbContext.Entity<HTrSurveyStudentAnswer>().RemoveRange(getAllHistoryAnswer);
                //}
                //await _dbContext.SaveChangesAsync();
                //await RemoveFileIfExists(fileName, containerName);
                throw new Exception(ex.Message);
            }
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        public static string GetDocumentLink(string filename, string containerLink)
        {
            string url = containerLink.Replace("?", "/" + filename + "?");

            return url;
        }

        public string GetContainerSasUri(int expiryHour, string containerName, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName);

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

        private string GetBlobSasUri(string blobName, int expiryHour, string containerName, string policyName = null)
        {
            string sasBlobToken;

            CloudBlobContainer container;


            //if (AppType == 1)
            //{
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName);

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            // Return the URI string for the container, including the SAS token.
            //return blob.Uri + sasBlobToken;

            //Return blob SAS Token
            return sasBlobToken;
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

        public void CreateContainerIfNotExists(string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        public Task UploadFile(IFormFile file, string containerName, out CloudBlockBlob blob, out string uploadedFileName)
        {
            string extension = Path.GetExtension(file.FileName);
            uploadedFileName = Path.GetFileNameWithoutExtension(file.FileName);

            #region Upload File to Azure Storage  
            // create file container in blob storage 
            CreateContainerIfNotExists(containerName);

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            uploadedFileName = uploadedFileName + "_" + _dateTime.ServerTime.ToString("yyyyHHmmss") + extension;

            blob = container.GetBlockBlobReference(uploadedFileName);
            blob.Properties.ContentType = file.ContentType;

            //uplod to azure storage
            return blob.UploadFromStreamAsync(file.OpenReadStream());
            #endregion
        }

        public Task RemoveFileIfExists(string fileName, string containerName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Task.CompletedTask;

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }
    }
}
