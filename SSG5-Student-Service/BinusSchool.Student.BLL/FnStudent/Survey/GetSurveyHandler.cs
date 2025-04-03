using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BinusSchool.Data.Configurations;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Survey;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.IO;
using static BinusSchool.Student.FnStudent.StudentDocument.AdmissionDocumentByStudentIDHandler;


namespace BinusSchool.Student.FnStudent.Survey
{
    public class GetSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        private readonly string _keyValue;
        private readonly string _getTokenEndPoint = "/binusschool/auth/token";
        private readonly string _getPostStudentSurveyEndPoint = "/binusschool/bss-student-survey";
        private readonly List<int> _listExcludedStatus = new List<int>{0, 2, 3, 5, 7, 9, 12, 15};
        public GetSurveyHandler(IStudentDbContext dbContext,
             IConfiguration configuration,
             IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
            _keyValue = configuration["BinusianService:Secret:BasicToken"];
        }
        public static string GetApi(string ApiUrl, string keyValue)
        {
            var responseString = "";
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", keyValue);
            try
            {
                using (var response1 = request.GetResponse())
                {
                    using (var reader = new StreamReader(response1.GetResponseStream()))
                    {
                        responseString = reader.ReadToEnd();
                    }
                }
                return responseString;
            }
            catch (WebException e)
            {
                return responseString = "{}";
            }
        }
        public static string PostApi(string ApiUrl, string keyValue, string IdSchool, string IdStudent)
        {
            var responseString = "";
            string json = JsonSerializer.Serialize(new
            {
                IdSchool = IdSchool,
                IdStudent = IdStudent
            });
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", keyValue);
            request.ContentLength = json.Length;
            using (var streamwriter = new StreamWriter(request.GetRequestStream()))
            {
                streamwriter.Write(json);
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (WebException e)
            {
                return responseString = "{}";
            }
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveyRequest>(nameof(GetSurveyRequest.IdSchool), nameof(GetSurveyRequest.Role), nameof(GetSurveyRequest.IdUser));
            #region Sync SISDB Questioner Data
            if (param.IdSchool == "2")
            {
                try
                {

                    var apiConfig = _configuration.GetSection("BinusianService").Get<ApiConfiguration>();
                    var Host = apiConfig.Host.Trim().ToString();
                    var responseString = GetApi(apiConfig.Host.Trim().ToString() + _getTokenEndPoint, "Basic " + _keyValue);
                    AuthenticationToken keyToken = JsonSerializer.Deserialize<AuthenticationToken>(responseString);

                    if (keyToken.resultCode == 200)
                    {
                        var dataSISDBSurvey = PostApi(apiConfig.Host.Trim().ToString() + _getPostStudentSurveyEndPoint, keyToken.errorMessage == null ? "" : "Bearer " + keyToken.data.token, param.IdSchool, (param.Role.ToLower() == "parent" ? (param.IdUser).Substring(1) : param.IdUser));
                        var resultCode = JsonDocument.Parse(dataSISDBSurvey).RootElement.GetProperty("resultCode").GetInt32();
                        var resultMessage = JsonDocument.Parse(dataSISDBSurvey).RootElement.GetProperty("errorMessage").GetString();
                        if (resultCode != 200)
                        {
                            throw new BadRequestException(resultMessage);
                        }
                    }
                    else
                    {
                        throw new BadRequestException(keyToken.errorMessage);
                    }
                }
                catch (Exception e)
                {
                    throw new BadRequestException(e.Message);
                }

            }
            #endregion

            List<string> filterLevel = new List<string>();
            List<string> filterGrade = new List<string>();


            var getPeriodActive = await _dbContext.Entity<MsPeriod>()
                            .Include(x => x.Grade)
                                .ThenInclude(y => y.MsLevel)
                                .ThenInclude(y => y.MsAcademicYear)
                            .Where(a => a.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool
                            && a.StartDate < _dateTime.ServerTime
                            && _dateTime.ServerTime < a.EndDate
                            )
                            .Select(a => new {
                                a.Grade.MsLevel.IdAcademicYear
                            })
                            .Distinct()
                            .ToListAsync();

            if (!getPeriodActive.Any())
                throw new BadRequestException("Period is not found");

            List<GetSurveyResult> ReturnResult = new List<GetSurveyResult>();
            if (param.Role == "PARENT")
            {
                var getStudentofParent = await _dbContext.Entity<MsStudentParent>()
                                             .Where(a => a.IdParent == param.IdUser)
                                             .Select(a => a.IdStudent)
                                             .ToListAsync();

                var getIdStudent = new String(param.IdUser.Where(Char.IsDigit).ToArray());


                #region Get List Sibling
                var siblingGroupId = await _dbContext.Entity<MsSiblingGroup>()
                        .Where(x => x.IdStudent == getIdStudent)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(CancellationToken);

                var SiblingList = await _dbContext.Entity<MsSiblingGroup>()
                        .Where(x => x.Id == siblingGroupId)
                        .Select(x => x.IdStudent)
                        .ToListAsync(CancellationToken);
                #endregion

                foreach(var Sibling in SiblingList)
                {
                    var getEnrollmentStudent = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                               .Where(a => getPeriodActive.First().IdAcademicYear == a.Lesson.Grade.MsLevel.IdAcademicYear
                                               && a.HomeroomStudent.IdStudent == Sibling
                                               //&& getStudentofParent.Contains(a.HomeroomStudent.IdStudent)
                                               //&& (SiblingList.Contains(a.HomeroomStudent.IdStudent) || getStudentofParent.Contains(a.HomeroomStudent.IdStudent))
                                               )
                                               .Select(a => new {
                                                   a.HomeroomStudent.IdStudent,
                                                   a.Lesson.Grade.IdLevel,
                                                   a.Lesson.IdGrade
                                               })
                                               .Distinct()
                                               .ToListAsync();
                    var checkStudentStatus = await _dbContext.Entity<MsStudent>()
                                                .Where(a => a.Id == Sibling && !_listExcludedStatus.Contains(a.IdStudentStatus))
                                                .Select(a => new
                                                {
                                                    a.IdStudentStatus
                                                })
                                                .Distinct()
                                                .AnyAsync();
                    if (checkStudentStatus)
                    {
                        filterLevel = getEnrollmentStudent.Select(a => a.IdLevel).Distinct().ToList();
                        filterGrade = getEnrollmentStudent.Select(a => a.IdGrade).Distinct().ToList();
                        var result = await _dbContext.Entity<MsSurveyRespondent>()
                                .Include(x => x.Survey)
                                    .ThenInclude(x => x.SurveyResponses)
                                .Where(a => a.Survey.IdSchool == param.IdSchool
                                && a.Role == param.Role
                                && a.Survey.StartDate < _dateTime.ServerTime
                                && _dateTime.ServerTime < a.Survey.EndDate
                                && (a.IdLevel != null ? filterLevel.Any(b => b == a.IdLevel) : true)
                                && (a.IdGrade != null ? filterGrade.Any(b => b == a.IdGrade) : true)
                                && ((param.Role.ToLower() == "parent" && a.IdParent != null ? a.IdParent == "P" + Sibling : ((a.IdLevel != null ? filterLevel.Any(b => b == a.IdLevel) : true)
                                            && (a.IdGrade != null ? filterGrade.Any(b => b == a.IdGrade) : true)))
                                    )
                                && !a.Survey.SurveyResponses.Any(z => z.Question1 == Sibling)
                                )
                                .Select(a => new GetSurveyResult()
                                {
                                    IdSurvey = a.IdSurvey,
                                    SurveyMessage = a.Survey.SurveyMessage,
                                    SurveyTitle = a.Survey.SurveyTitle,
                                    OrderNumber = a.Survey.OrderNumber,
                                    IsBlocking = a.Survey.IsBlocking
                                })
                                .OrderBy(a => a.OrderNumber)
                                .ToListAsync();
                        ReturnResult.AddRange(result);
                    }
                    
                }
                
            }
            else
            {
                if (param.Role == "STUDENT")
                {
                    var getEnrollmentStudent = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                .Where(a => getPeriodActive.First().IdAcademicYear == a.Lesson.Grade.MsLevel.IdAcademicYear
                                                && a.HomeroomStudent.IdStudent == param.IdUser)
                                                .Select(a => new {
                                                    a.HomeroomStudent.IdStudent,
                                                    a.Lesson.Grade.IdLevel,
                                                    a.Lesson.IdGrade
                                                })
                                                .Distinct()
                                                .ToListAsync();
                    var checkStudentStatus = await _dbContext.Entity<MsStudent>()
                                                .Where(a => a.Id == param.IdUser && !_listExcludedStatus.Contains(a.IdStudentStatus))
                                                .Select(a => new
                                                {
                                                    a.IdStudentStatus
                                                })
                                                .Distinct()
                                                .AnyAsync();
                    if (checkStudentStatus)
                    {

                        filterLevel = getEnrollmentStudent.Select(a => a.IdLevel).Distinct().ToList();
                        filterGrade = getEnrollmentStudent.Select(a => a.IdGrade).Distinct().ToList();
                    }
                }
                var result = await _dbContext.Entity<MsSurveyRespondent>()
                            .Include(x => x.Survey)
                                .ThenInclude(x => x.SurveyResponses)
                            .Where(a => a.Survey.IdSchool == param.IdSchool
                            && a.Role == param.Role
                            && a.Survey.StartDate < _dateTime.ServerTime
                            && _dateTime.ServerTime < a.Survey.EndDate
                            && (a.IdLevel != null ? filterLevel.Any(b => b == a.IdLevel) : true)
                            && (a.IdGrade != null ? filterGrade.Any(b => b == a.IdGrade) : true)
                            && (param.Role.ToLower() == "student" && a.IdStudent != null ? a.IdStudent == param.IdUser : true)
                            && (param.Role.ToLower() == "staff" && a.IdStaff != null ? a.IdStaff == param.IdUser : true)
                            && !a.Survey.SurveyResponses.Any(z => z.Question1 == param.IdUser)
                            )
                            .Select(a => new GetSurveyResult()
                            {
                                IdSurvey = a.IdSurvey,
                                SurveyMessage = a.Survey.SurveyMessage,
                                SurveyTitle = a.Survey.SurveyTitle,
                                OrderNumber = a.Survey.OrderNumber,
                                IsBlocking = a.Survey.IsBlocking
                            })
                            .OrderBy(a => a.OrderNumber)
                            .ToListAsync();
                ReturnResult.AddRange(result);
            }



            return Request.CreateApiResult2(ReturnResult.OrderBy(x => x.OrderNumber) as object);
        }
    }
}
