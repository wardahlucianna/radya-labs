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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule
{
    public class SupportingDucumentHandler : FunctionsHttpCrudHandler
    {
        private static readonly string[] _allowedFiles = new[] { ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".pdf" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        public SupportingDucumentHandler(ISchedulingDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsExtracurricularSupportDoc>()
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));


            _dbContext.Entity<MsExtracurricularSupportDoc>().RemoveRange(datas);
          
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var extracurricularSupportDoc = await _dbContext.Entity<MsExtracurricularSupportDoc>()
                                   .Include(x => x.ExtracurricularSupportDocGrades)
                                       .ThenInclude(y => y.Grade)
                                       .ThenInclude(y => y.Level)
                                       .ThenInclude(y => y.AcademicYear)
                                   .Where(a => a.Id == id).FirstOrDefaultAsync();

            if (extracurricularSupportDoc is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularSupportDoc"], "Id", id));

            var tempAcademicYear = extracurricularSupportDoc.ExtracurricularSupportDocGrades.Select(a => new ItemValueVm(a.Grade.Level.AcademicYear.Id, a.Grade.Level.AcademicYear.Description)).FirstOrDefault();
            
            var container = GetContainerSasUri(1);

            var ReturnResult = new GetSupportingDucumentDetailResult()
            {
                Id = extracurricularSupportDoc.Id,
                AcademicYear = tempAcademicYear,
                DocumentLink = (extracurricularSupportDoc.FileName != null || extracurricularSupportDoc.FileName != "" ? GetDocument(extracurricularSupportDoc.FileName, container) : null) ,
                DocumentName = extracurricularSupportDoc.Name,
                ShowToParent = extracurricularSupportDoc.ShowToParent,
                ShowToStudent = extracurricularSupportDoc.ShowToStudent,
                Grades = extracurricularSupportDoc.ExtracurricularSupportDocGrades
                                                        .Where(a => a.Grade.Level.IdAcademicYear == tempAcademicYear.Id)
                                                        .Select(a => new ItemValueVm() { Id = a.IdGrade, Description = a.Grade.Description }).ToList(),
                Status = extracurricularSupportDoc.Status
            };


            return Request.CreateApiResult2(ReturnResult as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetSupportingDucumentRequest>(nameof(GetMasterExtracurricularRuleRequest.IdSchool));

            var columns = new[] { "academicyear", "documentname" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "ExtracurricularSupportDocGrades.Grade.Level.AcademicYear.Description" },
                { columns[1], "Name" }
            };

            var predicate = PredicateBuilder.Create<MsExtracurricularSupportDoc>(x => x.Status == (param.Status != null ? param.Status : x.Status));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern()));
            //|| EF.Functions.Like(x.Name, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.ExtracurricularSupportDocGrades.Any(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear));

            var query = _dbContext.Entity<MsExtracurricularSupportDoc>()
                .Include(x => x.ExtracurricularSupportDocGrades)
                    .ThenInclude(y => y.Grade)
                    .ThenInclude(y => y.Level)
                    .ThenInclude(y => y.AcademicYear)
                .SearchByIds(param)
                .Where(predicate)
                .Where(a => a.ExtracurricularSupportDocGrades.Any(b => b.Grade.Level.AcademicYear.IdSchool == param.IdSchool))
                .OrderByDynamic(param, aliasColumns);


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                .Select(x => new ItemValueVm(x.Id, x.Name))
                 .ToListAsync(CancellationToken);
            }

            else
            {
               
                var container = GetContainerSasUri(1);      
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetSupportingDucumentResult
                    {
                        Id = x.Id,
                        AcademicYear = x.ExtracurricularSupportDocGrades.Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                                                        .Select(a => new ItemValueVm(a.Grade.Level.AcademicYear.Id, a.Grade.Level.AcademicYear.Description))
                                                                        .FirstOrDefault(),
                        DocumentLink = (x.FileName != null || x.FileName != "" ? GetDocument(x.FileName, container) : null),
                        DocumentName = x.Name,
                        ShowTo = ((x.ShowToParent == true ? (x.ShowToStudent == true ? "Parent, " : "Parent") : "") + (x.ShowToStudent == true ? "Student" : "")),
                        Grades = x.ExtracurricularSupportDocGrades
                                                        .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                                        .Select(a => new ItemValueVm() { Id = a.IdGrade, Description = a.Grade.Description }).ToList(),
                        Status = x.Status

                    })
                    .ToListAsync(CancellationToken);
            }
                    

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {

            //var body = await Request.ValidateBody<UpdateSupportingDocumentRequest, AddSupportingDocumentValidator>();
            var param = await Request.GetBodyForm<AddSupportingDocumentRequest>();
            var gradeList = param.Grades.Split("~");

            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
               throw new BadRequestException("Support Document file not found");

            var fileInfo = new FileInfo(file.FileName);
            if (!_allowedFiles.Any(x => x == fileInfo.Extension))
               throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedFiles)}");

           
            var grades = await _dbContext.Entity<MsGrade>().Where(a => gradeList.Select(b => b.ToString()).Contains(a.Id)).Select(c => c.Id).ToListAsync(CancellationToken);
            // find not found ids
            var gradeNotFound = gradeList.Select(a => a.ToString()).Except(grades);

            if ((gradeNotFound?.Count() ?? 0) > 0)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(",", gradeNotFound)));

               
           

            if (!string.IsNullOrEmpty(param.Name) && !string.IsNullOrEmpty(param.Grades))
            {
                #region testing
                /*
                string file_extension,
                filename_withExtension;
                Stream fileTest;
                fileTest = new FileStream("...link", FileMode.Open);
                file_extension = Path.GetExtension("...link");
                filename_withExtension = Path.GetFileName("...link");
                CloudStorageAccount storageAccount = GetCloudStorageAccount();
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("extracurricular-sup-doc");
                CloudBlockBlob blob;
                string NameFile = filename_withExtension + "_" + DateTime.Now.ToString("yyyyHHmmss") + file_extension;
                blob = container.GetBlockBlobReference(NameFile);
                blob.Properties.ContentType = "application/pdf";
                //uploda to azure storage
                await blob.UploadFromStreamAsync(fileTest);
                */
                #endregion end testing

                #region Upload File to azure storage             

                //create file container in blob storage 
                CreateContainerIfNotExists("extracurricular-sup-doc");

                // var file = Request.Form.Files.FirstOrDefault();

                string extent = Path.GetExtension(file.FileName);
                string NameFile = Path.GetFileNameWithoutExtension(file.FileName);

                CloudStorageAccount storageAccount = GetCloudStorageAccount();
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("extracurricular-sup-doc");

                CloudBlockBlob blob;

                NameFile = NameFile + "_" + _dateTime.ServerTime.ToString("yyyyHHmmss") + extent;

                blob = container.GetBlockBlobReference(NameFile);
                blob.Properties.ContentType = file.ContentType;

                //uploda to azure storage
                await blob.UploadFromStreamAsync(file.OpenReadStream());
                #endregion

                var IdDocument = Guid.NewGuid().ToString();
                var AddParam = new MsExtracurricularSupportDoc
                {
                    Id = IdDocument,                    
                    Name = param.Name,
                    ShowToParent = param.ShowToParent,
                    ShowToStudent = param.ShowToStudent,               
                    Status = param.Status,
                    FileName = NameFile,
                    FileSize = param.FileSize

                };

                _dbContext.Entity<MsExtracurricularSupportDoc>().Add(AddParam);


                var paramGrades = gradeList.Select(a => new MsExtracurricularSupportDocGrade()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdExtracurricularSupportDoc = IdDocument,
                    IdGrade = a.ToString()
                }).ToList();


                _dbContext.Entity<MsExtracurricularSupportDocGrade>().AddRange(paramGrades);

                await _dbContext.SaveChangesAsync(CancellationToken);
                  
                return Request.CreateApiResult2();
            }

            throw new BadRequestException(null);

        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {            
            //var body = await Request.ValidateBody<UpdateSupportingDocumentRequest, UpdateSupportingDocumentValidator>();
            var param = await Request.GetBodyForm<UpdateSupportingDocumentRequest>();
            var gradeList = param.Grades.Split("~");
           
         
            if (!string.IsNullOrEmpty(param.IdExtracurricularSupportDoc))
            {

                var data = await _dbContext.Entity<MsExtracurricularSupportDoc>().FindAsync(param.IdExtracurricularSupportDoc);
                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularSupportDoc"], "Id", param.IdExtracurricularSupportDoc));


                if (param.ActionUpdateStatus == false)
                {

                    var grades = await _dbContext.Entity<MsGrade>().Where(a => gradeList.Select(b => b.ToString()).Contains(a.Id)).Select(c => c.Id).ToListAsync(CancellationToken);

                    // find not found ids
                    var gradeNotFound = gradeList.Select(b => b.ToString()).Except(grades);
                    if ((gradeNotFound?.Count() ?? 0) > 0)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(",", gradeNotFound)));

                }

                if (param.ActionUpdateStatus == true)
                {
                    data.Status = param.Status;
                    data.UserUp = AuthInfo.UserId;

                    _dbContext.Entity<MsExtracurricularSupportDoc>().Update(data);
                }
                else
                {

                    #region Upload File to azure storage             


                    var file = Request.Form.Files.FirstOrDefault();
             
                    if (file is null || file.Length == 0)
                    throw new BadRequestException("Support Document file not found");

                    var fileInfo = new FileInfo(file.FileName);
                    if (!_allowedFiles.Any(x => x == fileInfo.Extension))
                    throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedFiles)}");

                    string NameFile = "";
                    
                    if (!string.IsNullOrWhiteSpace(param.FileName))
                    {

                        string extent = Path.GetExtension(file.FileName);
                        NameFile = Path.GetFileNameWithoutExtension(file.FileName);

                        CloudStorageAccount storageAccount = GetCloudStorageAccount();
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("extracurricular-sup-doc");

                        CloudBlockBlob blob;

                        NameFile = NameFile + "_" + _dateTime.ServerTime.ToString("yyyyHHmmss") + extent;

                        blob = container.GetBlockBlobReference(NameFile);
                        blob.Properties.ContentType = file.ContentType;

                        //uploda to azure storage
                        await blob.UploadFromStreamAsync(file.OpenReadStream());
                        
                        // remove file if already exist
                        await RemoveFile(data.FileName, "extracurricular-sup-doc");
                                                  
                    }

                    #endregion

                    data.Name = param.Name;
                    data.ShowToParent = param.ShowToParent;
                    data.ShowToStudent = param.ShowToStudent;
                    data.FileName = (!string.IsNullOrWhiteSpace(param.FileName) ? NameFile : data.FileName);
                    data.FileSize = (!string.IsNullOrWhiteSpace(param.FileName) ? param.FileSize : data.FileSize); 
                    data.UserUp = AuthInfo.UserId;

                    _dbContext.Entity<MsExtracurricularSupportDoc>().Update(data);


                    var existsGrades = await _dbContext.Entity<MsExtracurricularSupportDocGrade>()
                                .Where(a => a.IdExtracurricularSupportDoc == param.IdExtracurricularSupportDoc)
                                .ToListAsync(CancellationToken);

                    var gradeDeleted = existsGrades.Where(a => !gradeList.Select(b => b.ToString()).Contains(a.IdGrade)).ToList();                

                    _dbContext.Entity<MsExtracurricularSupportDocGrade>().RemoveRange(gradeDeleted);


                    var gradeInserted = gradeList.Where(a => !existsGrades.Select(b => b.IdGrade).Contains(a.ToString())).Select(c => new MsExtracurricularSupportDocGrade()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricularSupportDoc = param.IdExtracurricularSupportDoc,
                        IdGrade = c.ToString()
                    }).ToList();
                    _dbContext.Entity<MsExtracurricularSupportDocGrade>().AddRange(gradeInserted);

                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                return Request.CreateApiResult2();
            }
            throw new NotImplementedException();
         
        }

        public static string GetDocument(string filename, string containerLink)
        {
            string url = containerLink.Replace("?", "/" + filename + "?");

            return url;
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

        private string GetBlobSasUri(string blobName, int expiryHour, string policyName = null)
        {
            string sasBlobToken;

            CloudBlobContainer container;


            //if (AppType == 1)
            //{
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("extracurricular-sup-doc");

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

        public Task RemoveFile(string fileName, string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }


    }
}
