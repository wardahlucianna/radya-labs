using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.Data.Model.Util.FnConverter.ServiceAsActionToPdf;
using BinusSchool.Util.FnConverter.ServiceAsActionToPdf.Validator;
using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using NPOI.SS.Formula.Functions;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.ServiceAsActionToPdf
{
    public class ConvertServiceAsActionToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly ISchool _schoolService;
        private readonly IStudent _studentService;
        private readonly IStorageManager _storageManager;
        private readonly IServiceAsAction _serviceAsAction;
        private readonly IMasterServiceAsAction _masterServiceAsAction;

        public ConvertServiceAsActionToPdfHandler
        (
            IConverter converter,
            IEasyCachingProvider inMemoryCache,
            ISchool schoolService,
            IStorageManager storageManager,
            IServiceAsAction serviceAsAction,
            IMasterServiceAsAction masterServiceAsAction,
            IStudent studentService
        )
        {
            _converter = converter;
            _inMemoryCache = inMemoryCache;
            _schoolService = schoolService;
            _storageManager = storageManager;
            _serviceAsAction = serviceAsAction;
            _masterServiceAsAction = masterServiceAsAction;
            _studentService = studentService;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param  = await Request.ValidateBody<ConvertServiceAsActionToPdfRequest, ConvertServiceAsActionToPdfValidator>();

            var schoolResult = default(GetSchoolDetailResult);
            var schoolData = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{param.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(param.IdSchool);
                schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(schoolResultKey, schoolResult, TimeSpan.FromMinutes(10));
            }

            var school = await _schoolService.GetSchoolDetail(param.IdSchool);
            schoolData = school.IsSuccess ? school.Payload : throw new Exception(school.Message);
            var logo  = schoolData.LogoUrl ?? "https://bssschoolstorage.blob.core.windows.net/school-logo/BinusSchool.png";

            var getListExperience = await _serviceAsAction.GetListExperiencePerStudent(new GetListExperiencePerStudentRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                IdStudent = param.IdStudent,
                IsSupervisor = param.IsStudent ? false : !((bool) param.IsAdvisor)
            });

            if(!getListExperience.IsSuccess)
                throw new Exception(getListExperience.Message);


            var listExperience = getListExperience.Payload;

            if(listExperience.ExperienceList.Count == 0)
                throw new Exception("No experience found.");

            var listForm = new List<GetServiceAsActionDetailFormResult>();

            foreach (var experience in listExperience.ExperienceList)
            {
                var apiFormDetail = await _serviceAsAction.GetServiceAsActionDetailForm(new GetServiceAsActionDetailFormRequest
                {
                    IdServiceAsActionForm = experience.IdServiceAsActionForm,
                    IdUser = param.IdUser,
                    IsAdvisor = param.IsAdvisor,
                    IsIncludeComment = param.IsIncludeComment
                });

                var formDetail = apiFormDetail.Payload;

                listForm.Add(formDetail);
            }

            var getStudentDetail = await _studentService.GetStudentDetail(param.IdStudent);

            var studentDetail = getStudentDetail.IsSuccess ? getStudentDetail.Payload : throw new Exception(getStudentDetail.Message);

            var htmlOutput = await BuildPdf(param, listForm, logo, studentDetail.PersonalInfoVm);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Outline = false
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = htmlOutput,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };
            var bytes = _converter.Convert(doc);

            return new FileContentResult(bytes, "application/pdf")
            {
                FileDownloadName = $"{param.IdStudent}_ExperienceList_AY{listExperience.AcademicYear.Description}_{DateTime.Now.ToString("yy_MM_dd_hh_mm")}.pdf"
            };
        }

        public async Task<string> BuildPdf(ConvertServiceAsActionToPdfRequest param, List<GetServiceAsActionDetailFormResult> experiences, string schoolLogo, PersonalStudentInfoDetailVm studentInfo)
        {
            var template = "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\" /><meta name=\"viewport\" content=\"widtd=device-widtd, initial-scale=1.0\" /><title>List of Experience</title><style>{{style}}</style></head><body>{logoStudentInfo}{{body}}</body></html>\r\n";

            var logoStudentInfo = @"
                <img class=""binus-logo"" src=""{schoolLogo}"" alt=""Binus School Logo"" />
                <div class=""title"">
                  <h1>Title</h1>
                </div>
                <!-- Content (START) -->
                <div class=""section-content mt-15"">
                  <table>
                    <tbody>
                      <tr>
                        <!-- Profile Picture (START) -->
                        <td class=""profile-picture"">
                          <img src=""{studentPhoto}"" alt=""Profile"" />
                        </td>
                        <!-- Profile Picture (END) -->

                        <!-- Student Information (START) -->
                        <td>
                          <table class=""student-information"">
                            <tbody>
                              <tr>
                                <td>Student Name</td>
                                <td>:</td>
                                <td>{ studentName }</td>
                              </tr>
                              <tr>
                                <td>Student Id</td>
                                <td>:</td>
                                <td>{ studentId }</td>
                              </tr>
                              <tr>
                                <td>Academic Year</td>
                                <td>:</td>
                                <td>{ academicYear }</td>
                              </tr>
                              <tr>
                                <td>Grade/Class</td>
                                <td>:</td>
                                <td>{ gradeClass }</td>
                              </tr>
                            </tbody>
                          </table>
                        </td>
                        <!-- Student Information (END) -->
                      </tr>
                    </tbody>
                  </table>
                </div>";

            //school information 
            logoStudentInfo = logoStudentInfo.Replace("{schoolLogo}", schoolLogo);

            //StudentDetail
            logoStudentInfo = logoStudentInfo.Replace("{studentPhoto}", studentInfo.Photo);
            logoStudentInfo = logoStudentInfo.Replace("{ studentName }", experiences.FirstOrDefault().Student.Description);
            logoStudentInfo = logoStudentInfo.Replace("{ studentId }", experiences.FirstOrDefault().Student.Id);
            logoStudentInfo = logoStudentInfo.Replace("{ academicYear }", experiences.FirstOrDefault().ExperienceDetail.AcademicYear.Description);
            logoStudentInfo = logoStudentInfo.Replace("{ gradeClass }", experiences.FirstOrDefault().Grade.Code + experiences.FirstOrDefault().Classroom.Description);

            string style = await GetStyleCss();

            var experiencesHTML = "";

            foreach(var expereience in experiences)
            {
                string body = await SetBody(param, expereience, schoolLogo, studentInfo);

                experiencesHTML += body;
            }

            template = template.Replace("{{style}}", style);
            template = template.Replace("{logoStudentInfo}", logoStudentInfo);
            template = template.Replace("{{body}}", experiencesHTML);

            return template;

        }

        public async Task<string> GetStyleCss()
        {
            string style = @"body {
                font: 16px Times;
                letter-spacing: 1px;
                margin: 0;
              }

              .binus-logo {
                position: absolute;
                top: 30px;
                left: 40px;
                height: 100px;
              }

              .title {
                width: 100%;
                height: 160px;
                text-align: center;
                align-content: center;
                margin: 0;
                border-bottom: 3px solid #941711;
              }

              table {
                width: 100%;
                border-collapse: collapse;
              }

              td {
                padding: 5px;
                padding-left: 0;
                text-align: left;
              }

              .section-title {
                background-color: rgba(148, 23, 17, 0.5);
                color: #941711;
                overflow: hidden;
                padding: 5px 15px;
                margin-top: 15px;
              }

              .subsection-title {
                background-color: rgba(148, 23, 17, 0.5);
                color: white;
                overflow: hidden;
                padding: 5px 15px;
                margin-top: 10px;
              }

              .section-content {
                padding: 0 60px;
              }

              .section-table td {
                align-content: start;
              }

              .section-table td:first-child {
                width: 200px;
              }

              .section-table td:nth-child(2) {
                width: 5px;
              }

              .section {
                margin-top: 10px;
                padding-bottom: 10px;
                border-bottom: 2px solid #941711;
                page-break-inside: avoid;
              }

              .attachment-comment-section {
                page-break-inside: auto;
              }

              .orange-list-table td:nth-child(2) {
                color: #fd7e14;
              }

              .attachment-learning-outcome td {
                padding: 0;
              }

              .attachment-learning-outcome td:first-child {
                color: #fd7e14;
                width: 30px;
              }

              .green-list-table td:nth-child(2) {
                color: #00933a;
              }

              .profile-picture {
                width: 160px;
              }

              .profile-picture img {
                width: 125px;
                height: 125px;
                border-radius: 50%;
                object-fit: cover;
                object-position: center;
              }

              .student-information td {
                padding: 4px;
              }

              .student-information td:first-child {
                width: 150px;
              }

              .student-information td:nth-child(2) {
                width: 5px;
              }

              .float-left {
                float: left;
              }

              .subsection-content {
                margin-top: 10px;
                line-height: 25px;
                page-break-inside: avoid;
                overflow: auto;
              }

              .comment {
                margin-top: 5px;
              }

              .attachment-image img {
                height: 120px;
                margin-top: 3px;
                margin-right: 5px;
              }

              .file img {
                height: 70px;
              }

              .file {
                margin-top: 3px;
                margin-right: 8px;
              }

              .float-right {
                float: right;
              }

              .grey-text {
                color: #cccccc;
              }

              .mt-15 {
                margin-top: 15px;
              }
              .comment-list{
                page-break-inside: auto;
              }";

            return style;
        }

        public async Task<string> SetBody(ConvertServiceAsActionToPdfRequest param, GetServiceAsActionDetailFormResult detailForm, string schoolLogo, PersonalStudentInfoDetailVm studentInfo)
        {
            string body = @"
                <div class=""section-container"">
                  <div class=""section-title"">
                    <b class=""float-left""><em>{experienceName}</em></b>
                    <b class=""float-right""><em>{experienceDate}</em></b>
                  </div>
                  <div class=""section-content"">
                    <div class=""section"">
                      <table class=""section-table"">
                        <tbody>
                          <tr>
                            <td>Experience Name</td>
                            <td>:</td>
                            <td>{experienceName}</td>
                          </tr>
                          <tr>
                            <td>Experience Location</td>
                            <td>:</td>
                            <td>{experienceLocation}</td>
                          </tr>
                          <tr>
                            <td>Experience Type</td>
                            <td>:</td>
                            <td>{experienceTypes}</td>
                          </tr>
                          <tr>
                            <td>Start Date/End Date</td>
                            <td>:</td>
                            <td>{startEndDate}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    <div class=""section"">
                      <table class=""section-table"">
                        <tbody>
                          <tr>
                            <td>Supervisor Name</td>
                            <td>:</td>
                            <td>{supervisorName}</td>
                          </tr>
                          <tr>
                            <td>Supervisor Email</td>
                            <td>:</td>
                            <td>{supervisorEmail}</td>
                          </tr>
                          <tr>
                            <td>Supervisor Title</td>
                            <td>:</td>
                            <td>{supervisorTitle}</td>
                          </tr>
                          <tr>
                            <td>Supervisor Contact</td>
                            <td>:</td>
                            <td>{supervisorContact}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    <div class=""section"">
                      <table class=""section-table"">
                        <tbody>
                          <tr>
                            <td>Organization Name</td>
                            <td>:</td>
                            <td>{organizationName}</td>
                          </tr>
                          <tr>
                            <td>Contribution</td>
                            <td>:</td>
                            <td>
                              {organizationContribution}
                            </td>
                          </tr>
                          <tr>
                            <td>Description</td>
                            <td>:</td>
                            <td>
                              {organizationDescription}
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    <div class=""section"">
                      <table class=""section-table orange-list-table"">
                        <tbody>
                          <!-- Loopable: Learning Outcome Items (START) -->
                          <tr>
                              <td>Learning Outcome</td>
                              <td>&#8226;</td>
                          {loMappings}
                          <!-- Loopable: Learning Outcome Items (START) -->
                        </tbody>
                      </table>
                    </div>
                    <div class=""section"">
                      <table class=""section-table green-list-table"">
                        <tbody>
                          {TOCConditions}
                        </tbody>
                      </table>
                    </div>
        `       </div>";

            //Experiences
            body = body.Replace("{experienceName}", detailForm.ExperienceDetail.ExperienceName);
            body = body.Replace("{experienceLocation}", detailForm.ExperienceDetail.ExperienceLocation.Description);
            body = body.Replace("{experienceTypes}", detailForm.ExperienceDetail.ExperienceType != null ? string.Join(", ", detailForm.ExperienceDetail.ExperienceType.Select(x => x.Description)) : "");
            body = body.Replace("{experienceDate}", $"{detailForm.ExperienceDetail.StartDate:dd MMMM yyyy} - {detailForm.ExperienceDetail.EndDate:dd MMMM yyyy}");
            body = body.Replace("{startEndDate}", $"{detailForm.ExperienceDetail.StartDate:dd MMMM yyyy}/{detailForm.ExperienceDetail.EndDate:dd MMMM yyyy}");

            //set Supervisor Detail
            if(detailForm.SupervisorData != null)
            {
                body = body.Replace("{supervisorName}", detailForm.SupervisorData.Supervisor.Description);
                body = body.Replace("{supervisorEmail}", detailForm.SupervisorData.SupervisorEmail);
                body = body.Replace("{supervisorTitle}", detailForm.SupervisorData.SupervisorTitle);
                body = body.Replace("{supervisorContact}", detailForm.SupervisorData.SupervisorContact);
            }
            else
            {
                body = body.Replace("{supervisorName}", "");
                body = body.Replace("{supervisorEmail}", "");
                body = body.Replace("{supervisorTitle}", "");
                body = body.Replace("{supervisorContact}", "");
            }

            //set Organization Detail
            body = body.Replace("{organizationName}", detailForm.OrganizationDetail.Organization);
            body = body.Replace("{organizationContribution}", detailForm.OrganizationDetail.ContributionTMC);
            body = body.Replace("{organizationDescription}", detailForm.OrganizationDetail.ActivityDescription);

            //set Learning Outcome
            body = body.Replace("{loMappings}", SetLearningOutcome(detailForm));

            //set TOCConditions
            body = body.Replace("{TOCConditions}", SetTOCConditions(detailForm));

            if (detailForm.Evidences != null)
            {
                var initialEvidenceTemplate = @"
                     <div class=""section attachment-comment-section"">
                      {evidences}
                    </div>
                ";

                var evidences = "";

                foreach (var evidence in detailForm.Evidences)
                {
                    if(evidence.EvidenceType == "Text")
                    {
                        var evidenceText = SetEvidenceText(evidence, param);
                        evidences += evidenceText;
                    }
                    else if(evidence.EvidenceType == "Link")
                    {
                        var evidenceLink = SetEvidenceLink(evidence, param);
                        evidences += evidenceLink;
                    }
                    else if(evidence.EvidenceType == "Image")
                    {
                        var evidenceImage = SetEvidenceImage(evidence, param);
                        evidences += evidenceImage;
                    }
                    else if(evidence.EvidenceType == "File")
                    {
                        var evidenceFile = SetEvidenceFile(evidence, param);
                        evidences += evidenceFile;
                    }
                }

                initialEvidenceTemplate = initialEvidenceTemplate.Replace("{evidences}", evidences);

                body += initialEvidenceTemplate;
            }

            return body;
        }

        public string SetLearningOutcome(GetServiceAsActionDetailFormResult detailForm)
        {
            string loMappings = @"<td>{firstLO}</td>
              </tr>";

            if (detailForm.LearningOutcomes != null)
            {
                int Start = 0;

                foreach (var item in detailForm.LearningOutcomes)
                {
                    if(Start == 0)
                    {
                        loMappings = loMappings.Replace("{firstLO}", item.Description);
                        continue;
                    }

                    loMappings += @"<tr>
                        <td></td>
                        <td>&#8226;</td>
                        <td>" + item.Description + "</td>";
                }
            }
            else
            {
                loMappings = loMappings.Replace("{firstLO}", "");
            }
            return loMappings;
        }

        public string SetTOCConditions(GetServiceAsActionDetailFormResult detailForm)
        {
            string TOCConditions = @"<tr>
                <td>Terms and Conditions</td>
                <td>&#8226;</td>
                <td>
                  <em
                    >My homeroom teacher ( my Service as Action advisor ) is
                    aware of my activity / project.</em
                  >
                </td>
              </tr>
              <tr>
                <td></td>
                <td>&#8226;</td>
                <td>
                  <em
                    >My parents have given permission to do this activity
                    /project from the beginning until it is completed.</em
                  >
                </td>
              </tr>
              <tr>
                <td></td>
                <td>&#8226;</td>
                <td>
                  <em>
                    My parents are fully aware of my project/ activity. I have
                    already explained to them about the details of the activity
                    /project too as written in this proposal.</em
                  >
                </td>
              </tr>";

            return TOCConditions;
        }

        public string SetEvidenceLink(GetServiceAsActionDetailFormResult_Evidence evidence, ConvertServiceAsActionToPdfRequest param)
        {
            var evidenceTemplate = @"
                <div>
                    <div class=""subsection-title"">
                      <b class=""float-left"">{evidenceDateMonthYear}</b>
                      <b class=""float-right"">{evidenceTime}</b>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Link</b></div>
                        <div>
                          <a href=""{evidenceLink}""
                            >{evidenceLink}</a>
                        </div>
                      </div>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Learning Outcomes</b></div>
                        <table class=""attachment-learning-outcome"">
                          <tbody>
                            <!-- Loopable: Attachment Learning Outcome (START) -->
                            {loItems}
                            <!-- Loopable: Attachment Learning Outcome (END) -->
                          </tbody>
                        </table>
                      </div>
                    </div>
                    {comments}
                  </div>
            ";

            if(evidence.LearningOutcomes != null)
            {
                var loItems = "";

                foreach(var lo in evidence.LearningOutcomes)
                {
                    var template = @"
                        <tr>
                          <td>&#8226;</td>
                          <td>{loDescription}</td>
                        </tr>
                    ";

                    template = template.Replace("{loDescription}", lo.Description);
                    loItems += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{loItems}", loItems);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{loItems}", "");
            }


            evidenceTemplate = evidenceTemplate.Replace("{evidenceLink}", evidence.EvidenceURL);
            evidenceTemplate = evidenceTemplate.Replace("{evidenceDateMonthYear}", ((DateTime)evidence.Datein).ToString("dddd, dd MMMM"));
            evidenceTemplate = evidenceTemplate.Replace("{evidenceTime}", ((DateTime)evidence.Datein).ToString("HH:mm"));

            if (param.IsIncludeComment)
            {

                var commentContainer = @"
                    <div class=""subsection-content comment-list"">
                      <div>
                        {commentItems}
                      </div>
                    </div>
                ";

                if (evidence.Comments != null)
                {
                    var commmentTemplate = @"
                            <div><b>Comment ({commentCount})</b></div>
                            {commentDatas}";

                    var commentDatas = "";

                    foreach (var comment in evidence.Comments)
                    {
                        var template = @"
                            <div class=""comment"">
                              <div>
                                <a href=""#"">{comentatorName} - {comentatorId}</a
                                >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class=""grey-text""
                                  >{commentDate}
                                </span>
                              </div>
                              <div>
                                {comment}
                              </div>
                            </div>
                        ";

                        template = template.Replace("{comentatorName}", comment.Commentator.Description);
                        template = template.Replace("{comentatorId}", comment.Commentator.Id);
                        template = template.Replace("{commentDate}", ((DateTime) comment.CommentDate).ToString("dd MMMM yyyy"));
                        template = template.Replace("{comment}", comment.Comment);

                        commentDatas += template;   
                    }

                    commmentTemplate = commmentTemplate.Replace("{commentCount}", evidence.Comments.Count().ToString());
                    commmentTemplate = commmentTemplate.Replace("{commentDatas}", commentDatas);
                    commentContainer = commentContainer.Replace("{commentItems}", commmentTemplate);
                }

                commentContainer = commentContainer.Replace("{commentItems}", "");
                evidenceTemplate = evidenceTemplate.Replace("{comments}", commentContainer);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{comments}", "");
            }

            return evidenceTemplate;
        }

        public string SetEvidenceText(GetServiceAsActionDetailFormResult_Evidence evidence, ConvertServiceAsActionToPdfRequest param)
        {
            var evidenceTemplate = @"
                <div>
                    <div class=""subsection-title"">
                      <b class=""float-left"">{evidenceDateMonthYear}</b>
                      <b class=""float-right"">{evidenceTime}</b>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Text</b></div>
                        <div>
                          <a
                            >{evidenceText}</a
                          >
                        </div>
                      </div>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Learning Outcomes</b></div>
                        <table class=""attachment-learning-outcome"">
                          <tbody>
                            <!-- Loopable: Attachment Learning Outcome (START) -->
                            {loItems}
                            <!-- Loopable: Attachment Learning Outcome (END) -->
                          </tbody>
                        </table>
                      </div>
                    </div>
                    {comments}
                  </div>
            ";

            if (evidence.LearningOutcomes != null)
            {
                var loItems = "";

                foreach (var lo in evidence.LearningOutcomes)
                {
                    var template = @"
                        <tr>
                          <td>&#8226;</td>
                          <td>{loDescription}</td>
                        </tr>
                    ";

                    template = template.Replace("{loDescription}", lo.Description);
                    loItems += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{loItems}", loItems);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{loItems}", "");
            }


            evidenceTemplate = evidenceTemplate.Replace("{evidenceText}", evidence.EvidenceText);
            evidenceTemplate = evidenceTemplate.Replace("{evidenceDateMonthYear}", ((DateTime)evidence.Datein).ToString("dddd, dd MMMM"));
            evidenceTemplate = evidenceTemplate.Replace("{evidenceTime}", ((DateTime)evidence.Datein).ToString("HH:mm"));

            if (param.IsIncludeComment)
            {

                var commentContainer = @"
                    <div class=""subsection-content comment-list"">
                      <div>
                        {commentItems}
                      </div>
                    </div>
                ";

                if (evidence.Comments != null)
                {
                    var commmentTemplate = @"
                            <div><b>Comment ({commentCount})</b></div>
                            {commentDatas}";

                    var commentDatas = "";

                    foreach (var comment in evidence.Comments)
                    {
                        var template = @"
                            <div class=""comment"">
                              <div>
                                <a href=""#"">{comentatorName} - {comentatorId}</a
                                >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class=""grey-text""
                                  >{commentDate}
                                </span>
                              </div>
                              <div>
                                {comment}
                              </div>
                            </div>
                        ";

                        template = template.Replace("{comentatorName}", comment.Commentator.Description);
                        template = template.Replace("{comentatorId}", comment.Commentator.Id);
                        template = template.Replace("{commentDate}", ((DateTime)comment.CommentDate).ToString("dd MMMM yyyy"));
                        template = template.Replace("{comment}", comment.Comment);

                        commentDatas += template;
                    }

                    commmentTemplate = commmentTemplate.Replace("{commentCount}", evidence.Comments.Count().ToString());
                    commmentTemplate = commmentTemplate.Replace("{commentDatas}", commentDatas);
                    commentContainer = commentContainer.Replace("{commentItems}", commmentTemplate);
                }

                commentContainer = commentContainer.Replace("{commentItems}", "");
                evidenceTemplate = evidenceTemplate.Replace("{comments}", commentContainer);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{comments}", "");
            }

            return evidenceTemplate;
        }

        public string SetEvidenceImage(GetServiceAsActionDetailFormResult_Evidence evidence, ConvertServiceAsActionToPdfRequest param)
        {
            var evidenceTemplate = @"
                <div>
                    <div class=""subsection-title"">
                      <b class=""float-left"">{evidenceDateMonthYear}</b>
                      <b class=""float-right"">{evidenceTime}</b>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>({imageCount}) Attachment(s)</b></div>
                        <div class=""attachment-image"">
                            {images}
                        </div>
                      </div>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Learning Outcomes</b></div>
                        <table class=""attachment-learning-outcome"">
                          <tbody>
                            <!-- Loopable: Attachment Learning Outcome (START) -->
                            {loItems}
                            <!-- Loopable: Attachment Learning Outcome (END) -->
                          </tbody>
                        </table>
                      </div>
                    </div>
                    {comments}
                  </div>
            ";

            if(evidence.Urls != null)
            {
                var images = "";

                foreach(var url in evidence.Urls)
                {
                    var template = @"
                        <img src=""{imageUrl}"" alt=""Attachment"" />
                    ";

                    template = template.Replace("{imageUrl}", url.EvidenceFIGM);
                    images += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{images}", images);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{images}", "");
            }

            if (evidence.LearningOutcomes != null)
            {
                var loItems = "";

                foreach (var lo in evidence.LearningOutcomes)
                {
                    var template = @"
                        <tr>
                          <td>&#8226;</td>
                          <td>{loDescription}</td>
                        </tr>
                    ";

                    template = template.Replace("{loDescription}", lo.Description);
                    loItems += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{loItems}", loItems);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{loItems}", "");
            }


            evidenceTemplate = evidenceTemplate.Replace("{imageCount}", evidence.Urls.Count.ToString());
            evidenceTemplate = evidenceTemplate.Replace("{evidenceDateMonthYear}", ((DateTime)evidence.Datein).ToString("dddd, dd MMMM"));
            evidenceTemplate = evidenceTemplate.Replace("{evidenceTime}", ((DateTime)evidence.Datein).ToString("HH:mm"));

            if (param.IsIncludeComment)
            {

                var commentContainer = @"
                    <div class=""subsection-content comment-list"">
                      <div>
                        {commentItems}
                      </div>
                    </div>
                ";

                if (evidence.Comments != null)
                {
                    var commmentTemplate = @"
                            <div><b>Comment ({commentCount})</b></div>
                            {commentDatas}";

                    var commentDatas = "";

                    foreach (var comment in evidence.Comments)
                    {
                        var template = @"
                            <div class=""comment"">
                              <div>
                                <a href=""#"">{comentatorName} - {comentatorId}</a
                                >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class=""grey-text""
                                  >{commentDate}
                                </span>
                              </div>
                              <div>
                                {comment}
                              </div>
                            </div>
                        ";

                        template = template.Replace("{comentatorName}", comment.Commentator.Description);
                        template = template.Replace("{comentatorId}", comment.Commentator.Id);
                        template = template.Replace("{commentDate}", ((DateTime)comment.CommentDate).ToString("dd MMMM yyyy"));
                        template = template.Replace("{comment}", comment.Comment);

                        commentDatas += template;
                    }

                    commmentTemplate = commmentTemplate.Replace("{commentCount}", evidence.Comments.Count().ToString());
                    commmentTemplate = commmentTemplate.Replace("{commentDatas}", commentDatas);
                    commentContainer = commentContainer.Replace("{commentItems}", commmentTemplate);
                }

                commentContainer = commentContainer.Replace("{commentItems}", "");
                evidenceTemplate = evidenceTemplate.Replace("{comments}", commentContainer);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{comments}", "");
            }

            return evidenceTemplate;
        }

        public string SetEvidenceFile(GetServiceAsActionDetailFormResult_Evidence evidence, ConvertServiceAsActionToPdfRequest param)
        {
            var evidenceTemplate = @"
                <div>
                    <div class=""subsection-title"">
                      <b class=""float-left"">{evidenceDateMonthYear}</b>
                      <b class=""float-right"">{evidenceTime}</b>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>({fileCount}) Attachment(s)</b></div>
                        <div class=""attachment-image"">
                            {files}
                        </div>
                      </div>
                    </div>
                    <!-- If Link -->
                    <div class=""subsection-content"">
                      <div>
                        <div><b>Learning Outcomes</b></div>
                        <table class=""attachment-learning-outcome"">
                          <tbody>
                            <!-- Loopable: Attachment Learning Outcome (START) -->
                            {loItems}
                            <!-- Loopable: Attachment Learning Outcome (END) -->
                          </tbody>
                        </table>
                      </div>
                    </div>
                    {comments}
                  </div>
            ";

            if (evidence.Urls != null)
            {
                var files = "";

                foreach (var url in evidence.Urls)
                {
                    var template = @"
                        <div class=""file float-left"">
                            <img src=""https://bssstudentstorageuat.blob.core.windows.net/saa-evidences/file.png"" />
                            <div><a href=""#"">{fileName}</a></div>
                        </div>
                    ";

                    template = template.Replace("{fileName}", url.FileName);
                    files += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{files}", files);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{files}", "");
            }

            if (evidence.LearningOutcomes != null)
            {
                var loItems = "";

                foreach (var lo in evidence.LearningOutcomes)
                {
                    var template = @"
                        <tr>
                          <td>&#8226;</td>
                          <td>{loDescription}</td>
                        </tr>
                    ";

                    template = template.Replace("{loDescription}", lo.Description);
                    loItems += template;
                }

                evidenceTemplate = evidenceTemplate.Replace("{loItems}", loItems);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{loItems}", "");
            }


            evidenceTemplate = evidenceTemplate.Replace("{fileCount}", evidence.Urls.Count.ToString());
            evidenceTemplate = evidenceTemplate.Replace("{evidenceDateMonthYear}", ((DateTime)evidence.Datein).ToString("dddd, dd MMMM"));
            evidenceTemplate = evidenceTemplate.Replace("{evidenceTime}", ((DateTime)evidence.Datein).ToString("HH:mm"));

            if (param.IsIncludeComment)
            {

                var commentContainer = @"
                    <div class=""subsection-content comment-list"">
                      <div>
                        {commentItems}
                      </div>
                    </div>
                ";

                if (evidence.Comments != null)
                {
                    var commmentTemplate = @"
                            <div><b>Comment ({commentCount})</b></div>
                            {commentDatas}";

                    var commentDatas = "";

                    foreach (var comment in evidence.Comments)
                    {
                        var template = @"
                            <div class=""comment"">
                              <div>
                                <a href=""#"">{comentatorName} - {comentatorId}</a
                                >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class=""grey-text""
                                  >{commentDate}
                                </span>
                              </div>
                              <div>
                                {comment}
                              </div>
                            </div>
                        ";

                        template = template.Replace("{comentatorName}", comment.Commentator.Description);
                        template = template.Replace("{comentatorId}", comment.Commentator.Id);
                        template = template.Replace("{commentDate}", ((DateTime)comment.CommentDate).ToString("dd MMMM yyyy"));
                        template = template.Replace("{comment}", comment.Comment);

                        commentDatas += template;
                    }

                    commmentTemplate = commmentTemplate.Replace("{commentCount}", evidence.Comments.Count().ToString());
                    commmentTemplate = commmentTemplate.Replace("{commentDatas}", commentDatas);
                    commentContainer = commentContainer.Replace("{commentItems}", commmentTemplate);
                }

                commentContainer = commentContainer.Replace("{commentItems}", "");
                evidenceTemplate = evidenceTemplate.Replace("{comments}", commentContainer);
            }
            else
            {
                evidenceTemplate = evidenceTemplate.Replace("{comments}", "");
            }

            return evidenceTemplate;
        }
    }
}
