using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Document.FnDocument.SendEmail.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.SendEmail
{
    public class ResendEmailBLPForParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ISendGrid _sendGridApi;
        private readonly IMachineDateTime _dateTime;

        public ResendEmailBLPForParentHandler(
            IDocumentDbContext dbContext,
            ISendGrid sendGridApi,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _sendGridApi = sendGridApi;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var paramList = await Request.ValidateBody<List<ResendEmailBLPForParentRequest>, ResendEmailBLPForParentValidator>();

            var idSchoolParamList = new List<string>();
            var idSurveyPeriodParamList = new List<string>();
            var idClearanceWeekPeriodParamList = new List<string>();
            var idStudentParamList = new List<string>();

            foreach (var param in paramList)
            {
                idSchoolParamList.Add(param.IdSchool);
                idSurveyPeriodParamList.Add(param.IdSurveyPeriod);
                idClearanceWeekPeriodParamList.Add(param.IdClearanceWeekPeriod);
                idStudentParamList.Add(param.IdStudent);
            }

            var finalResult = new ResendEmailBLPForParentResult();
            finalResult.IsSuccessAll = true;

            var studentResultList = new List<ResendEmailBLPForParentResult_StudentResult>();

            var BLPPeriodAllStudents = _dbContext.Entity<TrBLPEmailSentLog>()
                                        .Include(besl => besl.SurveyPeriod)
                                        .Where(x => idSurveyPeriodParamList.Any(y => y == x.IdSurveyPeriod) &&
                                                    idClearanceWeekPeriodParamList.Any(y => y == x.IdClearanceWeekPeriod) &&
                                                    idStudentParamList.Any(y => y == x.IdStudent))
                                        .ToList();

            var BLPRespondentStudents = _dbContext.Entity<MsRespondent>()
                                        .Where(x => idSurveyPeriodParamList.Any(y => y == x.IdSurveyPeriod) &&
                                                    idClearanceWeekPeriodParamList.Any(y => y == x.IdClearanceWeekPeriod) &&
                                                    idStudentParamList.Any(y => y == x.IdStudent))
                                        .ToList();

            foreach (var param in paramList)
            {
                var BLPRespondent = BLPRespondentStudents
                                    .Where(x => x.IdSurveyPeriod == param.IdSurveyPeriod &&
                                                (string.IsNullOrEmpty(param.IdClearanceWeekPeriod) ? true : x.IdClearanceWeekPeriod == x.IdClearanceWeekPeriod) &&
                                                x.IdStudent == param.IdStudent)
                                    .FirstOrDefault();

                // if student has not filled CC form, then skip
                if (BLPRespondent != null)
                {
                    var studentResult = new ResendEmailBLPForParentResult_StudentResult();

                    // check active BLP survey period
                    var BLPPeriod = BLPPeriodAllStudents
                                        .Where(x => x.IdSurveyPeriod == param.IdSurveyPeriod &&
                                                    (string.IsNullOrEmpty(param.IdClearanceWeekPeriod) ? true : x.IdClearanceWeekPeriod == x.IdClearanceWeekPeriod) &&
                                                    x.IdStudent == param.IdStudent &&
                                                    _dateTime.ServerTime >= x.SurveyPeriod.StartDate &&
                                                    _dateTime.ServerTime <= x.SurveyPeriod.EndDate)
                                        .OrderByDescending(x => x.DateIn)
                                        .FirstOrDefault();

                    if (BLPPeriod == null)
                    {
                        studentResult.IdStudent = param.IdStudent;
                        studentResult.IsSuccess = false;
                        studentResult.ErrorMessage = "No active survey period";
                    }
                    else
                    {
                        try
                        {
                            var toList = new List<SendSendGridEmailRequest_AddressBuilder>();

                            var splitPrimaryToList = BLPPeriod.PrimaryToAddress.Split(";").ToList();

                            foreach (var splitPrimaryTo in splitPrimaryToList)
                            {
                                if (!string.IsNullOrEmpty(splitPrimaryTo))
                                {
                                    toList.Add(new SendSendGridEmailRequest_AddressBuilder
                                    {
                                        Address = splitPrimaryTo,
                                        DisplayName = null
                                    });
                                }
                            }

                            var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                            {
                                IdSchool = param.IdSchool,
                                RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                                {
                                    ToList = toList,
                                    CcList = null,
                                    BccList = null
                                },
                                MessageContent = new SendSendGridEmailRequest_MessageContent
                                {
                                    Subject = BLPPeriod.EmailSubject.Trim(),
                                    BodyHtml = BLPPeriod.HTMLDescription
                                }
                            });

                            if (sendEmail.Payload.IsSuccess == false)
                            {
                                throw new BadRequestException(null);
                            }

                            // update log
                            BLPPeriod.ResendDate = _dateTime.ServerTime;
                            _dbContext.Entity<TrBLPEmailSentLog>().Update(BLPPeriod);

                            studentResult.IdStudent = param.IdStudent;
                            studentResult.IsSuccess = true;
                            studentResult.ErrorMessage = null;
                        }
                        catch (Exception ex)
                        {
                            studentResult.IdStudent = param.IdStudent;
                            studentResult.IsSuccess = false;
                            studentResult.ErrorMessage = "Failed to send email";
                        }
                    }
                    studentResultList.Add(studentResult);

                    if (studentResult.IsSuccess == false)
                        finalResult.IsSuccessAll = false;
                }
            }

            finalResult.StudentResults = studentResultList;

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(finalResult as object);
        }
    }
}
