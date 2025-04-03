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
    public  class GetBLPQuestionWithHistoryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetBLPQuestionWithHistoryRequest.IdSchool), nameof(GetBLPQuestionWithHistoryRequest.IdSurveyCategory) };
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public GetBLPQuestionWithHistoryHandler(IDocumentDbContext dbContext,
             IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBLPQuestionWithHistoryRequest>(_requiredParams);

            var container = GetContainer("ccform-doc");

            var StudentAnswerList = await _dbContext.Entity<TrSurveyStudentAnswer>()
                                                .Include(x => x.SurveyAnswerMapping).ThenInclude(y => y.SurveyAnswer)
                                                .Include(x => x.Respondent)                                            
                                                .Where(a => a.Respondent.IdStudent == param.IdStudent
                                                && (param.IdSurveyPeriod != null || param.IdClearanceWeekPeriod != null)
                                                && a.Respondent.IdSurveyPeriod == (param.IdSurveyPeriod == null ? a.Respondent.IdSurveyPeriod : param.IdSurveyPeriod)
                                                && a.Respondent.IdClearanceWeekPeriod == (param.IdClearanceWeekPeriod == null ? a.Respondent.IdClearanceWeekPeriod : param.IdClearanceWeekPeriod))
                                                .Select(a => new BLPQuestionWithHistory_BLPStudentAnswerVm
                                                {
                                                    IdSurveyAnswerMapping = a.IdSurveyAnswerMapping,
                                                    SurveyAnswerDesc = a.SurveyAnswerMapping.SurveyAnswer.Description,
                                                    Description = a.Description,
                                                    FilePath = (a.FilePath != null ? GetDocument(a.FilePath, container) : null)
                                                })
                                                .ToListAsync();
            var StudentAnswerHistoryList = await _dbContext.Entity<HTrSurveyStudentAnswer>()
                                              .Include(x => x.SurveyAnswerMapping).ThenInclude(y => y.SurveyAnswer)
                                              .Include(x => x.Respondent)                                              
                                              .Where(a => a.Respondent.IdStudent == param.IdStudent
                                              && (param.IdSurveyPeriod != null || param.IdClearanceWeekPeriod != null)
                                              && a.Respondent.IdSurveyPeriod == (param.IdSurveyPeriod == null ? a.Respondent.IdSurveyPeriod : param.IdSurveyPeriod)
                                              && a.Respondent.IdClearanceWeekPeriod == (param.IdClearanceWeekPeriod == null ? a.Respondent.IdClearanceWeekPeriod : param.IdClearanceWeekPeriod))
                                              .Select(a => new BLPQuestionWithHistory_BLPStudentAnswerHistoryVm
                                              {
                                                  IdSurveyAnswerMapping = a.IdSurveyAnswerMapping,
                                                  SurveyAnswerDesc = a.SurveyAnswerMapping.SurveyAnswer.Description,
                                                  Description = a.Description,
                                                  FilePath = a.FilePath,
                                                  ActionDate =  a.ActionDate
                                              })
                                              .ToListAsync();

            var lastDateUpdated = StudentAnswerHistoryList.OrderByDescending(a => a.ActionDate).Select(b => b.ActionDate).FirstOrDefault();
            StudentAnswerHistoryList = StudentAnswerHistoryList.Where(a => (a.ActionDate != null ? ((DateTime)a.ActionDate).ToString("dd-MM-yyyy") : "-") == (lastDateUpdated != null ? ((DateTime)lastDateUpdated).ToString("dd-MM-yyyy") : "-")).ToList();


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
                StudentAnswerHistoryList = StudentAnswerHistoryList.Where(hasw => hasw.IdSurveyAnswerMapping == x.Id).OrderByDescending(a => a.ActionDate).FirstOrDefault(),
                isHaveChild = (x.NextGroupNumber != null ? true : false),
                Childs = GetChildsRecursion((x.NextGroupNumber == null ? -1 : (int)x.NextGroupNumber), query, StudentAnswerList, StudentAnswerHistoryList)
            }).ToList();

            var GetSurveySection = await _dbContext.Entity<MsSurveySection>()
                                        .Include(x => x.SurveyCategory)
                                        .Include(x => x.SurveyQuestionMappings)
                                            .ThenInclude(y => y.SurveyQuestion)
                                        .Where(a => a.IdSchool == param.IdSchool
                                        && a.IdSurveyCategory == param.IdSurveyCategory)
                                        .OrderBy(a => a.OrderNumber)
                                        .Select(a => new GetBLPQuestionWithHistoryResult()
                                        {
                                            SectionName = a.SectionName,
                                            Description = a.Description,
                                            QuestionAnswerList = a.SurveyQuestionMappings.Where(c => c.SurveyQuestion.IsParentQuestion == true).Select(b => new BLPQuestionWithHistory_QuestionAnswerVm()
                                            {
                                                IdSurveyQuestion = b.IdSurveyQuestion,
                                                QuestionName = b.SurveyQuestion.Description,
                                                IdSurveyQuestionMapping = b.Id
                                            }).ToList()
                                            //Answers = ListAnswer
                                        })
                                        .ToListAsync();

            foreach (var itemRpt in GetSurveySection)
            {
                foreach (var itemQA in itemRpt.QuestionAnswerList)
                {
                    itemQA.AnswerGroup = ListAnswer.Where(a => a.IdSurveyQuestionMapping == itemQA.IdSurveyQuestionMapping)
                                                   .GroupBy(a => a.GroupAnswer)
                                                   .Select(a => new BLPQuestionWithHistory_BLPAnswerVm()
                                                   {
                                                       GroupAnswer = a.Key,
                                                       AnswerType = a.First().AnswerType,
                                                       Answers = a.Select(b => new BLPQuestionWithHistory_BLPChildAnswerVm()
                                                       {
                                                           IdSurveyAnswerMapping = b.IdSurveyAnswerMapping,
                                                           IdSurveyQuestionMapping = b.IdSurveyQuestionMapping,
                                                           idSurveyAnswer = b.idSurveyAnswer,
                                                           SurveyAnswerDesc = b.SurveyAnswerDesc,
                                                           StudentAnswer = b.StudentAnswer,
                                                           StudentAnswerHistory = b.StudentAnswerHistoryList,
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

        private BLPQuestionWithHistory_BLPAnswerVm GetChildsRecursion(int NextGroupNumber, List<TrSurveyAnswerMapping> features, List<BLPQuestionWithHistory_BLPStudentAnswerVm> StudentAnswerList, List<BLPQuestionWithHistory_BLPStudentAnswerHistoryVm> StudentAnswerHistoryList)
        {

            return features.Any(x => x.GroupNumber == NextGroupNumber) ?
                features.Where(x => x.GroupNumber == NextGroupNumber).OrderBy(x => x.OrderNumber)
                         .GroupBy(a => a.GroupNumber)
                         .Select(a => new BLPQuestionWithHistory_BLPAnswerVm()
                         {
                             GroupAnswer = a.Key,
                             AnswerType = a.First().SurveyQuestionMapping.SurveyQuestionType.AnswerType,
                             ChildsQuestion = a.First().SurveyQuestionMapping.SurveyQuestion.Description,
                             Answers = a.Select(b => new BLPQuestionWithHistory_BLPChildAnswerVm()
                             {
                                 IdSurveyAnswerMapping = b.Id,
                                 IdSurveyQuestionMapping = b.IdSurveyQuestionMapping,

                                 idSurveyAnswer = b.IdSurveyAnswer,
                                 SurveyAnswerDesc = b.SurveyAnswer.Description,
                                 StudentAnswer = StudentAnswerList.Where(asw => asw.IdSurveyAnswerMapping == b.Id).FirstOrDefault(),
                                 StudentAnswerHistory = StudentAnswerHistoryList.Where(hasw => hasw.IdSurveyAnswerMapping == b.Id).OrderByDescending(a => a.ActionDate).FirstOrDefault(),
                                 isHaveChild = (b.NextGroupNumber != null ? true : false),

                                 Childs = (b.NextGroupNumber != null ? GetChildsRecursion((int)b.NextGroupNumber, features, StudentAnswerList, StudentAnswerHistoryList) : null)
                             }).ToList()
                         }).FirstOrDefault()               
                : null;

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
