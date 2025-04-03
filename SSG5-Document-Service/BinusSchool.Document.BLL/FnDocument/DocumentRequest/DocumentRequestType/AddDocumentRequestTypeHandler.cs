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
    public class AddDocumentRequestTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly UpdateImportedDataOptionCategoryHandler _updateImportedDataOptionCategoryHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;
        private IDbContextTransaction _transaction;

        public AddDocumentRequestTypeHandler(
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
            var param = await Request.ValidateBody<AddDocumentRequestTypeRequest, AddDocumentRequestTypeValidator>();
            
            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = param.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                // validate AcademicYearStart and AcademicYearEnd
                var academicYearRaw = await _dbContext.Entity<MsAcademicYear>()
                                        .Where(x => (string.IsNullOrEmpty(param.IdAcademicYearStart) ? false : x.Id == param.IdAcademicYearStart) ||
                                                     (string.IsNullOrEmpty(param.IdAcademicYearEnd) ? false : x.Id == param.IdAcademicYearEnd))
                                        .ToListAsync(CancellationToken);

                int startCodeAY = string.IsNullOrEmpty(param.IdAcademicYearStart) ? -1 : academicYearRaw.Where(x => x.Id == param.IdAcademicYearStart).Select(x => int.Parse(x.Code)).FirstOrDefault();
                int endCodeAY = string.IsNullOrEmpty(param.IdAcademicYearEnd) ? -1 : academicYearRaw.Where(x => x.Id == param.IdAcademicYearEnd).Select(x => int.Parse(x.Code)).FirstOrDefault();

                if (startCodeAY != -1 && endCodeAY != -1 && startCodeAY > endCodeAY)
                    throw new BadRequestException("End academic year must be greater or equal to start academic year");


                var newIdDocumentReqType = Guid.NewGuid().ToString();

                // Add MsDocumentReqType
                var insertDocumentReqType = new MsDocumentReqType()
                {
                    Id = newIdDocumentReqType,
                    IdSchool = param.IdSchool,
                    Name = param.DocumentName,
                    Description = param.DocumentDescription,
                    Status = param.ActiveStatus,
                    VisibleToParent = param.VisibleToParent,
                    IdAcademicYearStart = param.IdAcademicYearStart,
                    IdAcademicYearEnd = param.IdAcademicYearEnd,
                    IsAcademicDocument = param.IsAcademicDocument,
                    DocumentHasTerm = param.HasTermOptions,
                    Price = param.Price,
                    InvoiceDueHoursPayment = param.InvoicePaymentExpiredHours,
                    DefaultNoOfProcessDay = param.DefaultProcessDays,
                    IsUsingNoOfCopy = param.IsUsingNoOfCopy,
                    MaxNoOfCopy = param.MaxNoOfCopy,
                    IsUsingNoOfPages = param.IsUsingNoOfPages,
                    DefaultNoOfPages = param.DefaultNoOfPages,
                    ParentNeedApproval = param.ParentNeedApproval,
                    HardCopyAvailable = param.HardCopyAvailable,
                    SoftCopyAvailable = param.SoftCopyAvailable,
                    IsUsingGradeMapping = param.IsAcademicDocument ? true : false
                };

                _dbContext.Entity<MsDocumentReqType>().Add(insertDocumentReqType);

                // Add MsDocumentReqTypeGradeMapping
                if (insertDocumentReqType.IsUsingGradeMapping)
                {
                    var insertDocumentReqTypeGradeMappingList = param.CodeGrades
                                                            .Distinct()
                                                            .Select(x => new MsDocumentReqTypeGradeMapping
                                                            {
                                                                Id = Guid.NewGuid().ToString(),
                                                                IdDocumentReqType = newIdDocumentReqType,
                                                                CodeGrade = x
                                                            })
                                                            .ToList();

                    _dbContext.Entity<MsDocumentReqTypeGradeMapping>().AddRange(insertDocumentReqTypeGradeMappingList);
                }

                // Add Default PIC Individual
                if(param.IdBinusianDefaultPICIndividuals != null && param.IdBinusianDefaultPICIndividuals.Any())
                {
                    var insertDocumentReqDefaultPIC = param.IdBinusianDefaultPICIndividuals
                                                    .Distinct()
                                                    .Select(x => new MsDocumentReqDefaultPIC
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        IdDocumentReqType = newIdDocumentReqType,
                                                        IdBinusian = x
                                                    })
                                                    .ToList();

                    _dbContext.Entity<MsDocumentReqDefaultPIC>().AddRange(insertDocumentReqDefaultPIC);
                }

                if (param.IdRoleDefaultPICGroups != null && param.IdRoleDefaultPICGroups.Any())
                {
                    // Add Default PIC Group
                    var insertDocumentReqDefaultPICGroup = param.IdRoleDefaultPICGroups
                                                        .Distinct()
                                                        .Select(x => new MsDocumentReqDefaultPICGroup
                                                        {
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdDocumentReqType = newIdDocumentReqType,
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


                // Add MsDocumentReqFormField
                var insertMsDocumentReqFormField = param.AdditionalFields
                                                        .Select(x => new MsDocumentReqFormField
                                                        {
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdDocumentReqType = newIdDocumentReqType,
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

                    if(getImportedDataOptionCategory != null)
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
