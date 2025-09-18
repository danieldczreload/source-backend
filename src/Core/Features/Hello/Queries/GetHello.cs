namespace SourceBackend.Core.Features.Hello.Queries;

public static class GetHello
{
    public sealed record Query(string? Name);
    public sealed record Result(string Message, DateTimeOffset UtcNow);

    public sealed class Handler(TimeProvider clock)
    {
        public Task<Result> Handle(Query query, CancellationToken ct = default)
        {
            var name = string.IsNullOrWhiteSpace(query.Name) ? "world" : query.Name!.Trim();
            var msg  = $"Hello, {name}!";
            return Task.FromResult(new Result(msg, clock.GetUtcNow()));
        }
    }
}