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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;
namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritDisciplineMappingCopyHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchoolDbContext _dbContext;

        public AddMeritDemeritDisciplineMappingCopyHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritDisciplineMappingCopyRequest, AddMeritDemeritDisciplineMappingCopyValidator>();
            List<MeritDemeritDisciplineMapping> ExsisMeritDemeritCopy = new List<MeritDemeritDisciplineMapping>();

            var MeritDemeritMapping = await _dbContext.Entity<MsMeritDemeritMapping>()
                .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                .Include(e => e.LevelOfInteraction)
              .Where(e => body.IdDisciplineMapping.Contains(e.Id))
             .ToListAsync(CancellationToken);

            var MeritDemeritMappingCopyTo = await _dbContext.Entity<MsMeritDemeritMapping>()
                .Include(e => e.Grade).ThenInclude(e => e.Level)
              .Where(e => e.Grade.Level.IdAcademicYear == body.IdAcademicYearCopyTo 
                && MeritDemeritMapping.Select(e=>e.Grade.Level.Code).ToList().Contains(e.Grade.Level.Code)
                && MeritDemeritMapping.Select(e => e.Grade.Code).ToList().Contains(e.Grade.Code)
                && MeritDemeritMapping.Select(e => e.DisciplineName).ToList().Contains(e.DisciplineName)
                && MeritDemeritMapping.Select(e => e.Point).ToList().Contains(e.Point)
                )
             .ToListAsync(CancellationToken);

            var GetGrade = await _dbContext.Entity<MsGrade>()
                                .Include(e=>e.Level).ThenInclude(e=>e.AcademicYear)
                                .Where(e => e.Level.IdAcademicYear == body.IdAcademicYearCopyTo).ToListAsync(CancellationToken);

            var CountSuccses = 0;
            foreach (var ItemMeritDemeritMapping in MeritDemeritMapping)
            {
                var ExsisGradeByCode = GetGrade.Where(e=>e.Code==ItemMeritDemeritMapping.Grade.Code).Any();
                if (!ExsisGradeByCode)
                    throw new BadRequestException("Grade code : " +ItemMeritDemeritMapping.Grade.Code + "is not found in Academic Year :" + ItemMeritDemeritMapping.Grade.Level.IdAcademicYear);

                var ExsisLevelByCode = GetGrade.Where(e => e.Level.Code == ItemMeritDemeritMapping.Grade.Level.Code).Any();
                if (!ExsisLevelByCode)
                    throw new BadRequestException("Level code : " + ItemMeritDemeritMapping.Grade.Level.Code + "is not found in Academic Year :" + ItemMeritDemeritMapping.Grade.Level.IdAcademicYear);

                var exsisMappingCopyTo = MeritDemeritMappingCopyTo
                                        .Any(e => e.Grade.Code == ItemMeritDemeritMapping.Grade.Code
                                            && e.Grade.Level.Code == ItemMeritDemeritMapping.Grade.Level.Code
                                            && e.DisciplineName==ItemMeritDemeritMapping.DisciplineName
                                            && e.Point== ItemMeritDemeritMapping.Point);
                var MeritDemeritMappingById = MeritDemeritMapping.SingleOrDefault(e => e.Id == ItemMeritDemeritMapping.Id);

                if (!exsisMappingCopyTo)
                {
                    var IdGrade = GetGrade.SingleOrDefault(e => e.Code == MeritDemeritMappingById.Grade.Code &&
                                   e.Level.IdAcademicYear == body.IdAcademicYearCopyTo && e.Level.IsActive == true && e.Level.AcademicYear.IsActive == true)==null
                                   ?null
                                   : GetGrade.SingleOrDefault(e => e.Code == MeritDemeritMappingById.Grade.Code &&
                                    e.Level.IdAcademicYear == body.IdAcademicYearCopyTo && e.Level.IsActive == true && e.Level.AcademicYear.IsActive == true).Id;

                    if (IdGrade == null)
                        throw new BadRequestException("Grade with name: " + MeritDemeritMappingById.Grade.Code + " is not exists.");

                    var newMeritDemeritMapping = new MsMeritDemeritMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGrade = IdGrade,
                        Category = MeritDemeritMappingById.Category,
                        IdLevelOfInteraction = MeritDemeritMappingById.IdLevelOfInteraction,
                        DisciplineName = MeritDemeritMappingById.DisciplineName,
                        Point = MeritDemeritMappingById.Point.ToString() == "" ? 0 : Convert.ToInt32(MeritDemeritMappingById.Point),
                    };
                    _dbContext.Entity<MsMeritDemeritMapping>().Add(newMeritDemeritMapping);
                    CountSuccses++;
                }
                else
                {
                    ExsisMeritDemeritCopy.Add(new MeritDemeritDisciplineMapping
                    {
                        AcademicYear = MeritDemeritMappingById.Grade.Level.AcademicYear.Description,
                        Level = MeritDemeritMappingById.Grade.Level.Description,
                        Grade = MeritDemeritMappingById.Grade.Description,
                        Category = MeritDemeritMappingById.Category.ToString(),
                        LevelInfraction = MeritDemeritMappingById.LevelOfInteraction==null?"-": MeritDemeritMappingById.LevelOfInteraction.NameLevelOfInteraction,
                        NameDiscipline = MeritDemeritMappingById.DisciplineName,
                        Point = MeritDemeritMappingById.Point.ToString(),
                    });
                }
            }

            AddMeritDemeritDisciplineMappingCopyResult Items = new AddMeritDemeritDisciplineMappingCopyResult
            {
                CountSucces = CountSuccses.ToString(),
                MeritDemeritDisciplineMappingFailed = ExsisMeritDemeritCopy,
            };
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(Items as object);
        }
    }
}
