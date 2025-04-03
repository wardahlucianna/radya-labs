using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateDocumentRequestOptionCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public UpdateDocumentRequestOptionCategoryHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateDocumentRequestOptionCategoryRequest, UpdateDocumentRequestOptionCategoryValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqOptionCategory = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                                                    .FindAsync(param.IdDocumentReqOptionCategory);

                if (getDocumentReqOptionCategory == null)
                    throw new BadRequestException("Cannot find the option category");

                // validate if same category name is already exists
                var isExistsCategory = _dbContext.Entity<MsDocumentReqOptionCategory>()
                                        .Where(x => x.IdDocumentReqFieldType == getDocumentReqOptionCategory.IdDocumentReqFieldType &&
                                                    x.IdSchool == getDocumentReqOptionCategory.IdSchool &&
                                                    (getDocumentReqOptionCategory.CategoryDescription.ToUpper() != param.CategoryDescription.ToUpper() && x.CategoryDescription.ToUpper() == param.CategoryDescription.ToUpper()))
                                        .Any();

                if (isExistsCategory)
                    throw new BadRequestException("Cannot insert the category. Category name already exists.");

                // cannot change category description when IsDefaultImportData
                if (getDocumentReqOptionCategory.IsDefaultImportData && param.CategoryDescription != getDocumentReqOptionCategory.CategoryDescription)
                    throw new BadRequestException("Cannot change category description for category that using imported data");

                // update category description
                if (param.CategoryDescription != getDocumentReqOptionCategory.CategoryDescription)
                {
                    getDocumentReqOptionCategory.CategoryDescription = param.CategoryDescription;

                    _dbContext.Entity<MsDocumentReqOptionCategory>().Update(getDocumentReqOptionCategory);
                }

                // get all option 
                var allOptionsList = await _dbContext.Entity<MsDocumentReqOption>()
                                            .Where(x => x.IdDocumentReqOptionCategory == param.IdDocumentReqOptionCategory)
                                            .ToListAsync(CancellationToken);

                // new option
                if (param.NewOptionList != null && param.NewOptionList.Any())
                {
                    foreach (var newOption in param.NewOptionList)
                    {
                        // get existing / duplicate option value
                        var duplicateOptionValue = allOptionsList
                                                    .Where(x => x.OptionDescription.Trim() == newOption.OptionDescription.Trim())
                                                    .FirstOrDefault();

                        if (duplicateOptionValue != null && duplicateOptionValue.Status == false)
                        {
                            // activate duplicate value
                            duplicateOptionValue.Status = true;
                            _dbContext.Entity<MsDocumentReqOption>().Update(duplicateOptionValue);
                        }

                        if (duplicateOptionValue == null)
                        {
                            // create new option
                            var newDocumentReqOption = new MsDocumentReqOption
                            {
                                Id = Guid.NewGuid().ToString(),
                                OptionDescription = newOption.OptionDescription,
                                Status = true,
                                IdDocumentReqOptionCategory = param.IdDocumentReqOptionCategory,
                                IsImportOption = false
                            };
                            _dbContext.Entity<MsDocumentReqOption>().Add(newDocumentReqOption);
                        }
                    }
                }

                // deleted option
                if (param.IdDocumentReqOptionDeletedList != null && param.IdDocumentReqOptionDeletedList.Any())
                {
                    foreach (var IdDocumentReqOptionDeleted in param.IdDocumentReqOptionDeletedList)
                    {
                        // get option
                        var deletedOption = allOptionsList.Where(x => x.Id == IdDocumentReqOptionDeleted).FirstOrDefault();

                        // cannot delete if there is the same option description with the newly created option
                        bool isExistsInInsertOptionDescription = param.NewOptionList == null ? false : param.NewOptionList.Select(x => x.OptionDescription).Any(x => x == deletedOption.OptionDescription);

                        if (deletedOption != null && isExistsInInsertOptionDescription == false)
                        {
                            // update status of deleted option
                            deletedOption.Status = false;
                            _dbContext.Entity<MsDocumentReqOption>().Update(deletedOption);
                        }
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
