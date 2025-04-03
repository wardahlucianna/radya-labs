using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Attendance.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class AttendanceHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsAttendance>()
                .Include(x => x.AttendanceMappingAttendances)
                .Include(x => x.ListMappingAttendanceAbsents)
                .Include(x => x.MappingAttendanceQuotas)
                .Include(x => x.AttendanceAdministrations)
                .Include(x => x.AttendanceSummaryMappingAtds)
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
                if (data.AttendanceMappingAttendances.Any() || data.ListMappingAttendanceAbsents.Any() || data.MappingAttendanceQuotas.Any()
                     || data.AttendanceAdministrations.Any() || data.AttendanceSummaryMappingAtds.Any())
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsAttendance>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var result = await _dbContext.Entity<MsAttendance>()
                                         .Include(x => x.AcademicYear)
                                         .Where(x => x.Id == id)
                                         .Select(x => new MasterDataAttendanceDetailResult
                                         {
                                             Id = x.Id,
                                             AcademicYear = new CodeWithIdVm
                                             {
                                                 Id = x.AcademicYear.Id,
                                                 Code = x.AcademicYear.Code,
                                                 Description = x.AcademicYear.Description
                                             },
                                             AttendanceName = x.Description,
                                             ShortName = x.Code,
                                             AttendanceCategory = x.AttendanceCategory.ToString(),
                                             AbsenceCategory = $"{x.AbsenceCategory.ToString()} Absence",
                                             ExcusedAbsenceCategory = x.ExcusedAbsenceCategory.ToString(),
                                             Status = x.Status.ToString(),
                                             IsNeedFileAttachment = x.IsNeedFileAttachment
                                         }).SingleOrDefaultAsync();

            if (result is null)
                throw new NotFoundException("Attendance is not found");

            return Request.CreateApiResult2(result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMasterDataAttendanceRequest>(nameof(GetMasterDataAttendanceRequest.IdAcademicYear));
            string[] columns = { "AcademicYear", "AttendanceName", "ShortName", "Status", "AttendanceCategory", "AbsenceCategory", "ExcusedAbsenceCategory" };

            var predicate = PredicateBuilder.Create<MsAttendance>(x => param.IdAcademicYear.Contains(x.IdAcademicYear));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern()));

            if (param.AttendanceCategory.HasValue)
                predicate = predicate.And(x => x.AttendanceCategory == param.AttendanceCategory);

            if (param.AbsenceCategory.HasValue)
                predicate = predicate.And(x => x.AbsenceCategory == param.AbsenceCategory);

            var query = _dbContext.Entity<MsAttendance>()
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
                case "AttendanceName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "ShortName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Code)
                        : query.OrderBy(x => x.Code);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "AttendanceCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AttendanceCategory)
                        : query.OrderBy(x => x.AttendanceCategory);
                    break;
                case "AbsenceCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AbsenceCategory)
                        : query.OrderBy(x => x.AbsenceCategory);
                    break;
                case "ExcusedAbsenceCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ExcusedAbsenceCategory)
                        : query.OrderBy(x => x.ExcusedAbsenceCategory);
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
                    .Select(x => new GetMasterDataAttendanceResult
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.AcademicYear.Id,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        AttendanceName = x.Description,
                        ShortName = x.Code,
                        Status = x.Status.ToString(),
                        AttendanceCategory = x.Description != "Present" && x.Description != "Absent" ? x.AttendanceCategory.ToString() : "-",
                        AbsenceCategory = x.AbsenceCategory.HasValue && x.Description != "Present" && x.Description != "Absent" ? $"{x.AbsenceCategory.ToString()} Absence" : "-",
                        CanDeleted = x.Description != "Present" && x.Description != "Absent" && x.Description != "Late",
                        CanEdited = x.Description != "Present" && x.Description != "Absent",
                        Description = x.Description,
                        ExcusedAbsenceCategory = x.ExcusedAbsenceCategory.HasValue ? x.ExcusedAbsenceCategory.ToString() : "-"
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMasterDataAttendanceRequest, AddMasterDataAttendanceValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            List<AttendanceNameBySystem> listAttendanceBySystem = new List<AttendanceNameBySystem>()
            {
                new AttendanceNameBySystem
                {
                    Code = "PR",
                    Name = "Present",
                    AttendanceCategory = AttendanceCategory.Present,
                    AbsenceCategory = null,
                    ExcusedAbsenceCategory = null,
                    Status = AttendanceStatus.Attend,
                    IsNeedFileAttachment = false,
                },
                new AttendanceNameBySystem
                {
                    Code = "AB",
                    Name = "Absent",
                    AttendanceCategory = AttendanceCategory.Absent,
                    AbsenceCategory = AbsenceCategory.Unexcused,
                    ExcusedAbsenceCategory = null,
                    Status = AttendanceStatus.Unattend,
                    IsNeedFileAttachment = false,
                },
                new AttendanceNameBySystem
                {
                    Code = "LT",
                    Name = "Late",
                    AttendanceCategory = AttendanceCategory.Present,
                    AbsenceCategory = null,
                    ExcusedAbsenceCategory = null,
                    Status = AttendanceStatus.Attend,
                    IsNeedFileAttachment = false,
                },
            };

            var listAttendanceNameBySystem = listAttendanceBySystem.Select(e => e.Name).ToList();

            var listAttendanceBySistem = await _dbContext.Entity<MsAttendance>()
                                            .Where(x => x.IdAcademicYear == body.IdAcademicYear
                                                        && listAttendanceNameBySystem.Contains(x.Description))
                                            .ToListAsync(CancellationToken);

            foreach(var itemAttendanceBySystem in listAttendanceBySystem)
            {
                var exsisAttendanceBySystem = listAttendanceBySistem.Where(e => e.Description.ToLower() == itemAttendanceBySystem.Name.ToLower()).Any();

                if (!exsisAttendanceBySystem)
                {
                    var newAttendanceBySystem = new MsAttendance
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcademicYear,
                        Code = itemAttendanceBySystem.Code,
                        Description = itemAttendanceBySystem.Name,
                        AttendanceCategory = itemAttendanceBySystem.AttendanceCategory,
                        AbsenceCategory = itemAttendanceBySystem.AbsenceCategory,
                        ExcusedAbsenceCategory = itemAttendanceBySystem.ExcusedAbsenceCategory,
                        Status = itemAttendanceBySystem.Status,
                        IsNeedFileAttachment = itemAttendanceBySystem.IsNeedFileAttachment
                    };
                    _dbContext.Entity<MsAttendance>().Add(newAttendanceBySystem);
                }
            }

            if (await _dbContext.Entity<MsAttendance>()
                                .Where(x => x.IdAcademicYear == body.IdAcademicYear
                                            && x.Description == body.AttendanceName)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Attendance name {body.AttendanceName} already exists");

            if (await _dbContext.Entity<MsAttendance>()
                                .Where(x => x.IdAcademicYear == body.IdAcademicYear
                                            && x.Code == body.ShortName)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Short name {body.ShortName} already exists");

            var param = new MsAttendance
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                Code = body.ShortName,
                Description = body.AttendanceName,
                AttendanceCategory = body.AttendanceCategory,
                AbsenceCategory = body.AbsenceCategory,
                ExcusedAbsenceCategory = body?.ExcusedAbsenceCategory,
                Status = body.Status,
                IsNeedFileAttachment = body.IsNeedFileAttachment
            };
            _dbContext.Entity<MsAttendance>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMasterDataAttendanceRequest, UpdateMasterDataAttendanceValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsAttendance>()
                                    .Include(x => x.AttendanceMappingAttendances)
                                    .Include(x => x.ListMappingAttendanceAbsents)
                                    .Include(x => x.MappingAttendanceQuotas)
                                    .Include(x => x.AttendanceAdministrations)
                                    .Include(x => x.AttendanceSummaryMappingAtds)
                                    .FirstOrDefaultAsync(x => x.Id == body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Attendance"], "Id", body.Id));

            if (await _dbContext.Entity<MsAttendance>()
                                .Where(x => x.Id != body.Id
                                            && x.IdAcademicYear == body.IdAcademicYear
                                            && x.Description == body.AttendanceName)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Attendance name {body.AttendanceName} already exists");

            if (await _dbContext.Entity<MsAttendance>()
                                .Where(x => x.Id != body.Id
                                            && x.IdAcademicYear == body.IdAcademicYear
                                            && x.Code == body.ShortName)
                                .AnyAsync(CancellationToken))
                throw new BadRequestException($"Short name {body.ShortName} already exists");

            if (data.AttendanceMappingAttendances.Any() || data.ListMappingAttendanceAbsents.Any() || data.MappingAttendanceQuotas.Any()
                || data.AttendanceAdministrations.Any() || data.AttendanceSummaryMappingAtds.Any())
                throw new BadRequestException($"Attendance name {body.AttendanceName} already use");

            data.IdAcademicYear = body.IdAcademicYear;
            data.Description = body.AttendanceName;
            data.Code = body.ShortName;
            data.AttendanceCategory = body.AttendanceCategory;
            data.AbsenceCategory = body.AbsenceCategory;
            data.ExcusedAbsenceCategory = body.ExcusedAbsenceCategory;
            data.Status = body.Status;
            data.IsNeedFileAttachment = body.IsNeedFileAttachment;
            _dbContext.Entity<MsAttendance>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
