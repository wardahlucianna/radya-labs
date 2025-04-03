using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.File;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageUploadFileAttachmentHandler : FunctionsHttpSingleHandler
    {

        private readonly IUserDbContext _dbContext;

        public MessageUploadFileAttachmentHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            var param = Request.GetParams<GetFileRequest>();

            var file_extension = _dbContext.Entity<MsMessageOption>().Where(x => x.Code == "FILE" && x.IdSchool == param.IdSchool)
                        .Select(x=>new GetFileResult
                        {
                            Description = x.Value,
                        }).FirstOrDefault();

            var file_max = _dbContext.Entity<MsMessageOption>().Where(x => x.Code == "MAXFILESIZE" && x.IdSchool == param.IdSchool)
            .Select(x=>new GetFileResult
            {
                Description = x.Value,
            }).FirstOrDefault();

            var containerBlobName = Configuration["PathName"];

            //create file container in blob storage 
            CreateContainerIfNotExists(containerBlobName);

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerBlobName);

            var getData = Request.Form.Files.FirstOrDefault();
            CloudBlockBlob blob;

            //get file name and file extention
            string extent = Path.GetExtension(getData.FileName);

            //get file image extention allowed and size allowed
            // var fileExtentionImage = Configuration["Image:extention"].Split(",");
            // var fileMaxImage = Convert.ToInt16(Configuration["Image:maxSizeMB"]);

            //get file extention allowed and size allowed
            var fileExtentionFile = file_extension.Description.Split(";");
            var fileMaxFile = Convert.ToInt16(file_max.Description);

            var result = new UploadFileResult();
            var fileName = Path.GetFileNameWithoutExtension(getData.FileName) + "_" + DateTime.Now.ToString("yyyyHHmmss") + extent;
            result.FileName = fileName;
            // if (fileExtentionImage.Any(p => p.ToLower() == extent.ToLower()))
            // {
            //     var MaxFileSize = fileMaxImage * 1024 * 1024; //satuan mb unutk file image
            //     fileName = "Document_Image_Ref" + "_" + DateTime.Now.ToString("yyyyHHmmss") + extent;

            //     if (getData.Length > MaxFileSize)
            //         throw new BadRequestException($"File size Should Be UpTo {fileMaxImage} Mb");

            //     result.FileName = fileName;
            // }
            // else if (fileExtentionFile.Any(p => p.ToLower() == extent.ToLower()))
            // {
            //     var MaxFileSize = fileMaxFile * 1024 * 1024; //satuan mb unutk file File
            //     fileName = "Document_File_Ref" + "_" + DateTime.Now.ToString("yyyyHHmmss") + extent;

            //     if (getData.Length > MaxFileSize)
            //         throw new BadRequestException($"File size Should Be UpTo {fileMaxFile} Mb");

            //     result.FileName = fileName;
            // }
            // else
            // {
            //     throw new BadRequestException("File Extention Not Allowed for Upload");
            // }

            blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = getData.ContentType;

            //uploda to azure storage
            await blob.UploadFromStreamAsync(getData.OpenReadStream());

            // remove file if already exist
            if (!string.IsNullOrWhiteSpace(param.FileName))
                await RemoveFile(param.FileName, containerBlobName);

            var getUrl = blob.Uri;
            result.UrlPreview = getUrl.OriginalString;

            result.Type = extent.Replace(".", "");
            result.Size = getData.Length/1024;
            

            // var IdMessageAttachment = Guid.NewGuid().ToString();
            // _dbContext.Entity<TrMessageAttachment>().Add(new TrMessageAttachment
            // {
            //     Id = IdMessageAttachment,
            //     IdMessage = param.IdMessage,
            //     Url = getUrl.OriginalString
            // });

            // await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }

        public Task RemoveFile(string fileName, string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
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
        #region Private Method

        public CloudStorageAccount GetCloudStorageAccount()
        {
            var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:User:AccountStorage"]);
            return storageAccount;
        }

        #endregion
    }
}
