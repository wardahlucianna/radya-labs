using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnActivities;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetMasterImmersionDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IImmersion _immersionApi;

        public GetMasterImmersionDetailHandler(
            ISchedulingDbContext dbContext,
            IImmersion immersionApi)
        {
            _dbContext = dbContext;
            _immersionApi = immersionApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterImmersionDetailRequest>(
                                nameof(GetMasterImmersionDetailRequest.IdImmersion));

            //FillConfiguration();
            //_immersionApi.SetConfigurationFrom(ApiConfiguration);

            var paramIdImmersionList = new ImmersionDocumentRequest_Get()
            {
                IdImmersions = new string[] { param.IdImmersion }
            };
            var immersionDocuments = await _immersionApi.GetImmersionDocument(paramIdImmersionList);
            var immersionDocumentsList = immersionDocuments.Payload.ToList();

            var gradeLevelList = await _dbContext.Entity<TrImmersionGradeMapping>()
                                .Include(igm => igm.Grade)
                                .ThenInclude(g => g.Level)
                                .Where(x => x.IdImmersion == param.IdImmersion)
                                .OrderBy(x => x.Grade.OrderNumber)
                                .Select(x => new
                                {
                                    Level = new CodeWithIdVm
                                    {
                                        Id = x.Grade.IdLevel,
                                        Code = x.Grade.Level.Code,
                                        Description = x.Grade.Level.Description
                                    },
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = x.IdGrade,
                                        Code = x.Grade.Code,
                                        Description = x.Grade.Description
                                    }
                                })
                                .ToListAsync(CancellationToken);

            var immersionResult = await _dbContext.Entity<MsImmersion>()
                                .Include(i => i.Staff)
                                .Include(i => i.ImmersionPaymentMethod)
                                .Include(i => i.Currency)
                                    .ThenInclude(c => c.Country)
                                .Include(i => i.ImmersionPeriod)
                                    .ThenInclude(ip => ip.AcademicYear)
                                .Where(x => x.Id == param.IdImmersion)
                                .Select(x => new GetMasterImmersionDetailResult
                                {
                                    IdImmersion = x.Id,
                                    AcademicYear = new CodeWithIdVm
                                    {
                                        Id = x.ImmersionPeriod.IdAcademicYear,
                                        Code = x.ImmersionPeriod.AcademicYear.Code,
                                        Description = x.ImmersionPeriod.AcademicYear.Description
                                    },
                                    LevelList = gradeLevelList.Count != 0 ? gradeLevelList.Select(y => new CodeWithIdVm
                                    {
                                        Id = y.Level.Id,
                                        Code = y.Level.Code,
                                        Description = y.Level.Description
                                    }).Distinct().ToList()
                                    : null,
                                    GradeList = gradeLevelList.Count != 0 ? gradeLevelList.Select(y => new CodeWithIdVm
                                    {
                                        Id = y.Grade.Id,
                                        Code = y.Grade.Code,
                                        Description = y.Grade.Description
                                    }).Distinct().ToList()
                                    : null,
                                    Semester = new ItemValueVm
                                    {
                                        Id = x.Semester.ToString(),
                                        Description = x.Semester.ToString(),
                                    },
                                    Destination = x.Destination,
                                    ImmersionPeriod = new ItemValueVm
                                    {
                                        Id = x.IdImmersionPeriod,
                                        Description = x.ImmersionPeriod.Name
                                    },
                                    Description = x.Description,
                                    StartDate = x.StartDate,
                                    EndDate = x.EndDate,
                                    BinusianPIC = new NameValueVm
                                    {
                                        Id = x.IdBinusianPIC,
                                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                    },
                                    PICEmail = x.PICEmail,
                                    PICPhone = x.PICPhone,
                                    MinParticipant = x.MinParticipant,
                                    MaxParticipant = x.MaxParticipant,
                                    Currency = new ItemValueVm
                                    {
                                        Id = x.IdCurrency,
                                        Description = string.Format("{0} - {1}{2}", x.Currency.Name, x.Currency.Currency, (x.Currency.Symbol.Contains("?") ? "" : " (" + x.Currency.Symbol + ")"))
                                    },
                                    ImmersionPaymentMethod = new CodeWithIdVm
                                    {
                                        Id = x.IdImmersionPaymentMethod,
                                        Code = x.ImmersionPaymentMethod.Code,
                                        Description = x.ImmersionPaymentMethod.Description
                                    },
                                    RegistrationFee = x.RegistrationFee,
                                    TotalCost = x.TotalCost,
                                    PosterLink = immersionDocumentsList.FirstOrDefault().PosterLink,
                                    BrochureLink = immersionDocumentsList.FirstOrDefault().BrochureLink
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(immersionResult as object);
        }
    }
}
