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
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class UpdateDocumentRequestStaffHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetDateBusinessDaysByStartDateHandler _getDateBusinessDaysByStartDateHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;
        private readonly SendEmailDataUpdateRequestToParentHandler _sendEmailDataUpdateRequestToParentHandler;
        private readonly SendEmailDataUpdateRequestToStaffHandler _sendEmailDataUpdateRequestToStaffHandler;

        public UpdateDocumentRequestStaffHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            GetDateBusinessDaysByStartDateHandler getDateBusinessDaysByStartDateHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler,
            SendEmailDataUpdateRequestToParentHandler sendEmailDataUpdateRequestToParentHandler,
            SendEmailDataUpdateRequestToStaffHandler sendEmailDataUpdateRequestToStaffHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getDateBusinessDaysByStartDateHandler = getDateBusinessDaysByStartDateHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
            _sendEmailDataUpdateRequestToParentHandler = sendEmailDataUpdateRequestToParentHandler;
            _sendEmailDataUpdateRequestToStaffHandler = sendEmailDataUpdateRequestToStaffHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateDocumentRequestStaffRequest, UpdateDocumentRequestStaffValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var changesData = new UpdateDocumentRequestStaffResult_Change();
                var changesDetail = new UpdateDocumentRequestStaffResult_ChangeDetail();
                changesData.IdDocumentReqApplicant = param.IdDocumentReqApplicant;

                var getDocumentReqApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                            .Include(x => x.DocumentReqApplicantDetails)
                                                .ThenInclude(x => x.DocumentReqPICs)
                                            .Include(x => x.DocumentReqApplicantDetails)
                                                .ThenInclude(x => x.DocumentReqApplicantFormAnswers)
                                                    .ThenInclude(x => x.DocumentReqFormFieldAnswered)
                                                    .ThenInclude(x => x.DocumentReqFieldType)
                                            .Include(x => x.DocumentReqStatusTrackingHistories)
                                            .Include(x => x.Parent)
                                                .ThenInclude(x => x.ParentRole)
                                            .Where(x => x.Id == param.IdDocumentReqApplicant)
                                            .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentReqApplicant == null)
                    throw new BadRequestException("Document request applicant is not found");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                // validate status
                if (getDocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Canceled ||
                    getDocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Declined ||
                    getDocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ||
                    getDocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected)
                    throw new BadRequestException($"Cannot update request data for request with status: {getDocumentReqApplicant.IdDocumentReqStatusWorkflow.GetDescription()}");

                var getDocumentReqApplicantDetailList = getDocumentReqApplicant.DocumentReqApplicantDetails
                                                            .ToList();

                var getDocumentReqApplicantFormAnswerRawList = getDocumentReqApplicantDetailList
                                                                .SelectMany(y => y.DocumentReqApplicantFormAnswers)
                                                                .ToList();

                var getMsDocumentReqFormFieldAnsweredRawList = await _dbContext.Entity<MsDocumentReqFormFieldAnswered>()
                                                                .Include(x => x.DocumentReqOptionCategory)
                                                                .Where(x => getDocumentReqApplicantFormAnswerRawList.Select(y => y.IdDocumentReqFormFieldAnswered).Any(y => y == x.Id))
                                                                .ToListAsync(CancellationToken);

                var idDocumentReqOptionListParam = param.DocumentRequestList.SelectMany(x => x.AdditionalFieldsList).Where(x => x.IdDocumentReqOptionList != null).SelectMany(x => x.IdDocumentReqOptionList).ToList();

                var getMsDocumentReqOptionRaw = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                                                    .Include(x => x.DocumentReqOptions)
                                                    .Include(x => x.DocumentReqFormFieldAnswereds)
                                                    .Where(x => getMsDocumentReqFormFieldAnsweredRawList.Select(y => y.IdDocumentReqOptionCategory).Any(y => y == x.Id) ||
                                                                idDocumentReqOptionListParam.Any(y => y == x.Id))
                                                    .SelectMany(x => x.DocumentReqOptions)
                                                    .Distinct()
                                                    .ToListAsync(CancellationToken);

                var getTrDocumentReqApplicantFormAnswerRawList = await _dbContext.Entity<TrDocumentReqApplicantFormAnswer>()
                                                    .Where(x => getMsDocumentReqFormFieldAnsweredRawList.Select(y => y.Id).Any(y => y == x.IdDocumentReqFormFieldAnswered) &&
                                                                getDocumentReqApplicant.DocumentReqApplicantDetails.Select(y => y.Id).Any(y => y == x.IdDocumentReqApplicantDetail))
                                                    .ToListAsync(CancellationToken);

                var idBinusianPICListParam = param.DocumentRequestList.SelectMany(y => y.IdBinusianPICList).ToList();
                var idBinusianPICListExisting = getDocumentReqApplicant.DocumentReqApplicantDetails.SelectMany(y => y.DocumentReqPICs).Select(y => y.IdBinusian).ToList();

                var getPICDataRawList = await _dbContext.Entity<MsStaff>()
                                        .Where(x => idBinusianPICListExisting.Any(y => y == x.IdBinusian) ||
                                                    idBinusianPICListParam.Any(y => y == x.IdBinusian))
                                        .Select(x => new NameValueVm
                                        {
                                            Id = x.IdBinusian,
                                            Name = NameUtil.GenerateFullName(x.FirstName, x.LastName)
                                        })
                                        .Distinct()
                                        .ToListAsync(CancellationToken);

                var getLatestDocumentReqStatus = getDocumentReqApplicant.DocumentReqStatusTrackingHistories
                                                    .OrderByDescending(x => x.StatusDate)
                                                    .FirstOrDefault();

                var getRequestStartOnProcessDate = getDocumentReqApplicant.DocumentReqStatusTrackingHistories
                                                    .Where(x => x.IsOnProcess)
                                                    .OrderBy(x => x.StatusDate)
                                                    .FirstOrDefault();

                // condition when status is "ON PROCESS"
                // count estimation finish date
                if (getLatestDocumentReqStatus.IsOnProcess)
                {
                    var estimationFinishDate = await _getDateBusinessDaysByStartDateHandler.CountDateBusinessDaysByStartDate(new GetDateBusinessDaysByStartDateRequest
                    {
                        IdSchool = getDocumentReqApplicant.IdSchool,
                        StartDate = getRequestStartOnProcessDate.StatusDate,
                        TotalDays = param.EstimationFinishDays,
                        CountHoliday = true
                    });

                    if (estimationFinishDate.EndDate < _dateTime.ServerTime)
                        throw new BadRequestException("Estimated finish date should be greater than today");

                    changesDetail.OldEstimationFinishDate = getDocumentReqApplicant.EstimationFinishDate;
                    changesDetail.NewEstimationFinishDate = estimationFinishDate.EndDate;

                    getDocumentReqApplicant.EstimationFinishDate = estimationFinishDate.EndDate;
                }

                // update MsDocumentReqApplicant
                changesDetail.OldParentApplicant = new NameValueVm
                {
                    Id = getDocumentReqApplicant.IdParentApplicant,
                    Name = NameUtil.GenerateFullName(getDocumentReqApplicant.Parent.FirstName, getDocumentReqApplicant.Parent.MiddleName, getDocumentReqApplicant.Parent.LastName) + " (" + getDocumentReqApplicant.Parent.ParentRole.ParentRoleNameEng + ")"
                };

                var getNewParentApplicant = await _dbContext.Entity<MsParent>()
                                                .Include(x => x.ParentRole)
                                                .Where(x => x.Id == param.IdParentApplicant)
                                                .FirstOrDefaultAsync(CancellationToken);
                changesDetail.NewParentApplicant = new NameValueVm
                {
                    Id = getNewParentApplicant.Id,
                    Name = NameUtil.GenerateFullName(getNewParentApplicant.FirstName, getNewParentApplicant.MiddleName, getNewParentApplicant.LastName) + " (" + getNewParentApplicant.ParentRole.ParentRoleNameEng + ")"
                };

                changesDetail.OldEstimationFinishDays = getDocumentReqApplicant.EstimationFinishDays;
                changesDetail.NewEstimationFinishDays = param.EstimationFinishDays;

                getDocumentReqApplicant.IdParentApplicant = param.IdParentApplicant;
                getDocumentReqApplicant.EstimationFinishDays = param.EstimationFinishDays;
                _dbContext.Entity<MsDocumentReqApplicant>().Update(getDocumentReqApplicant);

                var documentChangesDataList = new List<UpdateDocumentRequestStaffResult_ChangeDetail_DocumentRequest>();
                foreach (var paramDocumentRequestList in param.DocumentRequestList)
                {
                    var tempdocumentChangesData = new UpdateDocumentRequestStaffResult_ChangeDetail_DocumentRequest();
                    tempdocumentChangesData.IdDocumentReqApplicantDetail = paramDocumentRequestList.IdDocumentReqApplicantDetail;

                    var tempDocumentReqApplicantDetail = getDocumentReqApplicantDetailList
                                                            .Where(x => x.Id == paramDocumentRequestList.IdDocumentReqApplicantDetail)
                                                            .FirstOrDefault();

                    if(tempDocumentReqApplicantDetail == null)
                        throw new BadRequestException("Document request applicant detail is not found");

                    // update TrDocumentReqApplicantDetail
                    var getPeriodRawList = await _dbContext.Entity<MsPeriod>()
                                        .Where(x => x.Id == tempDocumentReqApplicantDetail.IdPeriodDocument ||
                                                    x.Id == paramDocumentRequestList.IdPeriodDocument)
                                        .ToListAsync(CancellationToken);

                    tempdocumentChangesData.OldPeriodDocument = new ItemValueVm
                    {
                        Id = tempDocumentReqApplicantDetail.IdPeriodDocument,
                        Description = getPeriodRawList.Where(x => x.Id == tempDocumentReqApplicantDetail.IdPeriodDocument).Select(x => x.Description).FirstOrDefault()
                    };

                    tempdocumentChangesData.NewPeriodDocument = new ItemValueVm
                    {
                        Id = paramDocumentRequestList.IdPeriodDocument,
                        Description = getPeriodRawList.Where(x => x.Id == paramDocumentRequestList.IdPeriodDocument).Select(x => x.Description).FirstOrDefault()
                    };

                    tempDocumentReqApplicantDetail.IdPeriodDocument = paramDocumentRequestList.IdPeriodDocument;
                    _dbContext.Entity<TrDocumentReqApplicantDetail>().Update(tempDocumentReqApplicantDetail);

                    // remove existing PIC
                    var getOldPICData = getPICDataRawList
                                            .Where(x => tempDocumentReqApplicantDetail.DocumentReqPICs.Select(y => y.IdBinusian).Any(y => y == x.Id))
                                            .ToList();
                    tempdocumentChangesData.OldBinusianPICList = getOldPICData;

                    var getTrDocumentReqPIC = tempDocumentReqApplicantDetail.DocumentReqPICs.ToList();
                    _dbContext.Entity<TrDocumentReqPIC>().RemoveRange(getTrDocumentReqPIC);

                    // insert new PIC
                    var newTrDocumentReqPIC = paramDocumentRequestList.IdBinusianPICList
                                                .Select(x => new TrDocumentReqPIC
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    IdDocumentReqApplicantDetail = paramDocumentRequestList.IdDocumentReqApplicantDetail,
                                                    IdBinusian = x
                                                })
                                                .ToList();
                    _dbContext.Entity<TrDocumentReqPIC>().AddRange(newTrDocumentReqPIC);

                    var getNewPICData = getPICDataRawList
                                            .Where(x => newTrDocumentReqPIC.Select(y => y.IdBinusian).Any(y => y == x.Id))
                                            .ToList();
                    tempdocumentChangesData.NewBinusianPICList = getNewPICData;


                    var tempChangesAdditionalFieldDataList = new List<UpdateDocumentRequestStaffResult_ChangeDetail_FormAnswer>();

                    foreach (var additionalField in paramDocumentRequestList.AdditionalFieldsList)
                    {
                        var tempChangesAdditionalFieldData = new UpdateDocumentRequestStaffResult_ChangeDetail_FormAnswer();
                        tempChangesAdditionalFieldData.IdDocumentReqFormFieldAnswered = additionalField.IdDocumentReqFormFieldAnswered;

                        var getOldTrDocumentReqApplicantFormAnswerList = getTrDocumentReqApplicantFormAnswerRawList
                                                                        .Where(x => x.IdDocumentReqApplicantDetail == paramDocumentRequestList.IdDocumentReqApplicantDetail &&
                                                                                    x.IdDocumentReqFormFieldAnswered == additionalField.IdDocumentReqFormFieldAnswered)
                                                                        .ToList();

                        var getOldTextValueList = getOldTrDocumentReqApplicantFormAnswerList
                                                    .Select(x => x.TextValue)
                                                    .ToList();
                        tempChangesAdditionalFieldData.OldTextValueList = getOldTextValueList;

                        // remove old TrDocumentReqApplicantFormAnswer
                        _dbContext.Entity<TrDocumentReqApplicantFormAnswer>().RemoveRange(getOldTrDocumentReqApplicantFormAnswerList);

                        var getDocumentReqFormFieldAnswered = getMsDocumentReqFormFieldAnsweredRawList
                                                                .Where(x => x.Id == additionalField.IdDocumentReqFormFieldAnswered)
                                                                .FirstOrDefault();

                        if (getDocumentReqFormFieldAnswered == null)
                            throw new BadRequestException("Failed to save. Additional field does not exist. Please refresh the page or contact administrator");

                        // Update TrDocumentReqApplicantFormAnswer
                        // has option (ddl, checkbox)
                        if (getDocumentReqFormFieldAnswered.DocumentReqFieldType.HasOption)
                        {
                            if ((additionalField.IdDocumentReqOptionList == null || !additionalField.IdDocumentReqOptionList.Any()) && getDocumentReqFormFieldAnswered.IsRequired)
                                throw new BadRequestException("Please fill all of the required fields");

                            var getMsDocumentReqOptionList = getMsDocumentReqOptionRaw
                                                            .Where(x => additionalField.IdDocumentReqOptionList.Any(y => y == x.Id))
                                                            .ToList();

                            // add new TrDocumentReqApplicantFormAnswer
                            var newTrDocumentReqApplicantFormAnswerList = (additionalField.IdDocumentReqOptionList == null || additionalField.IdDocumentReqOptionList.Count() <= 0) ?
                            // condition if no answer and not required to answer
                            new List<TrDocumentReqApplicantFormAnswer>
                            {
                                new TrDocumentReqApplicantFormAnswer
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdDocumentReqApplicantDetail = paramDocumentRequestList.IdDocumentReqApplicantDetail,
                                    IdDocumentReqFormFieldAnswered = additionalField.IdDocumentReqFormFieldAnswered,
                                    IdDocumentReqOptionCategory = getDocumentReqFormFieldAnswered.IdDocumentReqOptionCategory,
                                    IdDocumentReqOption = null,
                                    TextValue = null
                                }
                            }
                            :
                            additionalField.IdDocumentReqOptionList
                                .Select(x => new TrDocumentReqApplicantFormAnswer
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdDocumentReqApplicantDetail = paramDocumentRequestList.IdDocumentReqApplicantDetail,
                                    IdDocumentReqFormFieldAnswered = additionalField.IdDocumentReqFormFieldAnswered,
                                    IdDocumentReqOptionCategory = getDocumentReqFormFieldAnswered.IdDocumentReqOptionCategory,
                                    IdDocumentReqOption = x,
                                    TextValue = getMsDocumentReqOptionList.Where(y => y.Id == x).Select(y => y.OptionDescription).FirstOrDefault()
                                })
                                .ToList();

                            _dbContext.Entity<TrDocumentReqApplicantFormAnswer>().AddRange(newTrDocumentReqApplicantFormAnswerList);

                            var getNewTextValueList = newTrDocumentReqApplicantFormAnswerList
                                                    .Select(x => x.TextValue)
                                                    .ToList();
                            tempChangesAdditionalFieldData.NewTextValueList = getNewTextValueList;
                        }
                        // textbox
                        else
                        {
                            var newTrDocumentReqApplicantFormAnswer = new TrDocumentReqApplicantFormAnswer
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdDocumentReqApplicantDetail = paramDocumentRequestList.IdDocumentReqApplicantDetail,
                                IdDocumentReqFormFieldAnswered = additionalField.IdDocumentReqFormFieldAnswered,
                                TextValue = additionalField.TextValue
                            };

                            _dbContext.Entity<TrDocumentReqApplicantFormAnswer>().Add(newTrDocumentReqApplicantFormAnswer);

                            tempChangesAdditionalFieldData.NewTextValueList = new List<string> { newTrDocumentReqApplicantFormAnswer.TextValue };
                        }

                        tempChangesAdditionalFieldDataList.Add(tempChangesAdditionalFieldData);
                    }

                    tempdocumentChangesData.AdditionalFieldsList = tempChangesAdditionalFieldDataList;
                    documentChangesDataList.Add(tempdocumentChangesData);
                }

                changesDetail.DocumentRequestList = documentChangesDataList;
                changesData.ChangeDetail = changesDetail;

                // insert update record
                var getStaffUpdater = await _dbContext.Entity<MsUser>()
                                        .FindAsync(AuthInfo.UserId);

                changesData.UpdateDate = _dateTime.ServerTime;
                changesData.UpdatedBy = new NameValueVm
                {
                    Id = getStaffUpdater.Id,
                    Name = getStaffUpdater.DisplayName + " (Staff)"
                };

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                try
                {
                    // send email to Parent
                    await _sendEmailDataUpdateRequestToParentHandler.SendEmailDataUpdateRequestToParent(new SendEmailDataUpdateRequestToParentRequest
                    {
                        IdSchool = getDocumentReqApplicant.IdSchool,
                        IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                        IdStudent = getDocumentReqApplicant.IdStudent,
                        ChangeData = changesData
                    });
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // send email to Staff
                    await _sendEmailDataUpdateRequestToStaffHandler.SendEmailDataUpdateRequestToStaff(new SendEmailDataUpdateRequestToStaffRequest
                    {
                        IdSchool = getDocumentReqApplicant.IdSchool,
                        IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                        IdStudent = getDocumentReqApplicant.IdStudent,
                        ChangeData = changesData
                    });
                }
                catch (Exception ex)
                {

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
