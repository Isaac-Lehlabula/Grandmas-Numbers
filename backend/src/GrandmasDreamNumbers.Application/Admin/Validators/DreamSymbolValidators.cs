using FluentValidation;
using GrandmasDreamNumbers.Application.Admin.Dtos;

namespace GrandmasDreamNumbers.Application.Admin.Validators;

public class CreateDreamSymbolRequestValidator : AbstractValidator<CreateDreamSymbolRequest>
{
    public CreateDreamSymbolRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.TraditionalMeaning).NotEmpty().MaximumLength(1000);
    }
}

public class UpdateDreamSymbolRequestValidator : AbstractValidator<UpdateDreamSymbolRequest>
{
    public UpdateDreamSymbolRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.TraditionalMeaning).NotEmpty().MaximumLength(1000);
    }
}

public class AddDreamSymbolAliasRequestValidator : AbstractValidator<AddDreamSymbolAliasRequest>
{
    public AddDreamSymbolAliasRequestValidator()
    {
        RuleFor(x => x.Alias).NotEmpty().MaximumLength(100);
    }
}

public class AddLuckyNumberRequestValidator : AbstractValidator<AddLuckyNumberRequest>
{
    public AddLuckyNumberRequestValidator()
    {
        RuleFor(x => x.Number).InclusiveBetween(1, 99);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
