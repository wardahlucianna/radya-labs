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
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator;
using BinusSchool.Student.FnStudent.LockerReservation.StudentBooking.Validator;
using HandlebarsDotNet.PathStructure;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.StudentBooking
{
    public class AddStudentReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AddStudentReservationHandler(IStudentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddStudentReservationRequest, AddStudentReservationValidator>();

            var StudentGrade = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .Where(a => a.IdHomeroom == body.IdHomeroom 
                && a.IdStudent == body.IdStudent
                && a.Homeroom.IdGrade == body.IdGrade)
                .FirstOrDefaultAsync(CancellationToken);

            if (StudentGrade == null)
                throw new BadRequestException($"Homeroom Student : {body.IdStudent} not exists");

            string[] LockerLocation = body.IdlockerLocation.Split('#'); //floor # building

            var CheckLocker  = await _dbContext.Entity<MsLocker>()
                                .Where(a => a.IdBuilding == LockerLocation[1]
                                 && a.IdFloor == LockerLocation[0]
                                 && a.IdAcademicYear == body.IdAcademicYear
                                 && a.Semester == body.Semester
                                 && a.IdLockerPosition == body.IdlockerPosition
                                 && a.IsLocked == false
                                 && !a.StudentLockerReservations.Any()
                                )
                                .OrderBy(a => a.LockerName)
                                .Select(a => new TrStudentLockerReservation()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdAcademicYear = body.IdAcademicYear,
                                    Semester = body.Semester,
                                    IdStudent = body.IdStudent,
                                    IdGrade = body.IdGrade,
                                    IdHomeroom = body.IdHomeroom,
                                    IdLocker = a.Id,
                                    IdReserver = (AuthInfo != null ? AuthInfo.UserId ?? body.IdStudent : body.IdStudent),
                                    IsAgree = body.IsAgree,
                                    Notes = body.Notes
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            if(CheckLocker == null)
            {
                throw new BadRequestException($"Locker not available.");
            }

            _dbContext.Entity<TrStudentLockerReservation>().Add(CheckLocker);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
