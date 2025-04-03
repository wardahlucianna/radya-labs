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
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentStatus.SendEmail;
using BinusSchool.Student.BLL.FnStudent.StudentStatus.SendEmail;
using BinusSchool.Student.DAL.Entities;
using BinusSchool.Student.FnStudent.StudentStatus.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class UpdateStudentStatusHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IStudentDbContext _dbContext;
        private readonly ICurrentAcademicYear _currentAcademicYearApi;
        private readonly IMachineDateTime _dateTime;
        private readonly SendEmailStudentStatusSpecialNeedFutureAdmission _sendEmail;

        public UpdateStudentStatusHandler(IStudentDbContext dbContext, ICurrentAcademicYear currentAcademicYearApi, IMachineDateTime dateTime, SendEmailStudentStatusSpecialNeedFutureAdmission sendEmail)
        {
            _dbContext = dbContext;
            _currentAcademicYearApi = currentAcademicYearApi;
            _dateTime = dateTime;
            _sendEmail = sendEmail;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateStudentStatusRequest, UpdateStudentStatusValidator>();

            using (_transaction = await _dbContext.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var getCurrentAYApi = await _currentAcademicYearApi.GetActiveAcademicYear(new GetActiveAcademicYearRequest
                    {
                        SchoolID = param.IdSchool
                    });

                    var getCurrentAY = getCurrentAYApi.Payload;

                    // validate academic year
                    if (getCurrentAY.AcademicYearId.ToUpper().Trim() != param.IdAcademicYear)
                        throw new BadRequestException("Cannot update student status for past/future academic year");

                    // validate start date
                    if (param.NewStatusStartDate.Date > _dateTime.ServerTime.Date)
                        throw new BadRequestException("Student status start date cannot be greater than today");

                    // validate student period
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

                    if (param.NewStatusStartDate < gerPeriodAY.MinAYStartDate)
                        throw new BadRequestException($"Student status start date must be greater or equal to active academic year start date ({gerPeriodAY.MinAYStartDate.ToString("yyyy-MM-dd")})");

                    // validate latest student status
                    var latestStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                                .Include(x => x.Student)
                                                .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                            x.IdStudent == param.IdStudent &&
                                                            x.CurrentStatus == "A")
                                                .OrderByDescending(x => x.StartDate)
                                                .ThenByDescending(x => x.DateIn)
                                                .FirstOrDefaultAsync(CancellationToken);

                    if (param.NewStatusStartDate < latestStudentStatus.StartDate)
                        throw new BadRequestException($"Student status start date must be greater or equal to latest student status start date (Latest: {latestStudentStatus.StartDate.ToString("yyyy-MM-dd")})");

                    if (param.IdStudentStatus == latestStudentStatus.IdStudentStatus)
                        throw new BadRequestException($"Cannot insert the same status as the latest status");

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
                     * - Withdrawal Process(Hidden) (16)
                     */
                    int[] activeStudentStatus = { 0, 1, 4, 6, 8, 10, 11, 13, 14, 16 };

                    var isActiveStudentStatus = activeStudentStatus.Any(x => x == param.IdStudentStatus);

                    // insert TrStudentStatus
                    var newTrStudentStatus = new TrStudentStatus
                    {
                        IdTrStudentStatus = Guid.NewGuid().ToString(),
                        IdAcademicYear = param.IdAcademicYear,
                        IdStudent = param.IdStudent,
                        IdStudentStatus = param.IdStudentStatus,
                        StartDate = param.NewStatusStartDate.Date,
                        EndDate = null,
                        CurrentStatus = "A",
                        Remarks = param.Remarks,
                        ActiveStatus = isActiveStudentStatus,
                        IdStudentStatusSpecial = param.IdStudentStatusSpecial,
                    };
                    _dbContext.Entity<TrStudentStatus>().Add(newTrStudentStatus);

                    // update old StudentStatus in TrStudentStatus
                    latestStudentStatus.CurrentStatus = "H";
                    latestStudentStatus.EndDate = param.NewStatusStartDate.Date == latestStudentStatus.StartDate.Date ? param.NewStatusStartDate.Date : param.NewStatusStartDate.AddDays(-1).Date;
                    _dbContext.Entity<TrStudentStatus>().Update(latestStudentStatus);

                    // update StudentStatus in MsStudent
                    var updateStudent = latestStudentStatus.Student;
                    updateStudent.IdStudentStatus = param.IdStudentStatus;
                    _dbContext.Entity<MsStudent>().Update(updateStudent);

                    // check need future admission
                    bool isNeedFutureAdmission = await _dbContext.Entity<LtStudentStatusSpecial>()
                        .Where(a => a.IdStudentStatusSpecial == param.IdStudentStatusSpecial)
                        .Select(a => a.NeedFutureAdmissionDecision)
                        .FirstOrDefaultAsync();

                    if (isNeedFutureAdmission)
                    {
                        var emailRequest = new SendEmailStudentStatusSpecialNeedFutureAdmissionRequest
                        {
                            IdAcademicYear = param.IdAcademicYear,
                            IdSchool = param.IdSchool,
                            IdStudent = param.IdStudent,
                            IdStudentStatusSpecial = param.IdStudentStatusSpecial.Value,
                        };

                        try
                        {
                            await _sendEmail.SendEmail(emailRequest);
                        }
                        catch
                        {
                            throw new BadRequestException($"Send email to principal is FAILED.");
                        }
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);

                    return Request.CreateApiResult2();
                }
                catch (Exception ex)
                {
                    _transaction?.RollbackAsync(CancellationToken);

                    throw new Exception($"{ex.Message.ToString()}{Environment.NewLine}{ex.InnerException?.Message ?? ""}");
                }
            }
        }
        #region unused code
        //protected override async Task<ApiErrorResult<object>> Handler()
        //{
        //    var param = await Request.ValidateBody<UpdateStudentStatusRequest, UpdateStudentStatusValidator>();

        //    try
        //    {
        //        _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

        //        var getCurrentAYApi = await _currentAcademicYearApi.GetActiveAcademicYear(new GetActiveAcademicYearRequest
        //        {
        //            SchoolID = param.IdSchool
        //        });

        //        var getCurrentAY = getCurrentAYApi.Payload;

        //        // validate academic year
        //        if (getCurrentAY.AcademicYearId.ToUpper().Trim() != param.IdAcademicYear)
        //            throw new BadRequestException("Cannot update student status for past/future academic year");

        //        // validate start date
        //        if (param.NewStatusStartDate.Date >= _dateTime.ServerTime.Date)
        //            throw new BadRequestException("Student status start date cannot be greater than today");

        //        // validate student period
        //        var studentPeriod = await _dbContext.Entity<MsHomeroomStudent>()
        //                                    .Include(x => x.Homeroom)
        //                                    .ThenInclude(x => x.Grade)
        //                                    .ThenInclude(x => x.MsLevel)
        //                                    .ThenInclude(x => x.MsAcademicYear)
        //                                    .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear)
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
        //                                                    IdGrade = period.IdGrade,
        //                                                    MinAYStartDate = period.MinAYStartDate,
        //                                                    MaxAYEndDate = period.MaxAYEndDate,
        //                                                })
        //                                        .FirstOrDefaultAsync(CancellationToken);

        //        if (param.NewStatusStartDate < studentPeriod.MinAYStartDate)
        //            throw new BadRequestException($"Student status start date must be greater or equal to active academic year start date ({studentPeriod.MinAYStartDate.ToString("dd MMM yyyy")})");

        //        // validate latest student status
        //        var latestStudentStatus = await _dbContext.Entity<TrStudentStatus>()
        //                                    .Include(x => x.Student)
        //                                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
        //                                                x.IdStudent == param.IdStudent)
        //                                    .OrderByDescending(x => x.StartDate)
        //                                    .ThenByDescending(x => x.UserIn)
        //                                    .FirstOrDefaultAsync(CancellationToken);

        //        if (param.NewStatusStartDate < latestStudentStatus.StartDate)
        //            throw new BadRequestException($"Student status start date must be greater or equal to latest student status start date (Latest: {latestStudentStatus.StartDate.ToString("dd MMM yyyy")})");

        //        if (param.IdStudentStatus == latestStudentStatus.IdStudentStatus)
        //            throw new BadRequestException($"Cannot insert the same status as the latest status");

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

        //        var isActiveStudentStatus = activeStudentStatus.Any(x => x == param.IdStudentStatus);

        //        // insert TrStudentStatus
        //        var newTrStudentStatus = new TrStudentStatus
        //        {
        //            IdTrStudentStatus = Guid.NewGuid().ToString(),
        //            IdAcademicYear = param.IdAcademicYear,
        //            IdStudent = param.IdStudent,
        //            IdStudentStatus = param.IdStudentStatus,
        //            StartDate = param.NewStatusStartDate.Date,
        //            EndDate = null,
        //            CurrentStatus = "A",
        //            Remarks = param.Remarks,
        //            ActiveStatus = isActiveStudentStatus
        //        };
        //        _dbContext.Entity<TrStudentStatus>().Add(newTrStudentStatus);

        //        // update old StudentStatus in TrStudentStatus
        //        latestStudentStatus.CurrentStatus = "H";
        //        latestStudentStatus.EndDate = param.NewStatusStartDate.AddDays(-1).Date;
        //        _dbContext.Entity<TrStudentStatus>().Update(latestStudentStatus);

        //        // update StudentStatus in MsStudent
        //        var updateStudent = latestStudentStatus.Student;
        //        updateStudent.IdStudentStatus = param.IdStudentStatus;
        //        _dbContext.Entity<MsStudent>().Update(updateStudent);

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
        //}
        #endregion
    }
}
