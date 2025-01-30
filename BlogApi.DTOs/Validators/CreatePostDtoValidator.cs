using FluentValidation;

namespace BlogApi.DTOs.Validators;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content cannot be empty")
            .MaximumLength(1000);
        RuleFor(c => c.FriendlyUrl)
            .NotEmpty()
            .WithMessage("FriendlyUrl is required");
    }
}