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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class GetDocumentRequestHistoryByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;

        public GetDocumentRequestHistoryByStudentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            IConfiguration configuration,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestHistoryByStudentRequest>(
                            nameof(GetDocumentRequestHistoryByStudentRequest.IdParent));


            var listIdStudent = new List<string>();
            if (string.IsNullOrEmpty(param.IdStudent))
            {
                #region Get all children if idstudent is null
                var username = await _dbContext.Entity<MsUser>()
                               .Where(x => x.Id == param.IdParent)
                               .Select(x => x.Username)
                               .FirstOrDefaultAsync(CancellationToken);

                if (username == null)
                    throw new BadRequestException("User is not found");

                var idStudent = string.Concat(username.Where(char.IsDigit));

                var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                        .Where(x => x.IdStudent == idStudent)
                                        .Select(x => new
                                        {
                                            idParent = x.IdParent
                                        }).FirstOrDefaultAsync(CancellationToken);

                var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                                    .Where(x => x.IdStudent == idStudent)
                                    .Select(x => x.Id)
                                    .FirstOrDefaultAsync(CancellationToken);

                if (sibligGroup != null)
                {
                    var siblingStudent = await _dbContext.Entity<MsSiblingGroup>()
                                            .Where(x => x.Id == sibligGroup)
                                            .Select(x => x.IdStudent)
                                            .ToListAsync(CancellationToken);

                    listIdStudent = await _dbContext.Entity<MsStudent>()
                                    .Where(x => siblingStudent.Any(y => y == x.Id))
                                    .Select(x => x.Id)
                                    .ToListAsync(CancellationToken);
                }
                else if (dataStudentParent != null)
                {
                    listIdStudent = await _dbContext.Entity<MsStudentParent>()
                                    .Include(x => x.Student)
                                    .Where(x => x.IdParent == dataStudentParent.idParent)
                                    .Select(x => x.Student.Id)
                                    .ToListAsync(CancellationToken);
                }
                #endregion
            }
            else
            {
                // get student info
                var student = await _dbContext.Entity<MsStudent>()
                                .Where(x => x.Id == param.IdStudent)
                                .Select(x => x.Id)
                                .FirstOrDefaultAsync(CancellationToken);

                listIdStudent.Add(student);
            }


            // get DocumentReqApplicant
            var documentReqApplicantList = await _dbContext.Entity<MsDocumentReqApplicant>()
                                        .Include(x => x.Student)
                                        .Include(x => x.DocumentReqApplicantDetails)
                                            .ThenInclude(x => x.DocumentReqType)
                                        .Include(x => x.DocumentReqApplicantDetails)
                                            .ThenInclude(x => x.DocumentReqAttachments)
                                        .Include(x => x.DocumentReqStatusTrackingHistories)
                                            .ThenInclude(x => x.DocumentReqStatusWorkflow)
                                        .Include(x => x.DocumentReqPaymentMappings)
                                        .Where(x => listIdStudent.Any(y => y == x.IdStudent) &&
                                                    (param.RequestYear == null ? true : x.RequestDate.Year == param.RequestYear) &&
                                                    (param.IdDocumentReqStatusWorkflow == null ? true : param.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ? (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess || x.DocumentReqStatusTrackingHistories.OrderByDescending(y => y.StatusDate).FirstOrDefault().IsOnProcess == true) : x.IdDocumentReqStatusWorkflow == param.IdDocumentReqStatusWorkflow)
                                                    )
                                        .Select(x => new
                                        {
                                            IdDocumentReqApplicant = x.Id,
                                            RequestNumber = x.RequestNumber,
                                            RequestDate = x.RequestDate,
                                            Student = new NameValueVm
                                            {
                                                Id = x.IdStudent,
                                                Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                                            },
                                            DocumentList = x.DocumentReqApplicantDetails
                                                            .Select(y => new 
                                                            {
                                                                Id = y.Id,
                                                                Description = y.DocumentReqType.Name,
                                                                PriceInvoice = y.PriceInvoice,
                                                                IdAcademicYearDocument = y.IdAcademicYearDocument,
                                                                //DocumentIsReady = y.ReceivedDateByStaff == null ? false : true,
                                                                AttachmentFileName = y.DocumentReqAttachments
                                                                                        .Where(z => z.ShowToParent)
                                                                                        .Select(z => z.FileName)
                                                                                        .FirstOrDefault()
                                                            })
                                                            .OrderBy(y => y.Description)
                                                            .ToList(),
                                            LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                                            .Select(y => new GetDocumentRequestHistoryByStudentResult_StatusWorkflow
                                                            {
                                                                IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                                                Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                                                IsOnProcess = y.IsOnProcess,
                                                                StatusDate = y.StatusDate
                                                            })
                                                            .OrderByDescending(y => y.StatusDate)
                                                            .FirstOrDefault(),
                                            DocumentReqStatusWorkflowHistoryList = x.DocumentReqStatusTrackingHistories
                                                            .Select(y => new GetDocumentRequestHistoryByStudentResult_StatusWorkflow
                                                            {
                                                                IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                                                Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                                                StatusDate = y.StatusDate,
                                                                IsOnProcess = y.IsOnProcess,
                                                                Remarks = y.Remarks
                                                            })
                                                            .OrderByDescending(y => y.StatusDate)
                                                            .ToList(),
                                            //PaymentMapping = x.DocumentReqPaymentMappings
                                            //                .Select(y => new
                                            //                {
                                            //                    IdDocumentReqPaymentMapping = y.Id,
                                            //                    TotalAmountReal = y.TotalAmountReal,
                                            //                    IdDocumentReqPaymentManual = y.IdDocumentReqPaymentManual,
                                            //                    StartDatePayment = y.StartDatePayment,
                                            //                    EndDatePayment = y.EndDatePayment
                                            //                })
                                            //                .FirstOrDefault()
                                        })
                                        .ToListAsync(CancellationToken);

            var getAcademicYearDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.AcademicYear)
                                                            .Where(x => documentReqApplicantList.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant) &&
                                                                        !string.IsNullOrEmpty(x.IdAcademicYearDocument))
                                                            .Select(x => new
                                                            {
                                                                IdDocumentReqApplicantDetail = x.Id,
                                                                AcademicYear = new ItemValueVm
                                                                {
                                                                    Id = x.IdAcademicYearDocument,
                                                                    Description = x.AcademicYear.Description
                                                                }
                                                            })
                                                            .ToListAsync(CancellationToken);

            var getGradeDocumentRawList = await _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                    .ThenInclude(x => x.Level)
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                                .Where(x => documentReqApplicantList.Select(y => y.Student.Id).Any(y => y == x.IdStudent) &&
                                                            getAcademicYearDocumentRawList.Select(y => y.AcademicYear.Id).Any(y => y == x.Homeroom.Grade.Level.IdAcademicYear))
                                                .Select(x => new
                                                {
                                                    IdStudent = x.IdStudent,
                                                    IdAcademicYear = x.Homeroom.Grade.Level.IdAcademicYear,
                                                    Grade = new ItemValueVm
                                                    {
                                                        Id = x.Homeroom.IdGrade,
                                                        Description = x.Homeroom.Grade.Description
                                                    },
                                                    HomeroomName = x.Homeroom.Grade.Description.ToUpper().Replace("GRADE ", "") + " " + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var getPeriodDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.Period)
                                                            .Where(x => documentReqApplicantList.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant) &&
                                                                        !string.IsNullOrEmpty(x.IdPeriodDocument))
                                                            .Select(x => new
                                                            {
                                                                IdDocumentReqApplicantDetail = x.Id,
                                                                Period = new ItemValueVm
                                                                {
                                                                    Id = x.IdPeriodDocument,
                                                                    Description = x.Period.Description
                                                                }
                                                            })
                                                            .ToListAsync(CancellationToken);

            // collection info
            var collectionInfoRawList = await _dbContext.Entity<MsDocumentReqApplicantCollection>()
                                            .Where(x => documentReqApplicantList.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant))
                                            .Select(x => new
                                            {
                                                IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                                FinishDate = x.FinishDate,
                                                ScheduleCollectionDateStart = x.ScheduleCollectionDateStart ?? null,
                                                ScheduleCollectionDateEnd = x.ScheduleCollectionDateEnd ?? null,
                                                IdVenue = x.IdVenue ?? null,
                                                CollectedBy = x.CollectedBy ?? null,
                                                CollectedDate = x.CollectedDate ?? null
                                            })
                                            .ToListAsync(CancellationToken);

            var getVenueRawList = await _dbContext.Entity<MsVenue>()
                                    .Where(x => collectionInfoRawList.Select(y => y.IdVenue).Any(y => y == x.Id))
                                    .ToListAsync(CancellationToken);

            var collectionInfoRawFinalList = collectionInfoRawList
                                                .Select(x => new
                                                {
                                                    IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                                    FinishDate = x.FinishDate,
                                                    ScheduleCollectionDateStart = x.ScheduleCollectionDateStart,
                                                    ScheduleCollectionDateEnd = x.ScheduleCollectionDateEnd,
                                                    ScheduleCollectionDateText = x.ScheduleCollectionDateStart == null && x.ScheduleCollectionDateEnd == null ? "Anytime" : (x.ScheduleCollectionDateStart.Value.ToString("dd MMMM yyyy, HH:mm:ss") + " - " + x.ScheduleCollectionDateEnd.Value.ToString("dd MMMM yyyy, HH:mm:ss")),
                                                    Venue = string.IsNullOrEmpty(x.IdVenue) ? null : new ItemValueVm
                                                    {
                                                        Id = x.IdVenue,
                                                        Description = getVenueRawList
                                                                        .Where(y => y.Id == x.IdVenue)
                                                                        .Select(y => y.Description)
                                                                        .FirstOrDefault()
                                                    },
                                                    CollectedBy = x.CollectedBy,
                                                    CollectedDate = x.CollectedDate
                                                })
                                                .ToList();

            // get payment info
            var getPaymentInfoList = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment.GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = documentReqApplicantList.Select(x => x.IdDocumentReqApplicant).ToList()
            });

            //var getDocumentPaymentManualInfo = documentReqApplicantList == null || documentReqApplicantList.Count == 0 ? null : await _dbContext.Entity<TrDocumentReqPaymentMapping>()
            //            .Include(x => x.DocumentReqPaymentManual)
            //            .Where(x => documentReqApplicantList
            //                        //.Where(y => y.PaymentMapping != null && !string.IsNullOrEmpty(y.PaymentMapping.IdDocumentReqPaymentManual))
            //                        .Select(y => y.PaymentMapping.IdDocumentReqPaymentManual)
            //                        .Any(y => y == x.IdDocumentReqPaymentManual))
            //            .ToListAsync(CancellationToken);

            var softCopyDocumentHelper = new SoftCopyDocumentBlobHelper(_dateTime, _configuration);
            #region 14 Feb 2025
            //var result = documentReqApplicantList
            //                .Select(x => new GetDocumentRequestHistoryByStudentResult
            //                {
            //                    IdDocumentReqApplicant = x.IdDocumentReqApplicant,
            //                    RequestNumber = x.RequestNumber,
            //                    RequestDate = x.RequestDate,
            //                    Student = x.Student,
            //                    DocumentList = x.DocumentList
            //                                    .Select(y => new GetDocumentRequestHistoryByStudentResult_Document
            //                                    {
            //                                        Id = y.Id,
            //                                        Description = string.IsNullOrEmpty(y.IdAcademicYearDocument) ? y.Description :
            //                                                    string.Format("{0}<br>({1}/{2}/{3})",
            //                                                        y.Description,

            //                                                        getAcademicYearDocumentRawList
            //                                                    .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
            //                                                    .Select(z => z.AcademicYear.Description)
            //                                                    .FirstOrDefault(),

            //                                                        getGradeDocumentRawList
            //                                                    .Where(z => z.IdAcademicYear == y.IdAcademicYearDocument &&
            //                                                                z.IdStudent == x.Student.Id)
            //                                                    .Select(z => z.HomeroomName)
            //                                                    .FirstOrDefault(),

            //                                                        getPeriodDocumentRawList
            //                                                        .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
            //                                                        .Any() ?
            //                                                    getPeriodDocumentRawList
            //                                                    .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
            //                                                    .Select(z => z.Period == null ? "-" : z.Period.Description)
            //                                                    .FirstOrDefault() : "-"
            //                                                        ),
            //                                        AttachmentUrl = !string.IsNullOrEmpty(y.AttachmentFileName) && (x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished || x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected) ? softCopyDocumentHelper.GetDocumentLink(y.AttachmentFileName, x.Student.Id, 3) : null
            //                                    })
            //                                    .ToList(),
            //                    TotalAmountReal = getPaymentInfoList
            //                                        .Where(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant)
            //                                        .Select(y => y.TotalAmountReal)
            //                                        .FirstOrDefault(),
            //                    PaymentStatus = new GetDocumentRequestHistoryByStudentResult_PaymentStatus
            //                    {
            //                        PaymentStatus = getPaymentInfoList
            //                                        .Where(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant)
            //                                        .Select(y => y.PaymentStatus)
            //                                        .FirstOrDefault(),

            //                        Description = getPaymentInfoList
            //                                        .Where(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant)
            //                                        .Select(y => y.PaymentStatus.GetDescription())
            //                                        .FirstOrDefault(),
            //                    },

            //                    LatestDocumentReqStatusWorkflow = x.LatestDocumentReqStatusWorkflow,
            //                    DocumentReqStatusWorkflowHistoryList = x.DocumentReqStatusWorkflowHistoryList
            //                                        .Select(y => new GetDocumentRequestHistoryByStudentResult_StatusWorkflow
            //                                        {
            //                                            IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
            //                                            Description = y.Description,
            //                                            StatusDate = y.StatusDate,
            //                                            IsOnProcess = y.IsOnProcess,
            //                                            workflowDetail = new GetDocumentRequestHistoryByStudentResult_StatusWorkflowDetail
            //                                            {
            //                                                ApprovalStatus = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected || y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished?"-": string.IsNullOrEmpty(y.Remarks) ? "-" : (y.Remarks.Contains("Notes: ") ? y.Remarks.Split("Notes: ")[0].Trim() : y.Remarks),
            //                                                ScheduleCollectionDateStart = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.ScheduleCollectionDateStart == null ? null : z.ScheduleCollectionDateStart).FirstOrDefault() : null,
            //                                                ScheduleCollectionDateEnd = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.ScheduleCollectionDateEnd == null ? null : z.ScheduleCollectionDateEnd).FirstOrDefault() : null,
            //                                                Notes = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected || y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks: string.IsNullOrEmpty(y.Remarks) ? "-" : (y.Remarks.Contains("Notes: ") ? y.Remarks.Split("Notes: ")[1] : y.Remarks),
            //                                                Venue = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.Venue == null ? "-" : z.Venue.Description).FirstOrDefault():"-",
            //                                                CollectedBy = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected ? collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.CollectedBy).FirstOrDefault():"-",
            //                                                CollectedDate = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected ? collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.CollectedDate).FirstOrDefault() : null
            //                                            },
            //                                            Remarks = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? string.Format("Collection Date Schedule: {0}<br>" +
            //                                                    "Venue: {1}<br>" +
            //                                                    "Notes: {2}",
            //                                                    collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.ScheduleCollectionDateText).FirstOrDefault(),
            //                                                    collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.Venue == null ? "-" : z.Venue.Description).FirstOrDefault(),
            //                                                    string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
            //                                                    ) :

            //                                                    y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected ? string.Format("Collected By: {0}<br>" +
            //                                                    "Collected Date: {1}<br>" +
            //                                                    "Notes: {2}",
            //                                                    collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.CollectedBy).FirstOrDefault(),
            //                                                    collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant).Select(z => z.CollectedDate == null ? "-" : z.CollectedDate.Value.ToString("dd MMMM yyyy, HH:mm:ss")).FirstOrDefault(),
            //                                                    string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
            //                                                    ) :
            //                                                    string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
            //                                        })
            //                                        .ToList(),
            //                    CanCancelRequest = (x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval || x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment || x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPaymentVerification)
            //                        && getPaymentInfoList
            //                                        .Where(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant)
            //                                        .Select(y => y.PaymentStatus)
            //                                        .FirstOrDefault() != PaymentStatus.Expired
            //                        && x.LatestDocumentReqStatusWorkflow.IsOnProcess == false,

            //                    CanConfirmPayment = x.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment && getPaymentInfoList
            //                                        .Where(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant)
            //                                        .Select(y => y.PaymentStatus)
            //                                        .FirstOrDefault() != PaymentStatus.Expired
            //                })
            //                .OrderByDescending(x => x.RequestDate)
            //                .ThenByDescending(x => x.RequestNumber)
            //                .ToList();
            #endregion

            var result = documentReqApplicantList
                        .Select(x =>
                        {
                            var latestStatus = x.LatestDocumentReqStatusWorkflow;
                            var isFinished = latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished;
                            var isCollected = latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected;

                            var collectionInfo = collectionInfoRawFinalList
                                .FirstOrDefault(z => z.IdDocumentReqApplicant == x.IdDocumentReqApplicant);

                            var paymentInfo = getPaymentInfoList
                                .FirstOrDefault(y => y.IdDocumentReqApplicant == x.IdDocumentReqApplicant);

                            return new GetDocumentRequestHistoryByStudentResult
                            {
                                IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                RequestNumber = x.RequestNumber,
                                RequestDate = x.RequestDate,
                                Student = x.Student,

                                DocumentList = x.DocumentList.Select(y =>
                                {
                                    var academicYear = getAcademicYearDocumentRawList
                                        .FirstOrDefault(z => z.IdDocumentReqApplicantDetail == y.Id)?
                                        .AcademicYear.Description ?? "-";

                                    var grade = getGradeDocumentRawList
                                        .FirstOrDefault(z => z.IdAcademicYear == y.IdAcademicYearDocument && z.IdStudent == x.Student.Id)?
                                        .HomeroomName ?? "-";

                                    var period = getPeriodDocumentRawList
                                        .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                        .Select(z => z.Period?.Description ?? "-")
                                        .FirstOrDefault() ?? "-";

                                    var documentDescription = string.IsNullOrEmpty(y.IdAcademicYearDocument)
                                        ? y.Description
                                        : $"{y.Description}<br>({academicYear}/{grade}/{period})";

                                    var hasAttachment = !string.IsNullOrEmpty(y.AttachmentFileName) &&
                                                        (isFinished || isCollected);

                                    return new GetDocumentRequestHistoryByStudentResult_Document
                                    {
                                        Id = y.Id,
                                        Description = documentDescription,
                                        AttachmentUrl = hasAttachment
                                            ? softCopyDocumentHelper.GetDocumentLink(y.AttachmentFileName, x.Student.Id, 3)
                                            : null
                                    };
                                }).ToList(),

                                TotalAmountReal = paymentInfo.TotalAmountReal,

                                PaymentStatus = new GetDocumentRequestHistoryByStudentResult_PaymentStatus
                                {
                                    PaymentStatus = paymentInfo?.PaymentStatus ?? PaymentStatus.Unpaid,
                                    Description = paymentInfo?.PaymentStatus.GetDescription() ?? PaymentStatus.Unpaid.GetDescription()
                                },

                                LatestDocumentReqStatusWorkflow = latestStatus,

                                DocumentReqStatusWorkflowHistoryList = x.DocumentReqStatusWorkflowHistoryList
                                    .Select(y =>
                                    {
                                        var isHistoryFinished = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished;
                                        var isHistoryCollected = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected;

                                        var remarks = string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks;
                                        var notesParts = y.Remarks?.Split("Notes: ");
                                        var hasNotes = notesParts?.Length > 1;

                                        var approvalStatus = isHistoryCollected || isHistoryFinished
                                            ? "-"
                                            : hasNotes ? notesParts[0].Trim() : remarks;

                                        var notes = isHistoryCollected || isHistoryFinished
                                            ? remarks
                                            : hasNotes ? notesParts[1] : remarks;

                                        var venue = isHistoryFinished ? collectionInfo?.Venue?.Description ?? "-" : "-";

                                        var collectedBy = isHistoryCollected ? collectionInfo?.CollectedBy ?? "-" : "-";

                                        var collectedDate = isHistoryCollected ? collectionInfo?.CollectedDate : null;

                                        var scheduleStart = isHistoryFinished ? collectionInfo?.ScheduleCollectionDateStart : null;
                                        var scheduleEnd = isHistoryFinished ? collectionInfo?.ScheduleCollectionDateEnd : null;

                                        var remarksText = isHistoryFinished
                                            ? $"Collection Date Schedule: {collectionInfo?.ScheduleCollectionDateText ?? "-"}<br>" +
                                                $"Venue: {venue}<br>" +
                                                $"Notes: {remarks}"
                                            : isHistoryCollected
                                                ? $"Collected By: {collectedBy}<br>" +
                                                    $"Collected Date: {(collectedDate?.ToString("dd MMMM yyyy, HH:mm:ss") ?? "-")}<br>" +
                                                    $"Notes: {remarks}"
                                                : remarks;

                                        return new GetDocumentRequestHistoryByStudentResult_StatusWorkflow
                                        {
                                            IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                            Description = y.Description,
                                            StatusDate = y.StatusDate,
                                            IsOnProcess = y.IsOnProcess,
                                            workflowDetail = new GetDocumentRequestHistoryByStudentResult_StatusWorkflowDetail
                                            {
                                                ApprovalStatus = approvalStatus,
                                                ScheduleCollectionDateStart = scheduleStart,
                                                ScheduleCollectionDateEnd = scheduleEnd,
                                                Notes = notes,
                                                Venue = venue,
                                                CollectedBy = collectedBy,
                                                CollectedDate = collectedDate
                                            },
                                            Remarks = remarksText
                                        };
                                    }).ToList(),

                                CanCancelRequest = (latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval ||
                                                    latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment ||
                                                    latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPaymentVerification)
                                                    && paymentInfo?.PaymentStatus != PaymentStatus.Expired
                                                    && !latestStatus.IsOnProcess,

                                CanConfirmPayment = latestStatus.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment
                                                    && paymentInfo?.PaymentStatus != PaymentStatus.Expired
                            };
                        })
                        .OrderByDescending(x => x.RequestDate)
                        .ThenByDescending(x => x.RequestNumber)
                        .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
