namespace SourceBackend.Core.Features.Hello.Commands;

public static class SendHello
{
    public sealed record Command(string? Name);
    public sealed record Result(string Message, Guid OperationId, DateTimeOffset UtcNow);

    public sealed class Handler(TimeProvider clock)
    {
        public Task<Result> Handle(Command cmd, CancellationToken ct = default)
        {
            var name = string.IsNullOrWhiteSpace(cmd.Name) ? "world" : cmd.Name!.Trim();
            var message = $"Hello, {name}!";
            // Futuro: persistir, publicar evento, etc.
            var opId = Guid.NewGuid();
            return Task.FromResult(new Result(message, opId, clock.GetUtcNow()));
        }
    }
}