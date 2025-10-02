using Serilog;
using SourceBackend.Web.Auth;
using SourceBackend.Core.Abstractions;
using SourceBackend.Web.Extensions;
using SourceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+", "."));
});


builder.Services.AddPluggableJwt(builder.Configuration);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUserFromHttpContext>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

builder.Services.AddScoped<SourceBackend.Core.Features.Hello.Queries.GetHello.Handler>();
builder.Services.AddScoped<SourceBackend.Core.Features.Hello.Commands.SendHello.Handler>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();