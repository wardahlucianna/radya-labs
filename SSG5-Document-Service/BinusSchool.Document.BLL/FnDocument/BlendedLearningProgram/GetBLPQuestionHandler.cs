using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace BinusSchool.Document.FnDocument.BlendedLearningProgram
{
    public class GetBLPQuestionHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetBLPQuestionRequest.IdSchool), nameof(GetBLPQuestionRequest.IdSurveyCategory) };
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        
        public GetBLPQuestionHandler(IDocumentDbContext dbContext,
             IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBLPQuestionRequest>(_requiredParams);


            var container = GetContainer("ccform-doc");

            var StudentAnswerList = await _dbContext.Entity<TrSurveyStudentAnswer>()
                                                .Include(x => x.Respondent)
                                                .Where(a => a.Respondent.IdStudent == param.IdStudent
                                                && (param.IdSurveyPeriod != null || param.IdClearanceWeekPeriod != null) 
                                                && a.Respondent.IdSurveyPeriod == (param.IdSurveyPeriod == null ? a.Respondent.IdSurveyPeriod : param.IdSurveyPeriod)
                                                && a.Respondent.IdClearanceWeekPeriod == (param.IdClearanceWeekPeriod == null ? a.Respondent.IdClearanceWeekPeriod : param.IdClearanceWeekPeriod))
                                                .Select(a => new BLPStudentAnswerVm{                                                
                                                     IdSurveyAnswerMapping = a.IdSurveyAnswerMapping,
                                                     Description = a.Description,
                                                     FilePath = (a.FilePath != null ? GetDocument(a.FilePath, container) : null)
                                                })
                                                .ToListAsync();
                

            var query = await _dbContext.Entity<TrSurveyAnswerMapping>()                                 
                                    .Include(x => x.SurveyAnswer)
                                    .Include(x => x.SurveyQuestionMapping)
                                        .ThenInclude(y => y.SurveySection)
                                    .Include(x => x.SurveyQuestionMapping)
                                        .ThenInclude(y => y.SurveyQuestion)
                                    .Include(x => x.SurveyQuestionMapping)
                                        .ThenInclude(y => y.SurveyQuestionType)
                                    .Where(a => a.SurveyQuestionMapping.SurveySection.IdSurveyCategory == param.IdSurveyCategory
                                    && a.SurveyQuestionMapping.SurveySection.IdSchool == param.IdSchool)
                                    .ToListAsync();


            var ListAnswer = query.Where(x => x.SurveyQuestionMapping.SurveyQuestion.IsParentQuestion == true).OrderBy(x => x.OrderNumber).Select(x => new // BLPChildAnswerVm()
            {
                IdSurveyAnswerMapping = x.Id,
                IdSurveyQuestionMapping = x.IdSurveyQuestionMapping,
                idSurveyAnswer = x.IdSurveyAnswer,
                SurveyAnswerDesc = x.SurveyAnswer.Description,
                AnswerType = x.SurveyQuestionMapping.SurveyQuestionType.AnswerType,
                GroupAnswer = x.GroupNumber,
                StudentAnswer = StudentAnswerList.Where(asw => asw.IdSurveyAnswerMapping == x.Id).FirstOrDefault(),
                isHaveChild = (x.NextGroupNumber != null ? true : false),               
                Childs = GetChildsRecursion((x.NextGroupNumber == null ? -1 : (int)x.NextGroupNumber), query, StudentAnswerList)
            }).ToList();

            var GetSurveySection = await _dbContext.Entity<MsSurveySection>()
                                        .Include(x => x.SurveyCategory)
                                        .Include(x => x.SurveyQuestionMappings)
                                            .ThenInclude(y => y.SurveyQuestion)                                                                  
                                        .Where(a => a.IdSchool == param.IdSchool
                                        && a.IdSurveyCategory == param.IdSurveyCategory)
                                        .OrderBy(a => a.OrderNumber)
                                        .Select(a => new GetBLPQuestionResult()
                                        {
                                            SectionName = a.SectionName,
                                            Description = a.Description,
                                            QuestionAnswerList = a.SurveyQuestionMappings.Where(c => c.SurveyQuestion.IsParentQuestion == true).OrderBy(c => c.OrderNumber).Select(b => new QuestionAnswerVm()
                                                                                            {
                                                                                                IdSurveyQuestion = b.IdSurveyQuestion,
                                                                                                QuestionName = b.SurveyQuestion.Description,
                                                                                                IdSurveyQuestionMapping = b.Id
                                                                                            }).ToList()
                                            //Answers = ListAnswer
                                        })
                                        .ToListAsync();

            foreach(var itemRpt in GetSurveySection)
            {
                foreach (var itemQA in itemRpt.QuestionAnswerList)
                {
                    itemQA.AnswerGroup = ListAnswer.Where(a => a.IdSurveyQuestionMapping == itemQA.IdSurveyQuestionMapping)
                                                   .GroupBy(a => a.GroupAnswer)
                                                   .Select(a => new BLPAnswerVm() {
                                                        GroupAnswer = a.Key,
                                                        AnswerType = a.First().AnswerType,
                                                        Answers = a.Select(b => new BLPChildAnswerVm() {
                                                                                IdSurveyAnswerMapping = b.IdSurveyAnswerMapping,
                                                                                IdSurveyQuestionMapping = b.IdSurveyQuestionMapping,
                                                                                idSurveyAnswer = b.idSurveyAnswer,
                                                                                SurveyAnswerDesc = b.SurveyAnswerDesc,
                                                                                StudentAnswer = b.StudentAnswer,
                                                                                isHaveChild = b.isHaveChild,
                                                                                //ChildsQuestion = null,
                                                                                Childs = b.Childs,
                                                        }).ToList()
                                                   }).FirstOrDefault();
                        //= ListAnswer.Where(a => a.IdSurveyQuestionMapping == itemQA.IdSurveyQuestionMapping).ToList();
                    
                }
            }

            return Request.CreateApiResult2(GetSurveySection as object);
        }

        private BLPAnswerVm GetChildsRecursion(int NextGroupNumber, List<TrSurveyAnswerMapping> features, List<BLPStudentAnswerVm> StudentAnswerList)
        {

            return features.Any(x => x.GroupNumber == NextGroupNumber) ? 
                features.Where(x => x.GroupNumber == NextGroupNumber).OrderBy(x => x.OrderNumber)
                         .GroupBy(a => a.GroupNumber)
                         .Select(a => new BLPAnswerVm()
                         {
                             GroupAnswer = a.Key,
                             AnswerType = a.First().SurveyQuestionMapping.SurveyQuestionType.AnswerType,
                             ChildsQuestion = a.First().SurveyQuestionMapping.SurveyQuestion.Description,
                             Answers = a.Select(b => new BLPChildAnswerVm()
                             {
                                 IdSurveyAnswerMapping = b.Id,
                                 IdSurveyQuestionMapping = b.IdSurveyQuestionMapping,

                                 idSurveyAnswer = b.IdSurveyAnswer,
                                 SurveyAnswerDesc = b.SurveyAnswer.Description,                              
                                 StudentAnswer = StudentAnswerList.Where(asw => asw.IdSurveyAnswerMapping == b.Id).FirstOrDefault(),
                                 isHaveChild = (b.NextGroupNumber != null ? true : false),
                                
                                 Childs = (b.NextGroupNumber != null ? GetChildsRecursion((int)b.NextGroupNumber, features, StudentAnswerList) : null)
                             }).ToList() 
                         }).FirstOrDefault()
                        
                        //.Select(x => new BLPChildAnswerVm//BLPChildAnswerVm
                        //{
                        //    IdSurveyAnswerMapping = x.Id,
                        //    idSurveyAnswer = x.IdSurveyAnswer,
                        //    SurveyAnswerDesc = x.SurveyAnswer.Description,
                        //    AnswerType = x.SurveyQuestionMapping.SurveyQuestionType.AnswerType,
                        //    Childs = (x.NextGroupNumber != null ? GetChildsRecursion((int)x.NextGroupNumber, features) : null) 
                        //}).ToList() 
                : null;
          
        }

        public string GetContainerSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("ccform-doc");

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

        public string GetContainer(string Containername)
        {
            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(Containername);

            return container.Uri.ToString();
        }

        public static string GetDocument(string filename, string containerLink)
        {
            string fileNameReplace = filename.Replace("ccform-doc/", "");
            string url = containerLink + "/" + fileNameReplace;

            return url;
        }
    }
}

