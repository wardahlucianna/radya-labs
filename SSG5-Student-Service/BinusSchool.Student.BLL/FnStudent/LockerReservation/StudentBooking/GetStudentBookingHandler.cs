using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.StudentBooking
{
    public class GetStudentBookingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentBookingHandler(IStudentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentBookingRequest>
             (nameof(GetStudentBookingRequest.IdSchool), nameof(GetStudentBookingRequest.IdStudent));

            GetStudentBookingResult ReturnResult = new GetStudentBookingResult();

            var GradeActive = await _dbContext.Entity<MsPeriod>()
             .Include(x => x.Grade)
                 .ThenInclude(y => y.MsLevel)
                 .ThenInclude(y => y.MsAcademicYear)
             .Where(a => a.StartDate <= _dateTime.ServerTime 
                     && a.EndDate >= _dateTime.ServerTime
                     && a.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
             .Select(a => new
             {
                 IdAcademicYear = a.Grade.MsLevel.IdAcademicYear,
                 a.IdGrade,
                 Semester = a.Semester.ToString(),
                 a.StartDate,
                 a.EndDate
                 
             })
             .ToListAsync(CancellationToken);

            if(GradeActive == null)
            {
                ReturnResult.Msg = "Locker reservation period has not started yet";
                return Request.CreateApiResult2(ReturnResult as object);
            }

            var StudentGrade = _dbContext.Entity<MsHomeroomStudent>()
            .Include(x => x.Homeroom)
            .Where(a => GradeActive.Select(b => b.IdGrade + "#" + b.Semester).Contains(a.Homeroom.IdGrade + "#" + a.Semester.ToString())
            && a.IdStudent == param.IdStudent
            )
            .Select(a => new {                
                a.Homeroom.Grade.MsLevel.IdAcademicYear,
                a.Semester,
                a.Homeroom.IdGrade,
                a.IdHomeroom,  
                })
            .FirstOrDefault();

            if (StudentGrade == null)
            {
                ReturnResult.Msg = "Period active has not found";
                return Request.CreateApiResult2(ReturnResult as object);
            }


            var ReservationPeriod = await _dbContext.Entity<MsLockerReservationPeriod>()
                .Include(a => a.Grade)
                    .ThenInclude(b => b.MsLevel) 
                .Where(a => a.IdGrade == StudentGrade.IdGrade
                && a.Semester == StudentGrade.Semester)
                .ToListAsync(CancellationToken);

            var ReservationActive  = ReservationPeriod
                    .Where(a => a.StartDate <= _dateTime.ServerTime && a.EndDate >= _dateTime.ServerTime)
                    .OrderBy(a => a.StartDate)
                    .ToList();


            var getStudentLockerReservation = await _dbContext.Entity<TrStudentLockerReservation>()
                .Include(a => a.Grade)
                    .ThenInclude(b => b.MsLevel)
                .Include(a => a.Locker)
                    .ThenInclude(b => b.Floor)
                    .ThenInclude(b => b.Building)
                .Include(a => a.Locker)
                    .ThenInclude(b => b.LockerPosition)
                .Where(a => a.IdGrade == StudentGrade.IdGrade
                && a.Semester == StudentGrade.Semester
                && a.IdAcademicYear == StudentGrade.IdAcademicYear
                && a.IdStudent == param.IdStudent)
                .ToListAsync(CancellationToken);
                        

            ReturnResult.AvailableBooking = false;
            if (getStudentLockerReservation.Count() > 0)
            {
                //reserved
                ReturnResult.Msg = "You have already reserved  a locker. <br>if there's any problem, please contact GOM or Building Management<br><br>";
                ReturnResult.Msg += "Locker Number : "+ getStudentLockerReservation.First().Locker.LockerName + "<br>";
                ReturnResult.Msg += "Locker Location : " + (getStudentLockerReservation.First().Locker.Floor.FloorName.Contains("floor") ? getStudentLockerReservation.First().Locker.Floor.FloorName : "Floor "+ getStudentLockerReservation.First().Locker.Floor.FloorName) + " - " + getStudentLockerReservation.First().Locker.Floor.Building.Description + "<br>";
                ReturnResult.Msg += "Locker Position : " + getStudentLockerReservation.First().Locker.LockerPosition.PositionName + "<br>";
            }
            else if (ReservationActive.Count() > 0)
            {
                //available to booking
                ReturnResult.AvailableBooking = true;
                ReturnResult.StudentData = new StudentBooking_StudDataVm()
                {
                    IdAcademicYear = StudentGrade.IdAcademicYear,
                    Semester = StudentGrade.Semester,
                    IdGrade = StudentGrade.IdGrade,
                    IdHomeroom = StudentGrade.IdHomeroom
                };

                ReturnResult.BookingPeriod = ((DateTime)ReservationActive.First().StartDate).ToString("dd-MMM-yyyy HH:mm") + " - " + ((DateTime)ReservationActive.First().EndDate).ToString("dd-MMM-yyyy HH:mm");

                var getLockerAllocation = await _dbContext.Entity<MsLockerAllocation>()
                    .Include(a => a.Grade)
                        .ThenInclude(b => b.MsLevel)
                    .Where(a => a.IdGrade == StudentGrade.IdGrade
                    && a.Semester == StudentGrade.Semester)
                    .Select(a => new
                    {
                        a.IdFloor,
                        a.Floor.FloorName,
                        a.Floor.IdBuilding,
                        BuildingName = a.Floor.Building.Description                       
                    })
                    .ToListAsync(CancellationToken);

                var getAvailableLocker = _dbContext.Entity<MsLocker>()
                                        .Include(x => x.Building)
                                        .Include(x => x.Floor)
                                        .Where(a =>
                                        a.IdAcademicYear == StudentGrade.IdAcademicYear
                                        && a.Semester == StudentGrade.Semester
                                        && getLockerAllocation.Select(b => b.IdFloor + "#" + b.IdBuilding).Contains(a.IdFloor + "#" + a.Floor.IdBuilding)
                                        && a.IsLocked == false
                                        && !a.StudentLockerReservations.Any())
                                        .Select(a => new
                                        {
                                            a.IdFloor,
                                            a.Floor.FloorName,
                                            a.Floor.IdBuilding,
                                            BuildingName = a.Floor.Building.Description,
                                            a.IdLockerPosition,
                                            a.LockerPosition.PositionName,
                                            IdLocker = a.Id,
                                            a.LockerName
                                        })
                                        .ToList();

                if(getAvailableLocker.Count > 0)
                {
                    var CurrLockerLocationAvailable = getAvailableLocker.GroupBy(a => new
                    {
                        a.IdFloor,
                        a.FloorName,
                        a.IdBuilding,
                        a.BuildingName
                    })
                      .Select(a => new StudentBooking_LockerLocationVm()
                      {
                          LockerLocation = new CodeWithIdVm()
                          {
                              Id = a.Key.IdFloor + "#" + a.Key.IdBuilding,
                              Code = (a.Key.FloorName.Contains("floor") ? a.Key.FloorName : "Floor " + a.Key.FloorName) + " - " + a.Key.BuildingName
                          },
                          LockerPosition = a.GroupBy(b => new { b.IdLockerPosition, b.PositionName })
                                            .Select(c => new CodeWithIdVm { 
                                             Id = c.Key.IdLockerPosition,
                                             Code = c.Key.PositionName,
                                             Description = c.Select(d => d.IdLocker).Count().ToString()
                                            })
                                            .OrderByDescending(a => a.Code)
                                            .ToList()


                      }).ToList();

                    ReturnResult.LockerLocations = CurrLockerLocationAvailable;
                }            
                else
                {

                    var getLockerPosition = await _dbContext.Entity<LtLockerPosition>()                                
                                .Select(a => new CodeWithIdVm {
                                    Id = a.Id,
                                    Code = a.PositionName,
                                    Description = "0" 
                                })
                                .OrderByDescending(a => a.Code)
                                .ToListAsync();

                    ReturnResult.LockerLocations = getLockerAllocation.GroupBy(a => new { a.IdFloor, a.IdBuilding, a.FloorName, a.BuildingName })
                                                                       .Select(a => new StudentBooking_LockerLocationVm()
                                                                       {
                                                                           LockerLocation = new CodeWithIdVm() { Id = a.Key.IdFloor+ "#"+ a.Key.IdBuilding,
                                                                                                                Code = a.Key.FloorName + " - " + a.Key.BuildingName},
                                                                           LockerPosition = getLockerPosition

                                                                       }).ToList();
                }



            }
            else if (ReservationPeriod.Where(a => a.StartDate > _dateTime.ServerTime).ToList().Count() > 0)
            {
                var OpenReservation = ReservationPeriod.Where(a => a.StartDate > _dateTime.ServerTime)
                                    .OrderBy(a => a.StartDate) 
                                    .First();
                ReturnResult.Msg = "The locker reservation period will be open on " + ((DateTime)OpenReservation.StartDate).ToString("dd-MMM-yyyy hh:mm tt") + " until " + ((DateTime)OpenReservation.EndDate).ToString("dd-MMM-yyyy hh:mm tt");
            }
            else if (ReservationPeriod.Where(a => a.EndDate < _dateTime.ServerTime).ToList().Count() > 0)
            {
                var OpenReservation = ReservationPeriod.Where(a => a.EndDate < _dateTime.ServerTime)
                                    .OrderByDescending(a => a.EndDate)
                                    .First();
                ReturnResult.Msg = "The locker reservation System has already closed on " + ((DateTime)OpenReservation.StartDate).ToString("dddd, dd-MMMM-yyyy HH:mm") + "<br><br>";
                ReturnResult.Msg += "It seems you still have not reserved a locker. <br> ";
                ReturnResult.Msg += "Please contact GOM for confirmation before going to Build Management.";
            }
            else
            {
                ReturnResult.Msg = "Locker reservation period has not started yet";
            }

            if (ReturnResult.AvailableBooking)
            {
                var getPolicyMessage = _dbContext.Entity<MsLockerReservationPeriod>()                          
                                     .Where(a =>
                                     a.IdAcademicYear == StudentGrade.IdAcademicYear
                                     && a.Semester == StudentGrade.Semester
                                     && a.IdGrade == StudentGrade.IdGrade
                                     && a.StartDate <= _dateTime.ServerTime
                                     && a.EndDate >= _dateTime.ServerTime
                                     )
                                     .Select(a => a.PolicyMessage)
                                     .FirstOrDefault();

                ReturnResult.PolicyMessage = getPolicyMessage;


            }


            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
