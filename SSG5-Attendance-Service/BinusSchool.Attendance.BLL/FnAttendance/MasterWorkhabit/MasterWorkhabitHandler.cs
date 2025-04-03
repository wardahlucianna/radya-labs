using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.MasterWorkhabit.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.MasterWorkhabit;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.MasterWorkhabit
{
    public class MasterWorkhabitHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public MasterWorkhabitHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsWorkhabit>()
                .Include(x => x.MappingAttendanceWorkhabits)
                .Include(x => x.AttendanceSummaryWorkhabits)
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
                if (data.MappingAttendanceWorkhabits.Count != 0 || data.AttendanceSummaryWorkhabits.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsWorkhabit>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsWorkhabit>()
                .Include(x => x.AcademicYear)
                .Where(x => x.Id == id)
                .Select(x => new GetWorkhabitResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    }
                }).FirstOrDefaultAsync(CancellationToken);

            if (data is null)
                throw new NotFoundException("Workhabit is not found");

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetWorkhabitRequest>(nameof(GetWorkhabitRequest.IdSchool));
            string[] columns = { "AcademicYear", "Description", "Code" };

            var predicate = PredicateBuilder.Create<MsWorkhabit>(x => param.IdSchool.Any(y => y == x.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            var query = _dbContext.Entity<MsWorkhabit>()
                        .Include(x => x.MappingAttendanceWorkhabits)
                        .Include(x => x.AttendanceSummaryWorkhabits)
                        .Include(x => x.AcademicYear)
                        .Where(predicate);

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "Code":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Code)
                        : query.OrderBy(x => x.Code);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetWorkhabitResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.AcademicYear.Id,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        IsEdit = x.MappingAttendanceWorkhabits.Count != 0 || x.AttendanceSummaryWorkhabits.Count != 0 ? false : true,
                        IsDelete = x.MappingAttendanceWorkhabits.Count != 0 || x.AttendanceSummaryWorkhabits.Count != 0 ? false : true
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddWorkhabitRequest, AddWorkhabitValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var academicYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
            if (academicYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Academic Year"], "Id", body.IdAcademicYear));

            var isExist = await _dbContext.Entity<MsWorkhabit>()
                .Where(x => x.IdAcademicYear == body.IdAcademicYear && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists in this academic year");

            var param = new MsWorkhabit
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code,
                Description = body.Description,
                IdAcademicYear = body.IdAcademicYear,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsWorkhabit>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateWorkhabitRequest, UpdateWorkhabitValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsWorkhabit>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Workhabit"], "Id", body.Id));

            var academicYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
            if (academicYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Academic Year"], "Id", body.IdAcademicYear));

            var isExist = await _dbContext.Entity<MsWorkhabit>()
                .Where(x => x.Id != body.Id && x.IdAcademicYear == body.IdAcademicYear && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists in this academic year");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdAcademicYear = body.IdAcademicYear;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsWorkhabit>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
