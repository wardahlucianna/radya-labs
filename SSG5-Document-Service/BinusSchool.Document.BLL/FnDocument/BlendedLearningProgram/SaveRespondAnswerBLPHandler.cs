using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using BinusSchool.Document.FnDocument.BlendedLearningProgram.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Document.FnDocument.BlendedLearningProgram
{
    public class SaveRespondAnswerBLPHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly ISendNotification _sendNotification;
        private readonly IClearanceForm _clearanceForm;
        private readonly IMachineDateTime _dateTime;

        public SaveRespondAnswerBLPHandler(IDocumentDbContext dbContext, ISendNotification sendNotification, IClearanceForm clearanceForm, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _sendNotification = sendNotification;
            _clearanceForm = clearanceForm;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveRespondAnswerBLPRequest, SaveRespondAnswerBLPValidator>();


            try
            {
                var groupByStudent = body.ListQuestionAnswer.GroupBy(x => x.IdStudent).ToList();

                var sendEmailClearanceStaff = new SendEmailClearanceFormForStaffRequest();
                var sendEmailClearance = new SendEmailClearanceFormForParentRequest();
                var sendEmailConcent = new SendEmailConcentFormForParentRequest();
                var checkID = new BLP_StudentParentInfo();
                var getRespondent = new MsRespondent();

                foreach (var group in groupByStudent)
                {
                    _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                    #region Get Needed List
                    var getSurveyAnswerMapping = await _dbContext.Entity<TrSurveyAnswerMapping>()
                        .Include(x => x.SurveyAnswer)
                        .Include(x => x.SurveyQuestionMapping)
                            .ThenInclude(x => x.SurveySection)
                            .ThenInclude(x => x.SurveyCategory)
                        .ToListAsync(CancellationToken);

                    var getSurveyStudentAnswer = await _dbContext.Entity<TrSurveyStudentAnswer>()
                        .Include(x => x.SurveyQuestionMapping)
                            .ThenInclude(x => x.SurveyQuestionType)
                        .Include(x => x.Respondent)
                        .Where(x => x.Respondent.IdStudent == group.Key)
                        .ToListAsync(CancellationToken);

                    var getHistorySurveyStudentAnswer = await _dbContext.Entity<HTrSurveyStudentAnswer>()
                        .Where(x => x.Respondent.IdStudent == group.Key)
                        .ToListAsync(CancellationToken);

                    var getStudentParent = await _dbContext.Entity<MsStudentParent>()
                        .Include(x => x.Parent)
                        .ThenInclude(x => x.ParentRole)
                        .Where(x => x.IdStudent == group.Key)
                        .ToListAsync(CancellationToken);

                    var BLPGroupStudent = await _dbContext.Entity<TrBLPGroupStudent>()
                        .ToListAsync(CancellationToken);

                    var getlistBLPUpdatedConsentStatus = await _dbContext.Entity<TrBLPUpdatedConsentStatus>()
                        .ToListAsync(CancellationToken);

                    var getListRespondent = await _dbContext.Entity<MsRespondent>()
                        .Where(x => x.IdStudent == group.Key)
                        .ToListAsync(CancellationToken);
                    #endregion

                    var student = await _dbContext.Entity<MsStudent>()
                        .Where(x => x.Id == group.Key)
                        .FirstOrDefaultAsync(CancellationToken);

                    var isUpdate = body.ListQuestionAnswer.Where(x => x.IdStudent == group.Key).Select(x => x.IsUpdate).FirstOrDefault();
                    var idSurveyPeriodxx = body.ListQuestionAnswer.Where(x => x.IdStudent == group.Key).FirstOrDefault();
                    var idSurveyPeriod = body.ListQuestionAnswer.Where(x => x.IdStudent == group.Key).Select(x => x.IdSurveyPeriod).FirstOrDefault();
                    var idHomeroomStudent = body.ListQuestionAnswer.Where(x => x.IdStudent == group.Key).Select(x => x.IdHomeroomStudent).FirstOrDefault();
                    var idClearanceWeekPeriod = body.ListQuestionAnswer.Where(x => x.IdStudent == group.Key).Select(x => x.IdClearanceWeekPeriod).FirstOrDefault();
                    var getAnswerFinal = group.Select(x => x.IdSurveyAnswerMapping).ToList();

                    var StudentAnswerList = getSurveyAnswerMapping
                        .Where(x => getAnswerFinal.Any(y => y == x.Id))
                        .Where(x => x.IsNotAllowed != null)
                        .Select(x => x.IsNotAllowed)
                        .Distinct()
                        .ToList();

                    var getSurveyPeriodData = await _dbContext.Entity<MsSurveyPeriod>()
                        .Include(x => x.Grade)
                            .ThenInclude(x => x.Level)
                        .Where(x => x.Id == idSurveyPeriod)
                        .SingleOrDefaultAsync(CancellationToken);

                    var containTrue = StudentAnswerList.Contains(true);
                    var getBLPFinalStatus = new BLPFinalStatus();

                    var setBLPFinalStatus = containTrue == true ? false : true;

                    if (setBLPFinalStatus == true)
                        getBLPFinalStatus = BLPFinalStatus.Allowed;
                    else
                        getBLPFinalStatus = BLPFinalStatus.NotAllowed;

                    //Simprug hanya ada Clearance Form
                    if (student.IdSchool == "1")
                    {
                        #region GetIdParent
                        var parentName = group
                                    .Where(x => x.IdSurveyQuestionMapping == "SQM20231SecA1")
                                    .Select(x => x.Description).FirstOrDefault();

                        //var getSSAParentName = group
                        //            .Where(x => x.IdSurveyQuestionMapping == "SQMS1C2SA03")
                        //            .Select(x => x.IdSurveyAnswerMapping).FirstOrDefault();

                        var getSSAParentRole = group
                                    .Where(x => x.IdSurveyQuestionMapping == "SQM20231SecA2")
                                    .Select(x => x.IdSurveyAnswerMapping).FirstOrDefault();

                        var getSSAEmailAddress = group
                            .Where(x => x.IdSurveyQuestionMapping == "SQM20231SecA3")
                            .Select(x => x.Description).FirstOrDefault();

                        var getSSAPhoneNumber = group
                            .Where(x => x.IdSurveyQuestionMapping == "SQM20231SecA4")
                            .Select(x => x.Description).FirstOrDefault();

                        var getParentRole = getSurveyAnswerMapping
                            .Where(x => x.Id == getSSAParentRole)
                            .Select(x => new ItemValueVm{
                                Id = x.SurveyAnswer.Description.Split(' ').Last().Substring(0, 1),
                                Description = x.SurveyAnswer.Description
                            })
                            .FirstOrDefault();

                        var isParentExist = getStudentParent
                            .Where(x => getParentRole == null || x.Parent.IdParentRole == getParentRole.Id)
                            .Select(x => x.IdParent)
                            .FirstOrDefault();

                        if (isParentExist is null)
                        {
                            throw new BadRequestException("Cant Find Parent With Role " + getParentRole.Description);
                        }

                        var checkParentSimprug = getStudentParent
                           .Where(x => x.IdParent == isParentExist)
                           .Select(x => new BLP_StudentParentInfo
                           {
                               IdParent = x.IdParent,
                               ParentName = (x.Parent.FirstName == null ? "" : x.Parent.FirstName.Trim().ToLower()) + " " + x.Parent.LastName.Trim().ToLower(),
                               ParentRole = x.Parent.ParentRole.Id,
                               EmailAddress = x.Parent.PersonalEmailAddress,
                               PhoneNumber = x.Parent.MobilePhoneNumber1
                           }).ToList();

                        //bool checkIDSimprug = new bool(checkIDSimprug);

                        //Comment check ID
                        //if (isParentExist is null)
                        //{
                        //    throw new BadRequestException("Cant Find Parent With Role " + getParentRole.Description);
                        //}
                        //else
                        //{
                        //    checkID = checkParentSimprug.Where(x => x.ParentName.Contains(parentName.Trim().ToLower())).FirstOrDefault();
                        //}

                        //if (checkID is null)
                        //{
                        //    throw new BadRequestException("Parent Name : " + parentName + " Not match in Database");
                        //}
                        //if (checkID != null && checkID.ParentRole != getParentRole.Id)
                        //{
                        //    throw new BadRequestException("Parent Role : " + parentName + " Not " + getParentRole.Description);
                        //}
                        #endregion

                        if (body.IdSurveyCategory == "2")
                        {
                            if (!isUpdate)
                            {
                                var AddRespondentClearanceSimprug = new MsRespondent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdStudent = group.Key,
                                    IdParent = isParentExist,
                                    IdSurveyPeriod = idSurveyPeriod,
                                    IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                    EmailAddress = getSSAEmailAddress ?? "",
                                    PhoneNumber = getSSAPhoneNumber ?? "",
                                    ResultSummary = containTrue == true ? false : true,
                                };
                                _dbContext.Entity<MsRespondent>().Add(AddRespondentClearanceSimprug);

                                foreach (var data in group)
                                {
                                    var AddParam = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = AddRespondentClearanceSimprug.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = AddRespondentClearanceSimprug.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FileName
                                    };
                                    _dbContext.Entity<TrSurveyStudentAnswer>().Add(AddParam);
                                }

                                #region Get Parameter Email
                                var getDataClearance = await _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest
                                {
                                    Username = group.Key,
                                    IsParent = false
                                });

                                if (getDataClearance?.Payload != null)
                                {
                                    var dataClearance = getDataClearance.Payload.ListChild.FirstOrDefault();

                                    sendEmailClearance = new SendEmailClearanceFormForParentRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForParentRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = parentName
                                            },
                                            ParentEmail = getSSAEmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                    };

                                    #region Email Staff
                                    sendEmailClearanceStaff = new SendEmailClearanceFormForStaffRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForStaffRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = parentName
                                            },
                                            ParentEmail = getSSAEmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                        AuditAction = AuditAction.Insert
                                    };
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                getRespondent = getListRespondent
                                        //.Where(x => x.IdParent == idParent)
                                        .Where(x => x.IdSurveyPeriod == idSurveyPeriod)
                                        .FirstOrDefault();

                                if (getRespondent is null)
                                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Data Respondent"], "Not Exist", "Data Respondent"));

                                getRespondent.IdParent = isParentExist;
                                getRespondent.IdClearanceWeekPeriod = idClearanceWeekPeriod;
                                getRespondent.EmailAddress = getSSAEmailAddress ?? "";
                                getRespondent.PhoneNumber = getSSAPhoneNumber ?? "";
                                getRespondent.ResultSummary = containTrue == true ? false : true;
                                _dbContext.Entity<MsRespondent>().Update(getRespondent);

                                #region Remove table History
                                var getAllHistoryAnswer = getHistorySurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                if (getAllHistoryAnswer != null)
                                    _dbContext.Entity<HTrSurveyStudentAnswer>().RemoveRange(getAllHistoryAnswer);
                                #endregion

                                #region Copy all answer to history
                                var getOldData = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                var newHistory = new List<HTrSurveyStudentAnswer>();

                                foreach (var data in getOldData)
                                {
                                    var history = new HTrSurveyStudentAnswer
                                    {
                                        IdHTrSurveyStudentAnswer = data.Id,
                                        IdRespondent = data.IdRespondent,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FilePath
                                    };
                                    newHistory.Add(history);
                                }

                                _dbContext.Entity<HTrSurveyStudentAnswer>().AddRange(newHistory);
                                #endregion

                                #region Remove table Answer
                                var getAllAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .Where(x => x.FilePath == null)
                                        //.Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType != "QT4")
                                        .ToList();

                                if (getAllAnswer != null)
                                    _dbContext.Entity<TrSurveyStudentAnswer>().RemoveRange(getAllAnswer);
                                else
                                    throw new BadRequestException("No Data to Update");
                                #endregion

                                #region New Answer
                                var newAnswer = new List<TrSurveyStudentAnswer>();
                                foreach (var data in group)
                                {
                                    var answer = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = getRespondent.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FileName
                                    };
                                    newAnswer.Add(answer);
                                }

                                var getFileAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        //.Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType == "QT4")
                                        .Where(x => x.FilePath != null)
                                        .ToList();

                                if(getFileAnswer != null)
                                {
                                    foreach (var item in getFileAnswer)
                                    {
                                        var setFilePath = newAnswer
                                                .Where(x => x.IdSurveyAnswerMapping == item.IdSurveyAnswerMapping)
                                                .Where(x => x.IdSurveyQuestionMapping == item.IdSurveyQuestionMapping)
                                                .FirstOrDefault();

                                        if (setFilePath != null)
                                        {
                                            if (setFilePath?.FilePath != null)
                                            {
                                                _dbContext.Entity<TrSurveyStudentAnswer>().Remove(item);
                                            }
                                            else
                                            {
                                                var index = newAnswer.FindIndex(r => r.Id == setFilePath.Id);
                                                setFilePath.FilePath = item.FilePath;
                                                if (index != -1)
                                                {
                                                    newAnswer[index] = setFilePath;
                                                }
                                            }
                                        }
                                    }
                                }

                                _dbContext.Entity<TrSurveyStudentAnswer>().AddRange(newAnswer);
                                #endregion

                                #region Get Parameter Email
                                var getDataClearance = await _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest
                                {
                                    Username = group.Key,
                                    IsParent = false
                                });

                                if (getDataClearance?.Payload != null)
                                {
                                    var dataClearance = getDataClearance.Payload.ListChild.FirstOrDefault();

                                    sendEmailClearance = new SendEmailClearanceFormForParentRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForParentRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = parentName
                                            },
                                            ParentEmail = getSSAEmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                    };

                                    #region Email Staff
                                    sendEmailClearanceStaff = new SendEmailClearanceFormForStaffRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForStaffRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = parentName
                                            },
                                            ParentEmail = getSSAEmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                        AuditAction = AuditAction.Update
                                    };
                                    #endregion

                                }
                                #endregion
                            }

                        }
                    }

                    if (student.IdSchool == "2")
                    {
                        if (body.IdSurveyCategory == "1")
                        {
                            #region get Parent Data
                            var getSSAParentNameConsent = group
                                        .Where(x => x.IdSurveyQuestionMapping == "SQMTextCCFS2SA02")
                                        .Select(x => x.Description.Trim().ToLower()).FirstOrDefault();

                            var getSSAEmailAddressConsent = group
                                    .Where(x => x.IdSurveyQuestionMapping == "SQMTextCCFS2SA03")
                                    .Select(x => x.Description).FirstOrDefault();

                            var getSSAPhoneNumberConsent = group
                                .Where(x => x.IdSurveyQuestionMapping == "SQMTextCCFS2SA04")
                                .Select(x => x.Description).FirstOrDefault();

                            var checkParent = getStudentParent
                               .Select(x => new BLP_StudentParentInfo
                               {
                                   IdParent = x.IdParent,
                                   ParentName = (x.Parent.FirstName == null ? "" : x.Parent.FirstName.Trim().ToLower()) + " " + x.Parent.LastName.Trim().ToLower(),
                                   EmailAddress = x.Parent.PersonalEmailAddress,
                                   PhoneNumber = x.Parent.MobilePhoneNumber1
                               }).ToList();

                            checkID = checkParent.Where(x => x.ParentName.Contains(getSSAParentNameConsent)).FirstOrDefault();

                            if (checkID is null)
                                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Parent Name"], "Not Exist", "Parent Name"));
                            #endregion

                            if (!isUpdate)
                            {
                                var AddRespondentConcentSerpong = new MsRespondent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdStudent = group.Key,
                                    IdParent = checkID.IdParent,
                                    IdSurveyPeriod = idSurveyPeriod,
                                    IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                    EmailAddress = getSSAEmailAddressConsent ?? "",
                                    PhoneNumber = getSSAPhoneNumberConsent ?? "",
                                    ResultSummary = containTrue == true ? false : true,
                                };
                                _dbContext.Entity<MsRespondent>().Add(AddRespondentConcentSerpong);

                                foreach (var data in group)
                                {
                                    var AddParam = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = AddRespondentConcentSerpong.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = AddRespondentConcentSerpong.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FileName
                                    };
                                    _dbContext.Entity<TrSurveyStudentAnswer>().Add(AddParam);
                                }

                                #region Get Parameter Email
                                sendEmailConcent = new SendEmailConcentFormForParentRequest
                                {
                                    IdAcademicYear = getSurveyPeriodData.Grade.Level.IdAcademicYear,
                                    Semester = getSurveyPeriodData.Semester,
                                    IdSchool = student.IdSchool,
                                    IdStudent = student.Id
                                };
                                #endregion
                            }
                            else
                            {
                                getRespondent = getListRespondent
                                    //.Where(x => x.IdParent == checkID.IdParent)
                                    .Where(x => x.IdSurveyPeriod == idSurveyPeriod)
                                    .FirstOrDefault();

                                if (getRespondent is null)
                                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Data Respondent"], "Not Exist", "Data Respondent"));

                                getRespondent.IdParent = checkID.IdParent;
                                getRespondent.IdClearanceWeekPeriod = idClearanceWeekPeriod;
                                getRespondent.EmailAddress = getSSAEmailAddressConsent ?? "";
                                getRespondent.PhoneNumber = getSSAPhoneNumberConsent ?? "";
                                getRespondent.ResultSummary = containTrue == true ? false : true;
                                _dbContext.Entity<MsRespondent>().Update(getRespondent);

                                #region Remove table History
                                var getAllHistoryAnswer = getHistorySurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                if (getAllHistoryAnswer != null)
                                    _dbContext.Entity<HTrSurveyStudentAnswer>().RemoveRange(getAllHistoryAnswer);
                                #endregion

                                #region Copy all answer to history
                                var getOldData = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                var newHistory = new List<HTrSurveyStudentAnswer>();

                                foreach (var data in getOldData)
                                {
                                    var history = new HTrSurveyStudentAnswer
                                    {
                                        IdHTrSurveyStudentAnswer = data.Id,
                                        IdRespondent = data.IdRespondent,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FilePath
                                    };
                                    newHistory.Add(history);
                                }

                                _dbContext.Entity<HTrSurveyStudentAnswer>().AddRange(newHistory);
                                #endregion

                                #region Remove table Answer
                                var getAllAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType != "QT4")
                                        .ToList();

                                if (getAllAnswer != null)
                                    _dbContext.Entity<TrSurveyStudentAnswer>().RemoveRange(getAllAnswer);
                                #endregion

                                #region New Answer
                                var newAnswer = new List<TrSurveyStudentAnswer>();
                                foreach (var data in group)
                                {
                                    var answer = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = getRespondent.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FileName
                                    };
                                    newAnswer.Add(answer);
                                }

                                var getFileAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType == "QT4")
                                        .ToList();

                                foreach (var item in getFileAnswer)
                                {
                                    var setFilePath = newAnswer
                                            .Where(x => x.IdSurveyAnswerMapping == item.IdSurveyAnswerMapping)
                                            .Where(x => x.IdSurveyQuestionMapping == item.IdSurveyQuestionMapping)
                                            .FirstOrDefault();

                                    if (setFilePath != null)
                                    {
                                        if (setFilePath?.FilePath != null)
                                        {
                                            _dbContext.Entity<TrSurveyStudentAnswer>().Remove(item);
                                        }
                                        else
                                        {
                                            var index = newAnswer.FindIndex(r => r.Id == setFilePath.Id);
                                            setFilePath.FilePath = item.FilePath;
                                            if (index != -1)
                                            {
                                                newAnswer[index] = setFilePath;
                                            }
                                        }
                                    }
                                }

                                _dbContext.Entity<TrSurveyStudentAnswer>().AddRange(newAnswer);
                                #endregion

                                #region Get Parameter Email
                                sendEmailConcent = new SendEmailConcentFormForParentRequest
                                {
                                    IdAcademicYear = getSurveyPeriodData.Grade.Level.IdAcademicYear,
                                    Semester = getSurveyPeriodData.Semester,
                                    IdSchool = student.IdSchool,
                                    IdStudent = student.Id
                                };
                                #endregion
                            }

                            #region Change TrBLPGroupStudent
                            var getBLPGroupStudent = BLPGroupStudent
                                .Where(x => x.IdStudent == group.Key && x.IdHomeroomStudent == idHomeroomStudent)
                                .FirstOrDefault(); //cek yg lain juga

                            if (getBLPGroupStudent != null)
                            {
                                var getBLPUpdatedConsentStatus = getlistBLPUpdatedConsentStatus
                                    .Where(x => x.IdStudent == group.Key && x.IdHomeroomStudent == idHomeroomStudent)
                                    .FirstOrDefault(); //cek yg lain juga'

                                if (getBLPUpdatedConsentStatus == null)
                                {
                                    var newBLPUpdatedConsentStatus = new TrBLPUpdatedConsentStatus
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdStudent = group.Key,
                                        IdHomeroomStudent = idHomeroomStudent,
                                        IdBLPStatusBefore = getBLPGroupStudent.IdBLPStatus,
                                        IdBLPStatusAfter = containTrue == true ? "2" : "1",
                                        TransactionDate = _dateTime.ServerTime.Date
                                    };
                                    _dbContext.Entity<TrBLPUpdatedConsentStatus>().Add(newBLPUpdatedConsentStatus);
                                }
                                else
                                {
                                    getBLPUpdatedConsentStatus.IdBLPStatusBefore = getBLPUpdatedConsentStatus.IdBLPStatusAfter;
                                    getBLPUpdatedConsentStatus.IdBLPStatusAfter = containTrue == true ? "2" : "1";
                                    getBLPUpdatedConsentStatus.TransactionDate = _dateTime.ServerTime.Date;
                                    _dbContext.Entity<TrBLPUpdatedConsentStatus>().Update(getBLPUpdatedConsentStatus);
                                }
                                getBLPGroupStudent.IdBLPStatus = containTrue == true ? "2" : "1";
                                _dbContext.Entity<TrBLPGroupStudent>().Update(getBLPGroupStudent);
                            }
                            #endregion
                        }
                        if (body.IdSurveyCategory == "2")
                        {
                            #region get Parent Data
                            var getSSAParentNameClearance = group
                                        .Where(x => x.IdSurveyQuestionMapping == "SQMRadioCFS2SF03")
                                        .Select(x => x.Description.Trim().ToLower()).FirstOrDefault();

                            var checkParent = getStudentParent
                               .Select(x => new BLP_StudentParentInfo
                               {
                                   IdParent = x.IdParent,
                                   ParentName = (x.Parent.FirstName == null ? "" : x.Parent.FirstName.Trim().ToLower()) + " " + x.Parent.LastName.Trim().ToLower(),
                                   EmailAddress = x.Parent.PersonalEmailAddress,
                                   PhoneNumber = x.Parent.MobilePhoneNumber1
                               }).ToList();

                            var checkIDs = checkParent.Where(p => string.Equals(p.ParentName, getSSAParentNameClearance, StringComparison.CurrentCulture)).ToList();

                            checkID = checkIDs.FirstOrDefault();
                            if (checkID == null)
                                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Parent Name"], "Not Exist", "Parent Name"));
                            #endregion

                            if (!isUpdate)
                            {
                                var AddRespondentClearanceSerpong = new MsRespondent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdStudent = group.Key,
                                    IdParent = checkID.IdParent,
                                    IdSurveyPeriod = idSurveyPeriod,
                                    IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                    EmailAddress = checkID.EmailAddress,
                                    PhoneNumber = checkID.PhoneNumber,
                                    ResultSummary = containTrue == true ? false : true,
                                };
                                _dbContext.Entity<MsRespondent>().Add(AddRespondentClearanceSerpong);

                                foreach (var data in group)
                                {
                                    var AddParam = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = AddRespondentClearanceSerpong.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = AddRespondentClearanceSerpong.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FileName
                                    };
                                    _dbContext.Entity<TrSurveyStudentAnswer>().Add(AddParam);
                                }

                                #region Get Parameter Email
                                var getDataClearance = await _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest
                                {
                                    Username = group.Key,
                                    IsParent = false
                                });

                                if (getDataClearance?.Payload != null)
                                {
                                    var dataClearance = getDataClearance.Payload.ListChild.FirstOrDefault();

                                    sendEmailClearance = new SendEmailClearanceFormForParentRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForParentRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = getSSAParentNameClearance
                                            },
                                            ParentEmail = checkID.EmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                    };

                                    #region Email Staff
                                    sendEmailClearanceStaff = new SendEmailClearanceFormForStaffRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForStaffRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = getSSAParentNameClearance
                                            },
                                            ParentEmail = checkID.EmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                        AuditAction = AuditAction.Insert
                                    };
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                getRespondent = getListRespondent
                                    //.Where(x => x.IdParent == checkID.IdParent)
                                    .Where(x => x.IdSurveyPeriod == idSurveyPeriod)
                                    .FirstOrDefault();

                                if (getRespondent is null)
                                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Data Respondent"], "Not Exist", "Data Respondent"));

                                getRespondent.IdParent = checkID.IdParent;
                                getRespondent.IdClearanceWeekPeriod = idClearanceWeekPeriod;
                                getRespondent.EmailAddress = checkID.EmailAddress;
                                getRespondent.PhoneNumber = checkID.PhoneNumber;
                                getRespondent.ResultSummary = containTrue == true ? false : true;
                                _dbContext.Entity<MsRespondent>().Update(getRespondent);

                                #region Remove table History
                                var getAllHistoryAnswer = getHistorySurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                if (getAllHistoryAnswer != null)
                                    _dbContext.Entity<HTrSurveyStudentAnswer>().RemoveRange(getAllHistoryAnswer);
                                #endregion

                                #region Copy all answer to history
                                var getOldData = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .ToList();

                                var newHistory = new List<HTrSurveyStudentAnswer>();

                                foreach (var data in getOldData)
                                {
                                    var history = new HTrSurveyStudentAnswer
                                    {
                                        IdHTrSurveyStudentAnswer = data.Id,
                                        IdRespondent = data.IdRespondent,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description,
                                        FilePath = data.FilePath
                                    };
                                    newHistory.Add(history);
                                }

                                _dbContext.Entity<HTrSurveyStudentAnswer>().AddRange(newHistory);
                                #endregion

                                #region Remove table Answer
                                var getAllAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType != "QT4")
                                        .ToList();

                                if (getAllAnswer != null)
                                    _dbContext.Entity<TrSurveyStudentAnswer>().RemoveRange(getAllAnswer);
                                #endregion

                                #region New Answer
                                var newAnswer = new List<TrSurveyStudentAnswer>();
                                foreach (var data in group)
                                {
                                    var answer = new TrSurveyStudentAnswer
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdRespondent = getRespondent.Id,
                                        IdSurveyQuestionMapping = data.IdSurveyQuestionMapping,
                                        IdSurveyAnswerMapping = data.IdSurveyAnswerMapping,
                                        IdClearanceWeekPeriod = data.IdClearanceWeekPeriod,
                                        Description = data.Description
                                    };
                                    newAnswer.Add(answer);
                                }

                                var getFileAnswer = getSurveyStudentAnswer
                                        .Where(x => x.IdRespondent == getRespondent.Id)
                                        .Where(x => x.SurveyQuestionMapping.IdSurveyQuestionType == "QT4")
                                        .ToList();

                                foreach (var item in getFileAnswer)
                                {
                                    var setFilePath = newAnswer
                                            .Where(x => x.IdSurveyAnswerMapping == item.IdSurveyAnswerMapping)
                                            .Where(x => x.IdSurveyQuestionMapping == item.IdSurveyQuestionMapping)
                                            .FirstOrDefault();

                                    if (setFilePath != null)
                                    {
                                        if (setFilePath?.FilePath != null)
                                        {
                                            _dbContext.Entity<TrSurveyStudentAnswer>().Remove(item);
                                        }
                                        else
                                        {
                                            var index = newAnswer.FindIndex(r => r.Id == setFilePath.Id);
                                            setFilePath.FilePath = item.FilePath;
                                            if (index != -1)
                                            {
                                                newAnswer[index] = setFilePath;
                                            }
                                        }
                                    }
                                }

                                _dbContext.Entity<TrSurveyStudentAnswer>().AddRange(newAnswer);
                                #endregion

                                #region Get Parameter Email
                                var getDataClearance = await _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest
                                {
                                    Username = group.Key,
                                    IsParent = false
                                });

                                if (getDataClearance?.Payload != null)
                                {
                                    var dataClearance = getDataClearance.Payload.ListChild.FirstOrDefault();

                                    sendEmailClearance = new SendEmailClearanceFormForParentRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForParentRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = getSSAParentNameClearance
                                            },
                                            ParentEmail = checkID.EmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                    };

                                    #region Email Staff
                                    sendEmailClearanceStaff = new SendEmailClearanceFormForStaffRequest
                                    {
                                        IdSchool = student.IdSchool,
                                        IdSurveyPeriod = idSurveyPeriod,
                                        IdClearanceWeekPeriod = idClearanceWeekPeriod,
                                        StudentSurveyData = new SendEmailClearanceFormForStaffRequest_StudentSurveyData
                                        {
                                            Student = dataClearance.Student,
                                            Homeroom = dataClearance.Homeroom == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Homeroom.Id,
                                                Name = dataClearance.Homeroom.Description
                                            },
                                            AcademicYear = dataClearance.AcademicYear,
                                            Parent = new NameValueVm
                                            {
                                                Id = checkID.IdParent,
                                                Name = getSSAParentNameClearance
                                            },
                                            ParentEmail = checkID.EmailAddress,
                                            SubmissionDate = _dateTime.ServerTime.Date,
                                            BLPGroup = dataClearance.Group == null ? null : new NameValueVm
                                            {
                                                Id = dataClearance.Group.Id,
                                                Name = dataClearance.Group.Description
                                            }
                                        },
                                        BLPFinalStatus = getBLPFinalStatus,
                                        AuditAction = AuditAction.Update
                                    };
                                    #endregion
                                }
                                #endregion
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);

                    //Send Email
                    if (body.IdSurveyCategory == "1")
                    {
                        _sendNotification.SendEmailConcentFormForParent(sendEmailConcent);
                    }
                    else
                    {
                        _sendNotification.SendEmailClearancFormForParent(sendEmailClearance);
                        _sendNotification.SendEmailClearancFormForStaff(sendEmailClearanceStaff);
                    }

                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
