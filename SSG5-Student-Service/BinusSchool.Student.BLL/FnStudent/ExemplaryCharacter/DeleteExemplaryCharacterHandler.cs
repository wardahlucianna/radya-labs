using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class DeleteExemplaryCharacterHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DeleteExemplaryCharacterHandler(
            IStudentDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteExemplaryCharacterRequest, DeleteExemplaryCharacterValidator>();

            var containerName = "exemplary-character";

            var deleteExemplaryAttachment = await _dbContext.Entity<TrExemplaryAttachment>()
                                                .Include(ea => ea.Exemplary)
                                                .ThenInclude(e => e.AcademicYear)
                                                .ThenInclude(ay => ay.MsSchool)
                                                .Where(x => param.IdExemplaryCharacter.Any(y => y == x.Exemplary.Id))
                                                .ToListAsync(CancellationToken);

            var deleteExemplaryList = deleteExemplaryAttachment
                                        .Select(x => x.Exemplary)
                                        .Distinct()
                                        .ToList();

            _dbContext.Entity<TrExemplary>().RemoveRange(deleteExemplaryList);

            await _dbContext.SaveChangesAsync();

            try
            {
                // delete attachment file
                foreach (var attachment in deleteExemplaryAttachment)
                {
                    var fileName = attachment.FileName;
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
