using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Helpers;
using BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable
{
    public class SaveFileXmlHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IApiService<ISubject> _subjectService;
        private IDbContextTransaction _transaction;
        public SaveFileXmlHandler(
            ISchedulingDbContext dbContext,
            IApiService<ISubject> subjectService)
        {
            _dbContext = dbContext;
            _subjectService = subjectService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            _subjectService.SetConfigurationFrom(ApiConfiguration);
            var _helper = new HelperXML(_dbContext, _subjectService);



            var check = await _helper.CheckVersionXmlHelpers(Request);
            if (string.IsNullOrWhiteSpace(check))
            {
                throw new BadRequestException("Version For XML file Not Suport for this system!");
            }
            else if (check != "aSc Timetables 2012 XML")
            {
                throw new BadRequestException("Version For XML file Not Suport for this system!");
            }

            var body = await Request.ValidateBodyForm<SaveFileAscTimetableRequest, SaveFileXmlValidator>();

            var getAsc = await _dbContext.Entity<TrAscTimetable>().Where(p => p.Id == body.IdAscTimetable).FirstOrDefaultAsync();

            if (getAsc == null)
                throw new BadRequestException("asc data not found");

            var containerBlobName = Configuration["PathName"];
            //create file container in blob storage 
            CreateContainerIfNotExists(containerBlobName);

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerBlobName);

            var getData = Request.Form.Files.FirstOrDefault();
            CloudBlockBlob blob;

            //get file name and file extention
            string extent = Path.GetExtension(getData.FileName);
            string NameFile = Path.GetFileNameWithoutExtension(getData.FileName);

            NameFile = NameFile + "_" + DateTime.Now.ToString("yyyyHHmmss") + extent;

            blob = container.GetBlockBlobReference(NameFile);
            blob.Properties.ContentType = getData.ContentType;

            //uploda to azure storage
            await blob.UploadFromStreamAsync(getData.OpenReadStream());

            //delete data jiga file name sebelum nya di inputkan kembali
            if (body.Type == "Reupload")
            {
                if (!string.IsNullOrWhiteSpace(getAsc.XmlFileName))
                {
                    await DeleteFile(getAsc.XmlFileName, containerBlobName);
                }
            }

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            getAsc.XmlFileName = NameFile;
            _dbContext.Entity<TrAscTimetable>().Update(getAsc);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }


        private async Task DeleteFile(string fileName, string containerName)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.DeleteIfExistsAsync();
        }

        private void CreateContainerIfNotExists(string containerName)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            string[] containers = new string[] { containerName };
            foreach (var item in containers)
            {
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Scheduling:AccountStorage"]);
            return storageAccount;
        }
    }
}
