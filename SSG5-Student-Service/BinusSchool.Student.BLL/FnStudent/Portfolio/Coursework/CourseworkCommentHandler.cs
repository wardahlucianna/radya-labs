using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class CourseworCommentHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public CourseworCommentHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        private string GetTeacherName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.FirstName + " " + dataTeacher.LastName;
        }

        private string GetSeenByName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsUser>().Where(x => x.Id == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.DisplayName;
        }

        private string GetDisplaName(string idBinusian)
        {
            var dataTeacher = _dbContext.Entity<MsUser>().Where(x => x.Id == idBinusian).First();
            if(dataTeacher == null) return "-";

            return dataTeacher.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<TrCourseworkAnecdotalStudentComment>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _dbContext.Entity<TrCourseworkAnecdotalStudentComment>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddCourseworkCommentRequest, AddCourseworkCommentValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);


            var CourseworkAnecdotalComment = new TrCourseworkAnecdotalStudentComment
            {
                Id = Guid.NewGuid().ToString(),
                IdCourseworkAnecdotalStudent = body.IdCourseworkAnecdotalStudent,
                IdUserComment = body.IdUser,
                Comment = body.Comment
            };

            _dbContext.Entity<TrCourseworkAnecdotalStudentComment>().Add(CourseworkAnecdotalComment);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            await ProcessEmailAndNotifScenario3(body, CourseworkAnecdotalComment.Id);

            await ProcessEmailAndNotifScenario4(body, CourseworkAnecdotalComment.Id);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateCourseworkCommentRequest, UpdateCourseworkCommentValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<TrCourseworkAnecdotalStudentComment>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id"], "Id", body.Id));

            data.Comment = body.Comment;

            _dbContext.Entity<TrCourseworkAnecdotalStudentComment>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task ProcessEmailAndNotifScenario3(AddCourseworkCommentRequest param, string IdCourseworkAnecdotalComment)
        {
            var User = await _dbContext.Entity<MsUser>()
            .Select(x => new {
                x.Id,
                x.DisplayName
            } 
            )
            .ToListAsync(CancellationToken);

            var Staff = await _dbContext.Entity<MsStaff>()
                    .Select(x => new {
                        x.IdBinusian,
                    }
                    )
                    .ToListAsync(CancellationToken);

            var dataReceipment = await _dbContext.Entity<TrCourseworkAnecdotalStudentComment>()
                      .Include(x => x.CourseworkAnecdotalStudent).ThenInclude(x => x.Student)
                      .Include(x => x.User)
                      .Where(x => x.IdCourseworkAnecdotalStudent == param.IdCourseworkAnecdotalStudent && x.IdUserComment != param.IdUser)
                      .Select(x => new EmailCommentPortofolioResult
                      {
                          IdReceive = x.IdUserComment,
                          ReceiveName = x.User.DisplayName
                      })
                      .ToListAsync();

            var dataComment = await _dbContext.Entity<TrCourseworkAnecdotalStudentComment>()
                              .Include(x => x.CourseworkAnecdotalStudent).ThenInclude(x=> x.Student)
                              .Include(x => x.User)
                              .Where(x => x.Id == IdCourseworkAnecdotalComment)
                              .Select(x=> new EmailCommentPortofolioResult
                              {
                                  //IdReceive = x.IdUserComment,
                                  //ReceiveName = x.User.DisplayName,
                                  IdAcademicYear = x.CourseworkAnecdotalStudent.IdAcademicYear,
                                  IdStudent = x.CourseworkAnecdotalStudent.IdStudent,
                                  Semester = x.CourseworkAnecdotalStudent.Semester,
                                  UserNameComment = x.User.DisplayName,
                                  IdUserCreate = x.CourseworkAnecdotalStudent.UserIn,
                                  //UserNameCreate =  User.Where(y=> y.Id == x.CourseworkAnecdotalStudent.UserIn).Select(y=>y.DisplayName).FirstOrDefault(),
                                  TabMenu = x.CourseworkAnecdotalStudent.Type == 0 ? "Coursework" : "Anecdotal Records",
                                  TabMenuIdx = "Coursework",
                                  Date = x.DateIn.Value,
                                  SchoolName = x.CourseworkAnecdotalStudent.Student.IdSchool,
                                  Comment = x.Comment
                              })
                              .FirstOrDefaultAsync();

            foreach (var item in dataReceipment)
            {
                item.IdAcademicYear = dataComment.IdAcademicYear;
                item.IdStudent = dataComment.IdStudent;
                item.Semester = dataComment.Semester;
                item.UserNameComment = dataComment.UserNameComment;
                item.IdUserCreate = dataComment.IdUserCreate;
                item.TabMenu = dataComment.TabMenu;
                item.TabMenuIdx = dataComment.TabMenuIdx;
                item.Date = dataComment.Date;
                item.SchoolName = dataComment.SchoolName;
                item.Comment = dataComment.Comment;
            }


            if (dataReceipment.Count ==0)
            {
                return;
            }

            foreach (var item in dataReceipment)
            {
                item.UserNameCreate = User.Where(y => y.Id == item.IdUserCreate).Select(y => y.DisplayName).FirstOrDefault();
            }

            var ListRecipients = dataReceipment.Where(x=>x.IdReceive != dataReceipment.First().IdUserCreate).Select(x => new CodeWithIdVm { 
                Id = x.IdReceive,
                Code = x.ReceiveName
            } ).ToList();

            if (ListRecipients.Any(x=> x.Code == "Parent"))
            {
                var GetData = ListRecipients.Where(x => x.Code == "Parent").ToList();

                foreach (var item in GetData)
                {
                    var getParent = item.Code.Split("P");

                    if (getParent.Length > 0)
                    {
                        var Parent = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                            .Where(x => x.IdStudent == getParent[1])
                            .Select(y => new DataParentCourseworkAnecdotal
                            {
                                EmailParent = y.Parent != null ? y.Parent.PersonalEmailAddress : "",
                                ParentName = y.Parent != null ? !string.IsNullOrEmpty(y.Parent.FirstName) ? y.Parent.FirstName : y.Parent.LastName : "",
                            })
                            .ToListAsync(CancellationToken);

                        foreach(var resultParent in dataReceipment)
                        {
                            if (resultParent.IdReceive == item.Id)
                            {
                                resultParent.DataParents = Parent;
                                resultParent.ForWho = "Parent";
                            }
                        }
                    }
                }
            }

            foreach (var result in dataReceipment)
            {
                if (Staff.Any(x=> x.IdBinusian == result.IdReceive))
                {
                    result.ForWho = "Teacher";
                    result.TabMenuIdx = result.TabMenu;
                }
                else
                {
                    if (string.IsNullOrEmpty(result.ForWho))
                    {
                        result.ForWho = "Student";
                    }
                }
            }

            var RecipientPosted = dataReceipment.Select(x => x.IdUserCreate).FirstOrDefault();
            
            if (dataReceipment != null)
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                if (KeyValues.ContainsKey("GetDataCourseworkAnecdotalCommentEmail"))
                {
                    KeyValues.Remove("GetDataCourseworkAnecdotalCommentEmail");
                }

                paramTemplateNotification.Add("GetDataCourseworkAnecdotalCommentEmail", dataReceipment);

                foreach (var item in ListRecipients)
                {
                    IEnumerable<string> IdRecipients = new string[] { item.Id };

                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "PAR3")
                        {
                            IdRecipients = ListRecipients.Select(x => x.Id).ToList(),
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }


        private async Task ProcessEmailAndNotifScenario4(AddCourseworkCommentRequest param, string IdCourseworkAnecdotalComment)
        {
            var User = await _dbContext.Entity<MsUser>()
            .Select(x => new {
                x.Id,
                x.DisplayName
            }
            )
            .ToListAsync(CancellationToken);

            var Staff = await _dbContext.Entity<MsStaff>()
                .Select(x => new {
                    x.IdBinusian,
                }
                )
                .ToListAsync(CancellationToken);

            var dataReceipment = await _dbContext.Entity<TrCourseworkAnecdotalStudentComment>()
                              .Include(x => x.CourseworkAnecdotalStudent).ThenInclude(x => x.Student)
                              .Include(x => x.User)
                              .Where(x => x.Id == IdCourseworkAnecdotalComment)
                              .Select(x => new EmailCommentPortofolioResult
                              {
                                  //IdReceive = x.IdUserComment,
                                  //ReceiveName = x.User.DisplayName,
                                  IdAcademicYear = x.CourseworkAnecdotalStudent.IdAcademicYear,
                                  IdStudent = x.CourseworkAnecdotalStudent.IdStudent,
                                  Semester = x.CourseworkAnecdotalStudent.Semester,
                                  UserNameComment = x.User.DisplayName,
                                  IdUserCreate = x.CourseworkAnecdotalStudent.UserIn,
                                  //UserNameCreate =  User.Where(y=> y.Id == x.CourseworkAnecdotalStudent.UserIn).Select(y=>y.DisplayName).FirstOrDefault(),
                                  TabMenu = x.CourseworkAnecdotalStudent.Type == 0 ? "Coursework" : "Anecdotal Records",
                                  TabMenuIdx = "Coursework",
                                  Date = x.DateIn.Value,
                                  SchoolName = x.CourseworkAnecdotalStudent.Student.IdSchool,
                                  Comment = x.Comment
                              })
                              .ToListAsync();

            if (dataReceipment == null)
            {
                return;
            }

            foreach (var item in dataReceipment)
            {
                item.UserNameCreate = User.Where(y => y.Id == item.IdUserCreate).Select(y => y.DisplayName).FirstOrDefault();
            }


            var ListRecipients = dataReceipment.Select(x => new CodeWithIdVm
            {
                Id = x.IdUserCreate,
                Code = x.UserNameCreate
            }).ToList();

            foreach (var result in dataReceipment)
            {
                if (Staff.Any(x => x.IdBinusian == result.IdUserCreate))
                {
                    result.ForWho = "Teacher";
                    result.TabMenuIdx = result.TabMenu;
                }
                else
                {
                    if (string.IsNullOrEmpty(result.ForWho))
                    {
                        result.ForWho = "Student";
                    }
                }
            }

            if (dataReceipment != null)
            {
                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                if (KeyValues.ContainsKey("GetDataCourseworkAnecdotalCommentEmail"))
                {
                    KeyValues.Remove("GetDataCourseworkAnecdotalCommentEmail");
                }

                paramTemplateNotification.Add("GetDataCourseworkAnecdotalCommentEmail", dataReceipment);

                foreach (var item in ListRecipients)
                {
                    IEnumerable<string> IdRecipients = new string[] { item.Id };

                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "PAR4")
                        {
                            IdRecipients = ListRecipients.Select(x => x.Id).ToList(),
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }
    }
}
