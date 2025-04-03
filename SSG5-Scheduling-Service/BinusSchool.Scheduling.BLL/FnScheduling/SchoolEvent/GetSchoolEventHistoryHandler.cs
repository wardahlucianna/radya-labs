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
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventHistoryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventHistoryRequest.IdEvent),
        };
        private readonly ISchedulingDbContext _dbContext;

        public GetSchoolEventHistoryHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        private string GetUserNameCreate(HTrEvent hTrEvent)
        {
            var user = _dbContext.Entity<MsUser>().Where(x => x.Id == hTrEvent.UserIn).FirstOrDefault();

            return user.DisplayName;
        }

        private string GetUserNameEdited(HTrEvent hTrEvent)
        {
            var user = _dbContext.Entity<MsUser>().Where(x => x.Id == hTrEvent.UserUp).FirstOrDefault();

            return user.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventHistoryRequest>();

            var query = await (from he in _dbContext.Entity<TrEventChange>()
                                   where he.IdEvent == param.IdEvent
                                    
                                  select new GetSchoolEventHistoryResult
                                  {
                                    Id = he.Id,
                                    IdEvent = he.IdEvent,
                                    ChangeNotes = he.ChangeNotes,
                                    ChangetDate = he.DateIn
                                  }).ToListAsync(CancellationToken);

            var dataEventPagination = query
                .Select(x => new GetSchoolEventHistoryResult
                {
                    Id = x.Id,
                    IdEvent = x.IdEvent,
                    ChangeNotes = x.ChangeNotes,
                    ChangetDate = x.ChangetDate
                })
                .ToList();

            return Request.CreateApiResult2(dataEventPagination as object);
        }
    }
}
