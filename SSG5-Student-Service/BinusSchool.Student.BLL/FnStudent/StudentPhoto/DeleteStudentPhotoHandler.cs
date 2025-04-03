using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.StudentPhoto.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentPhoto
{
    public class DeleteStudentPhotoHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DeleteStudentPhotoHandler(
            IStudentDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteStudentPhotoRequest, DeleteStudentPhotoValidator>();

            var containerName = "studentphoto";

            var deleteStudentPhoto = await _dbContext.Entity<TrStudentPhoto>()
                                                .Where(x => param.IdStudentPhoto.Any(y => y == x.Id))
                                                .ToListAsync(CancellationToken);

            _dbContext.Entity<TrStudentPhoto>().RemoveRange(deleteStudentPhoto);

            await _dbContext.SaveChangesAsync();

            try
            {
                // delete student photo
                foreach (var photo in deleteStudentPhoto)
                {
                    var fileName = photo.FileName;
                    await RemoveFileIfExists(fileName, containerName);
                }
            }
            catch (Exception ex)
            {

            }

            return Request.CreateApiResult2();
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }

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
