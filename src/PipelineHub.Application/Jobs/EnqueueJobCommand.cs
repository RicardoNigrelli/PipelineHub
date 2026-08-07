using MediatR;
using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

public sealed record EnqueueJobCommand(JobType Type, IReadOnlyDictionary<string, string> Parameters) : IRequest<Guid>;
