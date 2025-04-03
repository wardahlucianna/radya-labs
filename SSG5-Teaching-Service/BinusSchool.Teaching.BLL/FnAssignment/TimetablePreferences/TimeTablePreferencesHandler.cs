using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using CodeView = BinusSchool.Data.Model.Teaching.FnAssignment.Timetable.CodeView;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;

        public TimeTablePreferencesHandler(ITeachingDbContext teachingDbContext, IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var datas = await _teachingDbContext.Entity<TrTimetablePrefDetail>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(_localizer["ExNotFound"], x));

            // find already used ids
            foreach (var item in datas)
            {
                // harus di cek lagi nanti kalo teacher assign udah ada di sana di cek dulu sebelum di delete
                //kalo data nya ada ga bisa di delete
                item.IsActive = false;
                _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(item);

                var timeTableHead = await _teachingDbContext.Entity<TrTimeTablePrefHeader>().Where(x => x.Id == item.IdTimetablePrefHeader).FirstOrDefaultAsync();

                if (timeTableHead != null)
                {
                    timeTableHead.CanDelete = true;
                    timeTableHead.Status = false;
                    _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(timeTableHead);
                }

                var teachingLoad = await _teachingDbContext.Entity<TrTeachingLoad>().Where(x => x.IdTimetablePrefDetail == item.Id).FirstOrDefaultAsync();
                if (teachingLoad != null)
                {
                    teachingLoad.IsActive = false;
                    _teachingDbContext.Entity<TrTeachingLoad>().Update(teachingLoad);
                }
            }

            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var model = await Request.ValidateBody<AddTimeTableRequest, AddTimeTableValidator>();
            TrTimeTablePrefHeader tablePrefHeader = new TrTimeTablePrefHeader
            {
                Id = model.Id,
                CanDelete = model.CanDeleted,
                Status = model.Status,
                IsMerge = model.IsMerge
            };
            _teachingDbContext.Entity<TrTimeTablePrefHeader>().Add(tablePrefHeader);
            foreach (var item in model.TimeTableDetailRequests)
            {
                TrTimetablePrefDetail trTimetablePrefDetail = new TrTimetablePrefDetail
                {
                    Id = item.Id,
                    Count = item.Count,
                    Length = item.Length,
                    IdTimetablePrefHeader = item.IdTimeTablePrefHeader
                };
                _teachingDbContext.Entity<TrTimetablePrefDetail>().Add(trTimetablePrefDetail);
            }
            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var model = await Request.ValidateBody<UpdateTimetableRequest, UpdateTimeTableValidator>();
            var data = await _teachingDbContext.Entity<TrTimeTablePrefHeader>().Include(x => x.TimetablePrefDetails).Where(x => x.Id == model.Id).FirstOrDefaultAsync(CancellationToken);
            if (data != null)
            {
                data.IsActive = model.IsActive;
                _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(data);
                if (data.TimetablePrefDetails.Count > 0)
                {
                    foreach (var detail in data.TimetablePrefDetails)
                    {
                        detail.IsActive = false;
                        _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(detail);
                    }
                }
                await _teachingDbContext.SaveChangesAsync(CancellationToken);
            }
            else
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Document"], "Id"));
            }
            return Request.CreateApiResult2();
        }
    }
}
