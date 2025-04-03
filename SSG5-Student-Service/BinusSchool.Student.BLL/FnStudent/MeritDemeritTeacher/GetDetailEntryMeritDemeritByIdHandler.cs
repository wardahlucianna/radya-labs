using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritDemeritByIdHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailEntryMeritDemeritByIdHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailEntryMeritDemeritByIdRequest>(nameof(GetDetailEntryMeritDemeritByIdRequest.Id));
            var result = new GetDetailEntryMeritDemeritByIdResult();

            if (param.IsMerit)
            {
                result = await (from EntryMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                                join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                join Point in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals Point.IdHomeroomStudent into JoinedPoint
                                from Point in JoinedPoint.DefaultIfEmpty()
                                join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                where EntryMeritStudent.Id == param.Id
                                select new GetDetailEntryMeritDemeritByIdResult
                                {
                                    Id = EntryMeritStudent.Id,
                                    IdHomeroomStudent = HomeroomStudent.Id,
                                    AcademicYear = new CodeWithIdVm
                                    {
                                        Id = AcademicYear.Id,
                                        Code = AcademicYear.Code,
                                        Description = AcademicYear.Description
                                    },
                                    Level = new CodeWithIdVm
                                    {
                                        Id = Level.Id,
                                        Code = Level.Code,
                                        Description = Level.Description
                                    },
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = Grade.Id,
                                        Code = Grade.Code,
                                        Description = Grade.Description
                                    },
                                    Semester = HomeroomStudent.Semester,
                                    Date = EntryMeritStudent.DateMerit,
                                    Category = EntryMeritStudent.Type.ToString(),
                                    LevelOfInfraction = null,
                                    MeritDemeritMapping = new ItemValueVm
                                    {
                                        Id = MeritDemeritMapping.Id,
                                        Description = MeritDemeritMapping.DisciplineName
                                    },
                                    Point = Point.MeritPoint,
                                    Student = new DetailEntryMeritDemeritStudent
                                    {
                                        Fullname = NameUtil.GenerateFullName(Student.FirstName, Student.MiddleName, Student.LastName),
                                        Username = Student.Id,
                                        BinusianId = Student.IdBinusian,
                                        Grade = Grade.Description,
                                        Homeroom = Grade.Code + Classroom.Code,
                                        Note = EntryMeritStudent.Note
                                    }
                                }).SingleOrDefaultAsync(CancellationToken);

                if (result is null)
                    throw new NotFoundException("Entry Merit not found");
            }
            else
            {
                result = await (from EntryDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                join UserCreate in _dbContext.Entity<MsUser>() on EntryDemeritStudent.UserIn equals UserCreate.Id
                                join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                                join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelOfInfractionParent
                                from LevelOfInfractionParent in JoinedLevelOfInfractionParent.DefaultIfEmpty()
                                join Point in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals Point.IdHomeroomStudent into JoinedPoint
                                from Point in JoinedPoint.DefaultIfEmpty()
                                where EntryDemeritStudent.Id == param.Id
                                select new GetDetailEntryMeritDemeritByIdResult
                                {
                                    Id = EntryDemeritStudent.Id,
                                    IdHomeroomStudent = HomeroomStudent.Id,
                                    AcademicYear = new CodeWithIdVm
                                    {
                                        Id = AcademicYear.Id,
                                        Code = AcademicYear.Code,
                                        Description = AcademicYear.Description
                                    },
                                    Level = new CodeWithIdVm
                                    {
                                        Id = Level.Id,
                                        Code = Level.Code,
                                        Description = Level.Description
                                    },
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = Grade.Id,
                                        Code = Grade.Code,
                                        Description = Grade.Description
                                    },
                                    Semester = HomeroomStudent.Semester,
                                    Date = EntryDemeritStudent.DateDemerit,
                                    Category = "Demerit",
                                    LevelOfInfraction = new ItemValueVm
                                    {
                                        Id = LevelOfInfraction.Id,
                                        Description = LevelOfInfraction.NameLevelOfInteraction
                                    },
                                    MeritDemeritMapping = new ItemValueVm
                                    {
                                        Id = MeritDemeritMapping.Id,
                                        Description = MeritDemeritMapping.DisciplineName
                                    },
                                    Point = Point.DemeritPoint,
                                    Student = new DetailEntryMeritDemeritStudent
                                    {
                                        Fullname = NameUtil.GenerateFullName(Student.FirstName, Student.MiddleName, Student.LastName),
                                        Username = Student.Id,
                                        BinusianId = Student.IdBinusian,
                                        Grade = Grade.Description,
                                        Homeroom = Grade.Code + Classroom.Code,
                                        Note = EntryDemeritStudent.Note
                                    }
                                }).SingleOrDefaultAsync(CancellationToken);

                if (result is null)
                    throw new NotFoundException("Entry Demerit not found");
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
