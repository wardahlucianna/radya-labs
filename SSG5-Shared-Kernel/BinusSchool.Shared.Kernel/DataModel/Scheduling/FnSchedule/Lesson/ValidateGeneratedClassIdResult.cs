
namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class ValidateGeneratedClassIdResult
    {
        public bool IsValidated { get; set; }
        public string ClassId { get; set; }
        public string AutoIncreamentValue { get; set; }
    }

    public class GeneratedClassIdResult
    {
        public string ClassId { get; set; }
        public string AutoIncreamentValue { get; set; }
    }
}
