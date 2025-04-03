using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Common.Utils;

namespace BinusSchool.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class GetListPickedUpHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListPickedUpHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListPickedUpRequest>(
                nameof(GetListPickedUpRequest.Date), nameof(GetListPickedUpRequest.Status), nameof(GetListPickedUpRequest.IdSchool)
            );


            var res = await _dbContext.Entity<TrDigitalPickup>()
                        .Include(x => x.AcademicYear)
                        .Where(x => x.Date.Date == param.Date.Date && x.AcademicYear.IdSchool == param.IdSchool)
                        .Where(x => param.Status != "1" ? x.PickupTime == null : x.PickupTime != null)
                        .Join(
                            _dbContext.Entity<MsHomeroomStudent>()
                            .Include(x => x.Homeroom)
                                .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.MsLevel)
                            .Include(x => x.Homeroom)
                                .ThenInclude(x => x.MsGradePathwayClassroom)
                                .ThenInclude(x => x.Classroom)
                            .Include(x => x.Student)
                            .Where(x => x.Homeroom.Grade.IdLevel == (param.IdLevel == null ? x.Homeroom.Grade.IdLevel : param.IdLevel))
                            .Where(x => x.Homeroom.IdGrade == (param.IdGrade == null ? x.Homeroom.IdGrade : param.IdGrade))
                            .Where(x => x.IdHomeroom == (param.IdHomeroom == null ? x.IdHomeroom : param.IdHomeroom)),
                            pickup => new { pickup.IdStudent, pickup.IdAcademicYear, pickup.Semester },
                            student => new { student.IdStudent, student.Homeroom.Grade.MsLevel.IdAcademicYear, student.Semester },
                            (pickup, student) => new GetListPickedUpResult
                            {
                                IdDigitalPickUp = pickup.Id,
                                StudentName = student.IdStudent + " - " + NameUtil.GenerateFullName(student.Student.FirstName, student.Student.MiddleName, student.Student.LastName),
                                Homeroom = new ItemValueVm
                                {
                                    Id = student.Homeroom.Id,
                                    Description = student.Homeroom.Grade.Code + student.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                },
                                QrScanTime = pickup.QrScanTime,
                                PickupTime = pickup.PickupTime
                            }
                        ).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);
        }
    }
}
