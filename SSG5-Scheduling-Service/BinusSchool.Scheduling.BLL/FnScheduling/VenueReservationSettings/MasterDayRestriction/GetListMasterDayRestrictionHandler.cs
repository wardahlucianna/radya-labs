using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class GetListMasterDayRestrictionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetListMasterDayRestrictionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListMasterDayRestrictionRequest>(nameof(GetListMasterDayRestrictionRequest.IdSchool));

            var groupData = from x in 
                                _dbContext.Entity<MsRestrictionBookingVenue>()
                                .Include(x => x.Building)
                                .Where(x => x.Building.IdSchool == param.IdSchool)
                                .OrderByDynamic(param)
                                .ToList() 
                              group x by new { x.IdGroupRestriction, x.StartRestrictionDate, x.EndRestrictionDate };

            var data = groupData
                .Select(x => new
                {
                    x.Key.IdGroupRestriction,
                    x.Key.StartRestrictionDate,
                    x.Key.EndRestrictionDate,
                    BuildingDescription = x.Select(y => "All " + y.Building.Description).ToList(),
                    VenueDescription = x.Select(y => y.IdVenue)
                        .Join(_dbContext.Entity<MsVenue>(),
                            group => group,
                            venue => venue.Id,
                            (group, venue) => venue.Description
                        )
                })
                .ToList();

            var listData = new List<GetListMasterDayRestrictionResult>();
            foreach (var item in data)
            {
                var listRestrictionPlace = new List<string>();
                if (item.VenueDescription.Count() > 0)
                {
                    listRestrictionPlace.AddRange(item.VenueDescription);
                }
                else
                {
                    listRestrictionPlace.AddRange(item.BuildingDescription);
                }

                var itemData = new GetListMasterDayRestrictionResult
                {
                    IdGroupRestriction = item.IdGroupRestriction,
                    StartRestrictionDate = item.StartRestrictionDate,
                    EndRestrictionDate = item.EndRestrictionDate,
                    RestrictionPlace = string.Join(", ", listRestrictionPlace)
                };

                listData.Add(itemData);
            }

            var column = new[] { "startdate", "enddate", "restrictionplace" };

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                listData = listData.Where(x => x.RestrictionPlace.Contains(param.SearchPattern())
                                || x.StartRestrictionDate.ToString("dd MMMM yyyy, HH:mm").Contains(param.SearchPattern())
                                || x.EndRestrictionDate.ToString("dd MMMM yyyy, HH:mm").Contains(param.SearchPattern())).ToList();
            }

            

            listData = param.OrderBy switch
            {
                "startdate" => param.OrderType == OrderType.Asc ? listData.OrderBy(x => x.StartRestrictionDate).ToList() : listData.OrderByDescending(x => x.StartRestrictionDate).ToList(),
                "enddate" => param.OrderType == OrderType.Asc ? listData.OrderBy(x => x.EndRestrictionDate).ToList() : listData.OrderByDescending(x => x.EndRestrictionDate).ToList(),
                "restrictionplace" => param.OrderType == OrderType.Asc ? listData.OrderBy(x => x.RestrictionPlace).ToList() : listData.OrderByDescending(x => x.RestrictionPlace).ToList(),
                _ => listData
            };

            var listResPage = listData.SetPagination(param).ToList();

            var count = param.CanCountWithoutFetchDb(listResPage.Count) ? listResPage.Count : listData.Select(x => x.IdGroupRestriction).Count();

            return Request.CreateApiResult2(listResPage as object, param.CreatePaginationProperty(count).AddColumnProperty());

        }   
    }
}
