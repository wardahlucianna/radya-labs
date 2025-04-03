using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
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
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritDemeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailEntryMeritDemeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetDetailEntryMeritDemeritRequest, GetDetailEntryMeritDemeritValidator>();

            GetDetailEntryMeritDemeritResult items = null;
            if (body.Category == MeritDemeritCategory.Merit)
            {
                items = await (from EntMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                               join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                               join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                               join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                               join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                               join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                               join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                               join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                               join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                               join UserCreate in _dbContext.Entity<MsUser>() on EntMeritStudent.UserIn equals UserCreate.Id
                               join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                               join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                               join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelOfInfractionParent
                               from LevelOfInfractionParent in JoinedLevelOfInfractionParent.DefaultIfEmpty()
                               where EntMeritStudent.Id == body.Id
                               select new GetDetailEntryMeritDemeritResult
                               {
                                   Id = EntMeritStudent.Id,
                                   AcademicYear = AcademicYear.Description,
                                   RequestType = EntMeritStudent.RequestType.ToString(),
                                   Semester = Homeroom.Semester.ToString(),
                                   CreateBy = UserCreate.DisplayName,
                                   Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                   Homeroom = Grade.Code + Classroom.Code,
                                   Category = "Merit",
                                   LevelOfInfraction = "",
                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                   Point = EntMeritStudent.Point.ToString(),
                                   Note = EntMeritStudent.Note,
                               }).SingleOrDefaultAsync(CancellationToken);
            }
            else
            {
                items = await (from EntDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                               join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                               join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                               join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                               join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                               join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                               join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                               join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                               join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                               join UserCreate in _dbContext.Entity<MsUser>() on EntDemeritStudent.UserIn equals UserCreate.Id
                               join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                               join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                               join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelOfInfractionParent
                               from LevelOfInfractionParent in JoinedLevelOfInfractionParent.DefaultIfEmpty()
                               where EntDemeritStudent.Id == body.Id
                               select new GetDetailEntryMeritDemeritResult
                               {
                                   Id = EntDemeritStudent.Id,
                                   AcademicYear = AcademicYear.Description,
                                   RequestType = EntDemeritStudent.RequestType.ToString(),
                                   Semester = Homeroom.Semester.ToString(),
                                   CreateBy = UserCreate.DisplayName,
                                   Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                   Homeroom = Grade.Code + Classroom.Code,
                                   Category = "Demerit",
                                   LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null
                                       ? LevelOfInfraction.NameLevelOfInteraction
                                       : LevelOfInfractionParent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                   Point = EntDemeritStudent.Point.ToString(),
                                   Note = EntDemeritStudent.Note,
                               }).SingleOrDefaultAsync(CancellationToken);
            }
            return Request.CreateApiResult2(items as object);
        }




    }
}
