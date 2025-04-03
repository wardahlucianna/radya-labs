using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetDocumentRequestTypeDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public GetDocumentRequestTypeDetailHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestTypeDetailRequest>(
                            nameof(GetDocumentRequestTypeDetailRequest.IdDocumentReqType));

            // get formFieldList
            var formFieldList = await _dbContext.Entity<MsDocumentReqFormField>()
                                                .Include(x => x.DocumentReqFieldType)
                                                .Include(x => x.DocumentReqOptionCategory)
                                                    .ThenInclude(x => x.DocumentReqFieldType)
                                            .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                            .Select(x => new GetDocumentRequestTypeDetailResult_FormField
                                            {
                                                IdDocumentReqFormField = x.Id,
                                                FieldType = new ItemValueVm
                                                {
                                                    Id = x.DocumentReqFieldType.Id,
                                                    Description = x.DocumentReqFieldType.Type
                                                },
                                                HasOption = x.DocumentReqFieldType.HasOption,
                                                QuestionDescription = x.QuestionDescription,
                                                OrderNumber = x.OrderNumber,
                                                IsRequired = x.IsRequired,
                                                OptionCategory = x.DocumentReqOptionCategory != null ? new GetDocumentRequestTypeDetailResult_OptionCategory
                                                {
                                                    IdDocumentReqOptionCategory = x.IdDocumentReqOptionCategory,
                                                    FieldType = new ItemValueVm
                                                    {
                                                        Id = x.DocumentReqOptionCategory.DocumentReqFieldType.Id,
                                                        Description = x.DocumentReqOptionCategory.DocumentReqFieldType.Type
                                                    },
                                                    CategoryDescription = x.DocumentReqOptionCategory.CategoryDescription,
                                                    IsDefaultImportData = x.DocumentReqOptionCategory.IsDefaultImportData
                                                } : null
                                            })
                                            .OrderBy(x => x.OrderNumber)
                                            .ToListAsync(CancellationToken);


            var item = await _dbContext.Entity<MsDocumentReqType>()
                            .Include(x => x.DocumentReqTypeGradeMappings)
                            .Include(x => x.DocumentReqDefaultPICs)
                                .ThenInclude(x => x.Staff)
                            .Include(x => x.DocumentReqDefaultPICGroups)
                                .ThenInclude(x => x.Role)
                            .Include(x => x.DocumentReqApplicantDetails)
                            .Include(x => x.StartAcademicYear)
                            .Include(x => x.EndAcademicYear)
                            .Where(x => x.Id == param.IdDocumentReqType)
                            .Select(x => new GetDocumentRequestTypeDetailResult
                            {
                                IdDocumentReqType = x.Id,
                                ActiveStatus = x.Status,
                                CanDelete = x.DocumentReqApplicantDetails.Any() ? false : true,
                                IdSchool = x.IdSchool,
                                DocumentName = x.Name,
                                DocumentDescription = x.Description,
                                Price = x.Price,
                                InvoiceDueHoursPayment = x.InvoiceDueHoursPayment,
                                DefaultNoOfProcessDay = x.DefaultNoOfProcessDay,
                                HardCopyAvailable = x.HardCopyAvailable,
                                SoftCopyAvailable = x.SoftCopyAvailable,
                                AcademicYearStart = string.IsNullOrEmpty(x.IdAcademicYearStart) ? null : new CodeWithIdVm
                                {
                                    Id = x.IdAcademicYearStart,
                                    Code = x.StartAcademicYear.Code,
                                    Description = x.StartAcademicYear.Description
                                },
                                AcademicYearEnd = string.IsNullOrEmpty(x.IdAcademicYearEnd) ? null : new CodeWithIdVm
                                {
                                    Id = x.IdAcademicYearEnd,
                                    Code = x.EndAcademicYear.Code,
                                    Description = x.EndAcademicYear.Description
                                },
                                IsAcademicDocument = x.IsAcademicDocument,
                                DocumentHasTerm = x.DocumentHasTerm,
                                IsUsingNoOfPages = x.IsUsingNoOfPages,
                                DefaultNoOfPages = x.DefaultNoOfPages,
                                IsUsingNoOfCopy = x.IsUsingNoOfCopy,
                                MaxNoOfCopy = x.MaxNoOfCopy,
                                VisibleToParent = x.VisibleToParent,
                                ParentNeedApproval = x.ParentNeedApproval,
                                IsUsingGradeMapping = x.IsUsingGradeMapping,
                                CodeGrades = !x.IsUsingGradeMapping ? null : x.DocumentReqTypeGradeMappings.Select(x => x.CodeGrade).OrderBy(y => y.Length).ThenBy(y => y).ToList(),
                                PICIndividualList = x.DocumentReqDefaultPICs.Select(x => new NameValueVm
                                {
                                    Id = x.IdBinusian,
                                    Name = x.Staff.FirstName + (string.IsNullOrWhiteSpace(x.Staff.LastName) ? "" : " " + x.Staff.LastName)
                                }).OrderBy(x => x.Name).ToList(),
                                PICGroupList = x.DocumentReqDefaultPICGroups.Select(x => new ItemValueVm
                                {
                                    Id = x.IdRole,
                                    Description = x.Role.Description
                                }).OrderBy(x => x.Description).ToList(),
                                FormFieldList = formFieldList
                            })
                        .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }
    }
}
