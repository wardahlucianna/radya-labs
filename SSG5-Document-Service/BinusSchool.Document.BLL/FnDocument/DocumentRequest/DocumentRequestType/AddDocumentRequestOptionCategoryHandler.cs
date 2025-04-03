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
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class AddDocumentRequestOptionCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public AddDocumentRequestOptionCategoryHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddDocumentRequestOptionCategoryRequest, AddDocumentRequestOptionCategoryValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                // validate if same category name is already exists
                var isExistsCategory = _dbContext.Entity<MsDocumentReqOptionCategory>()
                                        .Where(x => x.IdDocumentReqFieldType == param.IdDocumentReqFieldType &&
                                                    x.CategoryDescription.ToUpper() == param.CategoryDescription.ToUpper() &&
                                                    x.IdSchool == param.IdSchool)
                                        .Any();

                if (isExistsCategory)
                    throw new BadRequestException("Cannot insert the category. Category name already exists.");

                var newIdDocumentReqOptionCategory = Guid.NewGuid().ToString();

                var insertDocumentReqOptionCategory = new MsDocumentReqOptionCategory
                {
                    Id = newIdDocumentReqOptionCategory,
                    CategoryDescription = param.CategoryDescription,
                    IdDocumentReqFieldType = param.IdDocumentReqFieldType,
                    IdSchool = param.IdSchool,
                    IsDefaultImportData = false
                };
                _dbContext.Entity<MsDocumentReqOptionCategory>().Add(insertDocumentReqOptionCategory);

                var insertDocumentReqOption = param.OptionList
                                                .Select(x => new MsDocumentReqOption
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    OptionDescription = x.OptionDescription,
                                                    IdDocumentReqOptionCategory = newIdDocumentReqOptionCategory,
                                                    IsImportOption = false,
                                                    Status = true
                                                })
                                                .ToList();

                _dbContext.Entity<MsDocumentReqOption>().AddRange(insertDocumentReqOption);

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
