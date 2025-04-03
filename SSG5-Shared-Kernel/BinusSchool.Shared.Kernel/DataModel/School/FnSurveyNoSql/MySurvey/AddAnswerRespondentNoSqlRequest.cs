using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSurveyNoSql.MySurvey
{
    public class AddAnswerRespondentNoSqlRequest
    {
        public string IdPublishSurvey { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdSurveyTemplateParent { get; set; }
        public string IdSurvey { get; set; }
        public string IdSurveyParent { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdHomeroomStudent { get; set; }
        public SurveyTemplateLanguage Language { get; set; }
        public string TemplateTitle { get; set; }
        public SurveyTemplateType SurveyTemplateType { get; set; }
        public MySurveyStatus Status { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public List<Section> Sections { get; set; }
    }

    public class Section
    {
        public int Number { get; set; }
        public string IdSection { get; set; }
        public string NameSection { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
        public List<Score> Scores { get; set; }
    }

    public class Question
    {
        public int Number { get; set; }
        public string IdQuestion { get; set; }
        public string QuestionText { get; set; }
        public bool UseDescription { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public List<TypeValue> TypeValues { get; set; }
        public List<LikertValue> LikertValues { get; set; }
        public bool IsRequired { get; set; }
        public List<RespondentAnswer> RespondentAnswers { get; set; }
    }
    public class Score
    {
        public string IdScore { get; set; }
        public string Value { get; set; }
        public string Perception { get; set; }
        public int? Number { get; set; }
    }

    public class TypeValue
    {
        public int Number { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
        public bool IsBranch { get; set; }
        public string BranchIdQuestion { get; set; }
        public bool BranchOtherSection { get; set; }
        public string BranchIdSection { get; set; }
        public bool IsOtherOption { get; set; }
        public string BranchName { get; set; }
    }

    public class LikertValue
    {
        public List<ColumnOption> ColumnOptions { get; set; }
        public List<ColumnOption> RowStatements { get; set; }
    }

    public class ColumnOption
    {
        public int Number { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class RespondentAnswer
    {
        public int Number { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
        public FileValueAnswer FileValueAnswer { get; set; }
        public ColumnOption LikertValueAnswer { get; set; }
        public List<LikertLearningStudentAnswer> LikertLearningStudentAnswers { get; set; }
    }

    public class FileValueAnswer : CodeWithIdVm
    {
        public string OriginalFilename { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
        public bool IsImage { get; set; }
    }

    public class LikertLearningStudentAnswer : Score
    {
        public string IdUserTeacher { get; set; }
    }
}
