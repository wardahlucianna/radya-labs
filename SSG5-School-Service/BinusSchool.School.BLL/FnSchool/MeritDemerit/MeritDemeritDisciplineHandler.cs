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
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class MeritDemeritDisciplineHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMeritDemeritTeacher _meritDemeritTeacher;
        public MeritDemeritDisciplineHandler(ISchoolDbContext schoolDbContext, IMeritDemeritTeacher meritDemeritTeacher)
        {
            _dbContext = schoolDbContext;
            _meritDemeritTeacher = meritDemeritTeacher;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetMeritDemeritMapping = await _dbContext.Entity<MsMeritDemeritMapping>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetMeritDemeritMapping.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsMeritDemeritMapping>().UpdateRange(GetMeritDemeritMapping);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var items = await _dbContext.Entity<MsMeritDemeritMapping>()
                .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
               .Where(e => e.Id == id)
              .Select(x => new GetMeritDemeritDisciplineMappingDetailResult
              {
                  Id = x.Id,
                  AcademicYear = new CodeWithIdVm
                  {
                      Id = x.Grade.Level.AcademicYear.Id,
                      Code = x.Grade.Level.AcademicYear.Code,
                      Description = x.Grade.Level.AcademicYear.Description
                  },
                  Level = new CodeWithIdVm
                  {
                      Id = x.Grade.Level.Id,
                      Code = x.Grade.Level.Code,
                      Description = x.Grade.Level.Description
                  },
                  Grade = new CodeWithIdVm
                  {
                      Id = x.Grade.Id,
                      Code = x.Grade.Code,
                      Description = x.Grade.Description
                  },
                  Catagory = x.Category,
                  LavelInfraction = new NameValueVm
                  {
                      Id = x.IdLevelOfInteraction,
                      Name = x.LevelOfInteraction.IdParentLevelOfInteraction == null ? x.LevelOfInteraction.NameLevelOfInteraction : $"{x.LevelOfInteraction.Parent.NameLevelOfInteraction} {x.LevelOfInteraction.NameLevelOfInteraction}",
                  },
                  NameDiscipline = x.DisciplineName,
                  Point = x.Point.ToString(),
              }).SingleOrDefaultAsync(CancellationToken);

            if (items is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["MeritDemeritMapping"], "Id", id));


            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMeritDemeritDisciplineMappingRequest>();
            var predicate = PredicateBuilder.Create<MsMeritDemeritMapping>(x => x.IsActive == true);
            string[] _columns = { "AcademicYear", "Level", "Grade", "Category", "LevelInfraction", "Name", "DisciplinePoint" };

            if (!string.IsNullOrEmpty(param.IdAcademiYear))
                predicate = predicate.And(x => x.Grade.Level.IdAcademicYear == param.IdAcademiYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Category.ToString()))
                predicate = predicate.And(x => x.Category == (MeritDemeritCategory)param.Category);
            if (!string.IsNullOrEmpty(param.IdLevelInfraction))
                predicate = predicate.And(x => x.IdLevelOfInteraction == param.IdLevelInfraction);
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.DisciplineName.Contains(param.Search));

            var listMeritDemerit = await _dbContext.Entity<MsMeritDemeritMapping>()
                .Where(predicate)
               .Select(x => new
               {
                   Id = x.Id,
                   AcademicYear = x.Grade.Level.AcademicYear.Description,
                   IdAcademicYear = x.Grade.Level.IdAcademicYear,
                   Level = x.Grade.Level.Description,
                   Grade = x.Grade.Description,
                   Category = x.Category.GetDescription(),
                   LevelOfInfraction = x.LevelOfInteraction.IdParentLevelOfInteraction == null ? x.LevelOfInteraction.NameLevelOfInteraction : $"{x.LevelOfInteraction.Parent.NameLevelOfInteraction} {x.LevelOfInteraction.NameLevelOfInteraction}",
                   IdLevelOfInfraction = x.LevelOfInteraction.Id,
                   Name = x.DisciplineName,
                   DisciplinePoint = x.Point,
               }).ToListAsync(CancellationToken);

            var query = listMeritDemerit.OrderBy(x => x.DisciplinePoint).ThenBy(x => x.Name).Distinct();
            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Category":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Category)
                        : query.OrderBy(x => x.Category);
                    break;
                case "LevelInfraction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LevelOfInfraction)
                        : query.OrderBy(x => x.LevelOfInfraction);
                    break;
                case "Name":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;
                case "DisciplinePoint":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.DisciplinePoint)
                        : query.OrderBy(x => x.DisciplinePoint);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = query.ToList();

                if (result.Any())
                {
                    var startProcess = await _meritDemeritTeacher.GetExsisEntryMeritDemeritTeacherById
                (new GetExsisEntryMeritDemeritTeacherByIdRequest
                {
                    IdMeritDemerit = result.Select(e => e.Id).ToList(),
                });

                    if (!startProcess.IsSuccess)
                        throw new BadRequestException(startProcess.Message);
                    var GetExsisEntryMeritDemeritTeacherById = startProcess.Payload;

                    items = result.Select(x => new GetMeritDemeritDisciplineMappingResult
                    {
                        Id = x.Id,
                        AcademicYear = x.AcademicYear,
                        IdAcademicYear = x.IdAcademicYear,
                        Level = x.Level,
                        Grade = x.Grade,
                        Category = x.Category.ToString(),
                        LevelInfraction = x.LevelOfInfraction,
                        IdLevelInfraction = x.IdLevelOfInfraction,
                        NameDiscipline = x.Name,
                        DisciplinePoint = x.DisciplinePoint,
                        IsDisableDelete = GetExsisEntryMeritDemeritTeacherById.Any(e => e.IdMeritDemerit == x.Id) ? GetExsisEntryMeritDemeritTeacherById.SingleOrDefault(e => e.IdMeritDemerit == x.Id).StatusExsis : true
                    }).ToList();
                }
                else
                    items = new List<GetMeritDemeritDisciplineMappingResult>();

            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                if (result.Any())
                {
                    var startProcess = await _meritDemeritTeacher.GetExsisEntryMeritDemeritTeacherById
                                       (new GetExsisEntryMeritDemeritTeacherByIdRequest
                                       {
                                           IdMeritDemerit = result.Select(e => e.Id).ToList(),
                                       });

                    if (!startProcess.IsSuccess)
                        throw new BadRequestException(startProcess.Message);

                    var GetExsisEntryMeritDemeritTeacherById = startProcess.Payload;

                    items = result.Select(x => new GetMeritDemeritDisciplineMappingResult
                    {
                        Id = x.Id,
                        AcademicYear = x.AcademicYear,
                        IdAcademicYear = x.IdAcademicYear,
                        Level = x.Level,
                        Grade = x.Grade,
                        Category = x.Category.ToString(),
                        LevelInfraction = x.LevelOfInfraction,
                        IdLevelInfraction = x.IdLevelOfInfraction,
                        NameDiscipline = x.Name,
                        DisciplinePoint = x.DisciplinePoint,
                        IsDisableDelete = GetExsisEntryMeritDemeritTeacherById.Any(e => e.IdMeritDemerit == x.Id) ? GetExsisEntryMeritDemeritTeacherById.SingleOrDefault(e => e.IdMeritDemerit == x.Id).StatusExsis : true
                    }).ToList();
                }
                else
                    items = new List<GetMeritDemeritDisciplineMappingResult>();

            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritDisciplineMappingRequest, AddMeritDemeritDisciplineMappingValidator>();

            var exsis = _dbContext.Entity<MsMeritDemeritMapping>().Any(e => e.DisciplineName == body.DisciplineName && e.IdGrade == body.IdGrade && e.Category == body.Category);

            if (!exsis)
            {
                var newMeritDemeritMapping = new MsMeritDemeritMapping
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGrade = body.IdGrade,
                    Category = body.Category,
                    IdLevelOfInteraction = body.IdLevelInfraction,
                    DisciplineName = body.DisciplineName,
                    Point = body.Point == null ? 0 : Convert.ToInt32(body.Point),
                };
                _dbContext.Entity<MsMeritDemeritMapping>().Add(newMeritDemeritMapping);
            }
            else
            {
                throw new BadRequestException("Merit demerit mapping with name: " + body.DisciplineName + " is exists.");
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMeritDemeritDisciplineMappingRequest, UpdateMeritDemeritDisciplineMappingValidator>();
            var MeritDemeritMapping = _dbContext.Entity<MsMeritDemeritMapping>().SingleOrDefault(e => e.Id == body.Id);

            if (MeritDemeritMapping == null)
                throw new BadRequestException("Merit demerit mapping with id: " + body.Id + " is not found.");

            var exsis = _dbContext.Entity<MsMeritDemeritMapping>().Any(e => e.DisciplineName == body.DisciplineName && e.IdGrade == body.IdGrade && e.Category == body.Category && e.Id != body.Id);
            if (exsis)
            {
                throw new BadRequestException("Merit demerit mapping with name: " + body.DisciplineName + " is exists.");
            }
            else
            {
                MeritDemeritMapping.IdGrade = body.IdGrade;
                MeritDemeritMapping.Category = body.Category;
                MeritDemeritMapping.IdLevelOfInteraction = body.IdLevelInfraction;
                MeritDemeritMapping.DisciplineName = body.DisciplineName;
                MeritDemeritMapping.Point = body.Point == "" ? 0 : Convert.ToInt32(body.Point);

                _dbContext.Entity<MsMeritDemeritMapping>().Update(MeritDemeritMapping);
            }


            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

    }
}
