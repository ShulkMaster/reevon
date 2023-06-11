﻿using FluentValidation;
using Reevon.Api.Contracts.Request;

namespace Reevon.Api.Validation;

public class DocumentParseValidator: AbstractValidator<DocumentParse>
{
    public DocumentParseValidator()
    {
        RuleFor(x => x.Separator)
            .NotEmpty()
            .Length(1);

        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Document)
            .NotNull()
            .Must(doc => doc?.Length > 0);
    }
}