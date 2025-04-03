using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.Utils;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularSummary
{
    public class GetExtracurricularSummaryByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetExtracurricularSummaryByStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularSummaryByStudentRequest>(
                nameof(GetExtracurricularSummaryByStudentRequest.IdGrade),
                nameof(GetExtracurricularSummaryByStudentRequest.Semester),
                nameof(GetExtracurricularSummaryByStudentRequest.IdStudent)
                );

            var IdStudent = ProtectedString.CheckIdStudentAndDate(param.IdStudent);

            var studentExtracurricularDataList = _dbContext.Entity<MsExtracurricularParticipant>()
                    .Where(x => x.Grade.Id == param.IdGrade &&
                                x.IdStudent == IdStudent &&
                                x.Status == true &&
                                x.Extracurricular.Status &&
                                x.IsPrimary == true)
                    .Select(x => new
                    {
                        ExtracurricularId = x.Extracurricular.Id,
                        ExtracurricularName = x.Extracurricular.Name,
                        ExtracurricularType = x.Extracurricular.ExtracurricularType.Code ?? null,
                        ShowAttendanceRC = x.Extracurricular.ShowAttendanceRC,
                        ShowScoreRC = x.Extracurricular.ShowScoreRC,
                        Semester = x.Extracurricular.Semester,
                        IsPrimary = x.IsPrimary,
                        IdSchool = x.Grade.Level.AcademicYear.IdSchool
                    })
                    .Distinct()
                    .OrderBy(x => x.ExtracurricularName)
                    .ToList();

            #region Semester 1
            var studentExtracurricularListSemester1 = studentExtracurricularDataList.Where(x => x.Semester == 1).Distinct().OrderBy(x => x.ExtracurricularName).ToList();
            var idExtracurricularListSemester1 = studentExtracurricularListSemester1.Select(x => x.ExtracurricularId).ToList();

            var studentScoreListSemester1 = _dbContext.Entity<TrExtracurricularScoreEntry>()
                            .Include(a => a.ExtracurricularScoreLegend)
                            .Join(_dbContext.Entity<MsExtracurricularScoreCompMapping>(),
                                a => a.IdExtracurricular,
                                b => b.IdExtracurricular,
                                (a, b) => new { ExtracurricularScoreEntry = a, ExtracurricularScoreCompMapping = b })
                            .Where(x => idExtracurricularListSemester1.Contains(x.ExtracurricularScoreEntry.IdExtracurricular)
                                && x.ExtracurricularScoreEntry.IdStudent == IdStudent)
                            .Select(x => new
                            {
                                IdExtracurricular = x.ExtracurricularScoreEntry.IdExtracurricular,
                                IdExtracurricularScoreCalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.IdExtracurricularScoreCalculationType,
                                CalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.ExtracurricularScoreCalculationType.CalculationType,
                                Score = int.Parse(x.ExtracurricularScoreEntry.ExtracurricularScoreLegend.Score)
                            })
                            .ToList();

            var groupStudentScoreListSemester1 = studentScoreListSemester1.GroupBy(x => new { x.IdExtracurricular, x.IdExtracurricularScoreCalculationType, x.CalculationType })
                            .Select(g => new
                            {
                                IdExtracurricular = g.Key.IdExtracurricular,
                                Grade = _dbContext.Entity<MsExtracurricularScoreGrade>()
                                    .Where(f => f.IdExtracurricularScoreCalculationType == g.Key.IdExtracurricularScoreCalculationType
                                                && ((g.Key.CalculationType == "AVG" && g.Select(x => x.Score).Average() >= Convert.ToDouble(f.MinScore)
                                                && g.Select(x => x.Score).Average() <= Convert.ToDouble(f.MaxScore))
                                                || (g.Key.CalculationType == "SUM" && g.Select(x => x.Score).Sum() >= Convert.ToDecimal(f.MinScore)
                                                && g.Select(x => x.Score).Sum() <= Convert.ToDecimal(f.MaxScore))))
                                    .Select(f => f.Grade)
                                    .FirstOrDefault() ?? "N/A"
                            })
                            .ToList();

            var studentAttendanceListSemester1 = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                            .Where(x => idExtracurricularListSemester1.Contains(x.IdExtracurricular))
                                            .ToList()
                                            .GroupJoin(
                                                _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                                .Include(x => x.ExtracurricularStatusAtt)
                                                .Where(x => x.IdStudent == IdStudent),
                                                generatedAtt => generatedAtt.Id,
                                                attEntry => attEntry.IdExtracurricularGeneratedAtt,
                                                (generatedAtt, attEntry) => new { generatedAtt, attEntry }
                                            )
                                            .SelectMany(
                                                x => x.attEntry.DefaultIfEmpty(),
                                                (generatedAtt, attEntry) => new { generatedAtt, attEntry }
                                            )
                                            .Distinct()
                                            .ToList();
            #endregion

            #region Semester 2
            var studentExtracurricularList = studentExtracurricularDataList.Where(x => x.Semester == param.Semester).Distinct().OrderBy(x => x.ExtracurricularName).ToList();
            var idExtracurricularList = studentExtracurricularList.Select(x => x.ExtracurricularId).ToList();

            var studentScoreList = _dbContext.Entity<TrExtracurricularScoreEntry>()
                            .Include(a => a.ExtracurricularScoreLegend)
                            .Join(_dbContext.Entity<MsExtracurricularScoreCompMapping>(),
                                a => a.IdExtracurricular,
                                b => b.IdExtracurricular,
                                (a, b) => new { ExtracurricularScoreEntry = a, ExtracurricularScoreCompMapping = b })
                            .Where(x => idExtracurricularList.Contains(x.ExtracurricularScoreEntry.IdExtracurricular)
                                && x.ExtracurricularScoreEntry.IdStudent == IdStudent)
                            .Select(x => new
                            {
                                IdExtracurricular = x.ExtracurricularScoreEntry.IdExtracurricular,
                                IdExtracurricularScoreCalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.IdExtracurricularScoreCalculationType,
                                CalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.ExtracurricularScoreCalculationType.CalculationType,
                                Score = int.Parse(x.ExtracurricularScoreEntry.ExtracurricularScoreLegend.Score)
                            })
                            .ToList();

            var groupStudentScoreList = studentScoreList.GroupBy(x => new { x.IdExtracurricular, x.IdExtracurricularScoreCalculationType, x.CalculationType })
                            .Select(g => new
                            {
                                IdExtracurricular = g.Key.IdExtracurricular,
                                Grade = _dbContext.Entity<MsExtracurricularScoreGrade>()
                                    .Where(f => f.IdExtracurricularScoreCalculationType == g.Key.IdExtracurricularScoreCalculationType
                                                && ((g.Key.CalculationType == "AVG" && g.Select(x => x.Score).Average() >= Convert.ToDouble(f.MinScore)
                                                && g.Select(x => x.Score).Average() <= Convert.ToDouble(f.MaxScore))
                                                || (g.Key.CalculationType == "SUM" && g.Select(x => x.Score).Sum() >= Convert.ToDecimal(f.MinScore)
                                                && g.Select(x => x.Score).Sum() <= Convert.ToDecimal(f.MaxScore))))
                                    .Select(f => f.Grade)
                                    .FirstOrDefault() ?? "N/A"
                            })
                            .ToList();

            var studentAttendanceList = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                            .Where(x => idExtracurricularList.Contains(x.IdExtracurricular))
                                            .ToList()
                                            .GroupJoin(
                                                _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                                .Include(x => x.ExtracurricularStatusAtt)
                                                .Where(x => x.IdStudent == IdStudent),
                                                generatedAtt => generatedAtt.Id,
                                                attEntry => attEntry.IdExtracurricularGeneratedAtt,
                                                (generatedAtt, attEntry) => new { generatedAtt, attEntry }
                                            )
                                            .SelectMany(
                                                x => x.attEntry.DefaultIfEmpty(),
                                                (generatedAtt, attEntry) => new { generatedAtt, attEntry }
                                            )
                                            .Distinct()
                                            .ToList();
            #endregion

            #region Semester 1 and Semester 2 (Both Semester)
            var studentExtracurricularListBothSemester = studentExtracurricularDataList;
            var idExtracurricularListBothSemester = studentExtracurricularListBothSemester.Select(x => x.ExtracurricularId).ToList();

            var studentScoreListBothSemester = _dbContext.Entity<TrExtracurricularScoreEntry>()
                            .Include(a => a.ExtracurricularScoreLegend)
                            .Join(_dbContext.Entity<MsExtracurricularScoreCompMapping>(),
                                a => a.IdExtracurricular,
                                b => b.IdExtracurricular,
                                (a, b) => new { ExtracurricularScoreEntry = a, ExtracurricularScoreCompMapping = b })
                            .Where(x => idExtracurricularListBothSemester.Contains(x.ExtracurricularScoreEntry.IdExtracurricular)
                                && x.ExtracurricularScoreEntry.IdStudent == IdStudent)
                            .Select(x => new
                            {
                                IdExtracurricular = x.ExtracurricularScoreEntry.IdExtracurricular,
                                IdExtracurricularScoreCalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.IdExtracurricularScoreCalculationType,
                                CalculationType = x.ExtracurricularScoreCompMapping.ExtracurricularScoreCompCategory.ExtracurricularScoreCalculationType.CalculationType,
                                Score = int.Parse(x.ExtracurricularScoreEntry.ExtracurricularScoreLegend.Score)
                            })
                            .ToList();

            var groupStudentScoreListBothSemester = studentScoreList.GroupBy(x => new { x.IdExtracurricular, x.IdExtracurricularScoreCalculationType, x.CalculationType })
                            .Select(g => new
                            {
                                IdExtracurricular = g.Key.IdExtracurricular,
                                Grade = _dbContext.Entity<MsExtracurricularScoreGrade>()
                                    .Where(f => f.IdExtracurricularScoreCalculationType == g.Key.IdExtracurricularScoreCalculationType
                                                && ((g.Key.CalculationType == "AVG" && g.Select(x => x.Score).Average() >= Convert.ToDouble(f.MinScore)
                                                && g.Select(x => x.Score).Average() <= Convert.ToDouble(f.MaxScore))
                                                || (g.Key.CalculationType == "SUM" && g.Select(x => x.Score).Sum() >= Convert.ToDecimal(f.MinScore)
                                                && g.Select(x => x.Score).Sum() <= Convert.ToDecimal(f.MaxScore))))
                                    .Select(f => f.Grade)
                                    .FirstOrDefault() ?? "N/A"
                            })
                            .ToList();
            #endregion

            var resultList = new List<GetExtracurricularSummaryByStudentResult>();

            if (param.Semester == 1)
            {
                foreach (var studentExtracurricular in studentExtracurricularList)
                {
                    // Score
                    var studentScore = groupStudentScoreList
                                    .Where(x => x.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                    .Select(x => x.Grade)
                                    .FirstOrDefault();
                    // Attendance
                    var studentAttendance = studentAttendanceList
                                                .Where(x => x.generatedAtt.generatedAtt.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                                .ToList();

                    var totalGeneratedAttendance = studentAttendance
                                                    .Where(x => x.attEntry != null)
                                                    .Select(x => x.generatedAtt.generatedAtt.Id)
                                                    .Distinct()
                                                    .Count();

                    var totalValidStudentAttendace = studentAttendance
                                                    .Where(x => x.attEntry != null &&
                                                                x.attEntry.ExtracurricularStatusAtt.IsPresent == true)
                                                    .Select(x => x.attEntry.Id)
                                                    .Distinct()
                                                    .Count();

                    var validAttendancePercentage = Math.Round((totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100, 2);
                    var getAttendancePercentage = (totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100;


                    var result = new GetExtracurricularSummaryByStudentResult
                    {
                        Extracurricular = new NameValueVm
                        {
                            Id = studentExtracurricular.ExtracurricularId,
                            Name = studentExtracurricular.ExtracurricularName
                        },
                        ExtracurricularType = studentExtracurricular.ExtracurricularType,
                        Semester = studentExtracurricular.Semester,
                        IsClub = ElectiveIsClub(studentExtracurricular.ExtracurricularType),
                        IsPrimary = studentExtracurricular.IsPrimary,
                        AttendancePercentage = (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool == "3" ? "-" : (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool != "3" ? "N/A" : validAttendancePercentage.ToString() + "%",
                        ScorePerformance = (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool == "3" ? "-" : (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool != "3" ? "N/A" : studentScore,
                        AttendancePercentageFinal = studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage) ? "N/A" : validAttendancePercentage.ToString(),
                        ScorePerformanceFinal = studentScore == null || studentExtracurricular.ShowScoreRC == false ? "N/A" : studentScore,
                        ShowScoreRC = studentExtracurricular.ShowScoreRC,
                        AttendanceScore = getAttendancePercentage
                    };

                    resultList.Add(result);
                }
            }
            else if (param.Semester == 2)
            {
                foreach (var studentExtracurricularSemester1 in studentExtracurricularListSemester1)
                {
                    var studentScoreSemester1 = groupStudentScoreListSemester1
                                    .Where(x => x.IdExtracurricular == studentExtracurricularSemester1.ExtracurricularId)
                                    .Select(x => x.Grade)
                                    .FirstOrDefault();

                    var studentAttendanceSemester1 = studentAttendanceListSemester1
                                            .Where(x => x.generatedAtt.generatedAtt.IdExtracurricular == studentExtracurricularSemester1.ExtracurricularId)
                                            .ToList();

                    var totalGeneratedAttendanceSemester1 = studentAttendanceSemester1
                                                .Where(x => x.attEntry != null)
                                                .Select(x => x.generatedAtt.generatedAtt.Id)
                                                .Distinct()
                                                .Count();

                    var totalValidStudentAttendaceSemester1 = studentAttendanceSemester1
                                                .Where(x => x.attEntry != null &&
                                                            x.attEntry.ExtracurricularStatusAtt.IsPresent == true)
                                                .Select(x => x.attEntry.Id)
                                                .Distinct()
                                                .Count();

                    var validAttendancePercentageSemester1 = Math.Round((totalValidStudentAttendaceSemester1 / (double)totalGeneratedAttendanceSemester1) * 100, 2);
                    var getAttendancePercentage = (totalValidStudentAttendaceSemester1 / (double)totalGeneratedAttendanceSemester1) * 100;

                    var result = new GetExtracurricularSummaryByStudentResult
                    {
                        Extracurricular = new NameValueVm
                        {
                            Id = studentExtracurricularSemester1.ExtracurricularId,
                            Name = studentExtracurricularSemester1.ExtracurricularName
                        },
                        ExtracurricularType = studentExtracurricularSemester1.ExtracurricularType,
                        Semester = studentExtracurricularSemester1.Semester,
                        IsClub = ElectiveIsClub(studentExtracurricularSemester1.ExtracurricularType),
                        IsPrimary = studentExtracurricularSemester1.IsPrimary,
                        AttendancePercentage = (studentExtracurricularSemester1.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentageSemester1)) && studentExtracurricularSemester1.IdSchool == "3" ? "-" : (studentExtracurricularSemester1.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentageSemester1)) && studentExtracurricularSemester1.IdSchool != "3" ? "N/A" : validAttendancePercentageSemester1.ToString() + "%",
                        ScorePerformance = (studentScoreSemester1 == null || studentExtracurricularSemester1.ShowScoreRC == false) && studentExtracurricularSemester1.IdSchool == "3" ? "-" : (studentScoreSemester1 == null || studentExtracurricularSemester1.ShowScoreRC == false) && studentExtracurricularSemester1.IdSchool != "3" ? "N/A" : studentScoreSemester1,
                        AttendancePercentageFinal = studentExtracurricularSemester1.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentageSemester1) ? "N/A" : validAttendancePercentageSemester1.ToString(),
                        ScorePerformanceFinal = studentScoreSemester1 == null || studentExtracurricularSemester1.ShowScoreRC == false ? "N/A" : studentScoreSemester1,
                        ShowScoreRC = studentExtracurricularSemester1.ShowScoreRC,
                        AttendanceScore = getAttendancePercentage
                    };

                    resultList.Add(result);
                }

                if (studentAttendanceListSemester1.Count() > 0)
                {
                    foreach (var studentExtracurricular in studentExtracurricularList)
                    {
                        // Score
                        var studentScore = groupStudentScoreList
                                            .Where(x => x.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                            .Select(x => x.Grade)
                                            .FirstOrDefault();

                        var studentScoreBothSemester = groupStudentScoreListBothSemester
                                            .Where(x => x.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                            .Select(x => x.Grade)
                                            .FirstOrDefault();
                        // Attendance
                        var studentAttendance = studentAttendanceList
                                                    .Where(x => x.generatedAtt.generatedAtt.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                                    .ToList();

                        var totalGeneratedAttendance = studentAttendance
                                                        .Where(x => x.attEntry != null)
                                                        .Select(x => x.generatedAtt.generatedAtt.Id)
                                                        .Distinct()
                                                        .Count();

                        var totalValidStudentAttendace = studentAttendance
                                                        .Where(x => x.attEntry != null &&
                                                                    x.attEntry.ExtracurricularStatusAtt.IsPresent == true)
                                                        .Select(x => x.attEntry.Id)
                                                        .Distinct()
                                                        .Count();

                        var validAttendancePercentage = Math.Round((totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100, 2);

                        foreach (var studentExtracurricularSemester1 in studentExtracurricularListSemester1)
                        {
                            var studentScoreSemester1 = groupStudentScoreListSemester1
                                            .Where(x => x.IdExtracurricular == studentExtracurricularSemester1.ExtracurricularId)
                                            .Select(x => x.Grade)
                                            .FirstOrDefault();

                            var studentAttendanceSemester1 = studentAttendanceListSemester1
                                                    .Where(x => x.generatedAtt.generatedAtt.IdExtracurricular == studentExtracurricularSemester1.ExtracurricularId)
                                                    .ToList();

                            var totalGeneratedAttendanceSemester1 = studentAttendanceSemester1
                                                        .Where(x => x.attEntry != null)
                                                        .Select(x => x.generatedAtt.generatedAtt.Id)
                                                        .Distinct()
                                                        .Count();

                            var totalValidStudentAttendaceSemester1 = studentAttendanceSemester1
                                                        .Where(x => x.attEntry != null &&
                                                                    x.attEntry.ExtracurricularStatusAtt.IsPresent == true)
                                                        .Select(x => x.attEntry.Id)
                                                        .Distinct()
                                                        .Count();

                            var validAttendancePercentageSemester1 = Math.Round((totalValidStudentAttendaceSemester1 / (double)totalGeneratedAttendanceSemester1) * 100, 2);

                            double totalAttendance = Math.Round((validAttendancePercentage + validAttendancePercentageSemester1) / 2, 2);
                            var getAttendancePercentage = (totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100;

                            var resultSemester2 = new GetExtracurricularSummaryByStudentResult
                            {
                                Extracurricular = new NameValueVm
                                {
                                    Id = studentExtracurricular.ExtracurricularId,
                                    Name = studentExtracurricular.ExtracurricularName
                                },
                                ExtracurricularType = studentExtracurricular.ExtracurricularType,
                                Semester = studentExtracurricular.Semester,
                                IsClub = ElectiveIsClub(studentExtracurricular.ExtracurricularType),
                                IsPrimary = studentExtracurricular.IsPrimary,
                                AttendancePercentage = (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool == "3" ? "-" : (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool != "3" ? "N/A" : validAttendancePercentage.ToString() + "%",
                                ScorePerformance = (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool == "3" ? "-" : (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool != "3" ? "N/A" : studentScore,
                                AttendancePercentageFinal = studentExtracurricularListBothSemester.Select(a => a.ShowAttendanceRC).Contains(false) || double.IsNaN(totalAttendance) ? "N/A" : totalAttendance.ToString(),
                                ScorePerformanceFinal = studentScore == null || studentExtracurricular.ShowScoreRC == false ? "N/A" : studentScore,
                                ShowScoreRC = studentExtracurricular.ShowScoreRC,
                                AttendanceScore = getAttendancePercentage
                            };

                            resultList.Add(resultSemester2);
                        }
                    }
                }
                else
                {
                    foreach (var studentExtracurricular in studentExtracurricularList)
                    {
                        var studentScore = groupStudentScoreList
                                            .Where(x => x.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                            .Select(x => x.Grade)
                                            .FirstOrDefault();

                        var studentScoreBothSemester = groupStudentScoreListBothSemester
                                            .Where(x => x.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                            .Select(x => x.Grade)
                                            .FirstOrDefault();
                        // Attendance
                        var studentAttendance = studentAttendanceList
                                                    .Where(x => x.generatedAtt.generatedAtt.IdExtracurricular == studentExtracurricular.ExtracurricularId)
                                                    .ToList();

                        var totalGeneratedAttendance = studentAttendance
                                                        .Where(x => x.attEntry != null)
                                                        .Select(x => x.generatedAtt.generatedAtt.Id)
                                                        .Distinct()
                                                        .Count();

                        var totalValidStudentAttendace = studentAttendance
                                                        .Where(x => x.attEntry != null &&
                                                                    x.attEntry.ExtracurricularStatusAtt.IsPresent == true)
                                                        .Select(x => x.attEntry.Id)
                                                        .Distinct()
                                                        .Count();

                        var validAttendancePercentage = Math.Round((totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100, 2);
                        var getAttendancePercentage = (totalValidStudentAttendace / (double)totalGeneratedAttendance) * 100;

                        var resultSemester2 = new GetExtracurricularSummaryByStudentResult
                        {
                            Extracurricular = new NameValueVm
                            {
                                Id = studentExtracurricular.ExtracurricularId,
                                Name = studentExtracurricular.ExtracurricularName
                            },
                            ExtracurricularType = studentExtracurricular.ExtracurricularType,
                            Semester = studentExtracurricular.Semester,
                            IsClub = ElectiveIsClub(studentExtracurricular.ExtracurricularType),
                            IsPrimary = studentExtracurricular.IsPrimary,
                            AttendancePercentage = (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool == "3" ? "-" : (studentExtracurricular.ShowAttendanceRC == false || double.IsNaN(validAttendancePercentage)) && studentExtracurricular.IdSchool != "3" ? "N/A" : validAttendancePercentage.ToString() + "%",
                            ScorePerformance = (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool == "3" ? "-" : (studentScore == null || studentExtracurricular.ShowScoreRC == false) && studentExtracurricular.IdSchool != "3" ? "N/A" : studentScore,
                            AttendancePercentageFinal = studentExtracurricularListBothSemester.Select(a => a.ShowAttendanceRC).Contains(false) || double.IsNaN(validAttendancePercentage) ? "N/A" : validAttendancePercentage.ToString(),
                            ScorePerformanceFinal = studentScore == null || studentExtracurricular.ShowScoreRC == false ? "N/A" : studentScore,
                            ShowScoreRC = studentExtracurricular.ShowScoreRC,
                            AttendanceScore = getAttendancePercentage
                        };
                        resultList.Add(resultSemester2);
                    }
                }
            }
            return Request.CreateApiResult2(resultList as object);
        }

        private bool? ElectiveIsClub(string? ExtracurricularType)
        {
            bool? isClub = null;

            if (ExtracurricularType != null)
            {
                isClub = ExtracurricularType.ToLower() == "club" ? true : false;
            }

            return isClub;
        }
    }
}
