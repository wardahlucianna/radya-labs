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
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.School.FnSchool.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class MeritDemeritSanctionHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public MeritDemeritSanctionHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetSanctionMapping.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsSanctionMapping>().UpdateRange(GetSanctionMapping);

            var GetSanctionMappingAttentionBy = await _dbContext.Entity<MsSanctionMappingAttentionBy>()
               .Where(x => ids.Contains(x.IdSanctionMapping))
               .ToListAsync(CancellationToken);

            GetSanctionMappingAttentionBy.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsSanctionMappingAttentionBy>().UpdateRange(GetSanctionMappingAttentionBy);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var GetSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.TeacherPosition)
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.Role)
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.TeacherPosition)
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.User)
                .Include(e=>e.AcademicYear)
                .Where(x => x.Id==id)
                .SingleOrDefaultAsync(CancellationToken);

            var items = new GetMeritDemeritSanctionMappingDetailResult
            {
                Id = GetSanctionMapping.Id,
                AcademicYear = new CodeWithIdVm 
                { 
                    Id = GetSanctionMapping.AcademicYear.Id, 
                    Code = GetSanctionMapping.AcademicYear.Code, 
                    Description = GetSanctionMapping.AcademicYear.Description
                },
                NameSanction = GetSanctionMapping.SanctionName,
                Min = GetSanctionMapping.Min,
                Max = GetSanctionMapping.Max,
                AttentionBy = GetSanctionMapping.SanctionMappingAttentionBies.Select(e => new DetailAttentionBy 
                {
                   IdTeacherPostion= new DetailAttentionByIdTeacherPostion
                   {
                       Id = e.TeacherPosition == null ? null : e.TeacherPosition.Id,
                       Code = e.TeacherPosition == null ? null : e.TeacherPosition.Code,
                       Description = e.TeacherPosition == null ? null : e.TeacherPosition.Description,
                       IdPostion= e.TeacherPosition == null ? null : e.TeacherPosition.IdPosition
                   },
                    IdRole = new CodeWithIdVm
                    {
                        Id = e.Role==null?null:e.Role.Id,
                        Code = e.Role == null ? null : e.Role.Code,
                        Description = e.Role == null ? null : e.Role.Description
                    },
                    IdUser = new CodeWithIdVm
                    {
                        Id = e.User == null ? null : e.User.Id,
                        Code = null,
                        Description = e.User == null ? null : e.User.DisplayName,
                    },
                }).ToList(),
            };

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMeritDemeritSanctionMappingRequest>();
            var predicate = PredicateBuilder.Create<MsSanctionMapping>(x => x.IsActive == true);
            string[] _columns = { "AcademicYear", "NameSanction", "MinSanction", "MaxSanction", "AttentionBy"};

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.SanctionName.Contains(param.Search));

            var query = _dbContext.Entity<MsSanctionMapping>()
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.TeacherPosition)
                .Include(e=>e.SanctionMappingAttentionBies).ThenInclude(e=>e.Role)
                .Where(predicate)
                .OrderBy(e=>e.UserIn)
               .Select(x => new
               {
                   Id = x.Id,
                   AcademicYear = x.AcademicYear.Description,
                   IdAcademicYear = x.IdAcademicYear,
                   NameSanction = x.SanctionName,
                   MinSanction = x.Min,
                   MaxSanction = x.Max,
                   AttentionBy = x.SanctionMappingAttentionBies.Select(e=>e.TeacherPosition==null?e.Role.Description: e.TeacherPosition.Description).ToList(),
                });

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "NameSanction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NameSanction)
                        : query.OrderBy(x => x.NameSanction);
                    break;
                case "MinSanction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.MinSanction)
                        : query.OrderBy(x => x.MinSanction);
                    break;
                case "MaxSanction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.MaxSanction)
                        : query.OrderBy(x => x.MaxSanction);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetMeritDemeritSanctionMappingResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    NameSanction = x.NameSanction,
                    MinSanction = x.MinSanction,
                    MaxSanction = x.MaxSanction,
                    AttentionBy = GetAttentionBy(x.AttentionBy),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetMeritDemeritSanctionMappingResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    NameSanction = x.NameSanction,
                    MinSanction = x.MinSanction,
                    MaxSanction = x.MaxSanction,
                    AttentionBy = GetAttentionBy(x.AttentionBy),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritSanctionMappingRequest, AddMeritDemeritSanctionMappingValidator>();
            var ay = _dbContext.Entity<MsAcademicYear>().SingleOrDefault(e => e.Id == body.IdAcademicYear);
            if (ay == null)
                throw new BadRequestException("Merit demerit mapping Sanction with academic year: " + body.IdAcademicYear + " is not exists.");

            var ExsisName = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && e.SanctionName == body.NameSanction);
            if (ExsisName)
                throw new BadRequestException("Merit demerit mapping Sanction with name: " + body.NameSanction + " is exists.");

            if (body.Min > body.Max)
                throw new BadRequestException("Merit demerit mapping Sanction with Min: " + body.Min + " max: " + body.Max + " are wrong number.");

            var ExsisMin = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (body.Min >= e.Min && body.Min <= e.Max));
            if (ExsisMin)
                throw new BadRequestException("Merit demerit mapping Sanction with Min: " + body.Min + " is exists.");

            var ExsisMax = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (body.Max >= e.Min && body.Max <= e.Max));
            if (ExsisMax)
                throw new BadRequestException("Merit demerit mapping Sanction with Max: " + body.Max + " is exists.");

            var ExsisMinBody = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (e.Min >= body.Min && e.Min <= body.Max));
            var ExsisMaxBody = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (e.Max >= body.Min && e.Max <= body.Max));

            if (ExsisMinBody || ExsisMaxBody)
                throw new BadRequestException("Merit demerit mapping Sanction with range: " + body.Min + " - " + body.Max + " are exists.");

            if (body.Attention.Count() != body.Attention.Select(e => new { e.IdPosition, e.IdRole, e.IdUser }).Distinct().Count())
                throw new BadRequestException("Attention by have the same data");


            var GetTeacherPosition = _dbContext.Entity<MsTeacherPosition>().Where(e=>body.Attention.Select(e=>e.IdPosition).ToList().Contains(e.Id) && e.IdSchool==ay.IdSchool).ToList();

            var IdSanction = Guid.NewGuid().ToString();
            var newSanctionMapping = new MsSanctionMapping
            {
                Id = IdSanction,
                SanctionName = body.NameSanction,
                IdAcademicYear = body.IdAcademicYear,
                Min = body.Min,
                Max = body.Max,
            };

            _dbContext.Entity<MsSanctionMapping>().Add(newSanctionMapping);

            foreach (var itemSuctionAttantionBy in body.Attention)
            {
                if (itemSuctionAttantionBy.IdPosition != null)
                {
                    var GetTeacherPositionById = GetTeacherPosition.SingleOrDefault(e => e.Id == itemSuctionAttantionBy.IdPosition);
                    if (GetTeacherPositionById == null)
                        throw new BadRequestException("Merit demerit mapping Sanction with id position: " + itemSuctionAttantionBy.IdPosition + " is not exists.");
                }

                var newSanctionMappingAttantionBy = new MsSanctionMappingAttentionBy
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSanctionMapping = IdSanction,
                    IdRole = itemSuctionAttantionBy.IdRole,
                    IdTeacherPosition = itemSuctionAttantionBy.IdPosition,
                    IdUser = itemSuctionAttantionBy.IdUser
                };
                _dbContext.Entity<MsSanctionMappingAttentionBy>().Add(newSanctionMappingAttantionBy);
            }
            

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMeritDemeritSanctionMappingRequest, UpdateMeritDemeritSanctionMappingValidator>();

            var ay = _dbContext.Entity<MsAcademicYear>().SingleOrDefault(e => e.Id == body.IdAcademicYear);
            if (ay == null)
                throw new BadRequestException("Merit demerit mapping Sanction with academic year: " + body.IdAcademicYear + " is not exists.");

            var SanctionMapping = _dbContext.Entity<MsSanctionMapping>().SingleOrDefault(e => e.Id == body.Id);
            if (SanctionMapping == null)
                throw new BadRequestException("Merit demerit mapping with id: " + body.Id + " is not found.");

            var GetTeacherPosition = _dbContext.Entity<MsTeacherPosition>().Where(e => body.Attention.Select(e => e.IdPosition).ToList().Contains(e.Id) && e.IdSchool == ay.IdSchool).ToList();

            var ExsisName = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && e.SanctionName == body.NameSanction && e.Id != body.Id);
            if (ExsisName)
                throw new BadRequestException("Merit demerit mapping Sanction with name: " + body.NameSanction + " is exists.");

            if (body.Min > body.Max)
                throw new BadRequestException("Merit demerit mapping Sanction with Min: " + body.Min + " max: " + body.Max + " are wrong number.");

            var ExsisMin = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (body.Min >= e.Min && body.Min <= e.Max) && e.Id != body.Id);
            if (ExsisMin)
                throw new BadRequestException("Merit demerit mapping Sanction with Min: " + body.Min + " is exists.");

            var ExsisMax = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (body.Max >= e.Min && body.Max <= e.Max) && e.Id != body.Id);
            if (ExsisMax)
                throw new BadRequestException("Merit demerit mapping Sanction with Max: " + body.Max + " is exists.");

            var ExsisMinBody = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (e.Min >= body.Min && e.Min <= body.Max) && e.Id != body.Id);
            var ExsisMaxBody = _dbContext.Entity<MsSanctionMapping>().Any(e => e.IdAcademicYear == body.IdAcademicYear && (e.Max >= body.Min && e.Max <= body.Max) && e.Id != body.Id);
            if (ExsisMinBody || ExsisMaxBody)
                throw new BadRequestException("Merit demerit mapping Sanction with range: " + body.Min + " - " + body.Max + " are exists.");

            if (body.Attention.Count()!= body.Attention.Select(e => new { e.IdPosition, e.IdRole, e.IdUser }).Distinct().Count())
                throw new BadRequestException("Attention by have the same data");

            if (!ExsisName)
            {
                SanctionMapping.IdAcademicYear = body.IdAcademicYear;
                SanctionMapping.Min = body.Min;
                SanctionMapping.Max = body.Max;
                SanctionMapping.SanctionName = body.NameSanction;
                _dbContext.Entity<MsSanctionMapping>().Update(SanctionMapping);

                var GetSunctionAttandentBy = _dbContext.Entity<MsSanctionMappingAttentionBy>().Where(e => e.IdSanctionMapping == body.Id).ToList();

                foreach (var itemSunctionAttandentBy in GetSunctionAttandentBy)
                {
                    var ExsisSunctionAttandentByRolePosition = body.Attention.FirstOrDefault(e => e.IdRole == itemSunctionAttandentBy.IdRole && e.IdPosition == itemSunctionAttandentBy.IdTeacherPosition);

                    if (ExsisSunctionAttandentByRolePosition == null)
                    {
                        itemSunctionAttandentBy.IsActive = false;
                        _dbContext.Entity<MsSanctionMappingAttentionBy>().Update(itemSunctionAttandentBy);
                    }
                    else
                    {
                        itemSunctionAttandentBy.IdUser = ExsisSunctionAttandentByRolePosition.IdUser == null ? null : ExsisSunctionAttandentByRolePosition.IdUser;
                        _dbContext.Entity<MsSanctionMappingAttentionBy>().Update(itemSunctionAttandentBy);
                    }
                }


                foreach ( var itemBodySunctionAttandentBy in body.Attention)
                {
                    var ExsisSunctionAttandentByRolePosition = GetSunctionAttandentBy.Any(e => e.IdRole == itemBodySunctionAttandentBy.IdRole && e.IdTeacherPosition == itemBodySunctionAttandentBy.IdPosition);

                    if (itemBodySunctionAttandentBy.IdPosition != null)
                    {
                        var GetTeacherPositionById = GetTeacherPosition.SingleOrDefault(e => e.Id == itemBodySunctionAttandentBy.IdPosition);
                        if (GetTeacherPositionById == null)
                            throw new BadRequestException("Merit demerit mapping Sanction with id position: " + itemBodySunctionAttandentBy.IdPosition + " is not exists.");
                    }

                    if (!ExsisSunctionAttandentByRolePosition) {
                        var NewSanctionMappingAttentionBy = new MsSanctionMappingAttentionBy
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSanctionMapping = body.Id,
                            IdTeacherPosition = itemBodySunctionAttandentBy.IdPosition,
                            IdRole = itemBodySunctionAttandentBy.IdRole,
                            IdUser = itemBodySunctionAttandentBy.IdUser
                        };
                        
                        _dbContext.Entity<MsSanctionMappingAttentionBy>().Add(NewSanctionMappingAttentionBy);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        public string GetAttentionBy(List<string> SanctionMappingAttentionBies)
        {
            var AttentionBy = string.Empty;
            foreach(var item in SanctionMappingAttentionBies.Distinct())
            {
                if (item != null)
                //AttentionBy += SanctionMappingAttentionBies.IndexOf(item)+1 == SanctionMappingAttentionBies.Distinct().Count() ? item : $", {item}";
                AttentionBy += AttentionBy == string.Empty ? item : $", {item}";
            }

            return AttentionBy;
        }
    }
}
