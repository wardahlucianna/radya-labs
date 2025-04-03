using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class CopyLockerReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public CopyLockerReservationHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CopyLockerReservationRequest, CopyLockerReservationValidator>();

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = 1
            });

            var getLockerSmt2 = await _dbContext.Entity<MsLocker>()
                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => x.Semester == 1)
                        .Select(x => new
                        {
                            IdLocker = x.Id,
                            LockerName = x.LockerName,
                            IdLockerPosition = x.IdLockerPosition,
                            LockerPosition = x.LockerPosition.LockerPosition,
                            LockerPositionName = x.LockerPosition.PositionName,
                            IdFloor = x.IdFloor,
                            FloorName = x.Floor.FloorName,
                            IdBuilding = x.IdBuilding,
                            BuildingName = x.Building.Description,
                            Status = x.IsLocked,
                            Semester = x.Semester
                        })
                        .ToListAsync(CancellationToken);

            var lockerReservationList = new List<TrStudentLockerReservation>();
            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.CopyLocker.Count() > 0)
                {
                    foreach(var item in param.CopyLocker)
                    {
                        string[] arrayOfStrings = item.Split('#');
                        if (arrayOfStrings.Length != 2)
                            throw new BadRequestException("Some of Id Not in Correct Format");

                        var anonymousType = new { IdFloor = arrayOfStrings[0], IdBuilding = arrayOfStrings[1]};

                        getStudentMasterDataForHeaderReport = getStudentMasterDataForHeaderReport
                            .Where(x => x.IdStudentLockerReservation != null)
                            .Where(x => x.IdBuilding == anonymousType.IdBuilding)
                            .Where(x => x.IdFloor == anonymousType.IdFloor)
                            .ToList();

                        foreach (var data in getStudentMasterDataForHeaderReport)
                        {
                            var isLockerSmt2Exist = getLockerSmt2
                                            .Where(x => x.IdBuilding == anonymousType.IdBuilding)
                                            .Where(x => x.IdFloor == anonymousType.IdFloor)
                                            .Where(x => x.IdLockerPosition == data.IdLockerPosition)
                                            .Where(x => x.Semester == 2)
                                            .Where(x => x.LockerName == data.LockerName)
                                            .FirstOrDefault();

                            if (isLockerSmt2Exist != null)
                            {
                                var lockerReservation = new TrStudentLockerReservation();

                                lockerReservation = new TrStudentLockerReservation()
                                {
                                    IdLocker = isLockerSmt2Exist.IdLocker,
                                    IdStudent = data.IdStudent,
                                    IdGrade = data.IdGrade,
                                    IdHomeroom = data.IdHomeroom,
                                    IdReserver = data.IdReserver,
                                    IsAgree = data.IsAgree,
                                    Notes = data.Notes
                                };

                                lockerReservationList.Add(lockerReservation);
                            }
                        }
                    }
                }

                _dbContext.Entity<TrStudentLockerReservation>().AddRange(lockerReservationList);

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
