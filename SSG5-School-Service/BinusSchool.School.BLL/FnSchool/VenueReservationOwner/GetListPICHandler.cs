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
    public class GetListPICHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetListPICHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListPICRequest>(nameof(GetListPICRequest.IdSchool));

            var data = await _dbContext.Entity<MsReservationOwner>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Include(x => x.VenueMappings)
                .Select(x => new GetListPICResult
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
                                IsOwnerEmailCC = y.IsOwnerEmailCC,
                            }
                        ),
                    CanDelete = !(x.VenueMappings.Any() || x.EquipmentTypes.Any())
                }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
