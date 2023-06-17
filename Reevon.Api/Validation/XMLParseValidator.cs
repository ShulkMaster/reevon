using FluentValidation;
using Reevon.Api.Contracts.Request;

namespace Reevon.Api.Validation;

public class XMLParseValidator: AbstractValidator<DocumentXMLParse>
{
    public XMLParseValidator()
    {
        RuleFor(x => x.Separator)
            .NotEmpty()
            .Length(1);

        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Document)
            .NotNull()
            .Must(doc => doc.FileName.EndsWith(".xml"))
            .WithMessage("Only xml files are allowed")
            .Must(doc => doc?.Length > 0)
            .WithMessage("File must not be empty");
    }
}