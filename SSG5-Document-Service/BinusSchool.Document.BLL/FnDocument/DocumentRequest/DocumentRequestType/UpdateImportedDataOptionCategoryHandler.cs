using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateImportedDataOptionCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ICountry _countryApi;

        public UpdateImportedDataOptionCategoryHandler(
            IDocumentDbContext dbContext,
            ICountry countryApi)
        {
            _dbContext = dbContext;
            _countryApi = countryApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateImportedDataOptionCategoryRequest, UpdateImportedDataOptionCategoryValidator>();

            var result = await UpdateImportedDataOptionCategory(new UpdateImportedDataOptionCategoryRequest
            {
                IdDocumentReqOptionCategory = param.IdDocumentReqOptionCategory
            });

            return Request.CreateApiResult2();
        }

        public async Task<bool> UpdateImportedDataOptionCategory(UpdateImportedDataOptionCategoryRequest param)
        {
            var getOptionCategory = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                                        .Include(x => x.DocumentReqOptions)
                                        .Where(x => x.Id == param.IdDocumentReqOptionCategory)
                                        .Select(x => new
                                        {
                                            IdDocumentReqOptionCategory = x.Id,
                                            IsDefaultImportData = x.IsDefaultImportData,
                                            Code = x.Code,
                                            ExistingImportedOptions = x.DocumentReqOptions.Where(x => x.IsImportOption).ToList()
                                        })
                                        .FirstOrDefaultAsync(CancellationToken);

            if (getOptionCategory.IsDefaultImportData)
            {
                #region Country Data
                if (getOptionCategory.Code?.ToUpper() == "COUNTRY")
                {
                    var getCountryApiResult = await _countryApi.GetCountries(new CollectionRequest
                    {
                        GetAll = true
                    });


                    if(getCountryApiResult.Payload != null)
                    {
                        var getCountryList = getCountryApiResult.Payload.ToList();
                        var existingCountryDescription = getOptionCategory.ExistingImportedOptions.Select(x => x.OptionDescription).ToList();


                        var countryNotYetMapped = getCountryList
                                                    .Where(x => existingCountryDescription.All(y => y != x.Description))
                                                    .ToList();

                        var insertNewCountry = countryNotYetMapped
                                                .Select(x => new MsDocumentReqOption
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    OptionDescription = x.Description,
                                                    Status = true,
                                                    IdDocumentReqOptionCategory = param.IdDocumentReqOptionCategory,
                                                    IsImportOption = true
                                                })
                                                .ToList();

                        _dbContext.Entity<MsDocumentReqOption>().AddRange(insertNewCountry);
                        await _dbContext.SaveChangesAsync(CancellationToken);
                    }
                }
                #endregion
            }

            return true;
        }
    }
}
