using System;
using System.Collections.Generic;

namespace BinusSchool.Attendance.FnAttendance.Utils
{
    public static class BlobUtil
    {
        /// <summary>
        /// Create file path for blob name
        /// with format: {yyyy}/{M}/{d}/{idStudent}/{idGeneratedScheduleLesson}
        /// </summary>
        /// <param name="date">File date</param>
        /// <param name="idStudent">File owner</param>
        /// <param name="idGeneratedScheduleLesson">Schedule lesson owner</param>
        /// <returns>File path</returns>
        public static string CreateBlobNameAttendance(DateTime date, string idStudent, string idGeneratedScheduleLesson)
        {
            var names = new[] { date.Year.ToString(), date.Month.ToString(), date.Day.ToString(), idStudent, idGeneratedScheduleLesson };

            return string.Join('/', names);
        }

        public static string CreateBlobNameEventAttendance(DateTime date, string idStudent, string idEventCheck)
        {
            var names = new[] { date.Year.ToString(), date.Month.ToString(), date.Day.ToString(), idStudent, idEventCheck };

            return string.Join('/', names);
        }

        public static IDictionary<string, string> ToAttendanceEntryMetadata(string userIn, string idSession, string fileName, string contentType)
        {
            return new Dictionary<string, string>
            {
                { "userIn", userIn },
                { "idSession", idSession },
                { "fileName", fileName },
                { "contentType", contentType },
            };
        }

        public static IDictionary<string, string> ToEventAttendanceEntryMetadata(string userIn, string fileName, string contentType)
        {
            return new Dictionary<string, string>
            {
                { "userIn", userIn },
                { "fileName", fileName },
                { "contentType", contentType },
            };
        }

        public static (string userIn, string idSession, string fileName, string contentType) GetAttendanceEntryMetadata(IDictionary<string, string> metadata)
        {
            metadata.TryGetValue("userIn", out var userIn);
            metadata.TryGetValue("idSession", out var idSession);
            metadata.TryGetValue("fileName", out var fileName);
            metadata.TryGetValue("contentType", out var contentType);

            return (userIn, idSession, fileName, contentType);
        }

        public static (string userIn, string fileName, string contentType) GetEventAttendanceEntryMetadata(IDictionary<string, string> metadata)
        {
            metadata.TryGetValue("userIn", out var userIn);
            metadata.TryGetValue("fileName", out var fileName);
            metadata.TryGetValue("contentType", out var contentType);

            return (userIn, fileName, contentType);
        }
    }
}
