using System;
using System.Collections.Generic;
using System.IO;
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
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class ImmersionDocumentHandler : FunctionsHttpCrudHandler
    {
        private static readonly IDictionary<string, string[]> _allowedExtensions = new Dictionary<string, string[]>()
        {
            { "image", new[]{".png", ".jpg", ".jpeg"} },
            { "document", new[]{".pdf" } }
        };

        private static readonly IDictionary<string, string> _containerName = new Dictionary<string, string>()
        {
            { Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.brochure), "immersion-brochure" },
            { Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.poster),  "immersion-poster" }
        };

        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public ImmersionDocumentHandler(ISchedulingDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> idImmersions)
        {
            var datas = await _dbContext.Entity<MsImmersion>()
                            .Where(x => idImmersions.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            idImmersions = idImmersions.Except(idImmersions.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = idImmersions.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // remove files
            var containerPoster = _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.poster)];
            var containerBrochure = _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.brochure)];

            foreach (var data in datas)
            {
                await RemoveFileIfExists(data.PosterFileName, containerPoster);
                data.BrochureFileName = "";

                await RemoveFileIfExists(data.BrochureFileName, containerBrochure);
                data.PosterFileName = "";

                _dbContext.Entity<MsImmersion>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var getImmersion = await _dbContext.Entity<MsImmersion>()
                                        .FindAsync(id);

            if (getImmersion is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Immersion"], "Id", id));

            var containerPoster = GetContainerSasUri(1, _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.poster)]);
            var containerBrochure = GetContainerSasUri(1, _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.brochure)]);

            var ReturnResult = new ImmersionDocumentResult_GetDetail()
            {
                IdImmersion = getImmersion.Id,
                BrochureFileName = getImmersion.BrochureFileName,
                BrochureLink = (!string.IsNullOrEmpty(getImmersion.BrochureFileName) ? GetDocumentLink(getImmersion.BrochureFileName, containerBrochure) : null),
                PosterFileName = getImmersion.PosterFileName,
                PosterLink = (!string.IsNullOrEmpty(getImmersion.PosterFileName) ? GetDocumentLink(getImmersion.PosterFileName, containerPoster) : null),
            };

            return Request.CreateApiResult2(ReturnResult as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<ImmersionDocumentRequest_Get>(nameof(ImmersionDocumentRequest_Get.IdImmersions));

            var idImmersionList = param.IdImmersions.ToList();
            idImmersionList = idImmersionList.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var containerPoster = GetContainerSasUri(1, _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.poster)]);
            var containerBrochure = GetContainerSasUri(1, _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), ImmersionDocumentRequest_ImmersionDocumentType.brochure)]);

            IReadOnlyList<IItemValueVm> returnResult;

            returnResult = await _dbContext.Entity<MsImmersion>()
                                .Where(x => idImmersionList.Any(y => y == x.Id))
                                .Select(x => new ImmersionDocumentResult_Get
                                {
                                    Id = x.Id,
                                    BrochureFileName = x.BrochureFileName,
                                    BrochureLink = (!string.IsNullOrEmpty(x.BrochureFileName) ? GetDocumentLink(x.BrochureFileName, containerBrochure) : null),
                                    PosterFileName = x.PosterFileName,
                                    PosterLink = (!string.IsNullOrEmpty(x.PosterFileName) ? GetDocumentLink(x.PosterFileName, containerPoster) : null),
                                })
                                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(returnResult);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
            #region not implemented
            //    var param = await Request.GetBodyForm<ImmersionDocumentRequest_Post>();

            //    if (string.IsNullOrEmpty(param.IdImmersion))
            //        throw new BadRequestException(null);

            //    var containerName = _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), param.ImmersionDocumentType)];

            //    var file = Request.Form.Files.FirstOrDefault();

            //    if (file is null || file.Length == 0)
            //        throw new BadRequestException("Support Document file not found");

            //    var fileInfo = new FileInfo(file.FileName);
            //    if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension))
            //        throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"])}");

            //    string fileName = "";
            //    try
            //    {
            //        await UploadFile(file, containerName, out var blob, out fileName);

            //        //var uploadedFileUri = blob.Uri.AbsoluteUri;

            //        // update MsImmersion -> insert uri to table
            //        var updateImmersion = _dbContext.Entity<MsImmersion>()
            //                                .Find(param.IdImmersion);

            //        if (updateImmersion == null)
            //            throw new BadRequestException(null);

            //        switch (param.ImmersionDocumentType)
            //        {
            //            case ImmersionDocumentRequest_ImmersionDocumentType.brochure:
            //                updateImmersion.BrochureFileName = fileName.Trim();
            //                break;

            //            case ImmersionDocumentRequest_ImmersionDocumentType.poster:
            //                updateImmersion.PosterFileName = fileName.Trim();
            //                break;
            //        };

            //        _dbContext.Entity<MsImmersion>().Update(updateImmersion);
            //        await _dbContext.SaveChangesAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        // If newly created master immersion, then delete the master immersion data
            //        if (param.NewCreatedImmersion)
            //        {
            //            var getImmersion = _dbContext.Entity<MsImmersion>()
            //                                    .Find(param.IdImmersion);

            //            if(getImmersion != null)
            //            {
            //                _dbContext.Entity<MsImmersion>().Remove(getImmersion);
            //                await _dbContext.SaveChangesAsync();
            //            }
            //        }

            //        await RemoveFileIfExists(fileName, containerName);

            //        throw new BadRequestException("Failed to save data, please try again");
            //    }

            //    return Request.CreateApiResult2();
            //}

            //protected override async Task<ApiErrorResult<object>> PutHandler()
            //{
            //    var param = await Request.GetBodyForm<ImmersionDocumentRequest_Put>();

            //    if (string.IsNullOrEmpty(param.IdImmersion))
            //        throw new BadRequestException(null);

            //    var containerName = _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), param.ImmersionDocumentType)];

            //    var file = Request.Form.Files.FirstOrDefault();

            //    if (file is null || file.Length == 0)
            //        throw new BadRequestException("Support Document file not found");

            //    var fileInfo = new FileInfo(file.FileName);
            //    if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension))
            //        throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"])}");

            //    string fileName = file.FileName;
            //    try
            //    {
            //        var updateImmersion = _dbContext.Entity<MsImmersion>()
            //                                .Find(param.IdImmersion);

            //        if (updateImmersion == null)
            //            throw new BadRequestException(null);

            //        await UploadFile(file, containerName, out var blob, out fileName);

            //        var oldFileName = "";

            //        switch (param.ImmersionDocumentType)
            //        {
            //            case ImmersionDocumentRequest_ImmersionDocumentType.brochure:
            //                oldFileName = updateImmersion.BrochureFileName?.Trim();
            //                updateImmersion.BrochureFileName = fileName;
            //                break;

            //            case ImmersionDocumentRequest_ImmersionDocumentType.poster:
            //                oldFileName = updateImmersion.PosterFileName?.Trim();
            //                updateImmersion.PosterFileName = fileName;
            //                break;
            //        };

            //        // update DB
            //        _dbContext.Entity<MsImmersion>().Update(updateImmersion);

            //        // remove old file
            //        await RemoveFileIfExists(oldFileName, containerName);

            //        await _dbContext.SaveChangesAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        // If newly created master immersion, then delete the master immersion data
            //        if (param.NewCreatedImmersion)
            //        {
            //            var getImmersion = _dbContext.Entity<MsImmersion>()
            //                                    .Find(param.IdImmersion);

            //            if (getImmersion != null)
            //            {
            //                _dbContext.Entity<MsImmersion>().Remove(getImmersion);
            //                await _dbContext.SaveChangesAsync();
            //            }
            //        }

            //        await RemoveFileIfExists(fileName, containerName);

            //        throw new BadRequestException("Failed to save data, please try again");
            //    }

            //    return Request.CreateApiResult2();
            #endregion
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var param = await Request.GetBodyForm<ImmersionDocumentRequest_Put>();

            string containerName = "";
            string fileName = "";

            try
            {
                if (string.IsNullOrEmpty(param.IdImmersion))
                throw new BadRequestException(null);

                var immersionDocumentTypeEnum = new ImmersionDocumentRequest_ImmersionDocumentType();

                if (param.ImmersionDocumentType == "brochure")
                    immersionDocumentTypeEnum = ImmersionDocumentRequest_ImmersionDocumentType.brochure;
                else if (param.ImmersionDocumentType == "poster")
                    immersionDocumentTypeEnum = ImmersionDocumentRequest_ImmersionDocumentType.poster;
                else
                    throw new BadRequestException("Failed to upload file (1)");

                containerName = _containerName[Enum.GetName(typeof(ImmersionDocumentRequest_ImmersionDocumentType), immersionDocumentTypeEnum)];

                var file = Request.Form.Files.FirstOrDefault();

                if (file is null || file.Length == 0)
                    throw new BadRequestException("Immersion Document file not found");

                var fileInfo = new FileInfo(file.FileName);
                if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension) && !_allowedExtensions["document"].Any(x => x == fileInfo.Extension))
                    throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"]) + ", " +  string.Join(", ", _allowedExtensions["document"])}");

                fileName = file.FileName;
                    var updateImmersion = _dbContext.Entity<MsImmersion>()
                                            .Find(param.IdImmersion);

                if (updateImmersion == null)
                    throw new BadRequestException(null);

                await UploadFile(file, containerName, out var blob, out fileName);

                var oldFileName = "";

                switch (immersionDocumentTypeEnum)
                {
                    case ImmersionDocumentRequest_ImmersionDocumentType.brochure:
                        oldFileName = updateImmersion.BrochureFileName?.Trim();
                        updateImmersion.BrochureFileName = fileName;
                        break;

                    case ImmersionDocumentRequest_ImmersionDocumentType.poster:
                        oldFileName = updateImmersion.PosterFileName?.Trim();
                        updateImmersion.PosterFileName = fileName;
                        break;
                };

                // update DB
                _dbContext.Entity<MsImmersion>().Update(updateImmersion);

                // remove old file
                await RemoveFileIfExists(oldFileName, containerName);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // If newly created master immersion, then delete the master immersion data
                if (param.NewCreatedImmersion)
                {
                    var immersionGradeMapping = _dbContext.Entity<TrImmersionGradeMapping>()
                                                    .Where(x => x.IdImmersion == param.IdImmersion)
                                                    .ToList();

                    var getImmersion = _dbContext.Entity<MsImmersion>()
                                            .Find(param.IdImmersion);

                    if (getImmersion != null)
                    {
                        if(immersionGradeMapping.Count > 0)
                            _dbContext.Entity<TrImmersionGradeMapping>().RemoveRange(immersionGradeMapping);
                        
                        _dbContext.Entity<MsImmersion>().Remove(getImmersion);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                await RemoveFileIfExists(fileName, containerName);

                throw new BadRequestException(string.IsNullOrEmpty(ex?.Message.ToString()) ? "Failed to save data, please try again" : ex.Message.ToString());
            }

            return Request.CreateApiResult2();
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
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Scheduling:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Scheduling:AccountStorage"]);
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
