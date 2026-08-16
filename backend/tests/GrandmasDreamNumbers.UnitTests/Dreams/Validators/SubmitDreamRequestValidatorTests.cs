using FluentAssertions;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Application.Dreams.Dtos;
using GrandmasDreamNumbers.Application.Dreams.Validators;
using Microsoft.Extensions.Options;

namespace GrandmasDreamNumbers.UnitTests.Dreams.Validators;

public class SubmitDreamRequestValidatorTests
{
    private readonly SubmitDreamRequestValidator _validator =
        new(Options.Create(new DreamAnalysisSettings { MaxDreamTextLength = 50 }));

    [Fact]
    public void Validate_EmptyText_Fails()
    {
        var result = _validator.Validate(new SubmitDreamRequest(""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TooLongText_Fails()
    {
        var tooLong = new string('a', 51);

        var result = _validator.Validate(new SubmitDreamRequest(tooLong));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidText_Passes()
    {
        var result = _validator.Validate(new SubmitDreamRequest("I dreamed about a snake near a river"));

        result.IsValid.Should().BeTrue();
    }
}
