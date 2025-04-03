using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;

namespace BinusSchool.Student.FnStudent.CreateEmailForStudent
{
    public class GetStudentEmailRecomendationHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        public GetStudentEmailRecomendationHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentEmailRecomendationRequest>();

            string[] name = param.StudentName.Split(' ');

            var stringEmailBasedOnName = "";

            if(name.Length > 1)
            {
                stringEmailBasedOnName = name[0].ToLower().Trim() + "." + name[name.Length-1].ToLower().Trim() + "@binusian.org";
            }
            else
            {
                stringEmailBasedOnName = name[0].ToLower().Trim() + "@binusian.org";
            }

            var stringEmailBasedonStudentId = $"{param.StudentId}@binusian.org";

            var stringEmailName = name[0].ToLower().Trim() + name[name.Length-1].ToLower().Trim() + "@binusian.org";

            List<GetStudentEmailRecomendationResult> retVal = new List<GetStudentEmailRecomendationResult>();

            #region Check DB
            bool checkstringEmailBasedOnName = true;
            bool checkstringEmailBasedonStudentId = true;
            bool checkstringEmailName = true;

            var query = _dbContext.Entity<MsStudent>().Where(x => x.BinusianEmailAddress == stringEmailBasedOnName).FirstOrDefault();
            
            if(query != null)
            {
                checkstringEmailBasedOnName = false;
            }
            else
            {
                #region check azure
                //string UrlCheckEmail = "http://10.200.207.65:8009/api/ActiveDirectory/ValidateEmail";

                //string keyValue = "Bearer 23bd1f2b31c064c4bb71abcba0a59bd4c5bc550bd7ac40e56e982c21aa5c5757";

                //WebRequest req = WebRequest.Create(UrlCheckEmail);
                //req.Headers.Add("Authorization", keyValue);
                //req.ContentType = "application/json";
                //req.Method = "POST";

                //string json = "{'username':'" + stringEmailBasedOnName + "'}";
                //using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                //{
                //    streamwriter.Write(json);
                //}

                //WebResponse wr = req.GetResponse();
                //Stream receiveStream = wr.GetResponseStream();

                //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
                //Result objResponseBaseonEmail = (Result)serializer.ReadObject(receiveStream);

                //checkstringEmailBasedOnName = objResponseBaseonEmail.Status == "1" ? false : true;

                #endregion
            }

            query = _dbContext.Entity<MsStudent>().Where(x => x.BinusianEmailAddress == stringEmailBasedonStudentId).FirstOrDefault();

            if (query != null)
            {
                checkstringEmailBasedonStudentId = false;
            }
            else
            {
                #region Check Azure
                //string UrlCheckEmail = "http://10.200.207.65:8009/api/ActiveDirectory/ValidateEmail";

                //string keyValue = "Bearer 23bd1f2b31c064c4bb71abcba0a59bd4c5bc550bd7ac40e56e982c21aa5c5757";

                //WebRequest req = WebRequest.Create(UrlCheckEmail);
                //req.Headers.Add("Authorization", keyValue);
                //req.ContentType = "application/json";
                //req.Method = "POST";

                //string json = "{'username':'" + stringEmailBasedonStudentId + "'}";
                //using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                //{
                //    streamwriter.Write(json);
                //}

                //WebResponse wr = req.GetResponse();
                //Stream receiveStream = wr.GetResponseStream();

                //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
                //Result objResponseBaseonNim = (Result)serializer.ReadObject(receiveStream);
                //checkstringEmailBasedonStudentId = objResponseBaseonNim.Status == "1" ? false : true;
                #endregion
            }

            query = _dbContext.Entity<MsStudent>().Where(x => x.BinusianEmailAddress == stringEmailName).FirstOrDefault();

            if (query != null)
            {
                checkstringEmailName = false;
            }
            else
            {
                #region Check Azure

                //string UrlCheckEmail = "http://10.200.207.65:8009/api/ActiveDirectory/ValidateEmail";

                //string keyValue = "Bearer 23bd1f2b31c064c4bb71abcba0a59bd4c5bc550bd7ac40e56e982c21aa5c5757";

                //WebRequest req = WebRequest.Create(UrlCheckEmail);
                //req.Headers.Add("Authorization", keyValue);
                //req.ContentType = "application/json";
                //req.Method = "POST";

                //string json = "{'username':'" + stringEmailName + "'}";
                //using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                //{
                //    streamwriter.Write(json);
                //}

                //WebResponse wr = req.GetResponse();
                //Stream receiveStream = wr.GetResponseStream();

                //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Result));
                //Result objResponseBaseonName = (Result)serializer.ReadObject(receiveStream);
                //checkstringEmailName = objResponseBaseonName.Status == "1" ? false : true;
                #endregion
            }
            #endregion

            #region Check Email exist or not


            #endregion

            #region isi data
            retVal.Add(new GetStudentEmailRecomendationResult
            {
                EmailRecomendation = stringEmailBasedOnName,
                Available = checkstringEmailBasedOnName
            });
            retVal.Add(new GetStudentEmailRecomendationResult
            {
                EmailRecomendation = stringEmailBasedonStudentId,
                Available = checkstringEmailBasedonStudentId
            });
            retVal.Add(new GetStudentEmailRecomendationResult
            {
                EmailRecomendation = stringEmailName,
                Available = checkstringEmailName
            });
            #endregion

            return Request.CreateApiResult2(retVal as object);
        }

        [DataContract]
        private class Result
        {
            [DataMember(Name ="Status")]
            public string Status { get; set; }

            [DataMember(Name = "msg")]
            public string Message { get; set; }
        }

    }
}
