using MediatR;

namespace PipelineHub.Application.Jobs;

public sealed record GetJobStatusQuery(Guid Id) : IRequest<JobStatusDto?>;

public sealed class GetJobStatusQueryHandler : IRequestHandler<GetJobStatusQuery, JobStatusDto?>
{
    private readonly IJobRepository _repository;

    public GetJobStatusQueryHandler(IJobRepository repository) => _repository = repository;

    public async Task<JobStatusDto?> Handle(GetJobStatusQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetAsync(request.Id, cancellationToken);
        return job is null ? null : JobStatusDto.FromDomain(job);
    }
}
