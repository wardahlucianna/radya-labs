using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnExtracurricular.Utils
{
    public static class CheckPositionUserElectiveUtil
    {
        /// <summary>
        /// get detail data from position principal or vice principal 
        /// </summary>
        /// <param name="positionResults">meta data position position principal or vice principal from user</param>
        /// <returns>Dictionary string object with key IdLevel and object can be converted to list of string</returns>
        public static Dictionary<string, object> PrincipalAndVicePrincipal(IReadOnlyList<OtherPositionUserElectiveResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.Principal)) //check P Or VP
            {
                if (positionResults.Where(y => y.Id == PositionConstant.Principal).ToList() != null && positionResults.Where(y => y.Id == PositionConstant.Principal).Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Id == PositionConstant.Principal).ToList();
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
        /// <summary>
        /// get detail data from position principal or vice principal 
        /// </summary>
        /// <param name="positionResults">meta data position position principal or vice principal from user</param>
        /// <returns>Dictionary string object with key IdLevel and IdGrade and each object can be converted to list of string</returns>
        public static Dictionary<string, object> LevelHead(IReadOnlyList<OtherPositionUserElectiveResult> positionResults)
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
        /// <summary>
        /// get detail data from subject head
        /// </summary>
        /// <param name="positionResults">meta data position subject head or subject head assistant from user</param>
        /// <returns>Dictionary string object with key IdLevel,IdGrade,IdDepartment and IdSubject and each object can be converted to list of string</returns>
        public static Dictionary<string, object> SubjectHeadAndSubjectHeadAsisstant(IReadOnlyList<OtherPositionUserElectiveResult> positionResults)
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
        /// <summary>
        /// get detail data from position head of department
        /// </summary>
        /// <param name="positionResults">meta data position head of department from user</param>
        /// <returns>Dictionary string object with key IdDepartment and object can be converted to list of string</returns>
        public static Dictionary<string, object> HeadOfDepartment(IReadOnlyList<OtherPositionUserElectiveResult> positionResults)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            if (positionResults.Any(y => y.Id == PositionConstant.HeadOfDepartment)) //check P Or VP
            {
                if (positionResults.Where(y => y.Id == PositionConstant.HeadOfDepartment).ToList() != null && positionResults.Where(y => y.Id == PositionConstant.HeadOfDepartment).Count() > 0)
                {
                    var Principal = positionResults.Where(x => x.Code == PositionConstant.HeadOfDepartment).ToList();
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
