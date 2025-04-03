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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetSoftCopyDocumentRequestListHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;

        public GetSoftCopyDocumentRequestListHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSoftCopyDocumentRequestListRequest>();

            var softCopyDocumentHelper = new SoftCopyDocumentBlobHelper(_dateTime, _configuration);

            var getDocumentReqApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                    .Where(x => x.Id == param.IdDocumentReqApplicant)
                                                    .FirstOrDefaultAsync(CancellationToken);

            if (getDocumentReqApplicant == null)
                throw new BadRequestException("Document request applicant is not found");

            var getAcademicYearDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.AcademicYear)
                                                            .Where(x => x.IdDocumentReqApplicant == getDocumentReqApplicant.Id &&
                                                                        !string.IsNullOrEmpty(x.IdAcademicYearDocument))
                                                            .Select(x => new ItemValueVm
                                                            {
                                                                Id = x.IdAcademicYearDocument,
                                                                Description = x.AcademicYear.Description
                                                            })
                                                            .ToListAsync(CancellationToken);

            var getGradeDocumentRawList = await _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                    .ThenInclude(x => x.Level)
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                                .Where(x => x.IdStudent == getDocumentReqApplicant.IdStudent &&
                                                            getAcademicYearDocumentRawList.Select(y => y.Id).Any(y => y == x.Homeroom.Grade.Level.IdAcademicYear))
                                                .Select(x => new
                                                {
                                                    IdStudent = x.IdStudent,
                                                    IdAcademicYear = x.Homeroom.Grade.Level.IdAcademicYear,
                                                    Grade = new ItemValueVm
                                                    {
                                                        Id = x.Homeroom.IdGrade,
                                                        Description = x.Homeroom.Grade.Description
                                                    },
                                                    HomeroomName = x.Homeroom.Grade.Description + " " + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var getPeriodDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.Period)
                                                            .Where(x => x.IdDocumentReqApplicant == getDocumentReqApplicant.Id &&
                                                                        !string.IsNullOrEmpty(x.IdPeriodDocument))
                                                            .Select(x => new ItemValueVm
                                                            {
                                                                Id = x.IdPeriodDocument,
                                                                Description = x.Period.Description
                                                            })
                                                            .ToListAsync(CancellationToken);

            var result = _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                .Include(x => x.DocumentReqType)
                                .Include(x => x.DocumentReqApplicant)
                                .Include(x => x.DocumentReqAttachments)
                                    .ThenInclude(x => x.User)
                                .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                .ToList()
                                .Select(x => new GetSoftCopyDocumentRequestListResult
                                {
                                    IdDocumentReqApplicantDetail = x.Id,
                                    DocumentTypeName = x.DocumentReqType.Name,
                                    AcademicYearDocument = string.IsNullOrEmpty(x.IdAcademicYearDocument) ? null :
                                                            getAcademicYearDocumentRawList
                                                            .Where(y => y.Id == x.IdAcademicYearDocument)
                                                            .FirstOrDefault(),
                                    GradeDocument = string.IsNullOrEmpty(x.IdAcademicYearDocument) ? null :
                                                        getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.Grade)
                                                            .FirstOrDefault(),
                                    HomeroomNameDocument = string.IsNullOrEmpty(x.IdAcademicYearDocument) ? null :
                                                            getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.HomeroomName)
                                                            .FirstOrDefault(),
                                    PeriodDocument = string.IsNullOrEmpty(x.IdPeriodDocument) ? null :
                                                        getPeriodDocumentRawList
                                                            .Where(y => y.Id == x.IdPeriodDocument)
                                                            .FirstOrDefault(),
                                    NeedHardCopy = x.NeedHardCopy,
                                    NeedSoftCopy = x.NeedSoftCopy,
                                    IsAttachmentShowToParent = x.DocumentReqAttachments
                                                                .Select(y => y.ShowToParent)
                                                                .Any() ?
                                                                x.DocumentReqAttachments
                                                                .Select(y => y.ShowToParent)
                                                                .FirstOrDefault() :
                                                                x.NeedSoftCopy,
                                    AttachmentUrl = x.DocumentReqAttachments != null &&
                                                    x.DocumentReqAttachments
                                                    .Select(y => y.FileName)
                                                    .Any() ?
                                                    softCopyDocumentHelper.GetDocumentLink(x.DocumentReqAttachments.Select(y => y.FileName).FirstOrDefault(), x.DocumentReqApplicant.IdStudent, 3, null)
                                                    : null,
                                    LastUpdateTime = x.DocumentReqAttachments
                                                    .Select(y => y.DateUp == null ? y.DateIn : y.DateUp)
                                                    .FirstOrDefault(),
                                    LastUpdateBy = x.DocumentReqAttachments
                                                    .Select(y => new NameValueVm
                                                    {
                                                        Id = y.IdUserModifier,
                                                        Name = y.User.DisplayName
                                                    })
                                                    .FirstOrDefault(),
                                    CanDeleteAttachment =

                                        // already have attachment
                                        (x.DocumentReqAttachments != null &&
                                        x.DocumentReqAttachments
                                            .Select(y => y.FileName)
                                            .Any())
                                        && x.DocumentReqApplicant.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled
                                        && x.DocumentReqApplicant.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined

                                        // cannot delete for softcopy available document
                                        && !(x.NeedSoftCopy
                                            && (
                                                x.DocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished
                                                || x.DocumentReqApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected
                                            )
                                        ),

                                    CanEditCbShowToParent = x.NeedSoftCopy == false
                                })
                                .OrderBy(x => x.DocumentTypeName)
                                .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
