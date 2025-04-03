using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class GetStudentLockerDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentLockerDataHandler(
            IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentLockerDataRequest, GetStudentLockerDataValidator>();
            var result = await GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester
            });
            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetStudentLockerDataResult>> GetStudentLockerData(GetStudentLockerDataRequest param)
        {
            var getLocker = await _dbContext.Entity<MsLocker>()
                        .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => x.Semester == param.Semester)
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
                            Status = x.IsLocked
                        })
                        .ToListAsync(CancellationToken);

            var getStudentLockerReservation = await _dbContext.Entity<TrStudentLockerReservation>()
                        .Where(x => 
                            x.IdAcademicYear == param.IdAcademicYear &&
                            x.Semester == param.Semester)
                        .Select(x => new
                        {
                            IdStudentLockerReservation = x.Id,
                            IdLocker = x.IdLocker,
                            IdStudent = x.IdStudent,
                            StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.LastName),
                            IdGrade = x.IdGrade,
                            GradeName = x.Grade.Description,
                            IdHomeroom = x.IdHomeroom,
                            HomeroomName = x.Homeroom.Grade.Code + " " + x.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                            IdReserver = x.IdReserver,
                            IsAgree = x.IsAgree,
                            Notes = x.Notes
                        }).ToListAsync(CancellationToken);

            var getStudentLockerData = getLocker
                        .GroupJoin(getStudentLockerReservation,
                            c => (c.IdLocker),
                            s => (s.IdLocker),
                            (c1, t1) => new { c = c1, ts = t1 }
                        ).SelectMany(c1 => c1.ts.DefaultIfEmpty(),
                        (c1, t1) => new GetStudentLockerDataResult
                        {
                            IdLocker = c1.c.IdLocker,
                            LockerName = c1.c.LockerName,
                            IdLockerPosition = c1.c.IdLockerPosition,
                            LockerPosition = c1.c.LockerPosition,
                            LockerPositionName = c1.c.LockerPositionName,
                            IdFloor = c1.c.IdFloor,
                            FloorName = c1.c.FloorName,
                            IdBuilding = c1.c.IdBuilding,
                            BuildingName = c1.c.BuildingName,
                            Status = c1.c.Status,
                            IdStudentLockerReservation = t1?.IdStudentLockerReservation ?? null,
                            IdStudent = t1?.IdStudent ?? null,
                            StudentName = t1?.StudentName ?? null,
                            IdGrade = t1?.IdGrade ?? null,
                            GradeName = t1?.GradeName ?? null,
                            IdHomeroom = t1?.IdHomeroom ?? null,
                            HomeroomName = t1?.HomeroomName ?? null,
                            IdReserver = t1?.IdReserver ?? null,
                            IsAgree = t1?.IsAgree ?? false,
                            Notes = t1?.Notes ?? null,
                        }).Distinct().ToList();

            return getStudentLockerData;
        }
    }
}
