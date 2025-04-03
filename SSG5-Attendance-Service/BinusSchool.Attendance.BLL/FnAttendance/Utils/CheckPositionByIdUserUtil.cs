using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.Utils
{
    public static class CheckPositionByIdUserUtil
    {
        public static Dictionary<string, object> Principal(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.Principal || y.Id.ToLower() == "acop" || y.Id.ToLower() == "gc"))
            {
                if (positionResults.Where(y => y.Id == PositionConstant.Principal || y.Id.ToLower() == "acop" || y.Id.ToLower() == "gc").ToList() != null && positionResults.Where(y => y.Id == PositionConstant.Principal || y.Id.ToLower() == "acop" || y.Id.ToLower() == "gc").Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Id == PositionConstant.Principal || x.Id.ToLower() == "acop" || x.Id.ToLower() == "gc").ToList();
                    List<string> idLevel = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevel.Add(_levelLH.Id);
                    }
                    results.Add("idLevel", idLevel);
                }

            }
            return results;
        }

        public static Dictionary<string, object> VicePrincipal(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.VicePrincipal))
            {
                if (positionResults.Where(y => y.Id == PositionConstant.VicePrincipal).ToList() != null && positionResults.Where(y => y.Id == PositionConstant.VicePrincipal).Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Id == PositionConstant.VicePrincipal).ToList();
                    List<string> idLevel = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevel.Add(_levelLH.Id);
                    }
                    results.Add("idLevel", idLevel);
                }

            }
            return results;
        }

        public static Dictionary<string, object> AffectiveCoordinator(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.AffectiveCoordinator))
            {
                if (positionResults.Where(y => y.Id == PositionConstant.AffectiveCoordinator).ToList() != null && positionResults.Where(y => y.Id == PositionConstant.AffectiveCoordinator).Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Id == PositionConstant.AffectiveCoordinator).ToList();
                    List<string> idLevel = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevel.Add(_levelLH.Id);
                    }
                    results.Add("idLevel", idLevel);
                }

            }
            return results;
        }

        public static Dictionary<string, object> LevelHead(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Where(y => y.Id == PositionConstant.LevelHead).ToList() != null)
            {
                var LevelHead = positionResults.Where(x => x.Id == PositionConstant.LevelHead).ToList();
                List<string> idLevel = new List<string>();
                List<string> IdGrade = new List<string>();
                foreach (var item in LevelHead)
                {
                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewLH.TryGetValue("Level", out var _levelLH);
                    _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                    idLevel.Add(_levelLH.Id);
                    IdGrade.Add(_gradeLH.Id);
                }
                results.Add("idLevel", idLevel);
                results.Add("idGrade", IdGrade);
            }
            return results;
        }

        public static Dictionary<string, object> SubjectHead(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Where(y => y.Id == PositionConstant.SubjectHead).ToList() != null)
            {
                var LevelHead = positionResults.Where(x => x.Id == PositionConstant.SubjectHead).ToList();
                List<string> IdLevel = new List<string>();
                List<string> IdGrade = new List<string>();
                List<string> IdDepartment = new List<string>();
                List<string> IdSubject = new List<string>();
                foreach (var item in LevelHead)
                {
                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
                    _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                    _dataNewSH.TryGetValue("Department", out var _departmentSH);
                    _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                    IdLevel.Add(_leveltSH.Id);
                    IdDepartment.Add(_departmentSH.Id);
                    IdGrade.Add(_gradeSH.Id);
                    IdSubject.Add(_subjectSH.Id);
                }
                results.Add("idLevel", IdLevel);
                results.Add("idGrade", IdGrade);
                results.Add("idDepartment", IdSubject);
                results.Add("idSubject", IdSubject);
            }
            return results;
        }

        public static Dictionary<string, object> SubjectHeadAsisstant(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Where(y => y.Id == PositionConstant.SubjectHeadAssitant).ToList() != null)
            {
                var LevelHead = positionResults.Where(x => x.Id == PositionConstant.SubjectHeadAssitant).ToList();
                List<string> IdLevel = new List<string>();
                List<string> IdGrade = new List<string>();
                List<string> IdDepartment = new List<string>();
                List<string> IdSubject = new List<string>();
                foreach (var item in LevelHead)
                {
                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
                    _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                    _dataNewSH.TryGetValue("Department", out var _departmentSH);
                    _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                    IdLevel.Add(_leveltSH.Id);
                    IdDepartment.Add(_departmentSH.Id);
                    IdGrade.Add(_gradeSH.Id);
                    IdSubject.Add(_subjectSH.Id);
                }
                results.Add("idLevel", IdLevel);
                results.Add("idGrade", IdGrade);
                results.Add("idDepartment", IdSubject);
                results.Add("idSubject", IdSubject);
            }
            return results;
        }

        public static Dictionary<string, object> HeadOfDepartment(IReadOnlyList<OtherPositionByIdUserResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.HeadOfDepartment)) //check P Or VP
            {
                if (positionResults.Where(y => y.Id == PositionConstant.HeadOfDepartment).ToList() != null && positionResults.Where(y => y.Id == PositionConstant.HeadOfDepartment).Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Id == PositionConstant.HeadOfDepartment).ToList();
                    List<string> idDepartment = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewHOD = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewHOD.TryGetValue("Department", out var _departmentSH);
                        idDepartment.Add(_departmentSH.Id);
                    }
                    results.Add("idDepartment", idDepartment);
                }

            }
            return results;
        }
    }
}
