﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.File.Validator
{
    public class DeleteFileValidator : AbstractValidator<FileRequest>
    {
        public DeleteFileValidator()
        {
            RuleFor(x => x.Container).NotEmpty();

            RuleFor(x => x.BlobName).NotEmpty();
        }
    }
}
