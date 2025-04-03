using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using System.Linq.Expressions;
using BinusSchool.Student.FnStudent.MasterSearching.Validator;
using System.Reflection;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetMasterSearchingDataTableHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetMasterSearchingDataTableHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetMasterSearchingDataTableRequest, GetMasterSearchingDataTableValidator>();

            var result = await GetMasterSearchingDataTable(new GetMasterSearchingDataTableRequest
            {
                IdSchool = param.IdSchool,
                SchoolName = param.SchoolName,
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                IdGrade = param.IdGrade,
                IdHomeroom = param.IdHomeroom,
                IdLevel = param.IdLevel,
                IdStudentStatus = param.IdStudentStatus,
                FieldData = param.FieldData,
                SearchByFieldData = param.SearchByFieldData,
                Keyword = param.Keyword
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<GetMasterSearchingDataTableResult> GetMasterSearchingDataTable(GetMasterSearchingDataTableRequest param)
        {
            var predicateStudent = PredicateBuilder.True<MsHomeroomStudent>();

            if (param.IdAcademicYear != null)
                predicateStudent = predicateStudent.And(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            if (param.Semester != 0)
                predicateStudent = predicateStudent.And(x => x.Semester == param.Semester);
            if (param.IdLevel != null)
                predicateStudent = predicateStudent.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            if (param.IdGrade != null)
                predicateStudent = predicateStudent.And(x => x.Homeroom.Grade.Id == param.IdGrade);
            if (param.IdHomeroom != null)
                predicateStudent = predicateStudent.And(x => x.IdHomeroom == param.IdHomeroom);
            //if (!string.IsNullOrWhiteSpace(param.Keyword))
            //{
            //    if (param.SearchByFieldData == null)
            //    {
            //        predicateStudent = predicateStudent.And(x
            //        => EF.Functions.Like(x.Student.FirstName, $"%{param.Keyword}%")
            //        || EF.Functions.Like(x.Student.MiddleName, $"%{param.Keyword}%")
            //        || EF.Functions.Like(x.Student.LastName, $"%{param.Keyword}%")
            //        || EF.Functions.Like(x.IdStudent, $"%{param.Keyword}%")
            //        );
            //    }
            //}

            //Get Field Student Data
            var getFieldStudent = await _dbContext.Entity<MsProfileDataField>()
                .Where(x => param.FieldData.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var getFieldFieldDataProfile = getFieldStudent.OrderBy(x => x.IdProfileDataFieldGroup).ThenBy(x => x.OrderNumber).ToList();

            var getStudentList = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Student)
                    .ThenInclude(x => x.AdmissionData)
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                    .ThenInclude(x => x.MsLevel)
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.MsGradePathwayClassroom)
                    .ThenInclude(x => x.Classroom)
                .Where(predicateStudent)
                .Join(_dbContext.Entity<MsSiblingGroup>(),
                    hs => hs.IdStudent,
                    sg => sg.IdStudent,
                    (hs, sg) => new { hs, sg })
                .ToListAsync(CancellationToken);

            var getStudentStatusList = getStudentList.GroupJoin(
                    _dbContext.Entity<TrStudentStatus>()
                    .Include(x => x.StudentStatus)
                    //.Where(x => x.CurrentStatus.Trim().ToUpper() == "A")
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                    .OrderByDescending(x => x.StartDate)
                        .ThenByDescending(x => x.DateIn),  //inner sequence
                    student => student.hs.IdStudent,
                    status => status.IdStudent,
                    (student, statusGroup) => new
                    {
                        IdStudent = student.hs.IdStudent,
                        StudentName = NameUtil.GenerateFullName(student.hs.Student?.FirstName, student.hs.Student?.LastName),
                        Homeroom = student.hs.Homeroom?.Grade?.Code + " " + student.hs.Homeroom?.MsGradePathwayClassroom?.Classroom?.Code,
                        YearLevel = student.hs.Homeroom?.Grade?.MsLevel?.Code,
                        Grade = student.hs.Homeroom?.Grade?.Description,
                        StudentStatus = statusGroup.Select(x => x.StudentStatus?.LongDesc).FirstOrDefault(),
                        IdStudentStatus = statusGroup.Select(x => x.IdStudentStatus).FirstOrDefault(),
                        IdSiblingGroup = student.sg.Id,
                        AdmissionData = student.hs.Student?.AdmissionData?.JoinToSchoolDate
                    }).ToList();

            getStudentStatusList = getStudentStatusList
                    .Where(x => x.IdStudentStatus == (param.IdStudentStatus == null ? x.IdStudentStatus : param.IdStudentStatus))
                    .OrderBy(x => x.Grade.Length)
                    .ThenBy(x => x.Grade)
                    .ThenBy(x => x.Homeroom.Length)
                    .ThenBy(x => x.Homeroom)
                    .ThenBy(x => x.StudentName)
                    .ToList();

            var getStudentParentList = await _dbContext.Entity<MsStudentParent>()
                .Include(x => x.Student)
                    .ThenInclude(x => x.Religion)
                .Include(x => x.Student)
                    .ThenInclude(x => x.ReligionSubject)
                .Include(x => x.Student)
                    .ThenInclude(x => x.Country)
                //    .ThenInclude(x => x.Province)
                //.Include(x => x.Student)
                //    .ThenInclude(x => x.Country)
                //    .ThenInclude(x => x.City)
                .Include(x => x.Student.StudentPrevSchoolInfo.PreviousSchoolOld)
                .Include(x => x.Student.StudentPrevSchoolInfo.PreviousSchoolNew)
                .Include(x => x.Parent)
                    .ThenInclude(x => x.Nationality)
                .Include(x => x.Parent)
                    .ThenInclude(x => x.Religion)
                .Where(x => getStudentList.Select(x => x.hs.IdStudent).Any(y => y == x.IdStudent))
                .ToListAsync(CancellationToken);

            #region Check Filter Photo
            var resultStudentPhotoData = new List<GetMasterSearchingDataTableResult_Photo>();
            var idx = getFieldFieldDataProfile.FindIndex(x => x.FieldDataProfile.Trim().ToLower() == "photo");
            if (idx != 0)
            {
                var getStudentPhoto = _dbContext.Entity<TrStudentPhoto>()
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                    .Where(x => getStudentList.Select(x => x.hs.IdStudent).Distinct().Any(y => y == x.IdStudent))
                    .ToList();

                resultStudentPhotoData = getStudentList
                    .GroupJoin(
                          getStudentPhoto,
                          std => new { std.hs.IdStudent, std.hs.Homeroom.Grade.MsLevel.IdAcademicYear },
                          pth => new { pth.IdStudent, pth.IdAcademicYear },
                          (std, pth) => new { std, pth }
                    )
                    .SelectMany(
                          x => x.pth.DefaultIfEmpty(),
                          (Student, Photo) => new GetMasterSearchingDataTableResult_Photo
                          {
                              IdBinusian = Student.std.hs.Student.IdBinusian,
                              Student = Student.std.hs.IdStudent,
                              StudentName = NameUtil.GenerateFullName(Student.std.hs.Student.FirstName, Student.std.hs.Student.LastName),
                              AcademicYear = Student.std.hs.Homeroom.Grade.MsLevel.IdAcademicYear,
                              IdPhoto = Photo == null ? "NA" : Photo.Id,
                              FileName = Photo == null ? "NA" : Photo.FileName,
                              FilePath = Photo == null ? "NA" : Photo.FilePath,
                          }
                    ).Distinct().ToList();
            }

            if (idx > 0)
            {
                var item = getFieldFieldDataProfile[idx];
                getFieldFieldDataProfile.RemoveAt(idx);
                getFieldFieldDataProfile.Insert(0, item);
            }
            #endregion

            var result = new GetMasterSearchingDataTableResult();
            var getColumnHeader = new List<string>();

            if (getStudentParentList.Count() > 0)
            {
                var Result_Body = new List<GetMasterSearchingDataTableResult_BodyVm>();

                var getCityList = await _dbContext.Entity<LtCity>()
                    .ToListAsync(CancellationToken);

                var getProvinceList = await _dbContext.Entity<LtProvince>()
                    .ToListAsync(CancellationToken);

                var getCountryList = await _dbContext.Entity<LtCountry>()
                    .ToListAsync(CancellationToken);

                foreach (var fieldStudent in getFieldFieldDataProfile)
                    getColumnHeader.Add(fieldStudent.FieldDataProfile);

                foreach (var DataStudent in getStudentStatusList)
                {
                    var retDataStudent = new GetMasterSearchingDataTableResult_Table();
                    var dataTable_Body = new GetMasterSearchingDataTableResult_BodyVm();
                    var getFieldDataProfileBody = new List<string>();

                    var dataStudent = getStudentParentList
                            .Where(x => x.IdStudent == DataStudent.IdStudent)
                            .ToList();

                    var isSearchByFieldDataFound = false;

                    foreach (var fieldStudent in getFieldFieldDataProfile)
                    {
                        var checkFieldIsSearch = (param.SearchByFieldData == null && (!string.IsNullOrWhiteSpace(param.Keyword))) ? true : (param.SearchByFieldData?.ToString() == fieldStudent.Id ? true : false);
                        var flowTableData = "";

                        if (fieldStudent.FieldDataProfile.Trim().ToLower() == "name")
                        {
                            var getcolumnName = new List<string>();

                            if (fieldStudent.Id.Substring(0, 1) == "1")
                            {
                                getcolumnName.Add("Student.FirstName:FirstName");
                                getcolumnName.Add("Student.LastName:LastName");
                            }
                            else
                            {
                                getcolumnName.Add("Parent.FirstName:FirstName");
                                getcolumnName.Add("Parent.LastName:LastName");
                            }

                            string[] getcolumnNameArray = getcolumnName.ToArray();
                            var dynamicSelectName = DynamicSelectGenerator<MsStudentParent, GetMasterSearchingDataTableResult_Table>(getcolumnNameArray);

                            retDataStudent = dataStudent.AsQueryable()
                                .Select(dynamicSelectName)
                                .FirstOrDefault();

                            if (fieldStudent.IdProfileDataFieldGroup == "1")
                            {
                                retDataStudent = dataStudent.AsQueryable().Select(dynamicSelectName).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "2")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "F").AsQueryable().Select(dynamicSelectName).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "3")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "M").AsQueryable().Select(dynamicSelectName).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "4")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "G").AsQueryable().Select(dynamicSelectName).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }

                            var dataPropFirstName = retDataStudent?.GetType().GetProperty("FirstName");
                            var dataValFirstName = dataPropFirstName?.GetValue(retDataStudent, null);

                            var dataPropLastName = retDataStudent?.GetType().GetProperty("LastName");
                            var dataValLastName = dataPropLastName?.GetValue(retDataStudent, null);

                            var Name = NameUtil.GenerateFullName(dataValFirstName == null ? "" : dataValFirstName.ToString(), dataValLastName == null ? "" : dataValLastName.ToString());

                            getFieldDataProfileBody.Add(Name == null ? "" : Name.ToString());
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "photo")
                        {
                            var getStudentPhotoExist = resultStudentPhotoData
                                    .Where(x => x.Student == DataStudent.IdStudent)
                                    .Where(x => x.AcademicYear == param.IdAcademicYear)
                                    .FirstOrDefault();
                            //if (getStudentPhotoExist != null && !string.IsNullOrEmpty(getStudentPhotoExist.FilePath))
                            if (getStudentPhotoExist != null && getStudentPhotoExist.FilePath != "NA")
                            {
                                dataTable_Body.IsPhotoExist = true;
                                dataTable_Body.FilePathPhoto = getStudentPhotoExist.FilePath;
                                getFieldDataProfileBody.Add("<div style=\"position: relative; width: 6rem; height: 6rem; border-radius:50%; overflow: hidden; \"><img style=\"position: absolute; top: 0; left: 0; bottom: 0; right: 0; width: 100 %; \" src=\"" + getStudentPhotoExist.FilePath + "\" /> </div> ");
                            }
                            else
                            {
                                dataTable_Body.FilePathPhoto = "https://e-desk.binus.sch.id/assets/img/user.jpg";
                                getFieldDataProfileBody.Add("<div style=\"position: relative; width: 6rem; height: 6rem; border-radius:50%; overflow: hidden; \"><img style=\"position: absolute; top: 0; left: 0; bottom: 0; right: 0; width: 100 %; \" src=\"" + "../assets/img/user.jpg" + "\" /> </div> ");
                            }

                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "cityname")
                        {
                            var addressCity = getCityList
                                .Where(x => x.Id == (dataStudent.Select(y => y.Student.IdAddressCity).FirstOrDefault() ?? "")
                                && x.IdProvince == (dataStudent.Select(y => y.Student.IdAddressStateProvince).FirstOrDefault() ?? "")
                                && x.IdCountry == (dataStudent.Select(y => y.Student.IdCountry).FirstOrDefault() ?? ""))
                                .FirstOrDefault();

                            if (addressCity != null)
                            {
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);

                                var dataProp = retDataStudent?.GetType().GetProperty("CityName");
                                var dataVal = dataProp?.GetValue(retDataStudent, null);

                                getFieldDataProfileBody.Add(addressCity.CityName);
                            }
                            else
                            {
                                getFieldDataProfileBody.Add("");
                            }
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "provincename")
                        {
                            var addressProvince = getProvinceList
                                .Where(x => x.Id == (dataStudent.Select(y => y.Student.IdAddressStateProvince).FirstOrDefault() ?? "")
                                && x.IdCountry == (dataStudent.Select(y => y.Student.IdCountry).FirstOrDefault() ?? ""))
                                .FirstOrDefault();

                            if (addressProvince != null)
                            {
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);

                                var dataProp = retDataStudent?.GetType().GetProperty("ProvinceName");
                                var dataVal = dataProp?.GetValue(retDataStudent, null);

                                getFieldDataProfileBody.Add(addressProvince.ProvinceName);
                            }
                            else
                            {
                                getFieldDataProfileBody.Add("");
                            }
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "countryname")
                        {
                            var addressCountry = getCountryList
                                .Where(x => x.Id == (dataStudent.Select(y => y.Student.IdCountry).FirstOrDefault() ?? ""))
                                .FirstOrDefault();

                            if (addressCountry != null)
                            {
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);

                                var dataProp = retDataStudent?.GetType().GetProperty("CountryName");
                                var dataVal = dataProp?.GetValue(retDataStudent, null);

                                getFieldDataProfileBody.Add(addressCountry.CountryName);
                            }
                            else
                            {
                                getFieldDataProfileBody.Add("");
                            }
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "homeroomclass")
                        {
                            getFieldDataProfileBody.Add(DataStudent.Homeroom);
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "studentstatus")
                        {
                            getFieldDataProfileBody.Add(DataStudent.StudentStatus);
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "yearlevel")
                        {
                            getFieldDataProfileBody.Add(DataStudent.YearLevel);
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "siblingid")
                        {
                            getFieldDataProfileBody.Add(DataStudent.IdSiblingGroup);
                        }
                        else if (fieldStudent.FieldDataProfile.Trim().ToLower() == "jointoschooldate")
                        {
                            getFieldDataProfileBody.Add(DataStudent.AdmissionData != null ? DataStudent.AdmissionData.Value.ToString("dd MMMM yyyy") : "");
                        }
                        else
                        {
                            flowTableData = fieldStudent.FlowTable;

                            var dynamicSelect = DynamicSelectGenerator<MsStudentParent, GetMasterSearchingDataTableResult_Table>(flowTableData + ":" + fieldStudent.FieldDataProfile);


                            if (fieldStudent.IdProfileDataFieldGroup == "1")
                            {
                                retDataStudent = dataStudent.AsQueryable().Select(dynamicSelect).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "2")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "F").AsQueryable().Select(dynamicSelect).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "3")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "M").AsQueryable().Select(dynamicSelect).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }
                            else if (fieldStudent.IdProfileDataFieldGroup == "4")
                            {
                                retDataStudent = dataStudent.Where(x => x.Parent?.IdParentRole == "G").AsQueryable().Select(dynamicSelect).FirstOrDefault();
                                var dataPropFieldStudent = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);
                                if (!isSearchByFieldDataFound && checkFieldIsSearch == true)
                                    isSearchByFieldDataFound = SetIsSearchByFieldDataFound(param.Keyword, param.SearchByFieldData, dataPropFieldStudent, retDataStudent, fieldStudent.FieldDataProfile);
                            }

                            var dataProp = retDataStudent?.GetType().GetProperty(fieldStudent.FieldDataProfile);

                            var isDateTime = false;
                            DateTime? dataDateTime = null;
                            if (dataProp != null && dataProp.PropertyType == typeof(DateTime?))
                            {
                                isDateTime = true;
                                dataDateTime = (DateTime?)dataProp?.GetValue(retDataStudent, null);
                            }

                            var dataVal = dataProp?.GetValue(retDataStudent, null);

                            getFieldDataProfileBody.Add(dataVal == null ? "" : (isDateTime == false ? dataVal.ToString() : dataDateTime?.ToString("dd MMMM yyyy")));
                        }
                    }

                    dataTable_Body.Student = new NameValueVm
                    {
                        Id = DataStudent.IdStudent,
                        Name = DataStudent.StudentName
                    };

                    if (param.SearchByFieldData != null)
                    {
                        if (isSearchByFieldDataFound)
                        {
                            dataTable_Body.FieldData = getFieldDataProfileBody;
                            Result_Body.Add(dataTable_Body);
                        }
                    }
                    else
                    {
                        dataTable_Body.FieldData = getFieldDataProfileBody;
                        Result_Body.Add(dataTable_Body);
                    }
                }
                result.Result_Head = getFieldFieldDataProfile.Select(x => x.AliasName).ToList();
                result.Result_Body = Result_Body;

                return result;
            }
            else
            {
                foreach (var fieldStudent in getFieldFieldDataProfile)
                    getColumnHeader.Add(fieldStudent.FieldDataProfile);

                result.Result_Head = getFieldFieldDataProfile.Select(x => x.AliasName).ToList();

                return result;
            }
        }

        private bool SetIsSearchByFieldDataFound(string Keyword, string SearchByFieldData, PropertyInfo dataPropFieldStudent, GetMasterSearchingDataTableResult_Table retDataStudent, string fieldStudent)
        {
            var isSearchByFieldDataFound = false;
            if (/*SearchByFieldData != null && */SearchByFieldData != null && (dataPropFieldStudent != null && dataPropFieldStudent.PropertyType == typeof(string)) && retDataStudent != null)
            {
                if (fieldStudent.Trim().ToLower() == "name")
                {
                    var dataPropFirstName = retDataStudent?.GetType().GetProperty("FirstName");
                    var dataValFirstName = dataPropFirstName?.GetValue(retDataStudent, null);

                    var dataPropLastName = retDataStudent?.GetType().GetProperty("LastName");
                    var dataValLastName = dataPropLastName?.GetValue(retDataStudent, null);

                    var Name = NameUtil.GenerateFullName(dataValFirstName == null ? "" : dataValFirstName.ToString(), dataValLastName == null ? "" : dataValLastName.ToString());

                    if (Name != null)
                        isSearchByFieldDataFound = Name.ToLower().Contains(Keyword.ToLower());
                }
                else if (fieldStudent.Trim().ToLower() == "cityname")
                {
                    var dataProp = retDataStudent?.GetType().GetProperty("CityName");
                    var dataVal = dataProp?.GetValue(retDataStudent, null);

                    var cityname = dataVal == null ? "" : dataVal.ToString();

                    if (cityname != null)
                        isSearchByFieldDataFound = cityname.ToLower().Contains(Keyword.ToLower());
                }
                else if (fieldStudent.Trim().ToLower() == "provincename")
                {
                    var dataProp = retDataStudent?.GetType().GetProperty("provincename");
                    var dataVal = dataProp?.GetValue(retDataStudent, null);

                    var provincename = dataVal == null ? "" : dataVal.ToString();

                    if (provincename != null)
                        isSearchByFieldDataFound = provincename.ToLower().Contains(Keyword.ToLower());
                }
                else if (fieldStudent.Trim().ToLower() == "countryname")
                {
                    var dataProp = retDataStudent?.GetType().GetProperty("countryname");
                    var dataVal = dataProp?.GetValue(retDataStudent, null);

                    var countryname = dataVal == null ? "" : dataVal.ToString();

                    if (countryname != null)
                        isSearchByFieldDataFound = countryname.ToLower().Contains(Keyword.ToLower());
                }
                else
                {
                    var searchByFieldData = dataPropFieldStudent?.GetValue(retDataStudent, null);
                    var dataVal = searchByFieldData == null ? "" : searchByFieldData.ToString();
                    if (dataVal != null)
                        isSearchByFieldDataFound = dataVal.ToLower().Contains(Keyword.ToLower());
                }
            }
            return isSearchByFieldDataFound;
        }

        public static Expression<Func<T, TSelect>> DynamicSelectGenerator<T, TSelect>(params string[] Fields)
        {
            string[] EntityFields = Fields;
            if (Fields == null || Fields.Length == 0)
                // get Properties of the T
                EntityFields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray();

            // input parameter "x"
            var xParameter = Expression.Parameter(typeof(T), "x");

            // new statement "new Data()"
            var xNew = Expression.New(typeof(TSelect));

            // create initializers
            var bindings = EntityFields
                .Select(x =>
                {
                    string[] xFieldAlias = x.Split(":");
                    string field = xFieldAlias[0];

                    string[] fieldSplit = field.Split(".");
                    if (fieldSplit.Length > 1)
                    {
                        // original value "x.Nested.Field1"
                        Expression exp = xParameter;
                        foreach (string item in fieldSplit)
                            exp = Expression.PropertyOrField(exp, item);

                        // property "Field1"
                        PropertyInfo member2 = null;
                        if (xFieldAlias.Length > 1)
                            member2 = typeof(TSelect).GetProperty(xFieldAlias[1]);
                        else
                            member2 = typeof(T).GetProperty(fieldSplit[fieldSplit.Length - 1]);

                        // set value "Field1 = x.Nested.Field1"
                        var res = Expression.Bind(member2, exp);
                        return res;
                    }
                    // property "Field1"
                    var mi = typeof(T).GetProperty(field);
                    PropertyInfo member;
                    if (xFieldAlias.Length > 1)
                        member = typeof(TSelect).GetProperty(xFieldAlias[1]);
                    else member = typeof(TSelect).GetProperty(field);

                    // original value "x.Field1"
                    var xOriginal = Expression.Property(xParameter, mi);

                    // set value "Field1 = x.Field1"
                    return Expression.Bind(member, xOriginal);
                }
            );

            // initialization "new Data { Field1 = x.Field1, Field2 = x.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "x => new Data { Field1 = x.Field1, Field2 = x.Field2 }"
            var lambda = Expression.Lambda<Func<T, TSelect>>(xInit, xParameter);

            return lambda;
        }
    }
}
