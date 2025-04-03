using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation
{
    public class DeleteLockerAllocationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteLockerAllocationHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteLockerAllocationRequest, DeleteLockerAllocationValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getLocker = await _dbContext.Entity<MsLocker>()
                    .Where(a => a.IdAcademicYear == body.IdAcademicYear
                        && a.Semester == body.Semester
                        && a.IdBuilding == body.IdBuilding
                        && a.IdFloor == body.IdFloor)
                    .ToListAsync(CancellationToken);

                var getStudentLockerReservation = await _dbContext.Entity<TrStudentLockerReservation>()
                    .Where(a => a.IdAcademicYear == body.IdAcademicYear
                        && a.Semester == body.Semester
                        && getLocker.Select(a => a.Id).Contains(a.IdLocker))
                    .ToListAsync(CancellationToken);

                if (getStudentLockerReservation.Any())
                {
                    throw new Exception("Can't delete. Locker(s) have been reserved in this period.");
                }

                var getLockerAllocation = await _dbContext.Entity<MsLockerAllocation>()
                    .Where(a => a.IdAcademicYear == body.IdAcademicYear
                        && a.Semester == body.Semester
                        && a.IdBuilding == body.IdBuilding
                        && a.IdFloor == body.IdFloor)
                    .ToListAsync(CancellationToken);

                foreach (var items in getLockerAllocation)
                {
                    items.IsActive = false;

                    _dbContext.Entity<MsLockerAllocation>().Update(items);
                }

                foreach (var items in getLocker)
                {
                    items.IsActive = false;

                    _dbContext.Entity<MsLocker>().Update(items);
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
