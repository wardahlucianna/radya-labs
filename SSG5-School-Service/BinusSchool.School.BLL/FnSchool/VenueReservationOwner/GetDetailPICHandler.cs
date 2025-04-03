using System;
using System.Collections.Generic;
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
    public class GetDetailPICHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDetailPICHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailPICRequest>(nameof(GetDetailPICRequest.IdReservationOwner));

            var data = await _dbContext.Entity<MsReservationOwner>()
                .Where(x => x.Id == param.IdReservationOwner)
                .Include(x => x.VenueMappings)
                .Select(x => new GetDetailPICResult
                {
                    IdReservationOwner = x.Id,
                    OwnerName = x.OwnerName,
                    IsPICVenue = x.IsPICVenue,
                    IsPICEquipment = x.IsPICEquipment,
                    PICEmail = (List<EmailRequest>)x.ReservationOwnerEmails.Select(y => 
                        new EmailRequest
                            {
                                OwnerEmail = y.OwnerEmail,
                                IsOwnerEmailTo = y.IsOwnerEmailTo,
                                IsOwnerEmailBCC = y.IsOwnerEmailBCC,
                                IsOwnerEmailCC = y.IsOwnerEmailCC
                            }
                        ),
                    VenueMapping = x.VenueMappings.Any()
                }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
