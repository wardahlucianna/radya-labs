using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetUnselectedStudentByHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetUnselectedStudentByHomeroomHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnselectedStudentByHomeroomRequest>(
                nameof(GetUnselectedStudentByHomeroomRequest.IdAcademicYear),
                nameof(GetUnselectedStudentByHomeroomRequest.Semester),
                nameof(GetUnselectedStudentByHomeroomRequest.IdExtracurricular));

            var columns = new[] { "idStudent", "studentName", "homeroomName" };

            //var aliasColumns = new Dictionary<string, string>
            //{
            //    { columns[0], "Student.Id" },
            //    { columns[1], "Student.Name" },
            //    { columns[2], "Homeroom.Name" }
            //};

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                        )
                    || EF.Functions.Like(x.IdStudent, param.SearchPattern())
                    );

            var extracurricularIdGradeMapping = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                                .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                                                .Select(x => x.IdGrade)
                                                .ToListAsync(CancellationToken);

            var homeroomStudentList = _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(hs => hs.Student)
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.GradePathwayClassroom)
                                        .ThenInclude(gpc => gpc.Classroom)
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.Grade)
                                        .ThenInclude(g => g.Level)
                                        .Where(x => x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                                    x.Semester == param.Semester &&
                                                    (string.IsNullOrEmpty(param.IdGrade) ? true : x.Homeroom.IdGrade == param.IdGrade) &&
                                                    (string.IsNullOrEmpty(param.IdHomeroom) ? true : x.IdHomeroom == param.IdHomeroom) &&
                                                    extracurricularIdGradeMapping.Any(y => y == x.Homeroom.Grade.Id)
                                                    )
                                        .Where(predicate)
                                        .Select(x => new
                                        {
                                            Student = new NameValueVm
                                            {
                                                Id = x.IdStudent,
                                                Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.LastName)
                                            },
                                            IdGrade = x.Homeroom.Grade.Id,
                                            Homeroom = new NameValueVm
                                            {
                                                Id = x.IdHomeroom,
                                                Name = string.Format("{0}{1}", x.Homeroom.Grade.Code, x.Homeroom.GradePathwayClassroom.Classroom.Code)
                                            }
                                        })
                                        //.OrderByDynamic(param, aliasColumns)
                                        .ToList();

            var unselectedHomeroomStudentList = homeroomStudentList.GroupJoin(_dbContext.Entity<MsExtracurricularParticipant>()
                                                  .Where(x => homeroomStudentList.Select(y => y.IdGrade).Any(y => y == x.IdGrade) &&
                                                              x.IdExtracurricular == param.IdExtracurricular),
                                                  hs => hs.Student.Id,
                                                  ep => ep.IdStudent,
                                                  (hs, ep) => new { hs, ep })
                                                .SelectMany(
                                                    x => x.ep.DefaultIfEmpty(),
                                                    (homeroomStudent, extracurricularParticipant) => new { homeroomStudent, extracurricularParticipant }
                                                )
                                                // eliminate student that already selected to this excul
                                                .Where(x => x.extracurricularParticipant == null)
                                                .Select(x => new
                                                {
                                                    Student = new NameValueVm
                                                    {
                                                        Id = x.homeroomStudent.hs.Student.Id,
                                                        Name = x.homeroomStudent.hs.Student.Name,
                                                    },
                                                    Homeroom = new NameValueVm
                                                    {
                                                        Id = x.homeroomStudent.hs.Homeroom.Id,
                                                        Name = x.homeroomStudent.hs.Homeroom.Name
                                                    },
                                                    IdGrade = x.homeroomStudent.hs.IdGrade
                                                })
                                                .ToList();


            unselectedHomeroomStudentList = param.OrderBy switch
            {
                "idStudent" => param.OrderType == OrderType.Asc
                    ? unselectedHomeroomStudentList.OrderBy(x => x.Student.Id).ToList() :
                    unselectedHomeroomStudentList.OrderByDescending(x => x.Student.Id).ToList(),
                "studentName" => param.OrderType == OrderType.Asc
                    ? unselectedHomeroomStudentList.OrderBy(x => x.Student.Name).ToList() :
                    unselectedHomeroomStudentList.OrderByDescending(x => x.Student.Name).ToList(),
                "homeroomName" => param.OrderType == OrderType.Asc
                    ? unselectedHomeroomStudentList.OrderBy(x => x.Homeroom.Name.Length).OrderBy(x => x.Homeroom.Name).ToList() :
                    unselectedHomeroomStudentList.OrderByDescending(x => x.Homeroom.Name.Length).OrderByDescending(x => x.Homeroom.Name).ToList(),
                _ => unselectedHomeroomStudentList.OrderBy(x => x.Student.Name).ToList()
            };

            #region no longer using extracurricular rule for this module
            // get max effective count di grade tersebut
            //var extracurricularRule = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
            //                                    .Include(ergm => ergm.ExtracurricularRule)
            //                                    .Where(x => x.IdGrade == homeroomStudent.IdGrade &&
            //                                                x.ExtracurricularRule.Status == true)
            //                                    .FirstOrDefault();
            #endregion

            #region unused code
            // get homeroom student yang sudah memenuhi max effective count extracurricular di grade tersebut
            //var studentIdJoinedMaxExtracurricularList = unselectedHomeroomStudentList
            //                                            .Join(
            //                                                _dbContext.Entity<MsExtracurricularParticipant>()
            //                                                .Include(ep => ep.Extracurricular)
            //                                                    .Where(x => x.IdGrade == homeroomStudent.IdGrade &&
            //                                                                x.Extracurricular.Semester == homeroomStudent.Semester),
            //                                                homeroomStudent => homeroomStudent.Student.Id,
            //                                                participant => participant.IdStudent,
            //                                                (homeroomStudent, participant) => new { homeroomStudent, participant })
            //                                            .GroupBy(x => new
            //                                            {
            //                                                studentId = x.homeroomStudent.Student.Id,
            //                                                studentName = x.homeroomStudent.Student.Name,
            //                                                homeroomId = x.homeroomStudent.Homeroom.Id,
            //                                                homeroomName = x.homeroomStudent.Homeroom.Name,
            //                                            })
            //                                            .Select(x => x.Key.studentId)
            //                                            .Distinct()
            //                                            .ToList();
            #endregion

            var resultItems = new List<GetUnselectedStudentByHomeroomResult>();

            foreach (var unselectedStudent in unselectedHomeroomStudentList)
            {
                var getReviewDate = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                    .Include(a => a.ExtracurricularRule)
                    .Where(a => a.ExtracurricularRule.IdAcademicYear == param.IdAcademicYear
                        && a.ExtracurricularRule.Semester == param.Semester
                        && a.IdGrade == unselectedStudent.IdGrade)
                    .FirstOrDefault();

                var insertItems = new GetUnselectedStudentByHomeroomResult
                {
                    Student = new NameValueVm
                    {
                        Id = unselectedStudent.Student.Id,
                        Name = unselectedStudent.Student.Name
                    },
                    Homeroom = new NameValueVm
                    {
                        Id = unselectedStudent.Homeroom.Id,
                        Name = unselectedStudent.Homeroom.Name
                    },
                    ReviewDate = getReviewDate == null ? "-" : (getReviewDate.ExtracurricularRule.ReviewDate == null ? "-" : (_dateTime.ServerTime >= getReviewDate.ExtracurricularRule.RegistrationStartDate && _dateTime.ServerTime <= getReviewDate.ExtracurricularRule.ReviewDate ? getReviewDate.ExtracurricularRule.ReviewDate.ToString() : "-"))
                };

                resultItems.Add(insertItems);
            }

            /*var resultItems = unselectedHomeroomStudentList
                                .Select(x => new GetUnselectedStudentByHomeroomResult
                                {
                                    Student = new NameValueVm
                                    {
                                        Id = x.Student.Id,
                                        Name = x.Student.Name
                                    },
                                    Homeroom = new NameValueVm
                                    {
                                        Id = x.Homeroom.Id,
                                        Name = x.Homeroom.Name
                                    },
                                    IsJoinedMaxExtracurricular = false
                                })
                                .SetPagination(param)
                                .ToList();*/

            resultItems = resultItems
                .SetPagination(param)
                .ToList();

            var count = unselectedHomeroomStudentList.Count;

            #region unused code
            // get homeroom student yang belum memenuhi max effective count extracurricular di grade tersebut
            //var counterHomeroomStudentListWithExtracurricular = homeroomStudentList
            //                               .GroupJoin(
            //                                    _dbContext.Entity<MsExtracurricularParticipant>()
            //                                        .Join(_dbContext.Entity<MsHomeroomStudent>()
            //                                                .Where(x => x.IdHomeroom == param.IdHomeroom),
            //                                                participant => participant.IdStudent,
            //                                                homeroomStudent => homeroomStudent.IdStudent,
            //                                                (participant, homeroomStudent) => new { participant, homeroomStudent }
            //                                               ),
            //                                    hs => hs.Student.Id,
            //                                    ep => ep.participant.IdStudent,
            //                                    (hs, ep) => new { hs, ep })
            //                                    .SelectMany(
            //                                        x => x.ep.DefaultIfEmpty(),
            //                                        (homeroomStudent, extracurricularParticipant) => new { homeroomStudent, extracurricularParticipant })
            //                                    .ToList();

            //var validHomeroomStudentList = counterHomeroomStudentListWithExtracurricular
            //                                .GroupBy(x => new
            //                                {
            //                                    studentId = x.homeroomStudent.hs.Student.Id,
            //                                    studentName = x.homeroomStudent.hs.Student.Name,
            //                                    homeroomId = x.homeroomStudent.hs.Homeroom.Id,
            //                                    homeroomName = x.homeroomStudent.hs.Homeroom.Name,
            //                                })
            //                                .Select(group => new GetUnselectedStudentByHomeroomResult
            //                                {
            //                                    Student = new NameValueVm
            //                                    {
            //                                        Id = group.Key.studentId,
            //                                        Name = group.Key.studentName
            //                                    },
            //                                    Homeroom = new NameValueVm
            //                                    {
            //                                        Id = group.Key.homeroomId,
            //                                        Name = group.Key.homeroomName
            //                                    },
            //                                    IsJoinedMaxExtracurricular = group.Count() >= maxEffectiveCount ? true : false
            //                                })
            //                                .ToList();

            #endregion

            return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(count));

        }
    }
}
