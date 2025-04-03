using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Information;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentBySiblingGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        public GetStudentBySiblingGroupHandler(IStudentDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentRequest>(nameof(GetStudentRequest.IdStudent));

            var siblingGroupId = _dbContext.Entity<MsSiblingGroup>().Where(x => x.IdStudent == param.IdStudent).Select(x => x.Id).FirstOrDefault();
            var studentSiblingList = _dbContext.Entity<MsSiblingGroup>().Where(x => x.Id == siblingGroupId).Select(x => x.IdStudent).ToList();
            var watingApprovalList = _dbContext.Entity<TrStudentInfoUpdate>()
                                .Where(x => x.FieldName == "IdSiblingGroup" && x.IdApprovalStatus == _newEntryApproval)
                                .Where(x => x.OldFieldValue == siblingGroupId || x.CurrentFieldValue == siblingGroupId)
                                .Select(x => x.IdUser)
                                .ToList();

            var retVal = new List<GetStudentBySiblingGroupResult>();

            if (param.ForApproval == false)
            {
                retVal = await _dbContext.Entity<MsStudent>()
                    .Where(x => studentSiblingList.Contains(x.Id))
                    .Select(x => new GetStudentBySiblingGroupResult
                    {
                        Id = x.Id,
                        Description = "",
                        IdSiblingGroup = siblingGroupId,
                        nameInfo = new NameInfoVm
                        {
                            FirstName = x.LastName.Contains(" ") == true ? x.FirstName.Trim() + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName.Trim(),
                            LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                        },
                        Homeroom = x.MsHomeroomStudents.Count != 0
                            ? x.MsHomeroomStudents.OrderByDescending(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Code)
                                .Select(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description + " "
                                + y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code
                                + y.Homeroom.MsGradePathwayClassroom.Classroom.Code)
                                .FirstOrDefault()
                            : Localizer["-"]
                    })
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var siblingList = new List<string>();
                siblingList.AddRange(studentSiblingList);
                siblingList.AddRange(watingApprovalList);
                siblingList.Remove(param.IdStudent);
                siblingList = siblingList.Distinct().ToList();

                foreach (var sibling in siblingList)
                {
                    var siblingName = _dbContext.Entity<MsStudent>()
                        .Where(x => x.Id == sibling)
                        .Select(x => new
                        {
                            nameInfo = new NameInfoVm
                            {
                                FirstName = x.LastName.Contains(" ") == true ? x.FirstName.Trim() + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName.Trim(),
                                LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                            },
                            Homeroom = x.MsHomeroomStudents.Count != 0
                            ? x.MsHomeroomStudents.OrderByDescending(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Code)
                                .Select(y => y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description + " "
                                + y.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code
                                + y.Homeroom.MsGradePathwayClassroom.Classroom.Code)
                                .FirstOrDefault()
                            : Localizer["-"]
                        })
                        .FirstOrDefault();

                    var siblingStatus = _dbContext.Entity<TrStudentInfoUpdate>()
                                .Where(x => x.FieldName == "IdSiblingGroup" && x.IdApprovalStatus == _newEntryApproval && x.IdUser == sibling)
                                .Select(x => new
                                {
                                    Action = x.Constraint1Value
                                })
                                .FirstOrDefault();

                    var siblingData = new GetStudentBySiblingGroupResult();

                    siblingData.Id = sibling;
                    siblingData.nameInfo = siblingName == null ? null : siblingName.nameInfo;
                    siblingData.Homeroom = siblingName == null ? null : siblingName.Homeroom;
                    siblingData.Description = siblingStatus == null ? null : siblingStatus.Action;
                    siblingData.IdSiblingGroup = siblingGroupId;

                    retVal.Add(siblingData);
                }

            }

            return Request.CreateApiResult2(retVal as object);
        }
    }
}
