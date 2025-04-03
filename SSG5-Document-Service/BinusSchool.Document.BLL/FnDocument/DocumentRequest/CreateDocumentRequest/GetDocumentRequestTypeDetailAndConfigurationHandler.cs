using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDocumentRequestTypeDetailAndConfigurationHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public GetDocumentRequestTypeDetailAndConfigurationHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestTypeDetailAndConfigurationRequest>(
                            nameof(GetDocumentRequestTypeDetailAndConfigurationRequest.IdDocumentReqType));

            // Get additionalFieldsList
            var additionalFieldsList = await _dbContext.Entity<MsDocumentReqFormField>()
                                                .Include(x => x.DocumentReqFieldType)
                                                .Include(x => x.DocumentReqOptionCategory)
                                                    .ThenInclude(x => x.DocumentReqOptions)
                                            .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                            .Select(x => new GetDocumentRequestTypeDetailAndConfigurationResult_FormField
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
                                                Options = x.DocumentReqOptionCategory.DocumentReqOptions
                                                            .Where(y => y.Status == true)
                                                            .Select(y => new GetDocumentRequestTypeDetailAndConfigurationResult_Option
                                                            {
                                                                IdDocumentReqOption = y.Id,
                                                                OptionDescription = y.OptionDescription
                                                            })
                                                            .OrderBy(y => y.OptionDescription)
                                                            .ToList()
                                            })
                                            .OrderBy(x => x.OrderNumber)
                                            .ToListAsync(CancellationToken);

            // Get PIC List
            var defaultPICAllTempList = new List<ItemValueVm>();

            var defaultPICIndividualList = await _dbContext.Entity<MsDocumentReqDefaultPIC>()
                                            .Include(x => x.Staff)
                                            .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                            .Select(x => new ItemValueVm
                                            {
                                                Id = x.Staff.IdBinusian,
                                                Description = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                            })
                                            .ToListAsync(CancellationToken);

            if (defaultPICIndividualList != null || defaultPICIndividualList.Any())
                defaultPICAllTempList.AddRange(defaultPICIndividualList);

            var defaultPICGroupIdBinusianRawList = await _dbContext.Entity<MsDocumentReqDefaultPICGroup>()
                                                .Include(x => x.Role)
                                                    .ThenInclude(x => x.UserRoles)
                                                .Where(x => x.IdDocumentReqType == param.IdDocumentReqType)
                                                .Select(x => x.Role.UserRoles.Select(x => x.IdUser).ToList())
                                                .ToListAsync(CancellationToken);

            var defaultPICGroupIdBinusianList = defaultPICGroupIdBinusianRawList
                                                    .SelectMany(x => x)
                                                    .ToList();

            if (defaultPICGroupIdBinusianList != null)
            {
                if (defaultPICGroupIdBinusianList.Any())
                {
                    var defaultPICGroupList = await _dbContext.Entity<MsStaff>()
                                            .Where(x => defaultPICGroupIdBinusianList.Any(y => y == x.IdBinusian))
                                            .Select(x => new ItemValueVm
                                            {
                                                Id = x.IdBinusian,
                                                Description = NameUtil.GenerateFullName(x.FirstName, x.LastName)
                                            })
                                            .ToListAsync(CancellationToken);

                    defaultPICAllTempList.AddRange(defaultPICGroupList);
                }
            }

            var defaultPICAllList = defaultPICAllTempList
                                        .OrderBy(x => x.Description)
                                        .Distinct()
                                        .Select(x => new ItemValueVm
                                        {
                                            Id = x.Id,
                                            Description = $"{x.Id} - {x.Description}"
                                        })
                                        .ToList();

            var items = await _dbContext.Entity<MsDocumentReqType>()
                            .Where(x => x.Id == param.IdDocumentReqType)
                            .Select(x => new GetDocumentRequestTypeDetailAndConfigurationResult
                            {
                                DocumentRequest = new NameValueVm
                                {
                                    Id = x.Id,
                                    Name = x.Name
                                },
                                DocumentTypeDescription = x.Description,
                                DocumentHasTerm = x.DocumentHasTerm,
                                Price = x.Price,
                                HardCopyAvailable = x.HardCopyAvailable,
                                SoftCopyAvailable = x.SoftCopyAvailable,
                                IsUsingNoOfPages = x.IsUsingNoOfPages,
                                DefaultNoOfPages = !x.IsUsingNoOfPages ? null : x.DefaultNoOfPages,
                                IsUsingNoOfCopy = x.IsUsingNoOfCopy,
                                MaxNoOfCopy = !x.IsUsingNoOfCopy ? null : x.MaxNoOfCopy,
                                DefaultNoOfProcessDay = x.DefaultNoOfProcessDay,
                                AdditionalFieldsList = additionalFieldsList,
                                PICList = defaultPICAllList
                            })
                            .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}
