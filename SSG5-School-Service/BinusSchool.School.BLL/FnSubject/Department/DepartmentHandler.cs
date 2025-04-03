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
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSubject.Department.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSubject.Department
{
    public class DepartmentHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public DepartmentHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsDepartment>()
                .Include(x => x.Subjects)
                .Include(x => x.DepartmentLevels)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var item in datas)
            {
                // don't set inactive when row have to-many relation
                if (item.Subjects.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(item.Id, string.Format(Localizer["ExAlreadyUse"], item.Description ?? item.Code ?? item.Id));
                }
                else
                {
                    item.IsActive = false;
                    _dbContext.Entity<MsDepartment>().Update(item);

                    foreach (var item2 in item.DepartmentLevels)
                    {
                        item2.IsActive = false;
                        _dbContext.Entity<MsDepartmentLevel>().Update(item2);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsDepartment>()
                 .Include(x => x.AcademicYear)
                 .Include(x => x.DepartmentLevels)
                 .ThenInclude(p => p.Level)
                 .Select(x => new GetDepartmenDetailResult
                 {
                     Id = x.Id,
                     DepartmentName = x.Description,
                     Acadyear = new CodeWithIdVm
                     {
                         Id = x.AcademicYear.Id,
                         Code = x.AcademicYear.Code,
                         Description = x.AcademicYear.Description,
                     },
                     TypeLevel = (int)x.Type,
                     TypeLevelName = x.Type.ToString(),
                     Level = string.Join("/", x.DepartmentLevels.Select(p => p.Level.Description)),
                     IdLevel = x.DepartmentLevels.Select(p => p.IdLevel).ToList(),
                 })
                 .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetDepartmentRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new[] { "acadyear", "level", "description" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "academicYear.Description" }
            };

            var predicate = PredicateBuilder.Create<MsDepartment>(x => x.AcademicYear.IdSchool == param.IdSchool);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
            {
                if (param.IdLevel == "general")
                {
                    predicate = predicate.And(x => x.Type == DepartmentType.General);
                }
                else
                {
                    predicate = predicate.And(x => x.DepartmentLevels.Any(x => x.IdLevel == param.IdLevel));
                }
            }

            if (!string.IsNullOrEmpty(param.Search))
            {
                if (param.Search.ToLower().Contains("gen"))
                {
                    predicate = predicate.And(x => x.Type == DepartmentType.General);
                }
                else if (param.Search.ToLower().Contains("/"))
                {
                    var search = param.Search.Split("/").ToList();
                    foreach (var item in search)
                    {
                        predicate = predicate.And(x => x.DepartmentLevels.Any(y => EF.Functions.Like(y.Level.Code, item)));
                    }
                }
                else
                {
                    predicate = predicate.And(x
                    => x.DepartmentLevels.Any(y => EF.Functions.Like(y.Level.Code, param.SearchPattern()))
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.AcademicYear.Description, param.SearchPattern()));
                }
            }

            var query = _dbContext.Entity<MsDepartment>()
                .Include(p => p.AcademicYear)
                .Include(p => p.DepartmentLevels).ThenInclude(p => p.Level)
                .Include(p => p.Subjects)
                .SearchByIds(param)
                .Where(predicate);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = string.Format("{0} {1}",
                            x.Type == DepartmentType.General
                                ? nameof(DepartmentType.General)
                                : string.Join("/", param.OrderType == OrderType.Asc
                                    ? x.DepartmentLevels.OrderBy(y => y.Level.Code).Select(y => y.Level.Code)
                                    : x.DepartmentLevels.OrderByDescending(y => y.Level.Code).Select(y => y.Level.Code)),
                            x.Description)
                    })
                    .ToListAsync(CancellationToken);
            }

            else
            {
                //items = await query
                //    .SetPagination(param)
                //    .Select(x => new GetDepartmentResult
                //    {
                //        Id = x.Id,
                //        Acadyear = new CodeWithIdVm
                //        {
                //            Id = x.IdAcademicYear,
                //            Code = x.AcademicYear.Code,
                //            Description = x.AcademicYear.Description
                //        },
                //        Level = x.Type == DepartmentType.General
                //            ? nameof(DepartmentType.General)
                //            : x.DepartmentLevels.Select(x => x.Level.Description)).Aggregate((a, b) => (a + ", " + b),
                //        Description = x.Description,
                //        Subjects = x.Subjects.Select(s => new CodeWithIdVm(s.Id, s.Code, s.Description))
                //    })
                //    .OrderBy(x=> x.Level)
                //    .ToListAsync(CancellationToken);
                var departmentGeneral = _dbContext.Entity<MsDepartment>()
                    .Include(x=>x.AcademicYear)
                        .ThenInclude(x=>x.School)
                    .Where(x => x.Type == DepartmentType.General)
                    .Where(x=>x.AcademicYear.IdSchool == param.IdSchool)
                    .Select(x => new GetDepartmentResult
                    {
                        Id = x.Id,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.IdAcademicYear,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        Level = nameof(DepartmentType.General),
                        Description = x.Description,
                        Subjects = x.Subjects.Select(s => new CodeWithIdVm(s.Id, s.Code, s.Description)),
                        IdLevel = "General"
                    })
                    .ToList();
                
                var departmentLevel = _dbContext.Entity<MsDepartment>()
                    .Include(x => x.DepartmentLevels)
                        .ThenInclude(x => x.Level)
                    .Include(x=>x.AcademicYear)
                        .ThenInclude(x=>x.School)
                    .Where(x => x.Type == DepartmentType.Level)
                    .Where(x => x.AcademicYear.IdSchool == param.IdSchool)
                    .Select(x => new GetDepartmentResult
                    {
                        Id = x.Id,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.IdAcademicYear,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        Level = string.Join("/", x.DepartmentLevels.Select(x => x.Level.Description)),
                        IdLevel = string.Join("/", x.DepartmentLevels.Select(x => x.Level.Id)),
                        Description = x.Description,
                        Subjects = x.Subjects.Select(s => new CodeWithIdVm(s.Id, s.Code, s.Description))
                    })
                    .ToList();
                var data = departmentGeneral.Union(departmentLevel);
                if (!string.IsNullOrEmpty(param.IdAcadyear))
                    data = data.Where(x => x.Acadyear.Id == param.IdAcadyear);
                if (!string.IsNullOrEmpty(param.IdLevel))
                {
                    if (param.IdLevel == "general")
                    {
                        data = data.Where(x => x.Level == "General");
                    }
                    else
                    {
                        data = data.Where(x => x.IdLevel.Contains(param.IdLevel));
                    }
                }

                if (!string.IsNullOrEmpty(param.Search))
                {
                    data = data.Where(x => x.Acadyear.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                    || x.Acadyear.Code.Contains(param.Search,StringComparison.OrdinalIgnoreCase)
                    || x.Level.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                    || x.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase));
                }
                data = param.OrderBy switch
                {
                    "level" => param.OrderType == OrderType.Asc
                        ? data.OrderBy(x => x.Level)
                        : data.OrderByDescending(x => x.Level),
                    "acadyear" => param.OrderType == OrderType.Asc
                        ? data.OrderBy(x => x.Acadyear.Description)
                        : data.OrderByDescending(x => x.Acadyear.Description),
                    "description" => param.OrderType == OrderType.Asc
                        ? data.OrderBy(x => x.Description)
                        : data.OrderByDescending(x => x.Description),
                    _ => data.OrderBy(x => x.Id)

                };

                items = data.SetPagination(param)
                    .Select(x => new GetDepartmentResult
                    {
                        Id = x.Id,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.Acadyear.Id,
                            Code = x.Acadyear.Code,
                            Description = x.Acadyear.Description
                        },
                        Level = x.Level,
                        Description = x.Description,
                        Subjects = x.Subjects.Select(s => new CodeWithIdVm(s.Id, s.Code, s.Description))
                    })
                    .ToList();
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddDepartmentRequest, AddDepartmentValidator>();
            if (body.LevelType == (int)DepartmentType.General)
            {
                var check = await _dbContext.Entity<MsDepartment>()
                               .Include(p => p.DepartmentLevels)
                               .Where(x => x.Description == body.DepartmentName &&
                                           x.Type == DepartmentType.General &&
                                           x.IdAcademicYear == body.IdAcadyear)
                               .FirstOrDefaultAsync();
                if (check != null)
                {
                    throw new Exception("Data already exist");
                    //_dbContext.AddFailure(string.Format(_localizer["ExAlreadyExist"], "Departemnt", "Name and level general"));
                }
            }
            else
            {
                var check = await _dbContext.Entity<MsDepartment>()
                               .Include(p => p.DepartmentLevels)
                               .ThenInclude(p => p.Level)
                               .Where(x => x.Description == body.DepartmentName &&
                                           x.DepartmentLevels.Any(p => body.IdLevel.Any(o => o == p.IdLevel)) &&
                                           x.IdAcademicYear == body.IdAcadyear)
                               .FirstOrDefaultAsync();
                if (check != null)
                {
                    throw new Exception("Data already exist");
                }
            }
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var param = new MsDepartment();
            param.Id = Guid.NewGuid().ToString();
            param.IdAcademicYear = body.IdAcadyear;
            param.Code = body.DepartmentName;
            param.Description = body.DepartmentName;
            param.Type = (DepartmentType)body.LevelType;
            param.UserIn = AuthInfo.UserId;

            _dbContext.Entity<MsDepartment>().Add(param);

            if (param.Type == DepartmentType.Level && body.IdLevel != null && body.IdLevel.Count > 0)
            {
                foreach (var item in body.IdLevel)
                {
                    var depScholLevel = new MsDepartmentLevel();
                    depScholLevel.Id = Guid.NewGuid().ToString();
                    depScholLevel.IdDepartment = param.Id;
                    depScholLevel.IdLevel = item;
                    depScholLevel.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsDepartmentLevel>().Add(depScholLevel);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var model = await Request.ValidateBody<UpdateDepartmentRequest, UpdateDepartmentValidator>();

            var getDetail = _dbContext.Entity<MsDepartment>()
                                      .Include(p => p.DepartmentLevels)
                                      .Where(p => p.Id == model.Id).FirstOrDefault();
            if (getDetail == null)
            {
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], "Department", "Id", model.Id));
            }

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            getDetail.UserUp = AuthInfo.UserId;
            getDetail.Type = (DepartmentType)model.LevelType;
            getDetail.Code = model.DepartmentName;
            getDetail.Description = model.DepartmentName;
            getDetail.IdAcademicYear = model.IdAcadyear;

            _dbContext.Entity<MsDepartment>().Update(getDetail);
            _dbContext.Entity<MsDepartmentLevel>().RemoveRange(getDetail.DepartmentLevels);

            if (getDetail.Type == DepartmentType.Level && model.IdLevel != null && model.IdLevel.Count > 0)
            {
                foreach (var item in model.IdLevel)
                {
                    var depScholLevel = new MsDepartmentLevel();
                    depScholLevel.Id = Guid.NewGuid().ToString();
                    depScholLevel.IdDepartment = getDetail.Id;
                    depScholLevel.IdLevel = item;
                    depScholLevel.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsDepartmentLevel>().Add(depScholLevel);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
