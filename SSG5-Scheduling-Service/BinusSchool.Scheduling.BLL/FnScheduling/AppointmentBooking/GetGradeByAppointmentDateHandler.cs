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
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetGradeByAppointmentDateHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetGradeByAppointmentDateRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "Grade" };
        private readonly ISchedulingDbContext _dbContext;

        public GetGradeByAppointmentDateHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeByAppointmentDateRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingDetail>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Grade.Description, $"%{param.Search}%"));

            var dataQuery = _dbContext.Entity<TrInvitationBookingSettingDetail>()
                .Include(x => x.InvitationBookingSetting)
                .Include(x => x.Grade)
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   IdGrade = x.IdGrade,
                   CodeGrade = x.Grade.Code,
                   NameGrade = x.Grade.Description,
                   AppointmentDate = x.InvitationBookingSetting.InvitationStartDate,
                   AppointmentEndDate = x.InvitationBookingSetting.InvitationEndDate
               }).ToList();

            var dates = new List<DateTime>();

            List<GetGradeByAppointmentDateResult> listData = new List<GetGradeByAppointmentDateResult>();

            foreach(var date in query)
            {
                for (var dt = date.AppointmentDate; dt <= date.AppointmentEndDate; dt = dt.AddDays(1))
                {
                    listData.Add(new GetGradeByAppointmentDateResult
                    {
                        IdGrade = date.IdGrade,
                        NameGrade = date.NameGrade,
                        AppointmentDate = dt,
                        Id = date.IdGrade,
                        Code = date.CodeGrade,
                        Description = date.NameGrade  
                    }); 

                    dates.Add(dt);
                }
            }

            var listDataGradeDate = listData.OrderBy(e => e.AppointmentDate).ThenBy(e => e.Code).ToList();

            //ordering
            switch (param.OrderBy)
            {
                case "Grade":
                    listDataGradeDate = param.OrderType == OrderType.Desc
                        ? listDataGradeDate.OrderByDescending(x => x.NameGrade).ToList()
                        : listDataGradeDate.OrderBy(x => x.NameGrade).ToList();
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = listDataGradeDate.Select(x => new GetGradeByAppointmentDateResult{
                    IdGrade = x.IdGrade,
                    NameGrade = x.NameGrade,
                    AppointmentDate = x.AppointmentDate,
                    Id = x.IdGrade,
                    Code = x.Code,
                    Description = x.NameGrade
                }).ToList();
            }
            else
            {
                items = listDataGradeDate
                .SetPagination(param)
                .Select(x => new GetGradeByAppointmentDateResult
                {
                    IdGrade = x.IdGrade,
                    NameGrade = x.NameGrade,
                    AppointmentDate = x.AppointmentDate,
                    Id = x.IdGrade,
                    Code = x.Code,
                    Description = x.NameGrade
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : listDataGradeDate.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            
        }
    }
}
