using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetUnSubmittedScoreHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;


        public GetUnSubmittedScoreHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnSubmittedScoreRequest>(nameof(GetUnSubmittedScoreRequest.IdAcademicYear), nameof(GetUnSubmittedScoreRequest.IdBinusian));



            bool IsACOP = false;

            var CheckUserACOP = await _dbContext.Entity<MsUserRole>()
                            .Include(x => x.Role)
                            .Where(a => a.IdUser == param.IdBinusian
                            && a.Role.Code.ToLower().Contains("acop"))
                            .FirstOrDefaultAsync();

            if (CheckUserACOP != null)
            {
                IsACOP = true;
            }


            //var predicate = PredicateBuilder.Create<MsExtracurricular>(x => x.ExtracurricularGradeMappings.Select(a => a.Grade.Level.IdAcademicYear).Contains(param.IdAcademicYear)
            //                                                                && x.Semester == param.Semester
            //                                                                && (x.ExtracurricularSpvCoach.Select(b => b.IdBinusian).Contains(param.IdBinusian) || IsACOP));


            var query = await _dbContext.Entity<MsExtracurricular>()
                .Include(a => a.ExtracurricularGradeMappings).ThenInclude(b => b.Grade).ThenInclude(b => b.Level)
                .Include(a => a.ExtracurricularSpvCoach)
            .Where(x => x.ExtracurricularGradeMappings.Select(a => a.Grade.Level.IdAcademicYear).Contains(param.IdAcademicYear)
                    && x.Semester == (param.Semester == 0 ? x.Semester : param.Semester) && x.Status == true)
            .Where(a => _dateTime.ServerTime >= a.ScoreStartDate)
            .ToListAsync(CancellationToken);

            if (query.Where(x => x.ExtracurricularSpvCoach.Select(b => b.IdBinusian).Contains(param.IdBinusian) || IsACOP).ToList().Count() > 0)
            {
                query = query.Where(x => x.ExtracurricularSpvCoach.Select(b => b.IdBinusian).Contains(param.IdBinusian) || IsACOP).ToList();
            }

            var componentData = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                .Join(_dbContext.Entity<MsExtracurricularScoreCompMapping>()
                    .Include(x => x.Extracurricular).ThenInclude(x => x.ExtracurricularSpvCoach),
                    (component) => component.IdExtracurricularScoreCompCategory,
                    (mapping) => mapping.IdExtracurricularScoreCompCategory,
                    (component, mapping) => new { component, mapping })
                .Where(x => query.Select(a => a.Id).Contains(x.mapping.IdExtracurricular))
                .Where(x => x.mapping.Extracurricular.Status == true)
                .Select(x => new
                {
                    IdExtracurricular = x.mapping.Extracurricular.Id,
                    IdComponent = x.component.Id
                })
                .ToListAsync(CancellationToken);

            var studentUnSubmitted = await _dbContext.Entity<MsExtracurricular>()
               .Include(a => a.ExtracurricularParticipants)
               .Include(a => a.ExtracurricularSpvCoach)
                    .ThenInclude(b => b.ExtracurricularCoachStatus)
               .Where(a => query.Select(b => b.Id.ToString()).Contains(a.Id)
                && a.Status == true)
               .OrderBy(a => a.Name)
               .Select(a => new GetUnSubmittedScoreResultWithShowRC
               {
                   Supervisior = string.Join("; ", a.ExtracurricularSpvCoach.Where(b => b.IsSpv == true || b.ExtracurricularCoachStatus.Code == "SPV").Select(c => (String.Format("{0} {1}", c.Staff.FirstName.Trim(), c.Staff.LastName.Trim())).Trim())),
                   Extracurricular = new ItemValueVm(a.Id, a.Name),
                   ShowScoreRC = a.ShowScoreRC,
                   Students = a.ExtracurricularParticipants
                    .Select(a => new ItemValueVm
                        {
                            Id = a.IdStudent,
                            Description = String.Format("{0} {1}", a.Student.FirstName, a.Student.LastName)
                        }
                    )
                    .ToList(),
                   Semester = a.Semester
               })
               .ToListAsync();

            var getStudentScore = await _dbContext.Entity<TrExtracurricularScoreEntry>()
                .Where(x => studentUnSubmitted.Select(y => y.Extracurricular.Id).Contains(x.IdExtracurricular))
                .ToListAsync(CancellationToken);

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                .Where(x => studentUnSubmitted.Select(y => y.Extracurricular.Id).Contains(x.IdExtracurricular))
                .ToListAsync(CancellationToken);

            foreach (var data in studentUnSubmitted)
            {
                var unsubmittedStudentList = new List<ItemValueVm>();

                foreach (var student in data.Students)
                {
                    var filterStudentScore = getStudentScore.Where(x => x.IdStudent == student.Id && x.IdExtracurricular == data.Extracurricular.Id).ToList();
                    var filteredComponentData = componentData.Where(x => x.IdExtracurricular == data.Extracurricular.Id && !filterStudentScore.Select(y => y.IdExtracurricularScoreComponent).Contains(x.IdComponent)).ToList();

                    if (filteredComponentData.Count() != 0)
                    {
                        unsubmittedStudentList.Add(student);
                    }
                }

                if (data.ShowScoreRC == false)
                {
                    var checkParticipant = getExtracurricularParticipant.Where(x => x.IdExtracurricular == data.Extracurricular.Id).Select(x => x.IdStudent).Distinct().Count();

                    if (checkParticipant - unsubmittedStudentList.Select(x => x.Id).Distinct().Count() == 0)
                    {
                        unsubmittedStudentList = new List<ItemValueVm>();
                    }
                }

                data.Students = unsubmittedStudentList;
            }

            IReadOnlyList<GetUnSubmittedScoreResult> items;
            if (param.Return == CollectionType.Lov)
                items = studentUnSubmitted
                    .Where(a => a.Students.Count() > 0)
                    .Select(x => new GetUnSubmittedScoreResult
                    {
                        Supervisior = x.Supervisior,
                        Extracurricular = x.Extracurricular,
                        Students = x.Students,
                        Total = x.Students.Count(),
                        Semester = x.Semester
                    })
                    .ToList();
            else
                items = studentUnSubmitted
                    .Where(a => a.Students.Count() > 0)
                    .SetPagination(param)
                      .Select(x => new GetUnSubmittedScoreResult
                      {
                          Supervisior = x.Supervisior,
                          Extracurricular = x.Extracurricular,
                          Students = x.Students,
                          Total = x.Students.Count(),
                          Semester = x.Semester
                      })
                    .ToList();
                  
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : studentUnSubmitted.Where(a => a.Students.Count() > 0).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));


        }

        private class GetUnSubmittedScoreResultWithShowRC : GetUnSubmittedScoreResult
        {
            public bool ShowScoreRC { get; set; }
        }
    }
}
