using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListBreakSettingHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListBreakSettingRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "BreakName" };
        private readonly ISchedulingDbContext _dbContext;

        public GetListBreakSettingHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListBreakSettingRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingBreak>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.BreakName, $"%{param.Search}%"));

            var dataGrade = _dbContext.Entity<MsGrade>();

            var dataQuery = _dbContext.Entity<TrInvitationBookingSettingBreak>()
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   Id = x.Id,
                   AppointmentDate = x.DateInvitation,
                   BreakName = x.BreakName,
                   StartTime = x.StartTime,
                   EndTime = x.EndTime,
                   BreakType = x.BreakType,
                   IdGrade =  x.IdGrade,
                   GradeName = x.IdGrade == null ? null : dataGrade.Where(e => e.Id == x.IdGrade).FirstOrDefault().Description
               });

            //ordering
            switch (param.OrderBy)
            {
                case "AppointmentDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AppointmentDate)
                        : query.OrderBy(x => x.AppointmentDate);
                    break;
                case "BreakName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BreakName)
                        : query.OrderBy(x => x.BreakName);
                    break;
                case "StartTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartTime)
                        : query.OrderBy(x => x.StartTime);
                    break;
                case "EndTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndTime)
                        : query.OrderBy(x => x.EndTime);
                    break;
                case "BreakType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BreakType)
                        : query.OrderBy(x => x.BreakType);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListBreakSettingResult
                {
                    Id = x.Id,
                    AppointmentDate = x.AppointmentDate,
                    BreakName = x.BreakName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    BreakType = x.BreakType,
                    IdGrade = x.IdGrade,
                    GradeName = x.GradeName
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetListBreakSettingResult
                {
                    Id = x.Id,
                    AppointmentDate = x.AppointmentDate,
                    BreakName = x.BreakName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    BreakType = x.BreakType,
                    IdGrade = x.IdGrade,
                    GradeName = x.GradeName
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
