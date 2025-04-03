using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Teaching.FnAssignment.TeacherPosition.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition
{
    public class TeacherPositionHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public TeacherPositionHandler(ITeachingDbContext schoolDbContext, IApiService<ILevel> levelApi)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsTeacherPosition>()
                .Include(x => x.TeacherPositionAliases)
                .Include(x => x.NonTeachingLoads)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas.ToArray())
            {
                // don't set inactive when row have to-many relation
                if (data.NonTeachingLoads.Count != 0)
                {
                    datas.Remove(data);
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    data.TeacherPositionAliases
                    .Select(x =>
                    {
                        x.IsActive = false;
                        return x;
                    })
                    .ToList();
                    _dbContext.Entity<MsTeacherPosition>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsTeacherPosition>()
            .Include(x => x.TeacherPositionAliases)
                .ThenInclude(x => x.Level)
                    .ThenInclude(x => x.AcademicYear)
            .Include(x => x.Position)
                .Where(x => x.Id == id)
                .Select(x => new GetTeacherPositionDetailResult()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    IdSchool = x.IdSchool,
                    Category = x.Category,
                    Position = new CodeWithIdVm
                    {
                        Id = x.IdPosition,
                        Code = x.Position.Code,
                        Description = x.Position.Description
                    }
                }).FirstOrDefaultAsync();

            var listLevel = _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .ThenInclude(x => x.School)
                .OrderBy(x => x.AcademicYear.OrderNumber).ThenBy(x => x.OrderNumber)
                .Where(x => x.AcademicYear.IdSchool == data.IdSchool.ToString())
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description
                })
                .ToList();

            data.TeacherPositionAlias =
                (
                    from _level in _dbContext.Entity<MsLevel>()
                    join _acadyear in _dbContext.Entity<MsAcademicYear>() on _level.IdAcademicYear equals _acadyear.Id
                    join _levelAlias in _dbContext.Entity<MsTeacherPositionAlias>() on _level.Id equals _levelAlias.IdLevel into joinedAlias
                    from _joinedAlias in joinedAlias.DefaultIfEmpty()
                    orderby
                     _acadyear.OrderNumber, _level.OrderNumber
                    where
                        _joinedAlias.IdTeacherPosition == data.Id
                    select new GetTeacherPositionAliasResult
                    {
                        Id = _joinedAlias.Id ?? null,
                        IdTeacherPosition = _joinedAlias.IdTeacherPosition ?? null,
                        Alias = _joinedAlias.Alias ?? null,
                        Level = new GetTeacherPositionAliasLevelResult
                        {
                            Id = _level.Id,
                            Code = _level.Code,
                            Description = _level.Description,
                            Academicyear = new CodeWithIdVm
                            {
                                Id = _acadyear.Id,
                                Code = _acadyear.Code,
                                Description = _acadyear.Description
                            }
                        }
                    }
                ).ToList();

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTeacherPositionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new List<CodeWithIdVm>()
            {
                 new CodeWithIdVm()
                {
                    Id="",
                    Code="",
                    Description = ""
                },
                new CodeWithIdVm()
                {
                    Id="",
                    Code="Position Name (Default)",
                    Description = "Description"
                },
                new CodeWithIdVm()
                {
                    Id="",
                    Code="Short Name",
                    Description = "Code"
                },
                new CodeWithIdVm()
                {
                    Id="",
                    Code="Position",
                    Description = "Description"
                },
                new CodeWithIdVm()
                {
                    Id = "",
                    Code = "Category",
                    Description = "Category"
                }
            };

            var listLevel = await _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Where(x => param.IdSchool.Contains(x.AcademicYear.IdSchool))
                .OrderBy(x => x.AcademicYear.OrderNumber).ThenBy(x => x.OrderNumber)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = string.Format("{0} ({1})", x.Code, x.AcademicYear.Code),
                    Description = x.Description
                })
                .ToListAsync(CancellationToken);

            if (listLevel.Count() > 0)
            {
                foreach (var item in listLevel)
                {
                    columns.Add(item);
                }
            }

            columns.Add(new CodeWithIdVm()
            {
                Id = "",
                Code = "Action",
                Description = "Action"
            });

            var predicate = PredicateBuilder.Create<MsTeacherPosition>(x => param.IdSchool.Contains(x.IdSchool) && x.IsActive);

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                if (AcademicType.Academic.ToString().ToLower().Contains(param.Search.ToLower()))
                {
                    predicate = predicate.And(x
                        => EF.Functions.Like(x.Code, param.SearchPattern())
                        || EF.Functions.Like(x.Description, param.SearchPattern())
                        || x.Category == AcademicType.Academic
                        || x.TeacherPositionAliases.Any(y => y.Alias.Contains(param.Search)));
                }
                else if (AcademicType.NonAcademic.ToString().ToLower().Contains(param.Search.ToLower()) || param.Search.ToLower().Contains("non-"))
                {
                    predicate = predicate.And(x
                        => EF.Functions.Like(x.Code, param.SearchPattern())
                        || EF.Functions.Like(x.Description, param.SearchPattern())
                        || x.Category == AcademicType.NonAcademic
                        || x.TeacherPositionAliases.Any(y => y.Alias.Contains(param.Search)));
                }
                else
                {
                    predicate = predicate.And(x
                        => EF.Functions.Like(x.Code, param.SearchPattern())
                        || EF.Functions.Like(x.Description, param.SearchPattern())
                        || x.TeacherPositionAliases.Any(y => y.Alias.Contains(param.Search)));
                }
            }

            if (param.PositionCode != null)
            {
                predicate = predicate.And(p => param.PositionCode.Contains(p.Position.Code));
            }

            var query = _dbContext.Entity<MsTeacherPosition>()
                .Include(x => x.TeacherPositionAliases)
                    .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                .Include(x => x.Position)
                .Include(x => x.NonTeachingLoads)
                .SearchByIds(param)
                .Where(predicate);

            switch (param.OrderBy)
            {
                case "Description":
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description);
                    break;
                case "Code":
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code);
                    break;
                case "Position":
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.Position.Description) : query.OrderBy(x => x.Position.Description);
                    break;
                case "Category":
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category);
                    break;
                case "DateIn":
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.DateIn) : query.OrderBy(x => x.DateIn);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description);
                    break;
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var dataItems = await query
                    .SetPagination(param)
                    .Select(x => new GetTeacherPositionResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        IdSchool = x.IdSchool,
                        Category = x.Category,
                        CanDeleted = x.NonTeachingLoads.Any() ? false : true,
                        TeacherPositionAlias = x.TeacherPositionAliases.OrderBy(x => x.Level.AcademicYear.OrderNumber).ThenBy(x => x.Level.OrderNumber).Select(x => new GetTeacherPositionAliasResult
                        {
                            Id = x.Id,
                            IdTeacherPosition = x.IdTeacherPosition,
                            Alias = x.Alias,
                            Level = new GetTeacherPositionAliasLevelResult
                            {
                                Id = x.Level.Id,
                                Code = x.Level.Code,
                                Description = x.Level.Description,
                                Academicyear = new CodeWithIdVm
                                {
                                    Id = x.Level.AcademicYear.Id,
                                    Code = x.Level.AcademicYear.Code,
                                    Description = x.Level.AcademicYear.Description
                                }
                            }
                        }).ToList(),
                        Position = x.Position.Description
                    })
                    .ToListAsync(CancellationToken);
                items = dataItems;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTeacherPositionRequest, AddTeacherPositionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isExist = await _dbContext.Entity<MsTeacherPosition>()
                .Where(x => x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower())
                .Where(x => x.IdSchool == body.IdSchool && x.Category == body.Category)
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists");

            var param = new MsTeacherPosition();
            param.Id = Guid.NewGuid().ToString();
            param.Code = body.Code;
            param.Description = body.Description;
            param.IdSchool = body.IdSchool;
            param.Category = body.Category;
            param.UserIn = AuthInfo.UserId;
            param.IdPosition = body.IdPosition;
            param.TeacherPositionAliases = body.AliasLevel.Select(x => new MsTeacherPositionAlias
            {
                Id = Guid.NewGuid().ToString(),
                IdLevel = x.IdLevel,
                Alias = x.Alias,
                IdTeacherPosition = param.Id,
                IsActive = true
            }).ToList();

            _dbContext.Entity<MsTeacherPosition>().Add(param);
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateTeacherPositionRequest, UpdateTeacherPositionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsTeacherPosition>().FindAsync(body.Id);
            if (data == null)
            {
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], "Teacher Position", "Id", body.Id));
            }

            var isExist = await _dbContext.Entity<MsTeacherPosition>()
                .Where(x => x.Id != body.Id && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .Where(x => x.IdSchool == body.IdSchool && x.Category == body.Category)
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists");
            var currentAliasForThisPosition = await _dbContext.Entity<MsTeacherPositionAlias>().Where(x => x.IdTeacherPosition == body.Id).AsNoTracking().ToListAsync(CancellationToken);
            #region Update Data Alias
            var dataAliasUpdated = (from current in currentAliasForThisPosition
                                    join updated in body.AliasLevel on current.Id equals updated.Id
                                    select new MsTeacherPositionAlias
                                    {
                                        Id = current.Id,
                                        IdTeacherPosition = current.IdTeacherPosition,
                                        IdLevel = current.IdLevel,
                                        Alias = updated.Alias,
                                        IsActive = current.IsActive,
                                        DateIn = current.DateIn,
                                        UserIn = current.UserIn
                                    }
                     ).ToList();
            _dbContext.Entity<MsTeacherPositionAlias>().UpdateRange(dataAliasUpdated);
            #endregion
            data.Code = body.Code;
            data.Description = body.Description;
            data.UserUp = AuthInfo.UserId;
            data.IdPosition = body.IdPosition;
            if (data.Category != body.Category)
                data.Category = body.Category;
            _dbContext.Entity<MsTeacherPosition>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2();
        }

        public string UpdateAliasValue(string alias, List<CodeWithIdVm> level)
        {
            var result = "";
            if (!string.IsNullOrWhiteSpace(alias))
            {
                var dataConverter = JsonConvert.DeserializeObject<List<dynamic>>(alias);
                var element = JArray.Parse(alias);
                if (element.Count > 0)
                {
                    var dataResult = element.Where(p => level.Any(x => x.Id == p["id"].Value<string>())).ToList();
                    result = JsonConvert.SerializeObject(dataResult);
                }
            }
            return result;
        }
    }
}
