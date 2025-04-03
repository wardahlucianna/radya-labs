using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.NationalityCountry;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.NationalityCountry
{
    public class NationalityCountryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public NationalityCountryHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var query = _dbContext.Entity<MsNationalityCountry>()
                //.SearchByIds(param)
                .OrderByDynamic(param);

            IReadOnlyList<GetNationalityCountryResult> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetNationalityCountryResult {
                        IdCountry = x.IdCountry,
                        CountryName = x.CountryName,
                        CountryIDBekasi = x.CountryIDBekasi,
                        CountryIDSerpong = x.CountryIDSerpong,
                        CountryIDSimprug = x.CountryIDSimprug,
                        CountryNameBekasi = x.CountryNameBekasi,
                        CountryNameSerpong = x.CountryNameSerpong,
                        CountryNameSimprug = x.CountryNameSimprug,
                        NationalityIDBekasi = x.NationalityIDBekasi,
                        NationalityIDSerpong = x.NationalityIDSerpong,
                        NationalityIDSimprug = x.NationalityIDSimprug,
                        NationalityNameBekasi = x.NationalityNameBekasi,
                        NationalityNameSerpong = x.NationalityNameSerpong,
                        NationalityNameSimprug = x.NationalityNameSimprug
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetNationalityCountryResult
                    {
                        IdCountry = x.IdCountry,
                        CountryName = x.CountryName,
                        CountryIDBekasi = x.CountryIDBekasi,
                        CountryIDSerpong = x.CountryIDSerpong,
                        CountryIDSimprug = x.CountryIDSimprug,
                        CountryNameBekasi = x.CountryNameBekasi,
                        CountryNameSerpong = x.CountryNameSerpong,
                        CountryNameSimprug = x.CountryNameSimprug,
                        NationalityIDBekasi = x.NationalityIDBekasi,
                        NationalityIDSerpong = x.NationalityIDSerpong,
                        NationalityIDSimprug = x.NationalityIDSimprug,
                        NationalityNameBekasi = x.NationalityNameBekasi,
                        NationalityNameSerpong = x.NationalityNameSerpong,
                        NationalityNameSimprug = x.NationalityNameSimprug
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdCountry).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
