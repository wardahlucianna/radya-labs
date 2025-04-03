using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.User.FnUser.HierarchyMapping;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.HierarchyMapping.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.HierarchyMapping
{
    public class HierarchyMappingHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;

        public HierarchyMappingHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<TrHierarchyMapping>()
                .Include(x => x.HierarchyMappingDetails)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                foreach (var detail in data.HierarchyMappingDetails)
                {
                    detail.IsActive = false;
                    _dbContext.Entity<TrHierarchyMappingDetail>().Update(detail);
                }
                data.IsActive = false;
                _dbContext.Entity<TrHierarchyMapping>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<TrHierarchyMapping>()
                                       .Include(x => x.HierarchyMappingDetails).ThenInclude(x => x.RolePosition).ThenInclude(x => x.Role)
                                       .Include(x => x.HierarchyMappingDetails).ThenInclude(x => x.RolePosition).ThenInclude(x => x.TeacherPosition)
                                       .Where(x => x.Id == id)
                                       .SingleOrDefaultAsync();

            if (data is null)
                throw new NotFoundException("Hierarchy is not found");

            var result = new HierarchyMappingDetailResult
            {
                Id = data.Id,
                Description = data.Name,
                DateIn = data.DateIn
            };

            result.Parents = new List<HierarchyResult>();
            foreach (var parent in data.HierarchyMappingDetails.Where(x => x.IdParent == null))
            {
                result.Parents.Add(new HierarchyResult
                {
                    IdRolePosition = parent.RolePosition.Id,
                    Role = new CodeWithIdVm
                    {
                        Id = parent.RolePosition.Role.Id,
                        Code = parent.RolePosition.Role.Code,
                        Description = parent.RolePosition.Role.Description
                    },
                    TeacherPosition = new CodeWithIdVm
                    {
                        Id = parent.RolePosition.TeacherPosition.Id,
                        Code = parent.RolePosition.TeacherPosition.Code,
                        Description = parent.RolePosition.TeacherPosition.Description
                    },
                    Childs = GetTrailDataRecursion(parent.Id, data.HierarchyMappingDetails, new List<HierarchyResult>())
                });
            }

            return Request.CreateApiResult2(result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));

            var columns = new[] { "description", "date" };

            var predicate = PredicateBuilder.Create<TrHierarchyMapping>(x => x.HierarchyMappingDetails.Any(y => param.IdSchool.Contains(y.RolePosition.Role.IdSchool)));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern()));

            var query = _dbContext.Entity<TrHierarchyMapping>()
                .SearchByIds(param)
                .Where(predicate)
                .SearchByDynamic(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Name))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new HierarchyMappingResult
                    {
                        Id = x.Id,
                        Description = x.Name,
                        DateIn = x.DateIn
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddHierarchyMappingRequest, AddHierarchyMappingValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            // throw if any existing hierarchy mapping name
            var existHierarchy = await _dbContext.Entity<TrHierarchyMappingDetail>()
                .Include(x => x.HierarchyMapping)
                .Where(x => x.RolePosition.Role.IdSchool == body.IdSchool && x.HierarchyMapping.Name == body.Name)
                .Select(x => new ItemValueVm(x.IdHierarchyMapping, x.HierarchyMapping.Name))
                .FirstOrDefaultAsync(CancellationToken);
            if (existHierarchy != null)
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], "Hierarchy", "Name", body.Name));

            // find not found parent
            var notFound = body.Hierarchies.Select(x => x.IdRolePositionParent)
                                           .Except(body.Hierarchies.Select(y => y.IdRolePositionParent).Intersect(body.Hierarchies.Select(y => y.IdRolePosition)))
                                           .Where(x => x != null);
            if (notFound != null && notFound.Any())
                throw new BadRequestException($"Position id {string.Join(", ", notFound)} cannot be set as parent because its not available on the list");

            var hierarchyId = Guid.NewGuid().ToString();
            var param = new TrHierarchyMapping
            {
                Id = hierarchyId,
                Name = body.Name,
                UserIn = AuthInfo.UserId
            };
            _dbContext.Entity<TrHierarchyMapping>().Add(param);

            var mappings = new List<TrHierarchyMappingDetail>();
            foreach (var parent in body.Hierarchies.Where(x => x.IdRolePositionParent == null))
            {
                mappings.Add(new TrHierarchyMappingDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHierarchyMapping = hierarchyId,
                    IdRolePosition = parent.IdRolePosition,
                    UserIn = AuthInfo.UserId
                });

                mappings = InsertDataRecursion(hierarchyId, parent.IdRolePosition, body.Hierarchies, mappings);
            }
            _dbContext.Entity<TrHierarchyMappingDetail>().AddRange(mappings);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateHierarchyMappingRequest, UpdateHierarchyMappingValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            // throw if any existing hierarchy mapping name
            var existHierarchy = await _dbContext.Entity<TrHierarchyMappingDetail>()
                .Include(x => x.HierarchyMapping)
                .Where(x => x.RolePosition.Role.IdSchool == body.IdSchool && x.HierarchyMapping.Name == body.Name && x.IdHierarchyMapping != body.Id)
                .Select(x => new ItemValueVm(x.IdHierarchyMapping, x.HierarchyMapping.Name))
                .FirstOrDefaultAsync(CancellationToken);
            if (existHierarchy != null)
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], "Hierarchy", "Name", body.Name));

            // find not found parent
            var notFound = body.Hierarchies.Select(x => x.IdRolePositionParent)
                                           .Except(body.Hierarchies.Select(y => y.IdRolePositionParent).Intersect(body.Hierarchies.Select(y => y.IdRolePosition)))
                                           .Where(x => x != null);
            if (notFound != null && notFound.Any())
                throw new BadRequestException($"Position id {string.Join(", ", notFound)} cannot be set as parent because its not available on the list");

            var hierarchy = await _dbContext.Entity<TrHierarchyMapping>()
                                            .Include(x => x.HierarchyMappingDetails)
                                            .Where(x => x.Id == body.Id)
                                            .SingleOrDefaultAsync();
            if (hierarchy is null)
                throw new NotFoundException("Hierarchy is not found");

            hierarchy.Name = body.Name;
            hierarchy.UserUp = AuthInfo.UserId;
            _dbContext.Entity<TrHierarchyMapping>().Update(hierarchy);

            foreach (var deleted in hierarchy.HierarchyMappingDetails)
            {
                deleted.IsActive = false;
                _dbContext.Entity<TrHierarchyMappingDetail>().Update(deleted);
            }

            var mappings = new List<TrHierarchyMappingDetail>();
            foreach (var parent in body.Hierarchies.Where(x => x.IdRolePositionParent == null))
            {
                mappings.Add(new TrHierarchyMappingDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHierarchyMapping = hierarchy.Id,
                    IdRolePosition = parent.IdRolePosition,
                    UserIn = AuthInfo.UserId
                });

                mappings = InsertDataRecursion(hierarchy.Id, parent.IdRolePosition, body.Hierarchies, mappings);
            }
            _dbContext.Entity<TrHierarchyMappingDetail>().AddRange(mappings);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private List<TrHierarchyMappingDetail> InsertDataRecursion(string hierarchyId, string parentId, List<HierarchyRequest> source, List<TrHierarchyMappingDetail> result)
        {
            foreach (var item in source.Where(x => x.IdRolePositionParent == parentId))
            {
                result.Add(new TrHierarchyMappingDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHierarchyMapping = hierarchyId,
                    IdRolePosition = item.IdRolePosition,
                    IdParent = result.First(x => x.IdRolePosition == parentId).Id,
                    UserIn = AuthInfo.UserId
                });
                InsertDataRecursion(hierarchyId, item.IdRolePosition, source, result);
            }
            return result;
        }

        private List<HierarchyResult> GetTrailDataRecursion(string parentId, ICollection<TrHierarchyMappingDetail> source, List<HierarchyResult> result)
        {
            foreach (var item in source.Where(x => x.IdParent == parentId))
            {
                result.Add(new HierarchyResult
                {
                    IdRolePosition = item.RolePosition.Id,
                    Role = new CodeWithIdVm
                    {
                        Id = item.RolePosition.Role.Id,
                        Code = item.RolePosition.Role.Code,
                        Description = item.RolePosition.Role.Description
                    },
                    TeacherPosition = new CodeWithIdVm
                    {
                        Id = item.RolePosition.TeacherPosition.Id,
                        Code = item.RolePosition.TeacherPosition.Code,
                        Description = item.RolePosition.TeacherPosition.Description
                    },
                    Childs = GetTrailDataRecursion(item.Id, source, new List<HierarchyResult>())
                });
            }
            return result;
        }
    }
}
