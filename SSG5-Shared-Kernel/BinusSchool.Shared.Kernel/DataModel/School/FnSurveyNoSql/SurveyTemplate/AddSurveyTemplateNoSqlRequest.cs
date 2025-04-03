using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSurveyNoSql.SurveyTemplate
{
    public class AddSurveyTemplateNoSqlRequest
    {
        public string Id { get; set; }
        public string IdTemplateChild { get; set; }
        public string IdSurveyTemplate { get; set; }
        public string IdSurveyTemplateParent { get; set; }
        public string IdAcademicYear { get; set; }
        public string Role { get; set; }
        public string Language { get; set; }
        public string AcademicYear { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string SurveyTemplateType { get; set; }
        public string Status { get; set; }
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
    }
    public class Score
    {
        public string IdScore { get; set; }
        public string Value { get; set; }
        public string Perception { get; set; }
        public int Number { get; set; }
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
        public string BranchName { get; set; }
        public bool IsOtherOption { get; set; }
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
}
