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
    public class GetListTeacherByInvitationHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListTeacherByInvitationRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "TeacherName" };
        private readonly ISchedulingDbContext _dbContext;

        public GetListTeacherByInvitationHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListTeacherByInvitationRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBooking>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.UserTeacher.DisplayName, $"%{param.Search}%"));

            var dataInvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
                .Include(x => x.UserTeacher)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.IdUserTeacher,
                    Code = x.IdUserTeacher,
                    Description = x.UserTeacher.DisplayName,
                    TeacherName = x.UserTeacher.DisplayName
                })
                .Distinct().ToListAsync(CancellationToken);

            var query = dataInvitationBooking.Distinct();

            //ordering
            switch (param.OrderBy)
            {
                case "TeacherName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TeacherName)
                        : query.OrderBy(x => x.TeacherName);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.Select(x => new GetListTeacherByInvitationResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    TeacherName = x.TeacherName
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetListTeacherByInvitationResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    TeacherName = x.TeacherName
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
