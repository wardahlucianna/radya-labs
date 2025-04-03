using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.Extensions.Localization;
using BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using System.Linq;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesMergeUnmergeHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IApiService<ISchool> _schoolApi;
        private readonly IStringLocalizer _localizer;
        public TimeTablePreferencesMergeUnmergeHandler(ITeachingDbContext teachingDbContext, IApiService<ISchool> schoolApi, IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _schoolApi = schoolApi;
            _localizer = localizer;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var model = await Request.ValidateBody<AddMergeUnmergeRequest, AddMergeUnMergeValidator>();
            var transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);
            var getData = await _teachingDbContext.Entity<TrTimeTablePrefHeader>().Include(p => p.TimetablePrefDetails).Where(p => p.Id == model.Id).FirstOrDefaultAsync();
            if (getData == null)
                throw new NotFoundException("data parent not found");
            //marge
            if (model.IsMarge)
            {
                getData.IsMerge = false;
                getData.IsParent = true;
                _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(getData);

                var datasCandidateChild = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
                           .Include(p => p.TimetablePrefDetails)
                           .Where(x => model.ChildId.Any(y => y == x.Id))
                           .ToListAsync(CancellationToken);

                if (datasCandidateChild.Count == 0)
                    throw new NotFoundException("Child not Found");

                foreach (var item in datasCandidateChild)
                {
                    item.IdParent = getData.Id;
                    item.IsMerge = true;
                    item.IsParent = false;
                    _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(item);

                    if (item.TimetablePrefDetails != null)
                    {
                        foreach (var itemDetails in item.TimetablePrefDetails)
                        {
                            itemDetails.IsActive = false;
                            _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(itemDetails);
                        }
                    }
                }
            }
            //un marge
            else
            {
                getData.IsMerge = false;
                getData.IsParent = false;
                _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(getData);

                var getChild = await _teachingDbContext.Entity<TrTimeTablePrefHeader>().Include(p => p.TimetablePrefDetails).Where(p => p.IdParent == getData.Id).ToListAsync();
                if (getChild.Count > 0)
                {
                    foreach (var item in getChild)
                    {
                        item.IdParent = null;
                        item.IsMerge = false;
                        item.IsParent = false;
                        _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(item);

                        if (item.TimetablePrefDetails != null)
                        {
                            foreach (var itemDetails in item.TimetablePrefDetails)
                            {
                                itemDetails.IsActive = false;
                                _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(itemDetails);
                            }
                        }
                    }
                }
            }

            if (getData.TimetablePrefDetails != null)
            {
                foreach (var itemDetails in getData.TimetablePrefDetails)
                {
                    itemDetails.IsActive = false;
                    _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(itemDetails);
                }
            }


            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            await transaction.CommitAsync(CancellationToken);
            
            return Request.CreateApiResult2(true as object);
        }
    }
}
