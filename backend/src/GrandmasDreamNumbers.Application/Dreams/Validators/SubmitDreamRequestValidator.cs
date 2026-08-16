using FluentValidation;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using Microsoft.Extensions.Options;

namespace GrandmasDreamNumbers.Application.Dreams.Validators;

public class SubmitDreamRequestValidator : AbstractValidator<SubmitDreamRequest>
{
    public SubmitDreamRequestValidator(IOptions<DreamAnalysisSettings> settings)
    {
        var maxLength = settings.Value.MaxDreamTextLength;

        RuleFor(x => x.DreamText)
            .NotEmpty()
            .WithMessage("Please describe your dream before submitting.")
            .MaximumLength(maxLength)
            .WithMessage($"Dream text can't be longer than {maxLength} characters.");
    }
}
