using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class MeritStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public MeritStudentHandler(IStudentDbContext MeritStudentDbContext)
        {
            _dbContext = MeritStudentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMeritStudentRequest, AddMeritStudentValidator>();

            List<string> NoSaveHomeroomStudent = new List<string>();
            List<TrEntryMeritStudent> EntryMeritStudent = new List<TrEntryMeritStudent>();
            List<TrEntryDemeritStudent> EntryDemeritStudent = new List<TrEntryDemeritStudent>();
            List<TrStudentPoint> StudentPoint = new List<TrStudentPoint>();
            List<AddMeritDemeritTeacherResult> MeritDemeritTeacherResult = new List<AddMeritDemeritTeacherResult>();

            var GetApproval = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
                  .Where(e => e.IdLevel == body.MeritStudents.Select(x => x.IdLevel).First())
                  .SingleOrDefaultAsync(CancellationToken);

            var GetSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                    .Where(e => e.IdAcademicYear == body.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            #region Merit
            var GetPoint = await _dbContext.Entity<TrStudentPoint>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                .Where(e => body.MeritStudents.Select(e => e.IdHomeroomStudent).Contains(e.IdHomeroomStudent) 
                    && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear==body.IdAcademicYear)
                .Select(e => new
                {
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    LevelOfInfractin = e.LevelOfInteraction,
                    MeritPoint = e.MeritPoint,
                    Semester = e.HomeroomStudent.Homeroom.Semester,
                    Merit = e,
                })
                .ToListAsync(CancellationToken);

            if(body.MeritStudents != null && body.MeritStudents.Any())
            {
                foreach (var ItemBodyMeritDemeritTeacher in body.MeritStudents)
                {
                    var IdEntryMeritStudent = Guid.NewGuid().ToString();
                    var NewEntryMeritStudent = new TrEntryMeritStudent
                    {
                        Id = IdEntryMeritStudent,
                        IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                        IdMeritDemeritMapping = ItemBodyMeritDemeritTeacher.IdMeritDemeritMapping,
                        Point = ItemBodyMeritDemeritTeacher.Point,
                        Note = ItemBodyMeritDemeritTeacher.Note,
                        RequestType = 0,
                        Status = "Approved",
                        IsHasBeenApproved = true
                    };
                    _dbContext.Entity<TrEntryMeritStudent>().Add(NewEntryMeritStudent);

                    var GetPointByStudentId = GetPoint.SingleOrDefault(e => e.IdHomeroomStudent == ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                    var MeritPoint = GetPointByStudentId == null ? ItemBodyMeritDemeritTeacher.Point : Convert.ToInt32(GetPointByStudentId.MeritPoint) + ItemBodyMeritDemeritTeacher.Point;
                    if (GetPointByStudentId == null)
                    {

                        var NewStudentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                            MeritPoint = MeritPoint,
                        };

                        _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);
                    }
                    else
                    {
                        GetPointByStudentId.Merit.MeritPoint = MeritPoint;
                        _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Merit);
                    }
                }
            }
            #endregion

            var GetDataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Where(e => NoSaveHomeroomStudent.Contains(e.Id))
                    .ToListAsync(CancellationToken);
            foreach (var ItemIdHomeroom in NoSaveHomeroomStudent)
            {
                var GetDataStudentById = GetDataStudent.SingleOrDefault(e => e.Id == ItemIdHomeroom);

                MeritDemeritTeacherResult.Add(new AddMeritDemeritTeacherResult
                {
                    IdBinusan = GetDataStudentById.Student.Id,
                    Name = (GetDataStudentById.Student.FirstName == null ? "" : GetDataStudentById.Student.FirstName) + (GetDataStudentById.Student.MiddleName == null ? "" : " " + GetDataStudentById.Student.MiddleName) + (GetDataStudentById.Student.LastName == null ? "" : " " + GetDataStudentById.Student.LastName),
                });
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(MeritDemeritTeacherResult as object);
        }
    }
}
