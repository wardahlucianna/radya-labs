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
    public class MeritDemeritLevelInfractionHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public MeritDemeritLevelInfractionHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetLevel = await _dbContext.Entity<MsLevelOfInteraction>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetLevel.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsLevelOfInteraction>().UpdateRange(GetLevel);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMeritDemeritLevelInfractionRequest>(nameof(GetMeritDemeritLevelInfractionRequest.IdSchool));
            var predicate = PredicateBuilder.Create<MsLevelOfInteraction>(x => x.IdSchool == param.IdSchool);
            IReadOnlyList<IItemValueVm> items = default;

            var result = await (from Level in _dbContext.Entity<MsLevelOfInteraction>()
                                join LevelChild in _dbContext.Entity<MsLevelOfInteraction>() on Level.Id equals LevelChild.IdParentLevelOfInteraction into JoinedLevel
                                from LevelChild in JoinedLevel.DefaultIfEmpty()
                                join MeritDemerit in _dbContext.Entity<MsMeritDemeritMapping>() on Level.Id equals MeritDemerit.IdLevelOfInteraction into JoinedMeritDemerit
                                from MeritDemerit in JoinedMeritDemerit.DefaultIfEmpty()
                                where Level.IdSchool == param.IdSchool
                                select new
                                {
                                    Id = Level.Id,
                                    SortParentName = Level.IdParentLevelOfInteraction == null ? Level.NameLevelOfInteraction : Level.Parent.NameLevelOfInteraction,
                                    sortName = Level.IdParentLevelOfInteraction == null ? Level.NameLevelOfInteraction : Level.Parent.NameLevelOfInteraction + Level.NameLevelOfInteraction,
                                    NameLavelInfraction = Level.NameLevelOfInteraction,
                                    Parent = Level.IdParentLevelOfInteraction == null ? "" : Level.Parent.NameLevelOfInteraction,
                                    IsApproval = Level.IsUseApproval,
                                    IsDisabledChecked = MeritDemerit != null || LevelChild != null ? true : false,
                                    IsDisabledDelete = MeritDemerit != null || LevelChild != null ? true : false,
                                }
                        ).Distinct().OrderBy(x => x.SortParentName).ThenBy(x => x.sortName).ToListAsync(CancellationToken);

            var ResultOrderBy = result.Select(e => new
            {
                SortParentName = Convert.ToInt32(e.SortParentName),
                sortName = e.sortName,
                Id = e.Id,
                NameLavelInfraction = e.NameLavelInfraction,
                Parent = e.Parent,
                IsApproval = e.IsApproval,
                IsDisabledChecked = e.IsDisabledChecked,
                IsDisabledDelete = e.IsDisabledDelete,
            }).Distinct().OrderBy(x => x.SortParentName).ToList();

            items = ResultOrderBy.Select(e => new GetMeritDemeritLevelInfractionResult
            {
                Id = e.Id,
                NameLavelInfraction = e.NameLavelInfraction,
                Parent = e.Parent,
                IsApproval = e.IsApproval,
                IsDisabledChecked = e.IsDisabledChecked,
                IsDisabledDelete = e.IsDisabledDelete,
            }).ToList();

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritLevelInfractionRequest, AddMeritDemeritLevelInfractionValidator>();

            var School = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == body.IdSchool)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(CancellationToken);

            if (School is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            var GetLevelOfInfraction = _dbContext.Entity<MsLevelOfInteraction>().Where(e => e.IdSchool == body.IdSchool).ToList();
            foreach (var itemInfraction in body.LevelInfraction)
            {
                if (itemInfraction.NameLevelOfInfraction == null || itemInfraction.NameLevelOfInfraction == "")
                    throw new BadRequestException("Name Lavel Of Infraction cannot empty");

                var parent = _dbContext.Entity<MsLevelOfInteraction>().SingleOrDefault(e => e.NameLevelOfInteraction == itemInfraction.NameParent && e.IdSchool == body.IdSchool);
                var LevelOfInfractionByName = GetLevelOfInfraction.SingleOrDefault(e => e.Id == itemInfraction.Id);
                if (LevelOfInfractionByName == null)
                {
                    if (itemInfraction.NameLevelOfInfraction == "NaN")
                        throw new BadRequestException(string.Format("value {0} in NameLevelOfInfraction invalid", itemInfraction.NameLevelOfInfraction));
                    var newLevelInfraction = new MsLevelOfInteraction
                    {
                        Id = Guid.NewGuid().ToString(),
                        NameLevelOfInteraction = itemInfraction.NameLevelOfInfraction,
                        IdParentLevelOfInteraction = parent == null ? null : parent.Id,
                        IdSchool = School.Id,
                        IsUseApproval = itemInfraction.IsUseApproval,
                    };

                    _dbContext.Entity<MsLevelOfInteraction>().Add(newLevelInfraction);
                }
                else
                {
                    LevelOfInfractionByName.NameLevelOfInteraction = itemInfraction.NameLevelOfInfraction;
                    LevelOfInfractionByName.IdParentLevelOfInteraction = parent == null ? null : parent.Id;
                    LevelOfInfractionByName.IsUseApproval = itemInfraction.IsUseApproval;
                    _dbContext.Entity<MsLevelOfInteraction>().Update(LevelOfInfractionByName);
                }
                await _dbContext.SaveChangesAsync(CancellationToken);

            }

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            return Request.CreateApiResult2();
        }
    }
}
