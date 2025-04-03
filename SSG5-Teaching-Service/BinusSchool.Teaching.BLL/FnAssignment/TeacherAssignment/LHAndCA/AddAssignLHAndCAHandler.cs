using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA.Validator;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Mvc;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class AddAssignLHAndCAHandler : FunctionsHttpSingleHandler
	{
        private IDbContextTransaction _transaction;

		private readonly ITeachingDbContext _dbContext;
        private readonly IServiceProvider _provider;

        public AddAssignLHAndCAHandler(ITeachingDbContext dbContext, IServiceProvider provider)
		{
			_dbContext = dbContext;
			_provider = provider;
		}

        protected override async Task<ApiErrorResult<object>> Handler()
        {
			var body = await Request.GetBody<AddAssignLHAndCARequest>();
			(await new AddAssignLHAndCAValidator(_provider).ValidateAsync(body)).EnsureValid();
			_transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var grade = _dbContext.Entity<MsGrade>()
                .Where(e => e.Id == body.IdGrade)
                .FirstOrDefault();

            var checkTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
               .Include(x => x.TeacherPosition).ThenInclude(x => x.Position)
               .Where(x => new[] { PositionConstant.LevelHead, PositionConstant.ClassAdvisor }.Contains(x.TeacherPosition.Position.Code))
               .Where(x => x.TeacherPosition.IdSchool == body.IdSchool)
               .Where(x => x.IdAcademicYear == body.IdAcademicYear)
               .ToListAsync();

            if (checkTeachingLoad.Where(x => x.TeacherPosition.Position.Code == PositionConstant.LevelHead).FirstOrDefault() == null)
                throw new Exception($"Position {PositionConstant.LevelHead} not exists in this academic year");

            if (checkTeachingLoad.Where(x => x.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor).FirstOrDefault() == null)
                throw new Exception($"Position {PositionConstant.ClassAdvisor} not exists in this academic year");

            if (checkTeachingLoad
                         .Where(x => x.Parameter != null)
                         .Where(x => x.TeacherPosition.Position.Code == PositionConstant.LevelHead)
                         .FirstOrDefault() == null)
                throw new Exception($"Parameter data for position {PositionConstant.LevelHead} have not been set");

            if (checkTeachingLoad
               .Where(x => x.Parameter != null)
               .Where(x => x.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor)
               .FirstOrDefault() == null)
                throw new Exception($"Parameter data for position {PositionConstant.ClassAdvisor} have not been set");

            var teacherAssignments = await _dbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
                .Where(x =>
                    new[] { PositionConstant.LevelHead, PositionConstant.ClassAdvisor }.Contains(x.MsNonTeachingLoad.TeacherPosition.Position.Code)
                    )
                .Where(x => x.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear)
                .ToListAsync(CancellationToken);

            // find level head
            var levelHeads = teacherAssignments.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead);
            var currentLevelHead = levelHeads
                .Select(x => new { usrLoad = x, lvlHead = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(x.Data) })
                .FirstOrDefault(x => x.lvlHead.TryGetValue("Grade", out var g) && g?.Id == grade.Id)?.usrLoad;

            // find class advisor
            var classAdvisors = teacherAssignments.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor);
            var currentClassAdvisors = classAdvisors
                .Select(x => new { usrLoad = x, classAdvisor = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(x.Data) })
                .Where(x => x.classAdvisor.TryGetValue("Grade", out var g) && g?.Id == grade.Id)
                .ToList();

            var (lh, ca) = await GetLHAndCALoad(body.IdAcademicYear);
            var anyTransaction = false;
            if (body.IdUserLevelHead != null)
            {
                // add when level head empty
                if (currentLevelHead is null)
                {
                    var newLevelHead = new TrNonTeachingLoad
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = body.IdUserLevelHead,
                        IdMsNonTeachingLoad = lh.Id,
                        Data = body.Data,
                        Load = lh.Load
                    };
                    _dbContext.Entity<TrNonTeachingLoad>().Add(newLevelHead);
                }
                // otherwise update
                else
                {
                    currentLevelHead.IdUser = body.IdUserLevelHead;
                    currentLevelHead.Data = body.Data;
                    currentLevelHead.Load = lh.Load;
                    _dbContext.Entity<TrNonTeachingLoad>().Update(currentLevelHead);
                }
                anyTransaction = true;
            }

            if (body.ClassAdvisors?.Any() ?? false)
			{
                var listIds = new List<string> { grade.Id };
                var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => listIds.Contains(x.GradePathway.IdGrade));

                var classrooms = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails)
                .Where(predicate)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                    Formatted = $"{x.GradePathway.Grade.Code}{x.Classroom.Code}",
                    Grade = new CodeWithIdVm
                    {
                        Id = x.GradePathway.IdGrade,
                        Code = x.GradePathway.Grade.Code,
                        Description = x.GradePathway.Grade.Description
                    },
                    Pathway = new ClassroomMapPathway
                    {
                        Id = x.GradePathway.Id,
                        PathwayDetails = x.GradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = x.Classroom.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    }
                })
                .OrderBy(x => x.Grade.Code).ThenBy(x => x.Code)
                .ToListAsync(CancellationToken);

				foreach (var classroom in classrooms)
				{
					var incomingClassAdvisor = body.ClassAdvisors.FirstOrDefault(x => x.IdClassroom == classroom.Id);
					if (incomingClassAdvisor != null)
					{
						var currentClassAdvisor = currentClassAdvisors.FirstOrDefault(x => x.classAdvisor.TryGetValue("Classroom", out var c) && c.Id == classroom.Id);
						if (currentClassAdvisor is null)
						{
							var newClassAdvisor = new TrNonTeachingLoad
							{
								Id = Guid.NewGuid().ToString(),
								IdUser = incomingClassAdvisor.IdUserClassAdvisor,
								IdMsNonTeachingLoad = ca.Id,
								Data = incomingClassAdvisor.Data,
								Load = ca.Load
							};
							_dbContext.Entity<TrNonTeachingLoad>().Add(newClassAdvisor);
						}
						else
						{
							currentClassAdvisor.usrLoad.IdUser = incomingClassAdvisor.IdUserClassAdvisor;
							currentClassAdvisor.usrLoad.Data = incomingClassAdvisor.Data;
							currentClassAdvisor.usrLoad.Load = ca.Load;
							_dbContext.Entity<TrNonTeachingLoad>().Update(currentClassAdvisor.usrLoad);
						}
						anyTransaction = true;
					}
				}
			}

			if (anyTransaction)
			{
				await _dbContext.SaveChangesAsync(CancellationToken);
				await _transaction.CommitAsync(CancellationToken);
			}

			return Request.CreateApiResult2();
		}

		private async Task<(NonTeachingLoadVm lh, NonTeachingLoadVm ca)> GetLHAndCALoad(string IdAcademicYear)
		{
			var result = await _dbContext.Entity<MsNonTeachingLoad>()
				.Include(x => x.TeacherPosition).ThenInclude(x => x.Position)
				.Where(x => new[] { PositionConstant.LevelHead, PositionConstant.ClassAdvisor }.Contains(x.TeacherPosition.Position.Code))
				.Where(x=>x.IdAcademicYear == IdAcademicYear)
				.Select(x => new { x.TeacherPosition.Position.Code, x.Id, x.Load })
				.ToListAsync();

			var lhLoad = result.FirstOrDefault(x => x.Code == PositionConstant.LevelHead);
			var caLoad = result.FirstOrDefault(x => x.Code == PositionConstant.ClassAdvisor);

			return (
				lhLoad is null ? null : new NonTeachingLoadVm { Id = lhLoad.Id, Load = lhLoad.Load },
				caLoad is null ? null : new NonTeachingLoadVm { Id = caLoad.Id, Load = caLoad.Load });
		}

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
	}
}
