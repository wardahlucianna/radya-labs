using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Attendance.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Attendance.FnAttendance.MapAttendance
{
    public class AddOrUpdateMappingAttendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IAttendanceDbContext _dbContext;

        public AddOrUpdateMappingAttendanceHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddOrUpdateMappingAttendanceRequest, AddOrUpdateMappingAttendanceValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (string.IsNullOrEmpty(body.Id))
            {
                var newMappingAttendance = new MsMappingAttendance
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                    AbsentTerms = body.AbsentTerms,
                    IsNeedValidation = body.IsNeedValidation,
                    IsUseWorkhabit = body.IsUseWorkHabit,
                    IsUseDueToLateness = body.IsDueToLateness,
                    UsingCheckboxAttendance = body.UsingCheckboxAttendance,
                    ShowingModalReminderAttendanceEntry = body.ShowingModalReminderAttendanceEntry,
                    RenderAttendance = body.RenderAttendance
                };

                _dbContext.Entity<MsMappingAttendance>().Add(newMappingAttendance);

                var ListAttendance = new List<MsAttendanceMappingAttendance>();
                foreach (var idAtd in body.AttendanceName)
                {
                    var newAttendanceNameMapping = new MsAttendanceMappingAttendance
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdMappingAttendance = newMappingAttendance.Id,
                        IdAttendance = idAtd
                    };

                    ListAttendance.Add(newAttendanceNameMapping);
                }

                _dbContext.Entity<MsAttendanceMappingAttendance>().AddRange(ListAttendance);

                var ListAbsentMapping = new List<MsAbsentMappingAttendance>();
                foreach (var itemAbsentMapping in body.AbsentMapping)
                {
                    var newAbsentMapping = new MsAbsentMappingAttendance
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdMappingAttendance = newMappingAttendance.Id,
                        IdTeacherPosition = itemAbsentMapping.IdTeacherPosition
                    };

                    ListAbsentMapping.Add(newAbsentMapping);

                    var listMappingAttendanceAbsent = new List<MsListMappingAttendanceAbsent>();
                    foreach (var itemMappingAttendanceAbsent in itemAbsentMapping.ListMappingAttendance)
                    {
                        var newMappingAttendanceAbsent = new MsListMappingAttendanceAbsent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAbsentMappingAttendance = newAbsentMapping.Id,
                            IdAttendance = itemMappingAttendanceAbsent.IdAttendance,
                            IsNeedValidation = itemMappingAttendanceAbsent.IsNeedValidation
                        };

                        listMappingAttendanceAbsent.Add(newMappingAttendanceAbsent);
                    }

                    _dbContext.Entity<MsListMappingAttendanceAbsent>().AddRange(listMappingAttendanceAbsent);
                }

                _dbContext.Entity<MsAbsentMappingAttendance>().AddRange(ListAbsentMapping);

                if (body.IsUseWorkHabit)
                {
                    var ListWorkhabit = new List<MsMappingAttendanceWorkhabit>();
                    foreach (var idWorkhabit in body.MappingWorkhabit)
                    {
                        var newMappingWorkhabit = new MsMappingAttendanceWorkhabit
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMappingAttendance = newMappingAttendance.Id,
                            IdWorkhabit = idWorkhabit
                        };

                        ListWorkhabit.Add(newMappingWorkhabit);
                    }

                    _dbContext.Entity<MsMappingAttendanceWorkhabit>().AddRange(ListWorkhabit);
                }
            }
            else
            {
                var data = await _dbContext.Entity<MsMappingAttendance>().FindAsync(body.Id);

                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["MappingAttendance"], "Id", body.Id));

                data.IdLevel = body.IdLevel;
                data.AbsentTerms = body.AbsentTerms;
                data.IsNeedValidation = body.IsNeedValidation;
                data.IsUseWorkhabit = body.IsUseWorkHabit;
                data.IsUseDueToLateness = body.IsDueToLateness;
                data.UsingCheckboxAttendance = body.UsingCheckboxAttendance;
                data.ShowingModalReminderAttendanceEntry = body.ShowingModalReminderAttendanceEntry;
                data.RenderAttendance = body.RenderAttendance;

                _dbContext.Entity<MsMappingAttendance>().Update(data);

                #region Mapping Attendance by Level
                var updateAttendanceName = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                                 .IgnoreQueryFilters()
                                                 .Where(x => x.IdMappingAttendance == body.Id)
                                                 .ToListAsync(CancellationToken);

                // update for existing data
                foreach (var itemAttendanceName in updateAttendanceName)
                {
                    var dataIsExist = body.AttendanceName.Contains(itemAttendanceName.IdAttendance);
                    itemAttendanceName.IsActive = dataIsExist;
                }

                _dbContext.Entity<MsAttendanceMappingAttendance>().UpdateRange(updateAttendanceName);

                // add attendance new mapping
                var ListIdAttendance = updateAttendanceName.Select(x => x.IdAttendance).ToList();
                foreach (var idAtd in body.AttendanceName)
                {
                    var dataIsExist = ListIdAttendance.Contains(idAtd);

                    if (!dataIsExist)
                    {
                        var newMappingAttendance = new MsAttendanceMappingAttendance
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMappingAttendance = body.Id,
                            IdAttendance = idAtd
                        };

                        _dbContext.Entity<MsAttendanceMappingAttendance>().Add(newMappingAttendance);
                    }
                }
                #endregion

                #region Mapping Absent Attendance By Teacher Position
                var ListAbsentMapping = await _dbContext.Entity<MsAbsentMappingAttendance>()
                                        .IgnoreQueryFilters()
                                        .Include(x => x.ListMappingAttendanceAbsents)
                                        .Where(x => x.IdMappingAttendance == body.Id)
                                        .ToListAsync(CancellationToken);

                // update for existing data
                foreach (var itemAbsentMapping in ListAbsentMapping)
                {
                    var getListMappingAttendanceBody = body.AbsentMapping.Where(x => x.IdTeacherPosition == itemAbsentMapping.IdTeacherPosition).FirstOrDefault();

                    if(getListMappingAttendanceBody != null)
                    {
                        var idAttendanceMappingAbsent = getListMappingAttendanceBody.ListMappingAttendance.Select(x => x.IdAttendance).ToList();

                        foreach (var itemMappingAttendanceAbsent in itemAbsentMapping.ListMappingAttendanceAbsents)
                        {
                            var getUpdateFromBody = getListMappingAttendanceBody.ListMappingAttendance.Where(x => x.IdAttendance == itemMappingAttendanceAbsent.IdAttendance).FirstOrDefault();

                            if (getUpdateFromBody != null)
                            {
                                itemMappingAttendanceAbsent.IsNeedValidation = getUpdateFromBody.IsNeedValidation;
                                itemMappingAttendanceAbsent.IsActive = true;
                            }
                            else
                            {
                                itemMappingAttendanceAbsent.IsActive = false;
                            }
                        }

                        _dbContext.Entity<MsListMappingAttendanceAbsent>().UpdateRange(itemAbsentMapping.ListMappingAttendanceAbsents);

                        // add absent attendance new mapping
                        var ListItemAbsentAttendance = itemAbsentMapping.ListMappingAttendanceAbsents.Select(x => x.IdAttendance).ToList();
                        foreach (var itemAbsentAttendance in getListMappingAttendanceBody.ListMappingAttendance)
                        {
                            var dataIsExist = ListItemAbsentAttendance.Contains(itemAbsentAttendance.IdAttendance);

                            if (!dataIsExist)
                            {
                                var newMappingAttendanceAbsent = new MsListMappingAttendanceAbsent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdAbsentMappingAttendance = itemAbsentMapping.Id,
                                    IdAttendance = itemAbsentAttendance.IdAttendance,
                                    IsNeedValidation = itemAbsentAttendance.IsNeedValidation
                                };

                                _dbContext.Entity<MsListMappingAttendanceAbsent>().Add(newMappingAttendanceAbsent);
                            }
                        }
                    }
                }
                #endregion

                #region Mapping Attendance Workhabit
                if (body.IsUseWorkHabit)
                {
                    var updateMappingWorkhabit = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                                            .IgnoreQueryFilters()
                                            .Where(x => x.IdMappingAttendance == body.Id)
                                            .ToListAsync(CancellationToken);

                    // update for existing data
                    foreach (var itemWorkhabit in updateMappingWorkhabit)
                    {
                        var dataIsExist = body.MappingWorkhabit.Contains(itemWorkhabit.IdWorkhabit);
                        itemWorkhabit.IsActive = dataIsExist;
                    }

                    _dbContext.Entity<MsMappingAttendanceWorkhabit>().UpdateRange(updateMappingWorkhabit);

                    // add attendance new mapping
                    var ListIdWorkhabit = updateMappingWorkhabit.Select(x => x.IdWorkhabit).ToList();
                    foreach (var idWH in body.MappingWorkhabit)
                    {
                        var dataIsExist = ListIdWorkhabit.Contains(idWH);

                        if (!dataIsExist)
                        {
                            var newMappingWorkhabit = new MsMappingAttendanceWorkhabit
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdMappingAttendance = body.Id,
                                IdWorkhabit = idWH
                            };

                            _dbContext.Entity<MsMappingAttendanceWorkhabit>().Add(newMappingWorkhabit);
                        }
                    }
                }
                #endregion
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
