using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.File;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection
{
    public class UpdateReflectionContentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _containerBlobName;
        public UpdateReflectionContentHandler(IStudentDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _containerBlobName = "portfolio";

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

        public CloudStorageAccount GetCloudStorageAccount()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration["ConnectionStrings:Student:AccountStorage"]);
            return storageAccount;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateReflectionContentRequest, UpdateReflectionContentValidator>();

            CloudBlockBlob blob;
            var paramRegex = "";
            var contentType = "";
            var extension = "";
            var nameFileOrigin = "";
            string pattern = @"(<img.*?src=\""data:.*?\/>)";
            var attachments = new List<TrReflectionStudentAttachment>();
            FillConfiguration();

            //create file container in blob storage 
            CreateContainerIfNotExists(_containerBlobName);

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerBlobName);

            var query = await (from trRefrection in _dbContext.Entity<TrReflectionStudent>()
                               join academicYears in _dbContext.Entity<MsAcademicYear>() on trRefrection.IdAcademicYear equals academicYears.Id
                               join schools in _dbContext.Entity<MsSchool>() on academicYears.IdSchool equals schools.Id
                               where trRefrection.Content.Contains("<img") && schools.Id == body.IdSchool
                               select trRefrection).ToListAsync(CancellationToken);

            foreach (var item in query)
            {
                var splitImage = item.Content.Split("<img");
                var listImage = splitImage.ToList();
                foreach (var itemData in listImage)
                {
                    var image = itemData.Contains("base64");
                    if (image)
                    {
                        var strImage = "";
                        var haveStyle = itemData.Contains("style=");

                        if (haveStyle)
                        {
                            var replaceStyle = Regex.Replace(itemData, "style=[\"'](.+?)[\"']", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            strImage = $"<img{replaceStyle}";
                        }
                        else
                        {
                            strImage = $"<img src={itemData}";
                        }

                        string matchString = Regex.Match(strImage, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                        if (matchString == "")
                        {
                            continue;
                        }

                        string typeFile = matchString.Split(";")[0];

                        switch (typeFile)
                        {//check image's extension
                            case "data:image/jpeg":
                                paramRegex = "data:image/jpeg;base64";
                                contentType = "image/jpeg";
                                break;
                            case "data:image/png":
                                paramRegex = "data:image/png;base64";
                                contentType = "image/png";
                                break;
                            case "data:image/jpg":
                                paramRegex = "data:image/jpg;base64";
                                contentType = "image/jpg";
                                break;
                            case "data:image/gif":
                                paramRegex = "data:image/gif;base64";
                                contentType = "image/gif";
                                break;
                            default://should write cases for more images types
                                throw new BadRequestException("File Extention Not Allowed for Upload");
                                break;
                        }

                        string finalString = Regex.Match(matchString, $"(?<={paramRegex},)[^\"]*").Value;
                        string forRemove = Regex.Match(item.Content, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Value;
                        nameFileOrigin = Regex.Match(forRemove, $"data-filename=[\"'](.+?)[\"']", RegexOptions.IgnoreCase).Groups[1].Value;

                        if (nameFileOrigin == "")
                        {
                            nameFileOrigin = item.Id.ToString();

                            switch (typeFile)
                            {//check image's extension
                                case "data:image/jpeg":
                                    extension = ".jpeg";
                                    break;
                                case "data:image/png":
                                    extension = ".png";
                                    break;
                                case "data:image/jpg":
                                    extension = ".jpg";
                                    break;
                                case "data:image/gif":
                                    extension = ".gif";
                                    break;
                                default://should write cases for more images types
                                    break;
                            }
                        }
                        else
                        {
                            extension = Path.GetExtension(nameFileOrigin);
                        }

                        var nameFile = Path.GetFileNameWithoutExtension(nameFileOrigin) + "_" + DateTime.Now.ToString("yyyyHHmmss") + extension;

                        try
                        {
                            blob = container.GetBlockBlobReference(nameFile);
                            blob.Properties.ContentType = contentType;

                            var bytes = Convert.FromBase64String(finalString);// without data:image/jpeg;base64 prefix, just base64 string
                            using (var stream = new MemoryStream(bytes))
                            {
                                await blob.UploadFromStreamAsync(stream);
                            }
                            var fileSize = blob.Properties.Length;

                            var getUrl = blob.Uri;

                            //add data to tr reflection student attachment
                            attachments.Add(new TrReflectionStudentAttachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdReflectionStudent = item.Id,
                                Url = getUrl.AbsoluteUri,
                                FileName = nameFile,
                                FileNameOriginal = nameFileOrigin,
                                FileType = extension,
                                FileSize = fileSize,
                            });

                            //replace content string imaage base 64
                            var replaceString = item.Content.Replace(forRemove, "");
                            item.Content = replaceString;

                            _dbContext.Entity<TrReflectionStudent>().Update(item);
                        }
                        catch (Exception ex)
                        {
                            throw new BadRequestException("Failed upload file to container");
                        }
                    }
                }
            }

            //add tr reflection student attachment
            if (attachments.Count > 0)
            {
                await _dbContext.Entity<TrReflectionStudentAttachment>().AddRangeAsync(attachments, CancellationToken);
            }

            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}
