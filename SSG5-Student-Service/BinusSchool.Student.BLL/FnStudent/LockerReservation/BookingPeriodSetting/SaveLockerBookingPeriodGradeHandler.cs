using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class SaveLockerBookingPeriodGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveLockerBookingPeriodGradeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveLockerBookingPeriodGradeRequest, SaveLockerBookingPeriodGradeValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var getGrade in param.Grades)
                {
                    var predicate = PredicateBuilder.Create<MsGrade>
                    (a => a.MsLevel.IdAcademicYear == param.IdAcademicYear);

                    var grade = _dbContext.Entity<MsGrade>()
                        .Include(a => a.MsLevel)
                        .Where(predicate)
                        .Where(a => a.Id == getGrade)
                        .FirstOrDefault();

                    var lockerReservationPeriod = _dbContext.Entity<MsLockerReservationPeriod>()
                        .Include(a => a.Grade)
                            .ThenInclude(b => b.MsLevel)
                        .Where(a => a.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && a.Semester == param.Semester);

                    if (!lockerReservationPeriod.Select(a => a.IdGrade).Contains(getGrade))
                    {
                        var insertLockerReservationPeriod = new MsLockerReservationPeriod
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            Semester = param.Semester,
                            IdLevel = grade.IdLevel,
                            IdGrade = getGrade,
                            StartDate = null,
                            EndDate = null,
                            PolicyMessage = "-"
                        };

                        _dbContext.Entity<MsLockerReservationPeriod>().Add(insertLockerReservationPeriod);
                    }
                    else
                    {
                        continue;
                    }
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
        }
    }
}
