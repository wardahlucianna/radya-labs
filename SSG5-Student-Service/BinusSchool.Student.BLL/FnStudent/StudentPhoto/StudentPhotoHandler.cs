using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.StudentPhoto.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentPhoto
{
    public class StudentPhotoHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public StudentPhotoHandler(IStudentDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
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
            var param = await Request.ValidateBody<SaveStudentPhotoDataRequest, SaveStudentPhotoDataValidator>();
            var deleteExemplaryAttachment = new List<string>();
            var containerName = "studentphoto";

            try
            {
                if (param.IdStudentPhoto == null)
                {

                    var isExist = await _dbContext.Entity<TrStudentPhoto>()
                        .Where(x => x.IdStudent == param.IdStudent)
                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (isExist != null)
                        throw new BadRequestException("Student Photo already exist in this AcademicYear");

                    var newTrStudentPhoto = new TrStudentPhoto()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = param.IdStudent,
                            IdBinusian = param.IdBinusian,
                            IdAcademicYear = param.IdAcademicYear,
                            FileName = param.FileName,
                            FilePath = param.FilePath,
                        };

                        _dbContext.Entity<TrStudentPhoto>().Add(newTrStudentPhoto);
                }
                else
                {
                    var isStudentPhotoExists = _dbContext.Entity<TrStudentPhoto>()
                        .Where(x => x.Id == param.IdStudentPhoto)
                        .FirstOrDefault();

                    if (isStudentPhotoExists == null)
                            throw new BadRequestException($"Error! No Photo found for StudentPhoto with Id: {param.IdStudentPhoto}");

                        isStudentPhotoExists.IdAcademicYear = param.IdAcademicYear;
                        isStudentPhotoExists.IdStudent = param.IdStudent;
                        isStudentPhotoExists.IdBinusian = param.IdBinusian;
                        isStudentPhotoExists.FileName = param.FileName;
                        isStudentPhotoExists.FilePath = param.FilePath;

                    _dbContext.Entity<TrStudentPhoto>().Update(isStudentPhotoExists);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                foreach (var fileName in deleteExemplaryAttachment)
                {
                    await RemoveFileIfExists(fileName, containerName);
                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
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

        public string GetSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("exemplary-character");

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
            return sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
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
