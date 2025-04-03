using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateDocumentRequestTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly UpdateImportedDataOptionCategoryHandler _updateImportedDataOptionCategoryHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;
        private IDbContextTransaction _transaction;

        public UpdateDocumentRequestTypeHandler(
            IDocumentDbContext dbContext,
            UpdateImportedDataOptionCategoryHandler updateImportedDataOptionCategoryHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _updateImportedDataOptionCategoryHandler = updateImportedDataOptionCategoryHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateDocumentRequestTypeRequest, UpdateDocumentRequestTypeValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                // validate AcademicYearStart and AcademicYearEnd
                var academicYearRaw = await _dbContext.Entity<MsAcademicYear>()
                                        .Where(x => (string.IsNullOrEmpty(param.IdAcademicYearStart) ? false : x.Id == param.IdAcademicYearStart) ||
                                                     (string.IsNullOrEmpty(param.IdAcademicYearEnd) ? false : x.Id == param.IdAcademicYearEnd))
                                        .ToListAsync(CancellationToken);

                int startCodeAY = string.IsNullOrEmpty(param.IdAcademicYearStart) ? -1 : academicYearRaw.Where(x => x.Id == param.IdAcademicYearStart).Select(x => int.Parse(x.Code)).FirstOrDefault();
                int endCodeAY = string.IsNullOrEmpty(param.IdAcademicYearEnd) ? -1 : academicYearRaw.Where(x => x.Id == param.IdAcademicYearEnd).Select(x => int.Parse(x.Code)).FirstOrDefault();

                if (startCodeAY != -1 && endCodeAY != -1 && startCodeAY > endCodeAY)
                    throw new BadRequestException("End academic year must be greater than start academic year");

                // get DocumentReqType
                var getDocumentReqType = await _dbContext.Entity<MsDocumentReqType>()
                                            .FindAsync(param.IdDocumentReqType);

                if(getDocumentReqType == null)
                    throw new BadRequestException("Cannot find document request type");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqType.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                // Update MsDocumentReqType
                getDocumentReqType.Name = param.DocumentName;
                getDocumentReqType.Description = param.DocumentDescription;
                getDocumentReqType.Status = param.ActiveStatus;
                getDocumentReqType.VisibleToParent = param.VisibleToParent;
                getDocumentReqType.IdAcademicYearStart = param.IdAcademicYearStart;
                getDocumentReqType.IdAcademicYearEnd = param.IdAcademicYearEnd;
                getDocumentReqType.IsAcademicDocument = param.IsAcademicDocument;
                getDocumentReqType.DocumentHasTerm = param.HasTermOptions;
                getDocumentReqType.Price = param.Price;
                getDocumentReqType.InvoiceDueHoursPayment = param.InvoicePaymentExpiredHours;
                getDocumentReqType.DefaultNoOfProcessDay = param.DefaultProcessDays;
                getDocumentReqType.IsUsingNoOfCopy = param.IsUsingNoOfCopy;
                getDocumentReqType.MaxNoOfCopy = param.MaxNoOfCopy;
                getDocumentReqType.IsUsingNoOfPages = param.IsUsingNoOfPages;
                getDocumentReqType.DefaultNoOfPages = param.DefaultNoOfPages;
                getDocumentReqType.ParentNeedApproval = param.ParentNeedApproval;
                getDocumentReqType.HardCopyAvailable = param.HardCopyAvailable;
                getDocumentReqType.SoftCopyAvailable = param.SoftCopyAvailable;
                getDocumentReqType.IsUsingGradeMapping = param.IsAcademicDocument ? true : false;

                _dbContext.Entity<MsDocumentReqType>().Update(getDocumentReqType);

                // Add MsDocumentReqTypeGradeMapping
                var getExistingGradeMapping = await _dbContext.Entity<MsDocumentReqTypeGradeMapping>()
                                                        .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                        .ToListAsync(CancellationToken);

                if (getDocumentReqType.IsUsingGradeMapping)
                {
                    // remove existing grade mapping
                    _dbContext.Entity<MsDocumentReqTypeGradeMapping>().RemoveRange(getExistingGradeMapping);

                    // insert grade mapping
                    var insertDocumentReqTypeGradeMappingList = param.CodeGrades
                                                            .Distinct()
                                                            .Select(x => new MsDocumentReqTypeGradeMapping
                                                            {
                                                                Id = Guid.NewGuid().ToString(),
                                                                IdDocumentReqType = param.IdDocumentReqType,
                                                                CodeGrade = x
                                                            })
                                                            .ToList();

                    _dbContext.Entity<MsDocumentReqTypeGradeMapping>().AddRange(insertDocumentReqTypeGradeMappingList);
                }
                else
                {
                    // remove existing grade mapping
                    _dbContext.Entity<MsDocumentReqTypeGradeMapping>().RemoveRange(getExistingGradeMapping);
                }

                // remove existing PIC Individual
                var getExistingPICIndividual = await _dbContext.Entity<MsDocumentReqDefaultPIC>()
                                                    .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                    .ToListAsync(CancellationToken);

                _dbContext.Entity<MsDocumentReqDefaultPIC>().RemoveRange(getExistingPICIndividual);
                
                // Add Default PIC Individual
                if (param.IdBinusianDefaultPICIndividuals != null && param.IdBinusianDefaultPICIndividuals.Any())
                {
                    var insertDocumentReqDefaultPIC = param.IdBinusianDefaultPICIndividuals
                                                    .Select(x => new MsDocumentReqDefaultPIC
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        IdDocumentReqType = param.IdDocumentReqType,
                                                        IdBinusian = x
                                                    })
                                                    .ToList();

                    _dbContext.Entity<MsDocumentReqDefaultPIC>().AddRange(insertDocumentReqDefaultPIC);
                }

                // remove existing PIC Group
                var getExistingPICGroup = await _dbContext.Entity<MsDocumentReqDefaultPICGroup>()
                                                    .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                    .ToListAsync(CancellationToken);

                _dbContext.Entity<MsDocumentReqDefaultPICGroup>().RemoveRange(getExistingPICGroup);


                if (param.IdRoleDefaultPICGroups != null && param.IdRoleDefaultPICGroups.Any())
                {
                    // Add Default PIC Group
                    var insertDocumentReqDefaultPICGroup = param.IdRoleDefaultPICGroups
                                                        .Select(x => new MsDocumentReqDefaultPICGroup
                                                        {
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdDocumentReqType = param.IdDocumentReqType,
                                                            IdRole = x
                                                        })
                                                        .ToList();

                    _dbContext.Entity<MsDocumentReqDefaultPICGroup>().AddRange(insertDocumentReqDefaultPICGroup);
                }

                // Additional Fields
                var getDocumentReqFieldTypeRaw = await _dbContext.Entity<LtDocumentReqFieldType>()
                                                .Where(x => param.AdditionalFields.Select(y => y.IdDocumentReqFieldType).Any(y => y == x.Id))
                                                .ToListAsync(CancellationToken);

                var getOptionCategoryRaw = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                                                .Where(x => param.AdditionalFields.Select(y => y.IdDocumentReqOptionCategory).Any(y => y == x.Id))
                                                .ToListAsync(CancellationToken);


                // remove existing Additional Fields
                var getExistingAdditionalField = await _dbContext.Entity<MsDocumentReqFormField>()
                                                    .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                    .ToListAsync(CancellationToken);

                _dbContext.Entity<MsDocumentReqFormField>().RemoveRange(getExistingAdditionalField);

                // Add MsDocumentReqFormField
                var insertMsDocumentReqFormField = param.AdditionalFields
                                                        .Select(x => new MsDocumentReqFormField
                                                        {
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdDocumentReqType = param.IdDocumentReqType,
                                                            IdDocumentReqFieldType = x.IdDocumentReqFieldType,
                                                            QuestionDescription = x.QuestionDescription,
                                                            OrderNumber = x.OrderNumber,
                                                            IsRequired = x.IsRequired,
                                                            IdDocumentReqOptionCategory = getDocumentReqFieldTypeRaw.Where(y => y.Id == x.IdDocumentReqFieldType).Select(x => x.HasOption).FirstOrDefault() ? x.IdDocumentReqOptionCategory : null
                                                        })
                                                        .ToList();

                _dbContext.Entity<MsDocumentReqFormField>().AddRange(insertMsDocumentReqFormField);

                // import data handler
                foreach (var additionalField in param.AdditionalFields)
                {
                    var getImportedDataOptionCategory = getOptionCategoryRaw
                                                        .Where(x => x.Id == additionalField.IdDocumentReqOptionCategory &&
                                                                    x.IsDefaultImportData == true)
                                                        .FirstOrDefault();

                    if (getImportedDataOptionCategory != null)
                    {
                        await _updateImportedDataOptionCategoryHandler.UpdateImportedDataOptionCategory(new UpdateImportedDataOptionCategoryRequest
                        {
                            IdDocumentReqOptionCategory = additionalField.IdDocumentReqOptionCategory
                        });
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

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
