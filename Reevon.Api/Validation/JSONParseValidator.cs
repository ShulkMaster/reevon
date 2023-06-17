using FluentValidation;
using Reevon.Api.Contracts.Request;

namespace Reevon.Api.Validation;

public class JSONParseValidator: AbstractValidator<DocumentJSONParse>
{
    public JSONParseValidator()
    {
        RuleFor(x => x.Separator)
            .NotEmpty()
            .Length(1);

        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Document)
            .NotNull()
            .Must(doc => doc.FileName.EndsWith(".json"))
            .WithMessage("Only json files are allowed")
            .Must(doc => doc?.Length > 0)
            .WithMessage("File must not be empty");
    }
}