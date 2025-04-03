using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.StudentStatus.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class GenerateStudentStatusMappingActiveAYHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IStudentDbContext _dbContext;
        private readonly ICurrentAcademicYear _currentAcademicYearApi;
        private readonly IMachineDateTime _dateTime;

        public GenerateStudentStatusMappingActiveAYHandler(
            IStudentDbContext dbContext,
            ICurrentAcademicYear currentAcademicYearApi,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _currentAcademicYearApi = currentAcademicYearApi;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GenerateStudentStatusMappingActiveAYRequest, GenerateStudentStatusMappingActiveAYValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getCurrentAYApi = await _currentAcademicYearApi.GetActiveAcademicYear(new GetActiveAcademicYearRequest
                {
                    SchoolID = param.IdSchool
                });

                var getCurrentAY = getCurrentAYApi.Payload;

                if (getCurrentAY.AcademicYearId != param.IdAcademicYear)
                    throw new BadRequestException($"Only can generate student status mapping for active academic year ({getCurrentAY.AcademicYear})");

                var currentAcademicYearDetail = await _dbContext.Entity<MsAcademicYear>()
                                       .Where(x => x.Id == getCurrentAY.AcademicYearId)
                                       .FirstOrDefaultAsync(CancellationToken);

                var intParamPrevAY = int.Parse(currentAcademicYearDetail.Code) - 1;
                var paramPrevAYString = intParamPrevAY.ToString();

                // get AY start date and end date
                var gerPeriodAY = await _dbContext.Entity<MsPeriod>()
                               .Include(x => x.Grade)
                                   .ThenInclude(x => x.MsLevel)
                               .Where(x => x.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                               .GroupBy(x => x.Grade.MsLevel.IdAcademicYear)
                               .Select(x => new
                               {
                                   IdAcademicYear = x.Key,
                                   MinAYStartDate = x.Min(y => y.StartDate),
                                   MaxAYEndDate = x.Max(y => y.EndDate)
                               })
                               .FirstOrDefaultAsync(CancellationToken);

                var studentAlreadyMappedActiveAYList = await _dbContext.Entity<TrStudentStatus>()
                                                        .Where(x => x.IdAcademicYear == getCurrentAY.AcademicYearId)
                                                        .Select(x => x.IdStudent)
                                                        .Distinct()
                                                        .ToListAsync(CancellationToken);

                // get exclude map status
                /*
                 * Tidak dapat di map apabila latest student status:
                 * - Expelled           (2)
                 * - KURNAS Graduate    (3)
                 * - Transfer (Resign)  (5)
                 * - Graduate           (7)
                 * - Pass Away          (9)
                 */
                int[] excludeMapStudentStatus = { 2, 3, 5, 7, 9 };

                // get active status
                /*
                 * ActiveStatus = 1 bila:
                 * - Admission Student          (0)
                 * - Active                     (1) 
                 * - Not Graduate               (4)
                 * - Incomplete Payment         (6)
                 * - Repeat                     (8)
                 * - Conditional Progression    (10)
                 * - Progression                (11)
                 * - Withdrawal Process         (13)
                 * - Non UN                     (14)
                 */
                int[] activeStudentStatus = { 0, 1, 4, 6, 8, 10, 11, 13, 14 };

                var prevAYStudentStatusList = _dbContext.Entity<TrStudentStatus>()
                                                .Include(x => x.Student)
                                                .Include(x => x.AcademicYear)
                                                .Where(x => x.AcademicYear.Code == paramPrevAYString &&
                                                            x.Student.IdSchool == param.IdSchool &&
                                                            !excludeMapStudentStatus.Any(y => y == x.IdStudentStatus) &&
                                                            x.CurrentStatus == "A")
                                                .ToList();

                var checkMappedStudent = prevAYStudentStatusList
                                                .Where(x => !studentAlreadyMappedActiveAYList.Any(y => y == x.IdStudent))
                                                .GroupBy(x => x.IdStudent, (key, y) => y.OrderByDescending(a => a.StartDate).ThenByDescending(a => a.DateIn).First())
                                                .GroupBy(x => x.IdStudent)
                                                .Select(x => x.OrderByDescending(a => a.StartDate).ThenByDescending(a => a.DateIn).FirstOrDefault())
                                                .ToList();

                var predicate = PredicateBuilder.Create<MsStudentGrade>
                    (a => a.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                    && a.Student.IdSchool == param.IdSchool
                    && a.Student.IdStudentStatus == 1
                    && !studentAlreadyMappedActiveAYList.Contains(a.IdStudent));

                var getStudentGrade = await _dbContext.Entity<MsStudentGrade>()
                    .Include(a => a.Student)
                    .Where(predicate)
                    .ToListAsync(CancellationToken);

                if (checkMappedStudent.Count == 0)
                {
                    foreach (var items in getStudentGrade)
                    {
                        var generateStudentStatus = new TrStudentStatus
                        {
                            IdTrStudentStatus = Guid.NewGuid().ToString(),
                            IdAcademicYear = getCurrentAY.AcademicYearId,
                            IdStudent = items.Student.Id,
                            // untuk latest student status Repeat (8) dan Progression (11), ubah statusnya menjadi Active (1)
                            IdStudentStatus = (items.Student.IdStudentStatus == 8 || items.Student.IdStudentStatus == 11) ? 1 : items.Student.IdStudentStatus,
                            StartDate = gerPeriodAY.MinAYStartDate,
                            EndDate = null,
                            CurrentStatus = "A",
                            Remarks = null,
                            ActiveStatus = activeStudentStatus.Any(y => y == items.Student.IdStudentStatus)
                        };
                        _dbContext.Entity<TrStudentStatus>().Add(generateStudentStatus);
                    }
                }

                // insert new student status for active AY
                if (checkMappedStudent.Count >= 1)
                {
                    var generateNewStudentStatusList = checkMappedStudent
                                                .Select(x => new TrStudentStatus
                                                {
                                                    IdTrStudentStatus = Guid.NewGuid().ToString(),
                                                    IdAcademicYear = getCurrentAY.AcademicYearId,
                                                    IdStudent = x.IdStudent,
                                                    // untuk latest student status Repeat (8) dan Progression (11), ubah statusnya menjadi Active (1)
                                                    IdStudentStatus = (x.IdStudentStatus == 8 || x.IdStudentStatus == 11) ? x.IdStudentStatus = 1 : x.IdStudentStatus,
                                                    StartDate = gerPeriodAY.MinAYStartDate,
                                                    EndDate = null,
                                                    CurrentStatus = "A",
                                                    Remarks = null,
                                                    ActiveStatus = activeStudentStatus.Any(y => y == x.IdStudentStatus)
                                                })
                                                .ToList();
                    _dbContext.Entity<TrStudentStatus>().AddRange(generateNewStudentStatusList);

                    // update latest student status to inactive
                    var updatePrevStudentStatusList = prevAYStudentStatusList;

                    updatePrevStudentStatusList.ForEach(x =>
                    {
                        x.EndDate = gerPeriodAY.MinAYStartDate.AddDays(-1);
                        x.CurrentStatus = "H";
                    });
                    _dbContext.Entity<TrStudentStatus>().UpdateRange(updatePrevStudentStatusList);

                    // update student status in MsStudent
                    var updateMsStudent = prevAYStudentStatusList
                                            .Select(x => x.Student)
                                            .ToList();

                    updateMsStudent.ForEach(x =>
                    {
                        x.IdStudentStatus = generateNewStudentStatusList.Where(y => y.IdStudent == x.Id).Select(y => y.IdStudentStatus).First();
                    });
                    _dbContext.Entity<MsStudent>().UpdateRange(updateMsStudent);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            throw new NotImplementedException();
        }

        #region unused code
        //protected override async Task<ApiErrorResult<object>> Handler()
        //{
        //    var param = await Request.ValidateBody<GenerateStudentStatusMappingActiveAYRequest, GenerateStudentStatusMappingActiveAYValidator>();

        //    try
        //    {
        //        _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

        //        var getCurrentAYApi = await _currentAcademicYearApi.GetActiveAcademicYear(new GetActiveAcademicYearRequest
        //        {
        //            SchoolID = param.IdSchool
        //        });

        //        var getCurrentAY = getCurrentAYApi.Payload;

        //        if (getCurrentAY.AcademicYearId != param.IdAcademicYear)
        //            throw new BadRequestException($"Only can generate student status mapping for active academic year ({getCurrentAY.AcademicYear})");

        //        var currentAcademicYearDetail = await _dbContext.Entity<MsAcademicYear>()
        //                               .Where(x => x.Id == getCurrentAY.AcademicYearId)
        //                               .FirstOrDefaultAsync(CancellationToken);

        //        var intParamPrevAY = int.Parse(currentAcademicYearDetail.Code) - 1;
        //        var paramPrevAYString = intParamPrevAY.ToString();

        //        var studentAlreadyMappedActiveAYList = await _dbContext.Entity<TrStudentStatus>()
        //                                                .Where(x => x.IdAcademicYear == getCurrentAY.AcademicYearId)
        //                                                .Select(x => x.IdStudent)
        //                                                .Distinct()
        //                                                .ToListAsync(CancellationToken);

        //        // get exclude map status
        //        /*
        //         * Tidak dapat di map apabila latest student status:
        //         * - Expelled           (2)
        //         * - KURNAS Graduate    (3)
        //         * - Transfer (Resign)  (5)
        //         * - Graduate           (7)
        //         * - Pass Away          (9)
        //         */
        //        int[] excludeMapStudentStatus = { 2, 3, 5, 7, 9 };

        //        // get active status
        //        /*
        //         * ActiveStatus = 1 bila:
        //         * - Admission Student          (0)
        //         * - Active                     (1) 
        //         * - Not Graduate               (4)
        //         * - Incomplete Payment         (6)
        //         * - Repeat                     (8)
        //         * - Conditional Progression    (10)
        //         * - Progression                (11)
        //         * - Withdrawal Process         (13)
        //         * - Non UN                     (14)
        //         */
        //        int[] activeStudentStatus = { 0, 1, 4, 6, 8, 10, 11, 13, 14 };

        //        var prevAYStudentStatusList = _dbContext.Entity<TrStudentStatus>()
        //                                        .Include(x => x.Student)
        //                                        .Include(x => x.AcademicYear)
        //                                        .Where(x => x.AcademicYear.Code == paramPrevAYString &&
        //                                                    !studentAlreadyMappedActiveAYList.Contains(x.IdStudent) &&
        //                                                    !excludeMapStudentStatus.Any(y => y == x.IdStudentStatus) &&
        //                                                    x.CurrentStatus == "A")
        //                                        .ToList()
        //                                        //.GroupBy(x => x.IdStudent, (key, y) => y.OrderByDescending(a => a.StartDate).ThenByDescending(a => a.DateIn).First())
        //                                        .GroupBy(x => x.IdStudent)
        //                                        .Select(x => x.OrderByDescending(a => a.StartDate).ThenByDescending(a => a.DateIn).FirstOrDefault())
        //                                        .ToList();

        //        var studentPeriodList = await _dbContext.Entity<MsHomeroomStudent>()
        //                                    .Include(x => x.Homeroom)
        //                                    .ThenInclude(x => x.Grade)
        //                                    .ThenInclude(x => x.MsLevel)
        //                                    .ThenInclude(x => x.MsAcademicYear)
        //                                    .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == getCurrentAY.AcademicYearId &&
        //                                                prevAYStudentStatusList.Select(y => y.IdStudent).Any(y => y == x.IdStudent))
        //                                    .Join(_dbContext.Entity<MsPeriod>()
        //                                                .GroupBy(x => x.IdGrade)
        //                                                .Select(x => new
        //                                                {
        //                                                    IdGrade = x.Key,
        //                                                    MinAYStartDate = x.Min(x => x.StartDate),
        //                                                    MaxAYEndDate = x.Max(x => x.EndDate),
        //                                                }),
        //                                                homeroomStudent => homeroomStudent.Homeroom.IdGrade,
        //                                                period => period.IdGrade,
        //                                                (homeroomStudent, period) => new
        //                                                {
        //                                                    IdStudent = homeroomStudent.IdStudent,
        //                                                    IdGrade = period.IdGrade,
        //                                                    MinAYStartDate = period.MinAYStartDate,
        //                                                    MaxAYEndDate = period.MaxAYEndDate,
        //                                                })
        //                                    .Distinct()
        //                                    .ToListAsync();

        //        // insert new student status for active AY
        //        var generateNewStudentStatusList = prevAYStudentStatusList
        //                                        .Select(x => new TrStudentStatus
        //                                        {
        //                                            IdTrStudentStatus = Guid.NewGuid().ToString(),
        //                                            IdAcademicYear = getCurrentAY.AcademicYearId,
        //                                            IdStudent = x.IdStudent,
        //                                            // untuk latest student status Repeat (8) dan Progression (11), ubah statusnya menjadi Active (1)
        //                                            IdStudentStatus = (x.IdStudentStatus == 8 || x.IdStudentStatus == 11) ? x.IdStudentStatus = 1 : x.IdStudentStatus,
        //                                            StartDate = studentPeriodList.Where(y => y.IdStudent == x.IdStudent).Select(y => y.MinAYStartDate).FirstOrDefault(),
        //                                            EndDate = null,
        //                                            CurrentStatus = "A",
        //                                            Remarks = null,
        //                                            ActiveStatus = activeStudentStatus.Any(y => y == x.IdStudentStatus)
        //                                        })
        //                                        .ToList();

        //        _dbContext.Entity<TrStudentStatus>().AddRange(generateNewStudentStatusList);

        //        // update latest student status to inactive
        //        var updatePrevStudentStatusList = prevAYStudentStatusList
        //                                            .Select(x => new TrStudentStatus
        //                                            {
        //                                                IdTrStudentStatus = x.IdTrStudentStatus,
        //                                                IdAcademicYear = x.IdAcademicYear,
        //                                                IdStudent = x.IdStudent,
        //                                                IdStudentStatus = x.IdStudentStatus,
        //                                                StartDate = x.StartDate,
        //                                                EndDate = studentPeriodList.Where(y => y.IdStudent == x.IdStudent).Select(y => y.MinAYStartDate).FirstOrDefault().AddDays(-1),
        //                                                CurrentStatus = "H",
        //                                                Remarks = x.Remarks,
        //                                                ActiveStatus = x.ActiveStatus
        //                                            })
        //                                        .ToList();

        //        _dbContext.Entity<TrStudentStatus>().UpdateRange(updatePrevStudentStatusList);

        //        await _dbContext.SaveChangesAsync(CancellationToken);
        //        await _transaction.CommitAsync(CancellationToken);

        //        return Request.CreateApiResult2();
        //    }
        //    catch (Exception ex)
        //    {
        //        _transaction?.Rollback();
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        _transaction?.Dispose();
        //    }

        //    throw new NotImplementedException();
        //}
        #endregion
    }
}
