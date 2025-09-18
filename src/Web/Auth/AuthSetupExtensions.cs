using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SourceBackend.Web.Auth;

public static class AuthSetupExtensions
{
    public static IServiceCollection AddPluggableJwt(this IServiceCollection services, IConfiguration cfg)
    {
        // Carga opciones de Auth desde appsettings / secrets
        services.Configure<AuthOptions>(cfg.GetSection("Auth"));

        // Registrar autenticación (JwtBearer) y autorización
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

        services.AddAuthorization();

        // Configurar JwtBearerOptions con acceso a AuthOptions vía DI
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<AuthOptions>>((o, auth) =>
                {
                    var opts = auth.Value;

                    o.TokenValidationParameters ??= new TokenValidationParameters();
                    o.TokenValidationParameters.RoleClaimType = opts.RoleClaimType;

                    switch (opts.Mode)
                    {
                        case AuthMode.LocalSymmetric:
                        {
                            if (string.IsNullOrWhiteSpace(opts.LocalSymmetric.Base64Key))
                                throw new InvalidOperationException("Auth:LocalSymmetric:Base64Key is required for LocalSymmetric mode.");

                            byte[] keyBytes;
                            try
                            {
                                keyBytes = Convert.FromBase64String(opts.LocalSymmetric.Base64Key);
                            }
                            catch (FormatException)
                            {
                                throw new InvalidOperationException("Auth:LocalSymmetric:Base64Key must be valid Base64.");
                            }

                            var key = new SymmetricSecurityKey(keyBytes);

                            o.TokenValidationParameters.ValidateIssuer = true;
                            o.TokenValidationParameters.ValidIssuer = opts.Issuer;
                            o.TokenValidationParameters.ValidateAudience = true;
                            o.TokenValidationParameters.ValidAudience = opts.Audience;
                            o.TokenValidationParameters.ValidateIssuerSigningKey = true;
                            o.TokenValidationParameters.IssuerSigningKey = key;
                            o.TokenValidationParameters.ValidateLifetime = true;
                            o.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
                            break;
                        }

                        case AuthMode.LocalAsymmetric:
                        {
                            var pubPath = opts.LocalAsymmetric.PublicKeyPemPath;
                            if (string.IsNullOrWhiteSpace(pubPath) || !File.Exists(pubPath))
                                throw new InvalidOperationException("Auth:LocalAsymmetric:PublicKeyPemPath is required and must exist.");

                            // Cargamos PEM pública y construimos RsaSecurityKey desde parámetros (no mantener RSA vivo)
                            using var rsa = RSA.Create();
                            rsa.ImportFromPem(File.ReadAllText(pubPath));
                            var rsaParams = rsa.ExportParameters(false);
                            var rsaKey = new RsaSecurityKey(rsaParams);

                            o.TokenValidationParameters.ValidateIssuer = true;
                            o.TokenValidationParameters.ValidIssuer = opts.Issuer;
                            o.TokenValidationParameters.ValidateAudience = true;
                            o.TokenValidationParameters.ValidAudience = opts.Audience;
                            o.TokenValidationParameters.ValidateIssuerSigningKey = true;
                            o.TokenValidationParameters.IssuerSigningKey = rsaKey;
                            o.TokenValidationParameters.ValidateLifetime = true;
                            o.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
                            break;
                        }

                        case AuthMode.Oidc:
                        {
                            o.Authority = opts.Oidc.Authority;
                            o.Audience  = string.IsNullOrWhiteSpace(opts.Oidc.Audience) ? opts.Audience : opts.Oidc.Audience;
                            o.RequireHttpsMetadata = true;

                            if (!string.IsNullOrWhiteSpace(opts.Oidc.Issuer))
                            {
                                o.TokenValidationParameters.ValidateIssuer = true;
                                o.TokenValidationParameters.ValidIssuer = opts.Oidc.Issuer;
                            }
                            else
                            {
                                // Con Authority es suficiente, pero si quieres ser laxo con issuer:
                                o.TokenValidationParameters.ValidateIssuer = false;
                            }

                            o.TokenValidationParameters.ValidateLifetime = true;
                            o.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
                            break;
                        }

                        default:
                            throw new InvalidOperationException($"Unknown Auth.Mode: {opts.Mode}");
                    }
                });

        // Emisor local (sólo en modos Local*). En OIDC devuelve Noop.
        services.AddScoped<ITokenIssuer>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AuthOptions>>().Value;
            return opts.Mode switch
            {
                AuthMode.LocalSymmetric => new SymmetricTokenIssuer(opts),
                AuthMode.LocalAsymmetric when !string.IsNullOrWhiteSpace(opts.LocalAsymmetric.PrivateKeyPemPath)
                    => new AsymmetricTokenIssuer(opts),
                _ => new NoopTokenIssuer()
            };
        });

        return services;
    }
}

// ===== Emisión de tokens (para modos Local*) =====

public interface ITokenIssuer
{
    string? Issue(string sub, IEnumerable<string> roles, TimeSpan? lifetime = null);
}

file sealed class NoopTokenIssuer : ITokenIssuer
{
    public string? Issue(string sub, IEnumerable<string> roles, TimeSpan? lifetime = null) => null;
}

file sealed class SymmetricTokenIssuer(AuthOptions opts) : ITokenIssuer
{
    public string? Issue(string sub, IEnumerable<string> roles, TimeSpan? lifetime = null)
    {
        if (string.IsNullOrWhiteSpace(opts.LocalSymmetric.Base64Key)) return null;

        var keyBytes = Convert.FromBase64String(opts.LocalSymmetric.Base64Key);
        var key   = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return JwtTokenFactory.Build(sub, roles, creds, opts, lifetime);
    }
}

file sealed class AsymmetricTokenIssuer(AuthOptions opts) : ITokenIssuer
{
    public string? Issue(string sub, IEnumerable<string> roles, TimeSpan? lifetime = null)
    {
        var privPath = opts.LocalAsymmetric.PrivateKeyPemPath;
        if (string.IsNullOrWhiteSpace(privPath) || !File.Exists(privPath)) return null;

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privPath));

        var creds = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        return JwtTokenFactory.Build(sub, roles, creds, opts, lifetime);
    }
}

// Helper interno (sólo visible en este archivo)
file static class JwtTokenFactory
{
    public static string Build(
        string sub,
        IEnumerable<string> roles,
        SigningCredentials creds,
        AuthOptions opts,
        TimeSpan? lifetime)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sub),
            new("sub", sub)
        };
        claims.AddRange(roles.Select(r => new Claim(opts.RoleClaimType, r)));

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now + (lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds);

        return handler.WriteToken(token);
    }
}