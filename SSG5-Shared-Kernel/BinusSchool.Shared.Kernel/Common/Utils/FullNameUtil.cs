using System.Linq;

namespace BinusSchool.Common.Utils
{
    public static class NameUtil
    {
        public static string GenerateFullName(params string[] names)
        {
            return string.Join(" ", names.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        }

        public static string GenerateFullNameWithId(string id, params string[] names)
        {
            var fullName = GenerateFullName(names);

            return $"{id} - {fullName}";
        }

        public static string GenerateStudentGradeId(string idSchool, string gradeCode, string ayCode, string idStudent)
        {
            return $"{idSchool}_{gradeCode}_AY{ayCode.Substring(ayCode.Length - 2)}_{idStudent}";
        }
    }
}
