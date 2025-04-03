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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting
{
    public class GetLockerBookingPeriodSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLockerBookingPeriodSettingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLockerBookingPeriodSettingRequest>
                (nameof(GetLockerBookingPeriodSettingRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsLockerReservationPeriod>
                (a => a.AcademicYear.IdSchool == param.IdSchool);

            var query = _dbContext.Entity<MsLockerReservationPeriod>()
                .Include(a => a.AcademicYear)
                .Where(predicate)
                .Select(a => new GetLockerBookingPeriodSettingResult
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = a.IdAcademicYear,
                        Description = a.AcademicYear.Code
                    },
                    Semester = a.Semester,
                    Grades = new List<GetLockerBookingPeriodSettingResult_Grade>
                    {
                        new GetLockerBookingPeriodSettingResult_Grade
                        {
                            Id = a.IdGrade,
                            Description = a.Grade.Description,
                            OrderNumber = a.Grade.OrderNumber
                        }
                    }
                })
                .ToList();

            var items = query.GroupBy(a => new
            {
                a.AcademicYear.Id,
                a.AcademicYear.Description,
                a.Semester
            })
                .Select(b => new GetLockerBookingPeriodSettingResult
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = b.Key.Id,
                        Description = b.Key.Description
                    },
                    Semester = b.Key.Semester,
                    Grades = b.SelectMany(c => c.Grades)
                        .OrderBy(c => c.OrderNumber)
                        .ToList()
                })
                .OrderByDescending(a => a.AcademicYear.Description)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
