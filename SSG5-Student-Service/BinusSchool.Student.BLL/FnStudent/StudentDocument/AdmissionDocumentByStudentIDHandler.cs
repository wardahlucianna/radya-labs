using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.StudentDocument
{
    public class AdmissionDocumentByStudentIDHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        public AdmissionDocumentByStudentIDHandler(IStudentDbContext schoolDbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var query = await _dbContext.Entity<TrStudentDocument>()
                        .Include(x => x.Document)
                        .Where(x => ids.Any(y => y == x.Id))
                        .ToListAsync();

            var undeleted = new UndeletedResult2();
            ids = ids.Except(ids.Intersect(query.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            if (query != null && query.Count > 0)
            {
                foreach (var document in query)
                {
                    document.IsActive = false;

                    _dbContext.Entity<TrStudentDocument>().Update(document);

                    await DeleteFile(document.FileName, "studentdocument");

                    await _dbContext.SaveChangesAsync(CancellationToken);

                    return Request.CreateApiResult2();
                }
            }

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var query = await _dbContext.Entity<TrStudentDocument>()
                        .Include(x => x.Document)
                        .Where(x => x.Document.IdDocumentType == "1" && x.IdStudent == id)
                        .Select(
                                    x => new GetDocumentByStudentResult
                                    {
                                        DocumentStudentID = x.Id,
                                        DocumentID = x.Document.Id,
                                        DocumentName = x.Document.DocumentName,
                                        DocumentType = x.Document.DocumentType.DocumentTypeName,
                                        LastModified = (x.DateUp != null ? Convert.ToDateTime(x.DateUp) : Convert.ToDateTime(x.DateIn)),
                                        ModifiedBy = (x.UserUp != null ? x.UserUp : x.UserIn),
                                        FileName = x.FileName
                                    }
                                )
                        .ToListAsync();
            //return Request.CreateApiResult2(query as object);

            try
            {
                #region Get Marketing document

                #region GetAuthorization
                var apiConfig = _configuration.GetSection("BinusianService").Get<ApiConfiguration>();

                //string urlAuth = "http://10.200.207.65:8012/auth/token";
                string urlAuth = apiConfig.Host.Trim().ToString() + "/binusschool/auth/token";

                apiConfig.Secret.TryGetValue("BasicToken", out var basicToken);
                string keyValue = "Basic " + basicToken.Trim().ToString();

                WebRequest authReq = WebRequest.Create(urlAuth);
                authReq.Headers.Add("Authorization", keyValue);
                authReq.ContentType = "application/json";
                authReq.Method = "GET";

                WebResponse authRes = authReq.GetResponse();
                StreamReader authStream = new StreamReader(authRes.GetResponseStream());
                string authToken = authStream.ReadToEnd();

                AuthenticationToken authenticationToken = JsonConvert.DeserializeObject<AuthenticationToken>(authToken);

                #endregion

                //string urlGetAdmissionDocument = "http://10.200.207.65:8012/studentdocument";
                string urlGetAdmissionDocument = apiConfig.Host.Trim().ToString() + "/binusschool/studentdocument";

                string tokenBearer = authenticationToken.data.token;

                #region Get Admission Document
                WebRequest req = WebRequest.Create(urlGetAdmissionDocument);
                req.Headers.Add("Authorization", "Bearer " + tokenBearer);
                req.ContentType = "application/json";
                req.Method = "POST";

                //tembak
                if (id == "0770000106")
                {
                    id = "2201736183";
                }

                //string json = "{'IdStudent':'" + id + "', 'IdLocation': ''}";

                string json = "{" + (char)34 + "IdStudent" + (char)34 + ":" + (char)34 + id + (char)34 + "," + (char)34 + "IdLocation" + (char)34 + ":" + (char)34 + (char)34 + "}";

                using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                {
                    streamwriter.Write(json);
                }

                WebResponse wr = req.GetResponse();
                StreamReader receiveStream = new StreamReader(wr.GetResponseStream());
                string JsonResponse = receiveStream.ReadToEnd();

                //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
                //Result objResponseBaseonEmail = (Result)serializer.ReadObject(receiveStream);

                Root result = JsonConvert.DeserializeObject<Root>(JsonResponse);

                List<StudentDocumentResponse> marketingDocumentList = result.studentDocumentResponse;

                #endregion
                var retVal = marketingDocumentList.Select(
                                        x => new GetDocumentByStudentResult
                                        {
                                            DocumentID = x.idDocument.ToString(),
                                            DocumentName = x.documentName,
                                            //LastModified = (x.DateUp != null ? Convert.ToDateTime(x.DateUp) : Convert.ToDateTime(x.DateIn)),
                                            ModifiedBy = "Marketing",
                                            MarketingDocumentLink = x.fileUrl,
                                            MarketingOnly = true,
                                            FileName = x.serverFileName
                                        }
                                    );

                if (query.Count > 0)
                {
                    foreach (GetDocumentByStudentResult admDocument in query)
                    {
                        var acopUpdate = retVal.Where(x => x.DocumentID == admDocument.DocumentID).FirstOrDefault();

                        if (acopUpdate != null)
                        {
                            acopUpdate.LastModified = admDocument.LastModified;
                            acopUpdate.MarketingOnly = false;
                            acopUpdate.DocumentStudentID = admDocument.DocumentStudentID;
                            acopUpdate.ModifiedBy = admDocument.ModifiedBy;
                            acopUpdate.FileName = admDocument.FileName;
                            acopUpdate.FileSize = admDocument.FileSize;
                        }
                    }
                }

                return Request.CreateApiResult2(retVal as object);

                #endregion
            }
            catch (Exception)
            {
                var empty = new List<GetDocumentByStudentResult>();
                return Request.CreateApiResult2(empty as object);
            }

        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            #region sebelum ada upload document
            //var param = await Request.GetBody<AddStudentDocumentRequest>();           

            //var count = Convert.ToInt32(_dbContext.Entity<TrStudentDocument>().Max(x => x.Id) ) + 1;            

            //var document = new TrStudentDocument
            //{
            //    Id = count.ToString(),
            //    IdStudent = param.IdStudent,
            //    IdDocument = param.IdDocument,
            //    FileName = param.FileName,
            //    FileSize = param.FileSize,
            //    IdVerificationStatus = param.IdVerificationStatus,
            //    Comment = param.Comment,
            //    IdDocumentStatus = param.IdDocumentStatus,
            //    UserIn = param.UserIn,
            //    DateIn = DateTime.Now
            //};

            //_dbContext.Entity<TrStudentDocument>().Add(document);

            //await _dbContext.SaveChangesAsync(CancellationToken);

            //return Request.CreateApiResult2();
            #endregion

            #region setelah ada upload document

            var param = await Request.GetBodyForm<AddStudentDocumentRequest>();

            //var count = _dbContext.Entity<TrStudentDocument>().Count() + 1;
            var id = Guid.NewGuid().ToString();

            var document = new TrStudentDocument
            {
                Id = id,
                IdStudent = param.IdStudent,
                IdDocument = param.IdDocument,
                FileName = param.FileName,
                FileSize = param.FileSize,
                IdVerificationStatus = param.IdVerificationStatus,
                Comment = param.Comment,
                IdDocumentStatus = param.IdDocumentStatus,
                isStudentView = param.IsStudentView,
                UserIn = param.UserIn,
                DateIn = _dateTime.ServerTime
            };

            _dbContext.Entity<TrStudentDocument>().Add(document);

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Upload File to azure storage
            var file = Request.Form.Files.FirstOrDefault();
            string extent = Path.GetExtension(file.FileName);
            string NameFile = Path.GetFileNameWithoutExtension(file.FileName);

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("studentdocument");

            CloudBlockBlob blob;

            NameFile = NameFile + extent;

            blob = container.GetBlockBlobReference(NameFile);
            blob.Properties.ContentType = file.ContentType;

            //uploda to azure storage
            await blob.UploadFromStreamAsync(file.OpenReadStream());
            #endregion

            return Request.CreateApiResult2();

            #endregion
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=bssstudentstorage;AccountKey=j8XfDXlD+KyoYc4Z9Puddi8+W2oGQ+MgxZ2+avOyq2vZFdXhbn091udJgUf1DZ+o8eJMD+38aPaTN9sWLUKBOg==;EndpointSuffix=core.windows.net");
            return storageAccount;
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            //var param = await Request.GetBody<UpdateStudentDocumentRequest>();

            var param = await Request.GetBodyForm<UpdateStudentDocumentRequest>();

            var query = await _dbContext.Entity<TrStudentDocument>().Where(x => x.Id == param.IdStudentDocument).FirstOrDefaultAsync();

            var oldFileName = query.FileName;

            bool exist = false;

            if (query != null) exist = true;

            if (exist)
            {
                query.IdDocument = param.IdDocument;
                query.IdVerificationStatus = param.IdVerificationStatus;
                query.Comment = param.Comment;
                query.IdDocumentStatus = param.IdDocumentStatus;
                query.isStudentView = param.IsStudentView;
                query.UserUp = param.UserUp;
                query.DateUp = _dateTime.ServerTime;

                if (Request.Form.Files.FirstOrDefault() == null || param.FileSize <= 0)
                {

                    _dbContext.Entity<TrStudentDocument>().Update(query);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    return Request.CreateApiResult2();
                }
                else
                {
                    query.FileName = param.FileName;
                    query.FileSize = param.FileSize;
                    _dbContext.Entity<TrStudentDocument>().Update(query);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    #region Upload dan Delete file azure storage
                    var file = Request.Form.Files.FirstOrDefault();
                    string extent = Path.GetExtension(file.FileName);
                    string NameFile = Path.GetFileNameWithoutExtension(file.FileName);

                    CloudStorageAccount storageAccount = GetCloudStorageAccount();
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("studentdocument");

                    CloudBlockBlob blob;

                    await DeleteFile(oldFileName, "studentdocument");

                    NameFile = NameFile + extent;

                    blob = container.GetBlockBlobReference(NameFile);
                    blob.Properties.ContentType = file.ContentType;

                    //uploda to azure storage
                    await blob.UploadFromStreamAsync(file.OpenReadStream());
                    #endregion

                    return Request.CreateApiResult2();
                }
            }
            else
            {
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["Document"], "Id", param.IdDocument));
            }
        }

        private async Task DeleteFile(string fileName, string containerName)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.DeleteIfExistsAsync();
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class StudentDocumentResponse
        {
            public string idStudent { get; set; }
            public string idBinusian { get; set; }
            public string registrantName { get; set; }
            public string idRegistrant { get; set; }
            public int idDocument { get; set; }
            public string documentName { get; set; }
            public string serverFileName { get; set; }
            public string fileUrl { get; set; }
        }

        public class Root
        {
            public List<StudentDocumentResponse> studentDocumentResponse { get; set; }
            public int resultCode { get; set; }
            public string errorMessage { get; set; }
        }

        public class Data
        {
            public string token { get; set; }
            public int duration { get; set; }
        }

        public class AuthenticationToken
        {
            public Data data { get; set; }
            public int resultCode { get; set; }
            public string errorMessage { get; set; }
        }


    }
}
