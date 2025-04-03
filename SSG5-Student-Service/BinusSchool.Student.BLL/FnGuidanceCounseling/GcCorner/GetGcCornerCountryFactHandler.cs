using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerCountryFactHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetGcCornerCountryFactHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcCornerCountryFactRequest>();

            string[] _columns = { "CountryName", "CountryDescription", "CountryWebsite", "ContactPerson" };

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "CountryName" },
                { _columns[1], "Description" },
                { _columns[2], "Website" },
                { _columns[3], "ContactPerson" }
            };

            var query = _dbContext.Entity<MsCountryFact>()
                        .Include(e => e.CountryFactSheet)
                        .Include(e => e.CountryFactLogo)
                        .OrderByDescending(x => x.DateIn)
                        .OrderByDynamic(param, aliasColumns)
                        .Select(x => new GetGcCornerCountryFactResult
                        {
                            Id = x.Id,
                            CountryName = x.Name,
                            Description = x.Description,
                            CountryWebsite = x.Website,
                            ContactPerson = x.ContactPerson,
                            FactSheet = x.CountryFactSheet.Select(e => new FactSheetUnivInformationManagementCountryFact
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),
                            Logo = x.CountryFactLogo.Select(e => new LogoUnivInformationManagementCountryFact
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList()
                        });


            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.CountryName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            ////ordering
            //switch (param.OrderBy)
            //{
            //    case "CountryName":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.CountryName)
            //            : query.OrderBy(x => x.CountryName);
            //        break;
            //    case "CountryDescription":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Description)
            //            : query.OrderBy(x => x.Description);
            //        break;
            //    case "CountryWebsite":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.CountryWebsite)
            //            : query.OrderBy(x => x.CountryWebsite);
            //        break;
            //    case "ContactPerson":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.ContactPerson)
            //            : query.OrderBy(x => x.ContactPerson);
            //        break;
            //};

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerCountryFactResult
                    {
                        Id = x.Id,
                        CountryName = x.CountryName,
                        Description = x.Description,
                        CountryWebsite = x.CountryWebsite,
                        ContactPerson = x.ContactPerson,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerCountryFactResult
                    {
                        Id = x.Id,
                        CountryName = x.CountryName,
                        Description = x.Description,
                        CountryWebsite = x.CountryWebsite,
                        ContactPerson = x.ContactPerson,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo
                    }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
