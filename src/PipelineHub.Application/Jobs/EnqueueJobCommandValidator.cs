using FluentValidation;

namespace PipelineHub.Application.Jobs;

public sealed class EnqueueJobCommandValidator : AbstractValidator<EnqueueJobCommand>
{
    public EnqueueJobCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Parameters)
            .NotNull();
    }
}
