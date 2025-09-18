namespace SourceBackend.Web.Auth;

public sealed class AuthOptions
{
    public AuthMode Mode { get; init; } = AuthMode.LocalSymmetric;
    public string Audience { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string RoleClaimType { get; init; } = "roles";

    public LocalSymmetricOptions LocalSymmetric { get; init; } = new();
    public LocalAsymmetricOptions LocalAsymmetric { get; init; } = new();
    public OidcOptions Oidc { get; init; } = new();
}
public sealed class LocalSymmetricOptions { public string Base64Key { get; init; } = ""; }
public sealed class LocalAsymmetricOptions { public string? PublicKeyPemPath { get; init; } public string? PrivateKeyPemPath { get; init; } }
public sealed class OidcOptions { public string Authority { get; init; } = ""; public string Audience { get; init; } = ""; public string Issuer { get; init; } = ""; }