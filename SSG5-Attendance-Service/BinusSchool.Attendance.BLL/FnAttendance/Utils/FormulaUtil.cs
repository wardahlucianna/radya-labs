using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Jace;

namespace BinusSchool.Attendance.FnAttendance.Utils
{
    public static class FormulaUtil
    {
        public static bool Validate(this string formula, IEnumerable<MsAttendance> source)
        {
            var result = true;
            var variables = GetVariables(ref formula);

            var variableValues = new Dictionary<string, double>();
            foreach (var variable in variables)
            {
                if (variable == "TotalDay")
                    variableValues.AddIfNotExist("TotalDay", 1);
                else
                {
                    if (!source.Any(x => x.Description.Replace(" ", string.Empty) == variable))
                        result = false;
                    else
                        variableValues.AddIfNotExist(variable, 1);
                }
            }
            if (result)
            {
                try
                {
                    new CalculationEngine().Calculate(formula, variableValues);
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }
        public static double Calculate(this string formula, AbsentTerm term, IEnumerable<TrGeneratedScheduleLesson> source)
        {
            var variables = GetVariables(ref formula);

            var variableValues = new Dictionary<string, double>();
            foreach (var variable in variables)
            {
                if (variable == "TotalDay")
                    variableValues.AddIfNotExist("TotalDay", term == AbsentTerm.Day ? source.GroupBy(x => x.ScheduleDate).Count() : source.Count());
                else
                    variableValues.AddIfNotExist(variable, term == AbsentTerm.Day ?
                                                                   source.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Description.Replace(" ", string.Empty) == variable && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() :
                                                                   source.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Description.Replace(" ", string.Empty) == variable && z.Status == AttendanceEntryStatus.Submitted)));               
            }

            return Math.Round(new CalculationEngine().Calculate(formula, variableValues), 2);
        }

        public static double CalculateNew(this string formula, AbsentTerm term, List<TrAttendanceSummaryTerm> source)
        {
            var variables = GetVariables(ref formula);

            var variableValues = new Dictionary<string, double>();
            foreach (var variable in variables)
            {
                if (variable == "TotalDay")
                    variableValues.AddIfNotExist("TotalDay", 
                                                    term == AbsentTerm.Day 
                                                    ? source.Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName).Sum(e => e.Total)
                                                    : source.Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName).Sum(e => e.Total));
                //else if (variable == "UnexplainedAbsence")
                //    variableValues.AddIfNotExist("UnexplainedAbsence", source.Where(x => x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance).Sum(e => e.Total));
                else
                    variableValues.AddIfNotExist(variable, source.Where(x => x.AttendanceWorkhabitName.Replace(" ", "") == variable && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance).Sum(e => e.Total)); 
            }

            return Math.Round(new CalculationEngine().Calculate(formula, variableValues), 2);
        }

        public static double CalculateAttendanceTerm(this string formula, AbsentTerm term, List<GetAttendanceCalculateRequest> source)
        {
            var variables = GetVariables(ref formula);

            var variableValues = new Dictionary<string, double>();
            foreach (var variable in variables)
            {
                if (variable == "TotalDay")
                    variableValues.AddIfNotExist("TotalDay",
                                                    term == AbsentTerm.Day
                                                    ? source.Where(x => x.AttendanceName == SummaryTermConstant.DefaultTotalDayName).Sum(e => e.Total)
                                                    : source.Where(x => x.AttendanceName == SummaryTermConstant.DefaultTotalSessionName).Sum(e => e.Total));
                else
                    variableValues.AddIfNotExist(variable, source.Where(x => x.AttendanceName.Replace(" ", "") == variable).Sum(e => e.Total));
            }

            return Math.Round(new CalculationEngine().Calculate(formula, variableValues), 2);
        }
        public static double Calculate(this string formula, AbsentTerm term, TrAttendanceSummary source)
        {
            var variables = GetVariables(ref formula);

            var variableValues = new Dictionary<string, double>();
            foreach (var variable in variables)
            {
                if (variable == "TotalDay")
                    variableValues.AddIfNotExist("TotalDay", term == AbsentTerm.Day ? source.TotalDays : source.TotalSession);
                else
                    variableValues.AddIfNotExist(variable, term == AbsentTerm.Day ?
                                                                   source.AttendanceSummaryMappingAtds.Where(y => y.Attendance.Description.Replace(" ", string.Empty) == variable).Select(z=> z.CountAsDay).FirstOrDefault() :
                                                                   source.AttendanceSummaryMappingAtds.Where(y => y.Attendance.Description.Replace(" ", string.Empty) == variable).Select(z => z.CountAsSession).FirstOrDefault());

            }

            return Math.Round(new CalculationEngine().Calculate(formula, variableValues), 2);
        }

        private static List<string> GetVariables(ref string formula)
        {
            // remove all {} and trim formula
            formula = formula.Replace("{", string.Empty);
            formula = formula.Replace("}", string.Empty);
            formula = formula.Replace(" ", string.Empty);

            // get all variables
            var variables = new List<string>();
            var operators = new List<char> { '+', '-', '*', '/', ')', '(', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var character = string.Empty;
            for (var i = 0; i < formula.Length; i++)
            {
                if (!operators.Contains(formula[i]))
                    character += formula[i];
                else
                {
                    if (!string.IsNullOrWhiteSpace(character))
                        variables.Add(character);
                    character = string.Empty;
                }
            }
            if (!string.IsNullOrWhiteSpace(character))
                variables.Add(character);

            return variables;
        }

        public static void AddIfNotExist(this Dictionary<string, double> source, string key, double value)
        {
            if (!source.ContainsKey(key))
                source.Add(key, value);
        }
    }
}
