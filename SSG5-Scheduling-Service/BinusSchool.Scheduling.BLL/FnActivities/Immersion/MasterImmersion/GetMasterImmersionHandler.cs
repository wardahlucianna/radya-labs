using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnActivities;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetMasterImmersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IImmersion _immersionApi;

        public GetMasterImmersionHandler(
                    ISchedulingDbContext dbContext,
                    IImmersion immersionApi)
        {
            _dbContext = dbContext;
            _immersionApi = immersionApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterImmersionRequest>(
                            nameof(GetMasterImmersionRequest.IdAcademicYear),
                            nameof(GetMasterImmersionRequest.Semester));

            //FillConfiguration();
            //_immersionApi.SetConfigurationFrom(ApiConfiguration);

            var columns = new[] { "academicYear", "destination", "startDate", "endDate", "picName" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Id" },
                { columns[1], "Destination" },
                { columns[2], "StartDate" },
                { columns[3], "EndDate" },
                { columns[4], "PIC.Name" }
            };

            var predicate = PredicateBuilder.True<TrImmersionGradeMapping>();
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                            EF.Functions.Like(x.Immersion.Destination, param.SearchPattern()) ||
                            EF.Functions.Like(x.Immersion.Staff.FirstName, param.SearchPattern()) ||
                            EF.Functions.Like(x.Immersion.Staff.LastName, param.SearchPattern()));

            var getImmersionQuery = _dbContext.Entity<TrImmersionGradeMapping>()
                                    .Include(igm => igm.Grade)
                                        .ThenInclude(g => g.Level)
                                        .ThenInclude(l => l.AcademicYear)
                                    .Include(igm => igm.Immersion)
                                        .ThenInclude(i => i.Staff)
                                    .Include(igm => igm.Immersion)
                                        .ThenInclude(i => i.Currency)
                                        .ThenInclude(c => c.Country)
                                    .Include(igm => igm.Immersion)
                                        .ThenInclude(i => i.ImmersionPaymentMethod)
                                    .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                                x.Immersion.Semester == param.Semester &&
                                                (string.IsNullOrEmpty(param.IdImmersionPeriod) ? true : x.Immersion.IdImmersionPeriod == param.IdImmersionPeriod))
                                    .Where(predicate);

            var idImmersionList = getImmersionQuery.Select(x => x.IdImmersion).Distinct().ToList();

            if (idImmersionList.Count <= 0)
                throw new BadRequestException(null);

            var paramIdImmersionList = new ImmersionDocumentRequest_Get()
            {
                IdImmersions = idImmersionList.ToArray()
            };

            var immersionDocuments = await _immersionApi.GetImmersionDocument(paramIdImmersionList);
            var immersionDocumentsList = immersionDocuments.Payload.ToList();

            var getImmersionList = getImmersionQuery
                                    //.SearchByDynamic(param)
                                    .Select(x => new
                                    {
                                        IdImmersion = x.IdImmersion,
                                        AcademicYear = new CodeWithIdVm
                                        {
                                            Id = x.Grade.Level.IdAcademicYear,
                                            Code = x.Grade.Level.AcademicYear.Code,
                                            Description = x.Grade.Level.AcademicYear.Description
                                        },
                                        Grade = new CodeWithIdVm
                                        {
                                            Id = x.IdGrade,
                                            Code = x.Grade.Code,
                                            Description = x.Grade.Description
                                        },
                                        Destination = x.Immersion.Destination,
                                        StartDate = x.Immersion.StartDate,
                                        EndDate = x.Immersion.EndDate,
                                        PIC = new GetMasterImmersionResult_PIC
                                        {
                                            IdBinusian = x.Immersion.IdBinusianPIC,
                                            Name = NameUtil.GenerateFullName(x.Immersion.Staff.FirstName, x.Immersion.Staff.LastName),
                                            Email = x.Immersion.PICEmail.Trim(),
                                            PhoneNumber = x.Immersion.PICPhone.Trim()
                                        },
                                        Description = x.Immersion.Description,
                                        MinParticipant = x.Immersion.MinParticipant,
                                        MaxParticipant = x.Immersion.MaxParticipant,
                                        Currency = new NameValueVm
                                        {
                                            Id = x.Immersion.IdCurrency,
                                            Name = x.Immersion.Currency.Currency
                                        },
                                        CurrencySymbol = x.Immersion.Currency.Symbol,
                                        CurrencyName = x.Immersion.Currency.Name,
                                        ImmersionPaymentMethod = new CodeWithIdVm
                                        {
                                            Id = x.Immersion.IdImmersionPaymentMethod,
                                            Code = x.Immersion.ImmersionPaymentMethod.Code,
                                            Description = x.Immersion.ImmersionPaymentMethod.Description
                                        },
                                        RegistrationFee = x.Immersion.RegistrationFee,
                                        TotalCost = x.Immersion.TotalCost
                                    })
                                    .ToList();

            var query = getImmersionList
                                .GroupBy(x => new
                                {
                                    x.IdImmersion,
                                    AcademicYearId = x.AcademicYear.Id,
                                    AcademicYearCode = x.AcademicYear.Code,
                                    AcademicYearDescription = x.AcademicYear.Description,
                                    x.Destination,
                                    x.StartDate,
                                    x.EndDate,
                                    PICIdBinusian = x.PIC.IdBinusian,
                                    PICName = x.PIC.Name,
                                    PICEmail = x.PIC.Email,
                                    PICPhoneNumber = x.PIC.PhoneNumber,
                                    x.Description,
                                    x.MinParticipant,
                                    x.MaxParticipant,
                                    CurrencyId = x.Currency.Id,
                                    CurrencyShortName = x.Currency.Name,
                                    x.CurrencySymbol,
                                    x.CurrencyName,
                                    ImmersionPaymentMethodId = x.ImmersionPaymentMethod.Id,
                                    ImmersionPaymentMethodCode = x.ImmersionPaymentMethod.Code,
                                    ImmersionPaymentMethodDescription = x.ImmersionPaymentMethod.Description,
                                    x.RegistrationFee,
                                    x.TotalCost
                                })
                                .Select(x => new GetMasterImmersionResult
                                {
                                    IdImmersion = x.Key.IdImmersion,
                                    AcademicYear = new CodeWithIdVm
                                    {
                                        Id = x.Key.AcademicYearId,
                                        Code = x.Key.AcademicYearCode,
                                        Description = x.Key.AcademicYearDescription
                                    },
                                    GradeList = x.Select(y => y.Grade).ToList(),
                                    Destination = x.Key.Destination,
                                    StartDate = x.Key.StartDate,
                                    EndDate = x.Key.EndDate,
                                    PIC = new GetMasterImmersionResult_PIC
                                    {
                                        IdBinusian = x.Key.PICIdBinusian,
                                        Name = x.Key.PICName,
                                        Email = x.Key.PICEmail,
                                        PhoneNumber = x.Key.PICPhoneNumber
                                    },
                                    Description = x.Key.Description,
                                    MinParticipant = x.Key.MinParticipant,
                                    MaxParticipant = x.Key.MaxParticipant,
                                    Currency = new NameValueVm
                                    {
                                        Id = x.Key.CurrencyId,
                                        Name = x.Key.CurrencyShortName
                                    },
                                    CurrencySymbol = x.Key.CurrencySymbol,
                                    CurrencyName = x.Key.CurrencyName,
                                    ImmersionPaymentMethod = new CodeWithIdVm
                                    {
                                        Id = x.Key.ImmersionPaymentMethodId,
                                        Code = x.Key.ImmersionPaymentMethodCode,
                                        Description = x.Key.ImmersionPaymentMethodDescription
                                    },
                                    RegistrationFee = x.Key.RegistrationFee,
                                    TotalCost = x.Key.TotalCost,
                                    PosterLink = null,
                                    BrochureLink = null
                                })
                                .OrderBy(x => x.IdImmersion)
                                .ToList();

            foreach (var getImmersion in query)
            {
                getImmersion.PosterLink = immersionDocumentsList.FirstOrDefault(x => x.Id == getImmersion.IdImmersion)?.PosterLink;
                getImmersion.BrochureLink = immersionDocumentsList.FirstOrDefault(x => x.Id == getImmersion.IdImmersion)?.BrochureLink;
            }

            var resultItems = query
                                .AsQueryable()
                                .OrderByDynamic(param, aliasColumns)
                                .SetPagination(param)
                                .ToList();

            var count = param.CanCountWithoutFetchDb(query.Count())
                ? query.Count()
                : query.AsQueryable().Select(x => x.IdImmersion).Count();

            return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
