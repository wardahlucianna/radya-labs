using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.VenueReservationOwner
{
    public class GetDDLVenueReservationOwnerHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDDLVenueReservationOwnerHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDDLVenueReservationOwnerRequest>(
                nameof(GetDDLVenueReservationOwnerRequest.IdSchool));

            var data = await _dbContext.Entity<MsReservationOwner>()
                .Where(x => x.IdSchool == param.IdSchool &&
                            x.IsPICVenue == (param.IsPICVenue == null? x.IsPICVenue : param.IsPICVenue) &&
                            x.IsPICEquipment == (param.IsPICEquipment == null ? x.IsPICEquipment : param.IsPICEquipment))
                .Select(x => new NameValueVm
                {
                    Id = x.Id,
                    Name = x.OwnerName
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
