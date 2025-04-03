using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.EventType.Validator;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnSchedule.EventType
{
    public class EventTypeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public EventTypeHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsEventType>()
                .Include(x => x.TrEvents)
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
                if (data.TrEvents.Any(x => x.IsActive))
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsEventType>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsEventType>()
                .Include(x => x.AcademicYear)
                .Include(x => x.TrEvents)
                .Select(x => new EventTypeResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Color = x.Color,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.IdAcademicYear,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    IsUsed = x.TrEvents.Any(y => y.IsActive)
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected async override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetEventTypeRequest>(nameof(GetEventTypeRequest.IdSchool));

            var columns = new[] { "academicyear", "description", "color" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "academicYear.code" }
            };

            var predicate = PredicateBuilder.Create<MsEventType>(x => param.IdSchool.Contains(x.AcademicYear.IdSchool));
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Color, param.SearchPattern())
                    || EF.Functions.Like(x.AcademicYear.Code, param.SearchPattern())
                    || EF.Functions.Like(x.AcademicYear.Description, param.SearchPattern()));

            var query = _dbContext.Entity<MsEventType>()
                .Include(x => x.AcademicYear)
                .Include(x => x.TrEvents)
                .SearchByIds(param)
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
                    .Select(x => new EventTypeResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Color = x.Color,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.IdAcademicYear,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        IsUsed = x.TrEvents.Any(y => y.IsActive)
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddEventTypeRequest, AddEventTypeValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isCodeExist = await _dbContext.Entity<MsEventType>()
                .Where(x => x.IdAcademicYear == body.IdAcademicYear && x.Description.ToLower() == body.Description.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Description} already exists in this academic year");

            var isColorExist = await _dbContext.Entity<MsEventType>()
                .Where(x => x.IdAcademicYear == body.IdAcademicYear && x.Color.ToLower() == body.Color.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isColorExist != null)
                throw new BadRequestException($"Color {body.Color} already exists in this academic year");

            var isClassDiaryColorExist = typeof(ColorsConstant)
                                           .GetFields(BindingFlags.Static | BindingFlags.Public)
                                           .Any(e => e.GetRawConstantValue().ToString() == body.Color); // that will return all fields of any type

            if (isClassDiaryColorExist)
                throw new BadRequestException($"color {body.Color} already exists in class diary");

            var param = new MsEventType
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code ?? body.Description,
                Description = body.Description,
                IdAcademicYear = body.IdAcademicYear,
                Color = body.Color
            };

            _dbContext.Entity<MsEventType>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateEventTypeRequest, UpdateEventTypeValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsEventType>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.Id));

            var isCodeExist = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id != body.Id && x.IdAcademicYear == body.IdAcademicYear && x.Description.ToLower() == body.Description.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Description} already exists in this academic year");

            var isColorExist = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id != body.Id && x.IdAcademicYear == body.IdAcademicYear && x.Color.ToLower() == body.Color.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isColorExist != null)
                throw new BadRequestException($"Color {body.Color} already exists in this academic year");


            var isClassDiaryColorExist = typeof(ColorsConstant)
                                            .GetFields(BindingFlags.Static | BindingFlags.Public)
                                            .Any(e => e.GetRawConstantValue().ToString()==body.Color); // that will return all fields of any type

            if (isClassDiaryColorExist)
                throw new BadRequestException($"color {body.Color} already exists in class diary");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdAcademicYear = body.IdAcademicYear;
            data.Color = body.Color;

            _dbContext.Entity<MsEventType>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        
    }
}
