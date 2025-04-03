using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        public TimeTablePreferencesDetailHandler(ITeachingDbContext teachingDbContext)
        {
            _teachingDbContext = teachingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var model = await Request.ValidateBody<TimetableDetailRequest, TimetableDetailValidator>();
            try
            {
                //foreach (var item in model.DetailRequests)
                //{
                //    switch (item.Action)
                //    {
                //        case "Add":
                //            foreach (var header in model.IdTimetablePrefHeader)
                //            {
                //                var _timeTablePrefDetail = new TrTimetablePrefDetail
                //                {
                //                    Id = Guid.NewGuid().ToString(),
                //                    Count = item.NewValue.Count,
                //                    Length = item.NewValue.Length,
                //                    IdTimetablePrefHeader = header
                //                };
                //                _teachingDbContext.Entity<TrTimetablePrefDetail>().Add(_timeTablePrefDetail);
                //            }
                //            break;
                //        case "Edit":
                //            var datas = _teachingDbContext.Entity<TrTimetablePrefDetail>()
                //                .Where(x => model.IdTimetablePrefHeader.Any(y => y == x.IdTimetablePrefHeader))
                //                .Where(x => x.Count == item.OldValue.Count)
                //                .Where(x => x.Length == item.OldValue.Length)
                //                .ToList();
                //            if (datas != null && datas.Count > 0)
                //            {
                //                foreach (var data in datas)
                //                {
                //                    data.Count = item.NewValue.Count;
                //                    data.Length = item.NewValue.Length;
                //                    _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(data);
                //                }
                //            }
                //            break;
                //        case "Delete":
                //            var dataDeletes = _teachingDbContext.Entity<TrTimetablePrefDetail>()
                //                .Where(x => model.IdTimetablePrefHeader.Any(y => y == x.IdTimetablePrefHeader))
                //                .Where(x => x.Count == item.OldValue.Count)
                //                .Where(x => x.Length == item.OldValue.Length)
                //                .ToList();
                //            if (dataDeletes != null && dataDeletes.Count > 0)
                //            {
                //                foreach (var dataDelete in dataDeletes)
                //                {
                //                    dataDelete.IsActive = false;
                //                    _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(dataDelete);
                //                }
                //            }
                //            break;
                //        default:
                //            break;
                //    }
                //}
                //var paramActionUpdateStatus = new string[] { "Add", "Delete" };
                //var checkAction = model.DetailRequests.Where(x => paramActionUpdateStatus.Any(y => y == x.Action)).Count();
                //if (checkAction > 0)
                //{
                var timeTablePrefHeader = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
                    .Where(x => model.IdTimetablePrefHeader.Any(y => y == x.Id))
                    .ToListAsync();
                foreach (var item in timeTablePrefHeader)
                {
                    item.Status = false;
                    item.CanDelete = true;
                    _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(item);
                }
                //}
                await _teachingDbContext.SaveChangesAsync(CancellationToken);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
            return Request.CreateApiResult2();
        }
    }
}
