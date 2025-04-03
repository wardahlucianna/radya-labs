using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class GetDetailStudentExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailStudentExtracurricularHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailStudentExtracurricularRequest>(
                            nameof(GetDetailStudentExtracurricularRequest.IdAcademicYear),
                            nameof(GetDetailStudentExtracurricularRequest.IdLevel),
                            nameof(GetDetailStudentExtracurricularRequest.IdGrade),
                            nameof(GetDetailStudentExtracurricularRequest.Semester),
                            nameof(GetDetailStudentExtracurricularRequest.IdStudent));

            // get max effective number for the grade
            //var maxEffectiveCount = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
            //                            .Include(ergm => ergm.ExtracurricularRule)
            //                            .Where(x => x.IdGrade == param.IdGrade)
            //                            .Select(x => x.ExtracurricularRule.MaxEffectives)
            //                            .FirstOrDefault();

            //var extracurricularIdGradeList = _dbContext.Entity<TrExtracurricularGradeMapping>()
            //                                .Include(egm => egm.Extracurricular)
            //                                .Include(egm => egm.Grade)
            //                                .ThenInclude(g => g.Level)
            //                                .ThenInclude(l => l.AcademicYear)
            //                                .Where(x => x.Grade.Id == param.IdGrade &&
            //                                            x.Grade.Level.Id == param.IdLevel &&
            //                                            x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
            //                                            x.Extracurricular.Semester == param.Semester)
            //                                .Select(x => x.Extracurricular.Id)
            //                                .Distinct()
            //                                .ToList();

            var extracurricularOfStudentList = _dbContext.Entity<MsExtracurricularParticipant>()
                                                    .Include(ep => ep.Extracurricular)
                                                    .Include(ep => ep.Grade)
                                                    .ThenInclude(g => g.Level)
                                                    .ThenInclude(l => l.AcademicYear)
                                                    .Where(x =>
                                                                //extracurricularIdGradeList.Contains(x.Extracurricular.Id &&
                                                                x.Status == true && 
                                                                x.Extracurricular.Status == true &&
                                                                x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
                                                                x.Grade.Level.Id == param.IdLevel &&
                                                                x.Grade.Id == param.IdGrade &&
                                                                x.Extracurricular.Semester == param.Semester &&
                                                                x.IdStudent == param.IdStudent)
                                                    .Select(x => new
                                                    {
                                                        extracurricular = new NameValueVm
                                                        {
                                                            Id = x.IdExtracurricular,
                                                            Name = x.Extracurricular.Name
                                                        },
                                                        showScoreRC = x.Extracurricular.ShowScoreRC,
                                                        //priority = x.Priority,
                                                        isPrimary = x.IsPrimary
                                                    })
                                                    .Distinct()
                                                    .OrderBy(x => x.extracurricular.Name)
                                                    .ToList();


            // get number of student extracurricular
            var studentExtracurricularCount = extracurricularOfStudentList.Count();

            var spvCoachList = _dbContext.Entity<MsExtracurricularSpvCoach>()
                                .Include(x => x.ExtracurricularCoachStatus)
                                .Include(esc => esc.Extracurricular)
                                .Include(esc => esc.Staff)
                                .Where(x => extracurricularOfStudentList.Select(e => e.extracurricular.Id).Contains(x.Extracurricular.Id) &&
                                            x.Extracurricular.Semester == param.Semester)
                                .Select(x => new
                                {
                                    extracurricularId = x.IdExtracurricular,
                                    spvCoach = new NameValueVm
                                    {
                                        Id = x.Staff.IdBinusian,
                                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                    },
                                    isSpv = (x.IsSpv || x.ExtracurricularCoachStatus.Code == "SPV"),
                                    IdExtracurricularCoachStatus = x.IdExtracurricularCoachStatus,
                                    ExtracurricularCoachStatusCode = x.ExtracurricularCoachStatus.Code,
                                    ExtracurricularCoachStatusDesc = x.ExtracurricularCoachStatus.Description
                                })
                                .Distinct()
                                .ToList();

            var studentExtracurricularList = new List<GetDetailStudentExtracurricularResult_Extracurricular>();

            // get supervisor and coach
            foreach (var extracurricular in extracurricularOfStudentList)
            {
                var supervisor = spvCoachList
                                    .Where(x => x.extracurricularId == extracurricular.extracurricular.Id &&
                                                (x.isSpv == true || x.ExtracurricularCoachStatusCode == "SPV"))
                                    .Select(x => x.spvCoach)
                                    .FirstOrDefault();

                var coach = spvCoachList
                                .Where(x => x.extracurricularId == extracurricular.extracurricular.Id &&
                                            (x.isSpv == false || x.ExtracurricularCoachStatusCode != "SPV"))
                                .Select(x => x.spvCoach)
                                .FirstOrDefault();

                var studentExtracurricular = new GetDetailStudentExtracurricularResult_Extracurricular
                {
                    Extracurricular = extracurricular.extracurricular,
                    Supervisor = supervisor,
                    Coach = coach,
                    //Priority = extracurricular.priority,
                    // cannot change to primary excul when showScoreRC is false (excul doesn't need scoring)
                    EnablePrimary = true,//extracurricular.showScoreRC == true ? true : false,
                    IsPrimary = extracurricular.isPrimary
                };

                studentExtracurricularList.Add(studentExtracurricular);
            }

            var result = new GetDetailStudentExtracurricularResult
            {
                MaxEffective = studentExtracurricularCount,
                StudentExtracurricularList = studentExtracurricularList.Count == 0 ? null : studentExtracurricularList
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
