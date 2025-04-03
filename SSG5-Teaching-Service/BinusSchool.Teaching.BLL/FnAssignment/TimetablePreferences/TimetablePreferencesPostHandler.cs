using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator;
using BinusSchool.Persistence.TeachingDb;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimetablePreferencesPostHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private IDbContextTransaction _transaction;
        public TimetablePreferencesPostHandler(TeachingDbContext teachingDbContext)
        {
            _teachingDbContext = teachingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var model = await Request.ValidateBody<PostTimetableRequest, PostTimetableValidator>();
            _transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                foreach (var item in model.TimeTable)
                {
                    //get data header kalo ada eksekusi
                    var getHeader = await _teachingDbContext.Entity<TrTimeTablePrefHeader>().Where(p => p.Id == item.Id).FirstOrDefaultAsync();
                    if (getHeader != null)
                    {
                        getHeader.Status = item.Status;
                        getHeader.CanDelete = false;
                        getHeader.UserUp = AuthInfo.UserId;
                        _teachingDbContext.Entity<TrTimeTablePrefHeader>().Update(getHeader);

                        if (item.TimeTableDetail != null)
                        {
                            foreach (var itemDetail in item.TimeTableDetail)
                            {
                                var getTimetableDetail = await _teachingDbContext.Entity<TrTimetablePrefDetail>()
                                .Include(p => p.TeachingLoads)
                                .Where(p => p.Id == itemDetail.Id).FirstOrDefaultAsync();
                                //update data
                                if (getTimetableDetail != null)
                                {
                                    getTimetableDetail.IdDivision = itemDetail.IdSchoolDivision;
                                    getTimetableDetail.Count = itemDetail.Count;
                                    getTimetableDetail.Length = itemDetail.Length;
                                    getTimetableDetail.Week = itemDetail.Week;
                                    getTimetableDetail.IdPeriod = itemDetail.IdSchoolTerm;
                                    getTimetableDetail.IdVenue = itemDetail.IdVenue;
                                    getTimetableDetail.UserUp = AuthInfo.UserId;
                                    _teachingDbContext.Entity<TrTimetablePrefDetail>().Update(getTimetableDetail);

                                    if (getTimetableDetail.TeachingLoads.Count > 0)
                                    {
                                        foreach (var itemLoad in getTimetableDetail.TeachingLoads)
                                        {
                                            itemLoad.IdUser = itemDetail.IdUserTeaching;
                                            itemLoad.Load = itemDetail.Count * itemDetail.Length;
                                            itemLoad.IdSubjectCombination = getTimetableDetail.IdTimetablePrefHeader;
                                            itemLoad.UserUp = AuthInfo.UserId;
                                            _teachingDbContext.Entity<TrTeachingLoad>().Update(itemLoad);

                                        }
                                    }
                                    else
                                    {
                                        var userload = new TrTeachingLoad();

                                        userload.Id = Guid.NewGuid().ToString();
                                        userload.IdUser = itemDetail.IdUserTeaching;
                                        userload.Load = itemDetail.Count * itemDetail.Length;
                                        userload.IdTimetablePrefDetail = getTimetableDetail.Id;
                                        userload.IdSubjectCombination = getTimetableDetail.IdTimetablePrefHeader;
                                        userload.UserIn = AuthInfo.UserId;
                                        await _teachingDbContext.Entity<TrTeachingLoad>().AddAsync(userload);
                                    }
                                }
                                //insert data
                                else
                                {
                                    var newDataDetail = new TrTimetablePrefDetail();
                                    newDataDetail.Id = Guid.NewGuid().ToString();
                                    newDataDetail.IdTimetablePrefHeader = item.Id;
                                    newDataDetail.IdDivision = itemDetail.IdSchoolDivision;
                                    newDataDetail.Count = itemDetail.Count;
                                    newDataDetail.Length = itemDetail.Length;
                                    newDataDetail.Week = itemDetail.Week;
                                    newDataDetail.IdPeriod = itemDetail.IdSchoolTerm;
                                    newDataDetail.IdVenue = itemDetail.IdVenue;
                                    newDataDetail.UserIn = AuthInfo.UserId;
                                    await _teachingDbContext.Entity<TrTimetablePrefDetail>().AddAsync(newDataDetail);

                                    var userload = new TrTeachingLoad();

                                    userload.Id = Guid.NewGuid().ToString();
                                    userload.IdUser = itemDetail.IdUserTeaching;
                                    userload.Load = itemDetail.Count * itemDetail.Length;
                                    userload.IdTimetablePrefDetail = newDataDetail.Id;
                                    userload.IdSubjectCombination = item.Id;
                                    newDataDetail.UserIn = AuthInfo.UserId;
                                    await _teachingDbContext.Entity<TrTeachingLoad>().AddAsync(userload);
                                }
                            }
                        }
                    }
                    await _teachingDbContext.SaveChangesAsync(CancellationToken);
                }
              
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw ex;
            }
            return Request.CreateApiResult2();
        }
    }
}
