using FluentValidation;

namespace HakuService.Application.Features.Hakus.Commands.CreateHaku;

/// <summary>
/// Validator for CreateHakuCommand
/// </summary>
public class CreateHakuCommandValidator : AbstractValidator<CreateHakuCommand>
{
    public CreateHakuCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Haku name is required")
            .MaximumLength(200).WithMessage("Haku name cannot exceed 200 characters");

        RuleFor(x => x.SSId)
            .GreaterThan(0).WithMessage("Valid SSId is required");
    }
}
