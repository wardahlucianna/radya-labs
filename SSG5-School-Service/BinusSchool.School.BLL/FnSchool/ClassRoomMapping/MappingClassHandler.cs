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
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.ClassRoomMapping.Validator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Api.Extensions;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class MappingClassHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public MappingClassHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsGradePathway>()
                .Include(p => p.GradePathwayDetails)
                .Include(p => p.GradePathwayClassrooms).ThenInclude(p => p.ClassroomDivisions)
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
                if (item.GradePathwayClassrooms.Any(x => x.ClassroomDivisions.Count != 0) || 
                    item.GradePathwayClassrooms.Count != 0 || item.GradePathwayDetails.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(item.Id, string.Format(Localizer["ExAlreadyUse"], item.Grade.Description ?? item.Grade.Code ?? item.Id));
                }
                else
                {
                    foreach (var item2 in item.GradePathwayClassrooms)
                    {
                        foreach (var item3 in item2.ClassroomDivisions)
                        {
                            item3.IsActive = false;
                            _dbContext.Entity<MsClassroomDivision>().Update(item3);
                        }

                        item2.IsActive = false;
                        _dbContext.Entity<MsGradePathwayClassroom>().Update(item2);
                    }

                    foreach (var item2 in item.GradePathwayDetails)
                    {
                        item2.IsActive = false;
                        _dbContext.Entity<MsGradePathwayDetail>().Update(item2);
                    }

                    item.IsActive = false;
                    _dbContext.Entity<MsGradePathway>().Update(item);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsGradePathway>()
                .Include(p => p.Grade.Level.AcademicYear.School)
                .Include(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway)
                .Include(p => p.GradePathwayDetails).ThenInclude(p => p.SubjectPathways)
                .Include(p => p.GradePathwayClassrooms).ThenInclude(p => p.Classroom)
                .Include(p => p.GradePathwayClassrooms).ThenInclude(p => p.SubjectCombinations)
                .Include(p => p.GradePathwayClassrooms).ThenInclude(p => p.ClassroomDivisions).ThenInclude(p => p.Division)
                .Where(p => p.Id == id)
                .Select(x => new GetMappingClassDetailResult
                {
                    Id = x.Id,
                    Acadyear = new CodeWithIdVm()
                    {
                        Id = x.Grade.Level.AcademicYear.Id,
                        Code = x.Grade.Level.AcademicYear.Code,
                        Description = x.Grade.Level.AcademicYear.Description,                                      
                    },
                    Level = new CodeWithIdVm()
                    {
                        Id = x.Grade.Level.Id,
                        Code = x.Grade.Level.Code,
                        Description = x.Grade.Level.Description,
                    },
                    Grade = new CodeWithIdVm()
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description,
                    },
                    Pathways = x.GradePathwayDetails
                        .Select(p => new PathwayDetail
                        {
                            IdPathwayDetail = p.Id,
                            IdGradePathway = p.IdGradePathway,
                            Id = p.IdPathway,
                            Code = p.Pathway.Code,
                            Description = p.Pathway.Description,
                            IsAlreadyUse = p.SubjectPathways.Any(p => p.IsActive),
                        }).ToList(),
                    Classrooms = x.GradePathwayClassrooms
                        .Select(p => new PathwayClassroom
                        {
                            IdPathwayClassroom = p.Id,
                            IdGradePathway = p.IdGradePathway,
                            Id = p.Classroom.Id,
                            Code = p.Classroom.Code,
                            Description = p.Classroom.Description,
                            IsAlreadyUse = p.SubjectCombinations.Any(p => p.IsActive),
                            Divisions = p.ClassroomDivisions.Select(z => new ClassroomDivision
                            {
                                IdClassroomDivision = z.Id,
                                IdPathwayClassroom = z.IdGradePathwayClassroom,
                                Id = z.IdDivision,
                                Code = z.Division.Code,
                                Description = z.Division.Description
                            }).ToList(),
                        }).ToList(),
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (data.Pathways.Count != 0)
            {
                var checkUsageService = Request.HttpContext.RequestServices.GetService<ICheckUsage>();
                var checkUsageResult = await checkUsageService.CheckUsagePathwayDetails(new IdCollection(data.Pathways.Select(x => x.IdPathwayDetail).Distinct()));
                
                if (checkUsageResult.IsSuccess)
                    foreach (var pathwayDetail in data.Pathways)
                        pathwayDetail.IsAlreadyUse = pathwayDetail.IsAlreadyUse || checkUsageResult.Payload[pathwayDetail.IdPathwayDetail];
            }

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMappingClassRequest>(nameof(GetMappingClassRequest.IdSchool));
            var columns = new[] { "grade" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "grade.code" }
            };

            var predicate = PredicateBuilder.Create<MsGradePathway>(x => param.IdSchool.Any(y => y == x.Grade.Level.AcademicYear.IdSchool));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Grade.Level.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPathway))
                predicate = predicate.And(x => x.GradePathwayDetails.Any(x => x.IdPathway.Contains(param.IdPathway)));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Grade.Code, param.SearchPattern())
                || x.GradePathwayDetails.Any(y => EF.Functions.Like(y.Pathway.Code, param.SearchPattern())));

            var query = _dbContext.Entity<MsGradePathway>()
                .Include(p => p.GradePathwayClassrooms).ThenInclude(p => p.Classroom)
                .Include(p => p.GradePathwayDetails).ThenInclude(p => p.Pathway)
                .Include(p => p.Grade.Level.AcademicYear)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            if (!string.IsNullOrWhiteSpace(param.OrderBy))
            {
                if (param.OrderBy.ToLower() == "grade")
                {
                    query = query.OrderBy(p => p.Grade.Code + (param.OrderType == OrderType.Asc ? " ASC" : " DESC"));
                }

                if (param.OrderBy.ToLower() == "pathway")
                {
                    if(param.OrderType == OrderType.Asc)
                    {
                        query = query.OrderBy(p => p.GradePathwayDetails.FirstOrDefault().Pathway.Description);
                    } else
                    {
                        query = query.OrderByDescending(p => p.GradePathwayDetails.FirstOrDefault().Pathway.Description);
                    }
                }
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, string.Join(", ", x.GradePathwayDetails.Select(y => y.Pathway.Description ?? y.Pathway.Code))))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetMappingClassResult
                    {
                        Id = x.Id,
                        Grade = x.Grade.Code,
                        Pathway = string.Join(", ", x.GradePathwayDetails.Where(p => p.IsActive).Select(p => p.Pathway.Description ?? p.Pathway.Code)),
                        Classroom = x.GradePathwayClassrooms.Where(p => p.IsActive).Count().ToString() + " Classes",
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMappingClass, AddMappingClassValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = new MsGradePathway();
            data.Id = Guid.NewGuid().ToString();
            data.IdGrade = body.IdGrade;
            data.UserIn = AuthInfo.UserId;

            _dbContext.Entity<MsGradePathway>().Add(data);

            foreach (var itemPathway in body.Pathways.Distinct())
            {
                var dataPathway = new MsGradePathwayDetail();
                dataPathway.Id = Guid.NewGuid().ToString();
                dataPathway.IdGradePathway = data.Id;
                dataPathway.IdPathway = itemPathway;
                dataPathway.UserIn = AuthInfo.UserId;

                _dbContext.Entity<MsGradePathwayDetail>().Add(dataPathway);
            }

            foreach (var itemClass in body.Classrooms.Distinct())
            {
                var isExist = await _dbContext.Entity<MsGradePathwayClassroom>()
                    .Include(p => p.GradePathway)
                    .Include(p => p.Classroom)
                    .Where(p => p.GradePathway.IdGrade == body.IdGrade && p.IdClassroom == itemClass.IdClassroom)
                    .FirstOrDefaultAsync();

                if (isExist != null)
                {
                    throw new BadRequestException($"Data class {isExist.Classroom.Description} for this grade already use");
                }

                var dataClass = new MsGradePathwayClassroom();
                dataClass.Id = Guid.NewGuid().ToString();
                dataClass.IdGradePathway = data.Id;
                dataClass.IdClassroom = itemClass.IdClassroom;
                dataClass.UserIn = AuthInfo.UserId;

                _dbContext.Entity<MsGradePathwayClassroom>().Add(dataClass);

                foreach (var itemDivision in itemClass.IdDivision.Distinct())
                {
                    var dataDivision = new MsClassroomDivision();
                    dataDivision.Id = Guid.NewGuid().ToString();
                    dataDivision.IdGradePathwayClassroom = dataClass.Id;
                    dataDivision.IdDivision = itemDivision;
                    dataDivision.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsClassroomDivision>().Add(dataDivision);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMappingClass, UpdateMappingClassValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = _dbContext.Entity<MsGradePathway>()
                .Include(p => p.GradePathwayDetails)
                .Include(p => p.GradePathwayClassrooms)
                .ThenInclude(p => p.Classroom)
                .Include(p => p.GradePathwayClassrooms)
                .ThenInclude(p => p.ClassroomDivisions)
                .Where(p => p.Id == body.Id).FirstOrDefault();

            if (data == null)
            {
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], "Mapping class", "Id", body.Id));
            }

            var CurrentPathwayForThisGrade = await _dbContext.Entity<MsGradePathwayDetail>()
                    .Include(x=>x.GradePathway)
                    .Where(x=>x.IdGradePathway == body.Id)
                    .Select(x=>x.IdPathway)
                    .ToListAsync();
            var PathwayWillBeDeleted = CurrentPathwayForThisGrade.Except(body.Pathways);

            foreach (var itemRemoved in PathwayWillBeDeleted)
            {
                var dataPathwayRemoved = await _dbContext.Entity<MsGradePathwayDetail>().Where(x=>x.IdGradePathway == body.Id && x.IdPathway == itemRemoved).FirstOrDefaultAsync();
                if (dataPathwayRemoved != null)
                {
                    dataPathwayRemoved.IsActive = false;
                    _dbContext.Entity<MsGradePathwayDetail>().Update(dataPathwayRemoved);
                }
            }

            foreach (var itemPathway in body.Pathways)
            {
                var dataPathway = await _dbContext.Entity<MsGradePathwayDetail>()
                    .Include(p => p.SubjectPathways)
                    .Include(p => p.Pathway)
                    .Where(p => p.IdPathway == itemPathway && p.IdGradePathway == data.Id)
                    .FirstOrDefaultAsync();

                if (dataPathway == null)
                {
                    var dataPathwayDetail = new MsGradePathwayDetail();
                    dataPathwayDetail.Id = Guid.NewGuid().ToString();
                    dataPathwayDetail.IdGradePathway = data.Id;
                    dataPathwayDetail.IdPathway = itemPathway;
                    dataPathwayDetail.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsGradePathwayDetail>().Add(dataPathwayDetail);
                }
            }

            var CurrentClassForThisGrade = await _dbContext.Entity<MsGradePathwayClassroom>()
            .Where(x=>x.IdGradePathway == body.Id).Select(x=>x.Id).ToListAsync();

            var classRoomWillBeDeleted = CurrentClassForThisGrade.Except(body.Classrooms.Select(x=>x.IdPathwayClassroom).ToList());

            foreach (var itemClassWillDeleted in classRoomWillBeDeleted)
            {
                var dataClassroomDeleted = await _dbContext.Entity<MsGradePathwayClassroom>()
                    .Include(p => p.SubjectCombinations)
                    .Include(p => p.Classroom)
                    .Include(p => p.ClassroomDivisions)
                    .Where(p => p.Id == itemClassWillDeleted).FirstOrDefaultAsync();
                dataClassroomDeleted.IsActive = false;
                _dbContext.Entity<MsGradePathwayClassroom>().Update(dataClassroomDeleted);
                foreach (var itemClassRoomDivison in dataClassroomDeleted.ClassroomDivisions)
                {
                    itemClassRoomDivison.IsActive = false;
                    _dbContext.Entity<MsClassroomDivision>().Update(itemClassRoomDivison);
                }
            }

            foreach (var itemClass in body.Classrooms)
            {
                var dataClassroom = await _dbContext.Entity<MsGradePathwayClassroom>()
                    .Include(p => p.SubjectCombinations)
                    .Include(p => p.Classroom)
                    .Include(p => p.ClassroomDivisions)
                    .Where(p => p.Id == itemClass.IdPathwayClassroom).FirstOrDefaultAsync();

                var dataDivisionForThisClass = await _dbContext.Entity<MsClassroomDivision>()
                .Where(x=>x.IdGradePathwayClassroom == itemClass.IdPathwayClassroom)
                .Select(x=>x.IdDivision)
                .ToListAsync();
                var dataDivisionNew = body.Classrooms.Where(x=>x.IdPathwayClassroom == itemClass.IdPathwayClassroom).Select(x=>x.Division).FirstOrDefault();
                var dataDivisionWillBeRemoved = dataDivisionForThisClass.Except(dataDivisionNew);
                foreach (var itemDivisionWillBeRemoved in dataDivisionWillBeRemoved)
                {
                    var dataDivisionRemoved = await _dbContext.Entity<MsClassroomDivision>().Where(x=>x.IdGradePathwayClassroom == itemClass.IdPathwayClassroom && x.IdDivision == itemDivisionWillBeRemoved).FirstOrDefaultAsync();
                    if (dataDivisionRemoved != null)
                    {
                        dataDivisionRemoved.IsActive = false;
                        _dbContext.Entity<MsClassroomDivision>().Update(dataDivisionRemoved);
                    }
                }
                //cek class mekanisme update
                if (dataClassroom != null)
                {
                    dataClassroom.IdClassroom = itemClass.IdClassroom;
                    dataClassroom.UserUp = AuthInfo.UserId;
                    _dbContext.Entity<MsGradePathwayClassroom>().Update(dataClassroom);
                    foreach (var itemDivision in itemClass.Division)
                    {
                        var dataDivision = await _dbContext.Entity<MsClassroomDivision>()
                            .Where(p => p.IdDivision == itemDivision && p.IdGradePathwayClassroom == dataClassroom.Id).FirstOrDefaultAsync();
                        if (dataDivision == null)
                        {
                            var dataDivisionToClass = new MsClassroomDivision();
                            dataDivisionToClass.Id = Guid.NewGuid().ToString();
                            dataDivisionToClass.IdGradePathwayClassroom = dataClassroom.Id;
                            dataDivisionToClass.IdDivision = itemDivision;
                            dataDivisionToClass.UserIn = AuthInfo.UserId;

                            _dbContext.Entity<MsClassroomDivision>().Add(dataDivisionToClass);
                        }
                    }
                }
                else //cek class mekanisme insert 
                {
                    var isExist = await _dbContext.Entity<MsGradePathwayClassroom>()
                        .Include(p => p.GradePathway)
                        .Include(p => p.Classroom)
                        .Where(p => p.GradePathway.IdGrade == body.IdGrade && p.IdClassroom == itemClass.IdClassroom)
                        .FirstOrDefaultAsync();

                    if (isExist != null)
                    {
                        throw new BadRequestException($"Data class {isExist.Classroom.Description} for this grade already use");
                    }

                    var dataClass = new MsGradePathwayClassroom();
                    dataClass.Id = Guid.NewGuid().ToString();
                    dataClass.IdGradePathway = data.Id;
                    dataClass.IdClassroom = itemClass.IdClassroom;
                    dataClass.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsGradePathwayClassroom>().Add(dataClass);

                    foreach (var itemDivision in itemClass.Division)
                    {
                        var dataDisionToClass = new MsClassroomDivision();
                        dataDisionToClass.Id = Guid.NewGuid().ToString();
                        dataDisionToClass.IdGradePathwayClassroom = dataClass.Id;
                        dataDisionToClass.IdDivision = itemDivision;
                        dataDisionToClass.UserIn = AuthInfo.UserId;

                        _dbContext.Entity<MsClassroomDivision>().Add(dataDisionToClass);
                    }
                }
            }

            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsGradePathway>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
