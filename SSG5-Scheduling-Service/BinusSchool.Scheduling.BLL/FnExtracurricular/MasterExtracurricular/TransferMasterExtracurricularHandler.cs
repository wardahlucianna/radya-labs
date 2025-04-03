using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class TransferMasterExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public TransferMasterExtracurricularHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<TransferMasterExtracurricularRequest, TransferMasterExtracurricularValidator>();

                var getAllParamIdExtracurricular = body.MasterExtracurricularData.Select(x => x.IdExtracurricular).ToList();

                var IdSchool = await _dbContext.Entity<MsAcademicYear>()
                    .Where(x => x.Id == body.IdAcademicYearFrom)
                    .Select(x => x.IdSchool)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(CancellationToken);

                #region Create Temp List
                var gradeList = await _dbContext.Entity<MsGrade>()
                    .Include(x => x.Level)
                    .Where(x => x.Level.IdAcademicYear == body.IdAcademicYearDest)
                    .ToListAsync(CancellationToken);

                var extracurricularList = await _dbContext.Entity<MsExtracurricular>()
                    .Where(x => x.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.Id))
                    .ToListAsync(CancellationToken);

                var extracurricularSpvCoachList = await _dbContext.Entity<MsExtracurricularSpvCoach>()
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                    .ToListAsync(CancellationToken);

                var extracurricularExtCoachList = await _dbContext.Entity<MsExtracurricularExtCoachMapping>()
                   .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                   .ToListAsync(CancellationToken);

                var extracurricularGradeMappingList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                    .Include(x => x.Grade).ThenInclude(x => x.Level)
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                    .ToListAsync(CancellationToken);

                var extracurricularParticipantList = await _dbContext.Entity<MsExtracurricularParticipant>()
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                    .ToListAsync(CancellationToken);

                var extracurricularSessionMappingList = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                    .Include(x => x.ExtracurricularSession)
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                    .ToListAsync(CancellationToken);

                var extracurricularScoreCompCategorySourceAYList = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                    .Where(x => x.IdAcademicYear == body.IdAcademicYearFrom)
                    .ToListAsync(CancellationToken);

                var extracurricularScoreComponentSourceAYList = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                   .Include(x => x.ExtracurricularScoreCompCategory)
                   .Where(x => extracurricularScoreCompCategorySourceAYList.Select(y => y.Id).Any(y => y == x.IdExtracurricularScoreCompCategory))
                   .ToListAsync(CancellationToken);

                var extracurricularScoreCompCategoryTargetAYList = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                    .Where(x => x.IdAcademicYear == body.IdAcademicYearDest)
                    .ToListAsync(CancellationToken);

                var extracurricularScoreComponentTargetAYList = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                   .Include(x => x.ExtracurricularScoreCompCategory)
                   .Where(x => extracurricularScoreCompCategoryTargetAYList.Select(y => y.Id).Any(y => y == x.IdExtracurricularScoreCompCategory))
                   .ToListAsync(CancellationToken);

                var extracurricularScoreCompMappingList = await _dbContext.Entity<MsExtracurricularScoreCompMapping>()
                    .Include(x => x.ExtracurricularScoreCompCategory)
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                                getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                    .ToListAsync(CancellationToken);

                var extracurricularScoreLegendMappingList = await _dbContext.Entity<MsExtracurricularScoreLegendMapping>()
                   .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == IdSchool &&
                               getAllParamIdExtracurricular.Any(y => y == x.IdExtracurricular))
                   .ToListAsync(CancellationToken);
                #endregion

                #region Validate MsExtracurricular
                var falseExtracurricularList = body.MasterExtracurricularData
                    .Where(ex => extracurricularList.All(ex2 => ex2.Id != ex.IdExtracurricular))
                    .Select(x => x.IdExtracurricular).ToList();

                if (falseExtracurricularList.Count() > 0)
                {
                    throw new BadRequestException($"Extracurricular with Id : {string.Join(",", falseExtracurricularList)} not exists");
                }
                #endregion

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Copy All MsExtracurricularScoreCompCategory and MsExtracurricularScoreComponent to AY Destination
                // MsExtracurricularScoreCompCategory
                var extracurricularScoreCompCategoryNeedClone = extracurricularScoreCompCategorySourceAYList
                        .Where(x => !extracurricularScoreCompCategoryTargetAYList.Select(y => y.Description).Any(y => y == x.Description))
                        .ToList();

                var newExtracurricularScoreCompCategory = extracurricularScoreCompCategoryNeedClone
                        .Select(x => new MsExtracurricularScoreCompCategory
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = body.IdAcademicYearDest,
                            Description = x.Description
                        })
                        .ToList();

                if (extracurricularScoreCompCategoryNeedClone != null && extracurricularScoreCompCategoryNeedClone.Any())
                {
                    _dbContext.Entity<MsExtracurricularScoreCompCategory>().AddRange(newExtracurricularScoreCompCategory);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    // renew extracurricularScoreCompCategoryTargetAYList
                    extracurricularScoreCompCategoryTargetAYList = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                        .Where(x => x.IdAcademicYear == body.IdAcademicYearDest)
                        .ToListAsync(CancellationToken);
                }

                // MsExtracurricularScoreComponent
                var extracurricularScoreComponentNeedClone = extracurricularScoreComponentSourceAYList
                        .Where(x => !extracurricularScoreComponentTargetAYList.Select(y => y.Description).Any(y => y == x.Description))
                        .Select(x => new MsExtracurricularScoreComponent
                        {
                            Id = x.Id,
                            IdExtracurricularScoreCompCategory = extracurricularScoreCompCategoryTargetAYList
                                                                    .Where(y => y.Description == x.ExtracurricularScoreCompCategory.Description)
                                                                    .Select(y => y.Id)
                                                                    .FirstOrDefault(),
                            Description = x.Description,
                            OrderNumber = x.OrderNumber
                        })
                        .ToList();

                if (extracurricularScoreComponentNeedClone != null && extracurricularScoreComponentNeedClone.Any())
                {
                    _dbContext.Entity<MsExtracurricularScoreComponent>().AddRange(extracurricularScoreComponentNeedClone);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    // renew MsExtracurricularScoreComponent
                    extracurricularScoreComponentTargetAYList = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                   .Include(x => x.ExtracurricularScoreCompCategory)
                   .Where(x => extracurricularScoreCompCategoryTargetAYList.Select(y => y.Id).Any(y => y == x.IdExtracurricularScoreCompCategory))
                   .ToListAsync(CancellationToken);
                }
                #endregion


                foreach (var Extracurricular in body.MasterExtracurricularData)
                {
                    #region Copy MsExtracurricular
                    var masterExtracurricular = extracurricularList
                        .Where(x => x.Id == Extracurricular.IdExtracurricular)
                        .FirstOrDefault();

                    var paramExcul = new MsExtracurricular
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricularGroup = masterExtracurricular.IdExtracurricularGroup,
                        Name = masterExtracurricular.Name,
                        Description = masterExtracurricular.Description,
                        ShowAttendanceRC = masterExtracurricular.ShowAttendanceRC,
                        ShowScoreRC = masterExtracurricular.ShowScoreRC,
                        IsRegularSchedule = masterExtracurricular.IsRegularSchedule,
                        ElectivesStartDate = masterExtracurricular.ElectivesStartDate,
                        ElectivesEndDate = masterExtracurricular.ElectivesEndDate,
                        AttendanceStartDate = masterExtracurricular.AttendanceStartDate,
                        AttendanceEndDate = masterExtracurricular.AttendanceEndDate,
                        Category = masterExtracurricular.Category,
                        MinParticipant = masterExtracurricular.MinParticipant,
                        MaxParticipant = masterExtracurricular.MaxParticipant,
                        ScoreStartDate = masterExtracurricular.ScoreStartDate,
                        ScoreEndDate = masterExtracurricular.ScoreEndDate,
                        Status = masterExtracurricular.Status,
                        Semester = body.SemesterDest,
                        NeedObjective = masterExtracurricular.NeedObjective,
                        Price = masterExtracurricular.Price,
                        IdExtracurricularType = masterExtracurricular.IdExtracurricularType,
                        ShowParentStudent = masterExtracurricular.ShowParentStudent
                    };

                    _dbContext.Entity<MsExtracurricular>().Add(paramExcul);
                    #endregion

                    #region Copy MsExtracurricularSpvCoach
                    var extracurricularSpvCoach = extracurricularSpvCoachList
                        .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                        .ToList();

                    foreach (var spvCoach in extracurricularSpvCoach)
                    {
                        var paramspvCoach = new MsExtracurricularSpvCoach
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdExtracurricular = paramExcul.Id,
                            IdBinusian = spvCoach.IdBinusian,
                            IsSpv = spvCoach.IsSpv,
                            IdExtracurricularCoachStatus = spvCoach.IdExtracurricularCoachStatus
                        };

                        _dbContext.Entity<MsExtracurricularSpvCoach>().Add(paramspvCoach);
                    }
                    #endregion

                    #region Copy MsExtracurricularExtCoachMapping
                    var extracurricularExtCoach = extracurricularExtCoachList
                        .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                        .ToList();

                    foreach (var extCoach in extracurricularExtCoach)
                    {
                        var paramextCoach = new MsExtracurricularExtCoachMapping
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdExtracurricular = paramExcul.Id,
                            IdExtracurricularExternalCoach = extCoach.IdExtracurricularExternalCoach
                        };

                        _dbContext.Entity<MsExtracurricularExtCoachMapping>().Add(paramextCoach);
                    }
                    #endregion

                    #region Copy TrExtracurricularGradeMapping
                    if (body.IdAcademicYearFrom == body.IdAcademicYearDest)
                    {
                        var extracurricularGradeMapping = extracurricularGradeMappingList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .ToList();

                        foreach (var gradeMapping in extracurricularGradeMapping)
                        {
                            var paramGradeMapping = new TrExtracurricularGradeMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = paramExcul.Id,
                                IdGrade = gradeMapping.IdGrade
                            };

                            _dbContext.Entity<TrExtracurricularGradeMapping>().Add(paramGradeMapping);
                        }
                    }
                    else
                    {
                        var oldGradeMappingList = extracurricularGradeMappingList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .Select(x => x.Grade.Code).ToList();

                        var newGradeMappingList = gradeList
                            .Where(x => oldGradeMappingList.Contains(x.Code))
                            .Select(x => new {
                                GradeId = x.Id
                            }).ToList();

                        if (newGradeMappingList.Count == 0)
                            throw new BadRequestException($"Can not found Grade that match");

                        foreach (var gradeMapping in newGradeMappingList)
                        {
                            var paramGradeMapping = new TrExtracurricularGradeMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = paramExcul.Id,
                                IdGrade = gradeMapping.GradeId
                            };

                            _dbContext.Entity<TrExtracurricularGradeMapping>().Add(paramGradeMapping);
                        }
                    }
                    #endregion

                    #region Copy MsExtracurricularParticipant
                    if (Extracurricular.IsTransferParticipant)
                    {
                        var extracurricularParticipant = extracurricularParticipantList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .ToList();

                        foreach (var Participant in extracurricularParticipant)
                        {
                            var paramParticipantMapping = new MsExtracurricularParticipant
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = paramExcul.Id,
                                IdStudent = Participant.IdStudent,
                                IdGrade = Participant.IdGrade,
                                JoinDate = Participant.JoinDate,
                                Status = Participant.Status,
                                //Priority = Participant.Priority,
                                IsPrimary = Participant.IsPrimary,
                            };

                            _dbContext.Entity<MsExtracurricularParticipant>().Add(paramParticipantMapping);
                        }
                    }
                    #endregion

                    #region Copy TrExtracurricularSessionMapping
                    var extracurricularSessionMapping = extracurricularSessionMappingList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .ToList();

                    var sourceExtracurricularSession = extracurricularSessionMapping
                                                        .Select(x => x.ExtracurricularSession)
                                                        .Distinct()
                                                        .ToList();

                    var newExtracurricularSession = sourceExtracurricularSession
                                                    .Select(x => new MsExtracurricularSession
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        IdDay = x.IdDay,
                                                        StartTime = x.StartTime,
                                                        EndTime = x.EndTime,
                                                        IdVenue = x.IdVenue
                                                    })
                                                    .ToList();

                    _dbContext.Entity<MsExtracurricularSession>().AddRange(newExtracurricularSession);

                    foreach (var sessionMapping in newExtracurricularSession)
                    {
                        var paramSessionMapping = new TrExtracurricularSessionMapping
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdExtracurricular = paramExcul.Id,
                            IdExtracurricularSession = sessionMapping.Id,
                        };

                        _dbContext.Entity<TrExtracurricularSessionMapping>().Add(paramSessionMapping);
                    }
                    #endregion

                    #region Copy MsExtracurricularScoreLegendMapping
                    var extracurricularScoreLegendMapping = extracurricularScoreLegendMappingList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .ToList();

                    var paramScoreLegendMapping = extracurricularScoreLegendMapping
                            .Select(x => new MsExtracurricularScoreLegendMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = paramExcul.Id,
                                IdExtracurricularScoreLegendCategory = x.IdExtracurricularScoreLegendCategory,
                            })
                            .ToList();

                    _dbContext.Entity<MsExtracurricularScoreLegendMapping>().AddRange(paramScoreLegendMapping);
                    #endregion

                    #region Copy MsExtracurricularScoreCompMapping
                    var extracurricularScoreCompMappingPrevAY = extracurricularScoreCompMappingList
                            .Where(x => x.IdExtracurricular == Extracurricular.IdExtracurricular)
                            .ToList();

                    var extracurricularScoreCompMapping = extracurricularScoreCompCategoryTargetAYList
                            .Where(x => extracurricularScoreCompMappingPrevAY.Select(y => y.ExtracurricularScoreCompCategory.Description).Any(y => y == x.Description))
                            .ToList();

                    var paramScoreCompMapping = extracurricularScoreCompMapping
                            .Select(x => new MsExtracurricularScoreCompMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = paramExcul.Id,
                                IdExtracurricularScoreCompCategory = x.Id,
                            })
                            .ToList();

                    _dbContext.Entity<MsExtracurricularScoreCompMapping>().AddRange(paramScoreCompMapping);
                    #endregion

                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
