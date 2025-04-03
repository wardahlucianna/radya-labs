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
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using BinusSchool.User.FnUser.Role.Validator;
using BinusSchool.User.FnUser.Utils;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BinusSchool.User.FnUser.Role
{
    public class RoleHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;

        public RoleHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<LtRole>()
                .Include(x => x.UserRoles)
                .Include(x => x.RolePositions).ThenInclude(x => x.HierarchyMappingDetails)
                .Include(x => x.ApprovalStates)
                .Include(x => x.MessageApprovals)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                // don't set inactive when row have to-many relation
                if (data.UserRoles.Any() || data.ApprovalStates.Any() || data.MessageApprovals.Any() || data.RolePositions.Any(y => y.HierarchyMappingDetails.Any()))
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description));
                }
                else if (!data.CanDeleted)
                {
                    undeleted.CurrentlyRun ??= new Dictionary<string, string>();
                    undeleted.CurrentlyRun.Add(data.Id, string.Format(Localizer["ExCurrentlyRun"], data.Description));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<LtRole>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var result = await _dbContext.Entity<LtRole>()
                                         .Include(x => x.RoleGroup)
                                         .Include(x => x.RoleSettings)
                                         .Include(x => x.RolePermissions)
                                         .Include(x => x.RolePositions).ThenInclude(x => x.TeacherPosition)
                                         .Where(x => x.Id == id)
                                         .Select(x => new RoleDetailResult
                                         {
                                             Id = x.Id,
                                             RoleGroup = new CodeWithIdVm
                                             {
                                                 Id = x.RoleGroup.Id,
                                                 Code = x.RoleGroup.Code,
                                                 Description = x.RoleGroup.Description
                                             },
                                             Code = x.Code,
                                             Description = x.Description,
                                             IsArrangeUsernameFormat = x.RoleSettings.Any() ? x.RoleSettings.FirstOrDefault().IsArrangeUsernameFormat : false,
                                             UsernameFormat = x.RoleSettings.Any() ? x.RoleSettings.FirstOrDefault().UsernameFormat : null,
                                             UsernameExample = x.RoleSettings.Any() ? x.RoleSettings.FirstOrDefault().UsernameExample : null,
                                             PermissionIds = x.RolePermissions.Any() ? x.RolePermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Select(y => y.IdFeaturePermission).ToList() : null,
                                             //MobilePermissionIds = x.RolePermissions.Any() ? x.RolePermissions.Where(y => y.Type != RolePermissionType.Web.ToString())
                                             //.Select(y => new MobilePermissionRequest
                                             //{
                                             //    Id = y.IdFeaturePermission,
                                             //    Type = y.Type,
                                             //    FeatureOrderNumber = y.FeaturePermission.Feature.OrderNumber
                                             //}).OrderBy(y => y.FeatureOrderNumber).ToList() : null,
                                             RolePositions = x.RolePositions.Any() ? x.RolePositions.Select(y => new RolePositionResult
                                             {
                                                 Id = y.Id,
                                                 TeacherPosition = new CodeWithIdVm
                                                 {
                                                     Id = y.TeacherPosition.Id,
                                                     Code = y.TeacherPosition.Code,
                                                     Description = y.TeacherPosition.Description
                                                 },
                                                 PermissionIds = y.RolePositionPermissions.Where(z => z.Type == RolePermissionType.Web.ToString()).Any() ? y.RolePositionPermissions.Where(z => z.Type == RolePermissionType.Web.ToString()).Select(z => z.IdFeaturePermission).ToList() : null,
                                                 //MobilePermissionIds = y.RolePositionPermissions.Where(z => z.Type != RolePermissionType.Web.ToString()).Any() ? y.RolePositionPermissions.Where(z => z.Type != RolePermissionType.Web.ToString()).Select(z => new MobilePermissionRequest
                                                 //{
                                                 //    Id = z.IdFeaturePermission,
                                                 //    Type = z.Type,
                                                 //    FeatureOrderNumber = z.FeaturePermission.Feature.OrderNumber
                                                 //}).OrderBy(z => z.FeatureOrderNumber).ToList() : null
                                             }).ToList() : null
                                         }).SingleOrDefaultAsync();

            if (result is null)
                throw new NotFoundException("Role is not found");

            return Request.CreateApiResult2(result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetRoleRequest>(nameof(GetRoleRequest.IdSchool));

            var columns = new[] { "description", "code", "date" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "description" },
                { columns[1], "code" },
                { columns[2], "dateIn" }
            };

            var predicate = PredicateBuilder.Create<LtRole>(x => param.IdSchool.Contains(x.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                       || EF.Functions.Like(x.Code, param.SearchPattern()));

            if (param.HasPosition.HasValue)
                predicate = predicate.And(x => x.RolePositions.Any() == param.HasPosition.Value);

            if (param.IdRoleGroups != null)
                predicate = predicate.And(x => param.IdRoleGroups.Distinct().Contains(x.IdRoleGroup));

            var query = _dbContext.Entity<LtRole>()
                                  .Include(x => x.RoleGroup)
                                  .Include(x => x.RolePositions).ThenInclude(x => x.HierarchyMappingDetails)
                                  .Include(x => x.UserRoles)
                                  .Include(x => x.ApprovalStates)
                                  .Include(x => x.MessageApprovals)
                                  .Where(predicate)
                                  .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new RoleResult
                    {
                        Id = x.Id,
                        RoleGroup = new CodeWithIdVm
                        {
                            Id = x.RoleGroup.Id,
                            Code = x.RoleGroup.Code,
                            Description = x.RoleGroup.Description
                        },
                        Code = x.Code,
                        Description = x.Description,
                        CanDeleted = x.CanDeleted
                                     && !x.UserRoles.Any()
                                     && !x.ApprovalStates.Any()
                                     && !x.MessageApprovals.Any()
                                     && !x.RolePositions.Any(y => y.HierarchyMappingDetails.Any()),
                        CreatedDate = x.DateIn
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddRoleRequest, AddRoleValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (await _dbContext.Entity<LtRole>()
                                .Where(x => x.IdSchool == body.IdSchool
                                            && x.Code == body.Code)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Role code {body.Code} already exists");

            if (await _dbContext.Entity<LtRole>()
                                .Where(x => x.IdSchool == body.IdSchool
                                            && x.Description == body.Description)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Role name {body.Description} already exists");

            if (!await _dbContext.Entity<LtRoleGroup>().AnyAsync(x => x.Id == body.IdRoleGroup))
                throw new BadRequestException("Role group is not found");

            var roleId = Guid.NewGuid().ToString();
            var param = new LtRole
            {
                Id = roleId,
                IdSchool = body.IdSchool,
                IdRoleGroup = body.IdRoleGroup,
                Code = body.Code,
                Description = body.Description,
                CanDeleted = true
            };
            _dbContext.Entity<LtRole>().Add(param);

            _dbContext.Entity<LtRoleSetting>().Add(new LtRoleSetting
            {
                Id = Guid.NewGuid().ToString(),
                IdRole = roleId,
                IsArrangeUsernameFormat = body.IsArrangeUsernameFormat,
                UsernameFormat = body.UsernameFormat,
                UsernameExample = body.UsernameExample,
            });

            if (body.Positions != null && body.Positions.Any())
            {
                var idTeacherPositions = body.Positions.Select(x => x.IdTeacherPosition).ToList();
                var teacherPositions = await _dbContext.Entity<MsTeacherPosition>()
                                                       .Where(x => x.IdSchool == body.IdSchool
                                                                   && idTeacherPositions.Contains(x.Id))
                                                       .ToListAsync();

                var notFoundPositions = idTeacherPositions.Except(idTeacherPositions.Intersect(teacherPositions.Select(x => x.Id)));
                if (notFoundPositions.Any())
                    throw new NotFoundException($"Some position id is not found ({string.Join(", ", notFoundPositions)})");

                foreach (var position in body.Positions)
                {
                    var positionId = Guid.NewGuid().ToString();
                    _dbContext.Entity<TrRolePosition>().Add(new TrRolePosition
                    {
                        Id = positionId,
                        IdRole = roleId,
                        IdTeacherPosition = position.IdTeacherPosition
                    });

                    #region EH-41
                    if (position.WebPermissionIds != null && position.WebPermissionIds.Any())
                    {
                        var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                                          .Where(x => position.WebPermissionIds.Contains(x.Id))
                                                          .ToListAsync();
                        var notFoundPermissions = position.WebPermissionIds.Except(position.WebPermissionIds.Intersect(permissions.Select(x => x.Id)));
                        if (notFoundPermissions.Any())
                            throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                        _dbContext.Entity<TrRolePositionPermission>().AddRange(position.WebPermissionIds.Select(x => new TrRolePositionPermission
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdRolePosition = positionId,
                            IdFeaturePermission = x,
                            Type = RolePermissionType.Web.ToString()
                        }));
                    }

                    if (position.MobilePermissionIds != null && position.MobilePermissionIds.Any())
                    {
                        if (position.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != position.MobilePermissionIds.Count())
                            throw new BadRequestException("Cannot add duplicate mobile menus");

                        var mobilePermissionIds = position.MobilePermissionIds.Select(x => x.Id).ToList();
                        var permissions = await _dbContext.Entity<MsFeaturePermission>()
                            .Include(x => x.Feature)
                            .Where(x => mobilePermissionIds.Contains(x.Id))
                            .ToListAsync();

                        var notFoundPermissions = mobilePermissionIds.Except(mobilePermissionIds.Intersect(permissions.Select(x => x.Id)));
                        if (notFoundPermissions.Any())
                            throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                        var dashboardFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Dashboard).Select(x => x.Id).FirstOrDefault();
                        if (dashboardFeaturePermission is null)
                            throw new NotFoundException("Dashboard menu for mobile not found");
                        if (position.MobilePermissionIds
                                .Any(x => x.Id == dashboardFeaturePermission && x.Type == RolePermissionType.BottomNavigation1.ToString()) == false)
                            throw new BadRequestException("Invalid dashboard for mobile type");
                        
                        var logoutFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Logout).Select(x => x.Id).FirstOrDefault();
                        if (logoutFeaturePermission is null)
                            throw new NotFoundException("Logout menu for mobile not found");

                        _dbContext.Entity<TrRolePositionPermission>().AddRange(position.MobilePermissionIds.Select(x => new TrRolePositionPermission
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdRolePosition = positionId,
                            IdFeaturePermission = x.Id,
                            Type = x.Type
                        }));
                    }
                    #endregion
                }
            }
            else
            {
                #region EH-41
                if (body.WebPermissionIds != null && body.WebPermissionIds.Any())
                {
                    var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                  .Where(x => body.WebPermissionIds.Contains(x.Id))
                                  .ToListAsync();

                    var notFoundPermissions = body.WebPermissionIds.Except(body.WebPermissionIds.Intersect(permissions.Select(x => x.Id)));
                    if (notFoundPermissions.Any())
                        throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                    _dbContext.Entity<TrRolePermission>().AddRange(body.WebPermissionIds.Select(x => new TrRolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = roleId,
                        IdFeaturePermission = x,
                        Type = RolePermissionType.Web.ToString(),
                    }));
                }

                if (body.MobilePermissionIds != null && body.MobilePermissionIds.Any())
                {
                    if (body.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != body.MobilePermissionIds.Count())
                        throw new BadRequestException("Cannot add duplicate mobile menus");

                    var mobilePermissions = await _dbContext.Entity<MsFeaturePermission>()
                        .Include(x => x.Feature)
                        .Where(x => body.MobilePermissionIds.Select(x => x.Id).Contains(x.Id))
                        .ToListAsync();

                    var notFoundMobilePermissions = body.MobilePermissionIds.Select(x => x.Id).Except(body.MobilePermissionIds.Select(x => x.Id).Intersect(mobilePermissions.Select(x => x.Id)));
                    if (notFoundMobilePermissions.Any())
                        throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundMobilePermissions)})");

                    // Mandatory dashboard and type validation
                    var dashboardFeaturePermission = mobilePermissions.Where(x => x.Feature.Action == MobileActionString.Dashboard).Select(x => x.Id).FirstOrDefault();
                    if (dashboardFeaturePermission is null)
                        throw new NotFoundException("Dashboard menu for mobile not found");
                    if (body.MobilePermissionIds.Any(x => x.Id == dashboardFeaturePermission && x.Type == RolePermissionType.BottomNavigation1.ToString()) == false)
                        throw new BadRequestException("Invalid dashboard for mobile type");

                    var logoutFeaturePermission = mobilePermissions.Where(x => x.Feature.Action == MobileActionString.Logout).Select(x => x.Id).FirstOrDefault();
                    if (logoutFeaturePermission is null)
                        throw new NotFoundException("Logout menu for mobile not found");

                    _dbContext.Entity<TrRolePermission>().AddRange(body.MobilePermissionIds.Select(x => new TrRolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = roleId,
                        IdFeaturePermission = x.Id,
                        Type = x.Type
                    }));
                }
                #endregion
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateRoleRequest, UpdateRoleValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<LtRole>()
                                       .Include(x => x.RoleSettings)
                                       .Include(x => x.RolePermissions)
                                       .Include(x => x.RolePositions).ThenInclude(x => x.HierarchyMappingDetails)
                                       .Include(x => x.RolePositions).ThenInclude(x => x.RolePositionPermissions)
                                       .Include(x => x.RolePositions).ThenInclude(x => x.TeacherPosition)
                                       .FirstOrDefaultAsync(x => x.Id == body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Role"], "Id", body.Id));

            if (await _dbContext.Entity<LtRole>()
                                .Where(x => x.Id != body.Id
                                            && x.IdSchool == body.IdSchool
                                            && x.Code == body.Code)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Role code {body.Code} already exists");

            if (await _dbContext.Entity<LtRole>()
                                .Where(x => x.Id != body.Id
                                            && x.IdSchool == body.IdSchool
                                            && x.Description == body.Description)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Role name {body.Description} already exists");

            if (!await _dbContext.Entity<LtRoleGroup>().AnyAsync(x => x.Id == body.IdRoleGroup))
                throw new BadRequestException("Role group is not found");

            data.IdSchool = body.IdSchool;
            data.IdRoleGroup = body.IdRoleGroup;
            data.Code = body.Code;
            data.Description = body.Description;
            _dbContext.Entity<LtRole>().Update(data);

            var setting = data.RoleSettings.FirstOrDefault(x => x.IsActive);
            if (setting is null)
                _dbContext.Entity<LtRoleSetting>().Add(new LtRoleSetting
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = body.Id,
                    IsArrangeUsernameFormat = body.IsArrangeUsernameFormat,
                    UsernameFormat = body.UsernameFormat,
                    UsernameExample = body.UsernameExample
                });
            else
            {
                setting.IsArrangeUsernameFormat = body.IsArrangeUsernameFormat;
                setting.UsernameFormat = body.UsernameFormat;
                setting.UsernameExample = body.UsernameExample;
                _dbContext.Entity<LtRoleSetting>().Update(setting);
            }

            if (body.Positions != null && body.Positions.Any())
            {
                // delete role permission if any
                if (data.RolePermissions.Any())
                {
                    foreach (var deleted in data.RolePermissions)
                    {
                        deleted.IsActive = false;
                        _dbContext.Entity<TrRolePermission>().Update(deleted);
                    }
                }

                var idTeacherPositions = body.Positions.Select(x => x.IdTeacherPosition).ToList();
                var teacherPositions = await _dbContext.Entity<MsTeacherPosition>()
                                                       .Where(x => x.IdSchool == body.IdSchool
                                                                   && idTeacherPositions.Contains(x.Id))
                                                       .ToListAsync();

                var notFoundPositions = idTeacherPositions.Except(idTeacherPositions.Intersect(teacherPositions.Select(x => x.Id)));
                if (notFoundPositions.Any())
                    throw new NotFoundException($"Some position id is not found ({string.Join(", ", notFoundPositions)})");

                if (data.RolePositions.Any())
                {
                    foreach (var deleted in data.RolePositions.Where(x => !idTeacherPositions.Any(y => y == x.IdTeacherPosition)))
                    {
                        if (deleted.HierarchyMappingDetails.Any())
                            throw new BadRequestException($"Cannot remove {deleted.TeacherPosition.Description} because its already used on hierarchy mapping");

                        deleted.IsActive = false;
                        _dbContext.Entity<TrRolePosition>().Update(deleted);
                    }
                }

                // update role position permission
                var updated = body.Positions.Where(x => data.RolePositions.Select(y => y.IdTeacherPosition).Contains(x.IdTeacherPosition));
                if (updated.Any())
                {
                    foreach (var update in updated)
                    {
                        var dataUpdated = data.RolePositions.First(x => x.IdTeacherPosition == update.IdTeacherPosition);

                        if (update.WebPermissionIds != null && update.WebPermissionIds.Any())
                        {
                            var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                  .Where(x => update.WebPermissionIds.Contains(x.Id))
                                  .ToListAsync();

                            var notFoundPermissions = update.WebPermissionIds.Except(update.WebPermissionIds.Intersect(permissions.Select(x => x.Id)));
                            if (notFoundPermissions.Any())
                                throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                            var webRolePositionPermissions = dataUpdated.RolePositionPermissions.Where(x => x.Type == RolePermissionType.Web.ToString()).ToList();

                            if (webRolePositionPermissions.Any())
                            {
                                foreach (var deleted in webRolePositionPermissions.Where(x => !update.WebPermissionIds.Any(y => y == x.IdFeaturePermission)))
                                {
                                    deleted.IsActive = false;
                                    _dbContext.Entity<TrRolePositionPermission>().Update(deleted);
                                }
                            }

                            _dbContext.Entity<TrRolePositionPermission>()
                                .AddRange(update.WebPermissionIds.Where(x => !webRolePositionPermissions.Any(y => y.IdFeaturePermission == x)).Select(x => new TrRolePositionPermission
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdRolePosition = dataUpdated.Id,
                                    IdFeaturePermission = x,
                                    Type = RolePermissionType.Web.ToString()
                                }));
                        }
                        else if (dataUpdated.RolePositionPermissions.Any())
                        {
                            foreach (var deleted in dataUpdated.RolePositionPermissions)
                            {
                                deleted.IsActive = false;
                                _dbContext.Entity<TrRolePositionPermission>().Update(deleted);
                            }
                        }

                        if (update.MobilePermissionIds != null && update.MobilePermissionIds.Any())
                        {
                            if (update.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != update.MobilePermissionIds.Count())
                                throw new BadRequestException("Cannot add duplicate mobile menus");

                            var mobilePermissionIds = update.MobilePermissionIds.Select(x => new
                            {
                                id = x.Id,
                                type = x.Type
                            }).ToList();
                            var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                .Include(x => x.Feature)
                                .Where(x => mobilePermissionIds.Select(y => y.id).Contains(x.Id))
                                .ToListAsync();

                            var notFoundPermissions = mobilePermissionIds.Select(y => y.id).Except(mobilePermissionIds.Select(x => x.id).Intersect(permissions.Select(x => x.Id)));
                            if (notFoundPermissions.Any())
                                throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                            var dashboardFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Dashboard).Select(x => x.Id).FirstOrDefault();
                            if (dashboardFeaturePermission is null)
                                throw new NotFoundException("Dashboard menu for mobile not found");
                            if (update.MobilePermissionIds
                                    .Any(x => x.Id == dashboardFeaturePermission && x.Type == RolePermissionType.BottomNavigation1.ToString()) == false)
                                throw new BadRequestException("Invalid dashboard for mobile type");

                            var logoutFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Logout).Select(x => x.Id).FirstOrDefault();
                            if (logoutFeaturePermission is null)
                                throw new NotFoundException("Logout menu for mobile not found");
                            if (update.MobilePermissionIds.Any(x => x.Id == logoutFeaturePermission && x.Type == RolePermissionType.MoreMenu.ToString()) == false)
                                throw new BadRequestException("Invalid logout for mobile type");

                            var mobileRolePositionPermissions = dataUpdated.RolePositionPermissions.Where(x => x.Type != RolePermissionType.Web.ToString()).ToList();

                            if (mobileRolePositionPermissions.Any())
                            {
                                foreach (var deleted in mobileRolePositionPermissions.Where(x => !mobilePermissionIds.Any(y => y.id == x.IdFeaturePermission && y.type == x.Type)))
                                {
                                    deleted.IsActive = false;
                                    _dbContext.Entity<TrRolePositionPermission>().Update(deleted);
                                }
                            }

                            _dbContext.Entity<TrRolePositionPermission>().AddRange(update.MobilePermissionIds.Where(x => !mobileRolePositionPermissions.Any(y => y.IdFeaturePermission == x.Id && x.Type == y.Type)).Select(x => new TrRolePositionPermission
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdRolePosition = dataUpdated.Id,
                                IdFeaturePermission = x.Id,
                                Type = x.Type
                            }));
                        }

                    }
                }

                // add new role position
                var newest = body.Positions.Where(x => !data.RolePositions.Select(y => y.IdTeacherPosition).Contains(x.IdTeacherPosition));
                if (newest.Any())
                {
                    foreach (var position in newest)
                    {
                        var positionId = Guid.NewGuid().ToString();
                        _dbContext.Entity<TrRolePosition>().Add(new TrRolePosition
                        {
                            Id = positionId,
                            IdRole = data.Id,
                            IdTeacherPosition = position.IdTeacherPosition
                        });

                        if (position.WebPermissionIds != null && position.WebPermissionIds.Any())
                        {
                            if (position.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != position.MobilePermissionIds.Count())
                                throw new BadRequestException("Cannot add duplicate mobile menus");

                            var mobilePermissionIds = position.MobilePermissionIds.Select(x => x.Id).ToList();
                            var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                .Where(x => position.WebPermissionIds.Contains(x.Id))
                                .ToListAsync();

                            var notFoundPermissions = position.WebPermissionIds.Except(position.WebPermissionIds.Intersect(permissions.Select(x => x.Id)));
                            if (notFoundPermissions.Any())
                                throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                            _dbContext.Entity<TrRolePositionPermission>().AddRange(position.WebPermissionIds.Select(x => new TrRolePositionPermission
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdRolePosition = positionId,
                                IdFeaturePermission = x,
                                Type = RolePermissionType.Web.ToString()
                            }));
                        }

                        if (position.MobilePermissionIds != null && position.MobilePermissionIds.Any())
                        {
                            if (position.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != position.MobilePermissionIds.Count())
                                throw new BadRequestException("Cannot add duplicate mobile menus");

                            var mobilePermissionIds = position.MobilePermissionIds.Select(x => x.Id).ToList();
                            var permissions = await _dbContext.Entity<MsFeaturePermission>()
                                .Include(x => x.Feature)
                                .Where(x => mobilePermissionIds.Contains(x.Id))
                                .ToListAsync();

                            var notFoundPermissions = mobilePermissionIds.Except(mobilePermissionIds.Intersect(permissions.Select(x => x.Id)));
                            if (notFoundPermissions.Any())
                                throw new NotFoundException($"Some permission id is not found ({string.Join(", ", notFoundPermissions)})");

                            var dashboardFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Dashboard).Select(x => x.Id).FirstOrDefault();
                            if (dashboardFeaturePermission is null)
                                throw new NotFoundException("Dashboard menu for mobile not found");
                            if (position.MobilePermissionIds.Any(x => x.Id == dashboardFeaturePermission && x.Type == RolePermissionType.BottomNavigation1.ToString()) == false)
                                throw new BadRequestException("Invalid dashboard for mobile type");

                            var logoutFeaturePermission = permissions.Where(x => x.Feature.Action == MobileActionString.Logout).Select(x => x.Id).FirstOrDefault();
                            if (logoutFeaturePermission is null)
                                throw new NotFoundException("Logout menu for mobile not found");
                            if (position.MobilePermissionIds.Any(x => x.Id == logoutFeaturePermission && x.Type == RolePermissionType.MoreMenu.ToString()) == false)
                                throw new BadRequestException("Invalid logout for mobile type");

                            _dbContext.Entity<TrRolePositionPermission>().AddRange(position.MobilePermissionIds.Select(x => new TrRolePositionPermission
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdRolePosition = positionId,
                                IdFeaturePermission = x.Id,
                                Type = x.Type
                            }));
                        }
                    }
                }
            }
            else if ((body.WebPermissionIds != null && body.WebPermissionIds.Any()) || (body.MobilePermissionIds != null && body.MobilePermissionIds.Any()))
            {
                // delete role position if any
                if (data.RolePositions.Any())
                {
                    foreach (var deleted in data.RolePositions)
                    {
                        if (deleted.HierarchyMappingDetails.Any())
                            throw new BadRequestException($"Cannot remove {deleted.TeacherPosition.Description} because its already used on hierarchy mapping");

                        deleted.IsActive = false;
                        _dbContext.Entity<TrRolePosition>().Update(deleted);
                    }
                }

                if (body.WebPermissionIds != null && body.WebPermissionIds.Any())
                {
                    var webPermissions = await _dbContext.Entity<MsFeaturePermission>()
                                                      .Where(x => body.WebPermissionIds.Contains(x.Id))
                                                      .ToListAsync();

                    var webNotFoundPermissions = body.WebPermissionIds.Except(body.WebPermissionIds.Intersect(webPermissions.Select(x => x.Id)));
                    if (webNotFoundPermissions.Any())
                        throw new NotFoundException($"Some permission id is not found ({string.Join(", ", webNotFoundPermissions)})");

                    if (data.RolePermissions.Any(x => x.Type == RolePermissionType.Web.ToString()))
                    {
                        foreach (var deleted in data.RolePermissions.Where(x => x.Type == RolePermissionType.Web.ToString() && !body.WebPermissionIds.Any(y => y == x.IdFeaturePermission)))
                        {
                            deleted.IsActive = false;
                            _dbContext.Entity<TrRolePermission>().Update(deleted);
                        }
                    }

                    _dbContext.Entity<TrRolePermission>().AddRange(body.WebPermissionIds.Where(x => !data.RolePermissions.Any(y => y.IdFeaturePermission == x)).Select(x => new TrRolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = data.Id,
                        IdFeaturePermission = x,
                        Type = RolePermissionType.Web.ToString()
                    }));
                }

                if (body.MobilePermissionIds != null && body.MobilePermissionIds.Any())
                {
                    if (body.MobilePermissionIds.Select(x => x.Id).Distinct().Count() != body.MobilePermissionIds.Count())
                        throw new BadRequestException("Cannot add duplicate mobile menus");

                    var mobilePermissionIds = body.MobilePermissionIds.Select(x => new
                    {
                        id = x.Id,
                        type = x.Type
                    }).ToList();
                    var mobilePermissions = await _dbContext.Entity<MsFeaturePermission>()
                        .Include(x => x.Feature)
                        .Where(x => mobilePermissionIds.Select(y => y.id).Contains(x.Id))
                        .ToListAsync();

                    var mobileNotFoundPermissions = mobilePermissionIds.Select(x => x.id).Except(mobilePermissionIds.Select(y => y.id).Intersect(mobilePermissions.Select(x => x.Id)));
                    if (mobileNotFoundPermissions.Any())
                        throw new NotFoundException($"Some permission id is not found ({string.Join(", ", mobileNotFoundPermissions)})");

                    var dashboardFeaturePermission = mobilePermissions.Where(x => x.Feature.Action == MobileActionString.Dashboard).Select(x => x.Id).FirstOrDefault();
                    if (dashboardFeaturePermission is null)
                        throw new NotFoundException("Dashboard menu for mobile not found");
                    if (body.MobilePermissionIds.Any(x => x.Id == dashboardFeaturePermission && x.Type == RolePermissionType.BottomNavigation1.ToString()) == false)
                        throw new BadRequestException("Invalid dashboard for mobile type");
                    
                    var logoutFeaturePermission = mobilePermissions.Where(x => x.Feature.Action == MobileActionString.Logout).Select(x => x.Id).FirstOrDefault();
                    if (logoutFeaturePermission is null)
                        throw new NotFoundException("Logout menu for mobile not found");
                    if (body.MobilePermissionIds.Any(x => x.Id == logoutFeaturePermission && x.Type == RolePermissionType.MoreMenu.ToString()) == false)
                        throw new BadRequestException("Invalid logout for mobile type");

                    if (data.RolePermissions.Any(x => x.Type != RolePermissionType.Web.ToString()))
                    {
                        foreach (var deleted in data.RolePermissions.Where(x => x.Type != RolePermissionType.Web.ToString() && !mobilePermissionIds.Any(y => y.id == x.IdFeaturePermission && y.type == x.Type)))
                        {
                            deleted.IsActive = false;
                            _dbContext.Entity<TrRolePermission>().Update(deleted);
                        }
                    }

                    _dbContext.Entity<TrRolePermission>().AddRange(body.MobilePermissionIds.Where(x => !data.RolePermissions.Any(y => y.IdFeaturePermission == x.Id && y.Type == x.Type)).Select(x => new TrRolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = data.Id,
                        IdFeaturePermission = x.Id,
                        Type = x.Type
                    }));
                }
            }
            else
            {
                // delete role permission if any
                if (data.RolePermissions.Any())
                {
                    foreach (var deleted in data.RolePermissions)
                    {
                        deleted.IsActive = false;
                        _dbContext.Entity<TrRolePermission>().Update(deleted);
                    }
                }

                // delete role position if any
                if (data.RolePositions.Any())
                {
                    foreach (var deleted in data.RolePositions)
                    {
                        if (deleted.HierarchyMappingDetails.Any())
                            throw new BadRequestException($"Cannot remove {deleted.TeacherPosition.Description} because its already used on hierarchy mapping");

                        deleted.IsActive = false;
                        _dbContext.Entity<TrRolePosition>().Update(deleted);

                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
