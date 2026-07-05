using FluentValidation;
using TaskManagement.Application.DTOs.Task;

namespace TaskManagement.Application.Validators;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.AssignedToId).NotEmpty();
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Due date must not be earlier than start date.");
    }
}

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.AssignedToId).NotEmpty();
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
