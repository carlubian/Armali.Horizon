using System.Security.Cryptography;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Core.Model;
using Armali.Horizon.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IdentityUser = Armali.Horizon.Identity.Model.IdentityUser;

namespace Armali.Horizon.Identity.Services;

/// <summary>
/// Servicio núcleo de Horizon.Identity. Maneja credenciales, tokens (sesión y
/// API key), roles y usuarios. Pensado para ser invocado desde los handlers
/// IO; los handlers añaden la capa de autorización por token + rol.
/// </summary>
public class IdentityService
{
    private static readonly TimeSpan DefaultSessionLifetime = TimeSpan.FromHours(12);

    private readonly IDbContextFactory<IdentityDbContext> Factory;
    private readonly ILogger<IdentityService> Logger;
    private readonly PasswordHasher<IdentityUser> Hasher = new();

    public IdentityService(IDbContextFactory<IdentityDbContext> factory, ILogger<IdentityService> logger)
    {
        Factory = factory;
        Logger = logger;
    }

    // ── Helpers de tokens ────────────────────────────────────────────────

    private static string GeneratePlainToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string HashToken(string plain)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plain);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static IdentityTokenDto ToDto(IdentityToken t) => new()
    {
        TokenId = t.Id,
        UserId = t.UserId,
        Label = t.Label,
        Kind = t.Kind,
        CreatedAt = t.CreatedAt,
        LastUsedAt = t.LastUsedAt,
        ExpiresAt = t.ExpiresAt,
        IsRevoked = t.IsRevoked,
    };

    private static async Task<IdentityUserDto> ToDtoAsync(IdentityUser u, IdentityDbContext ctx)
    {
        var roles = await ctx.UserRoles.AsNoTracking()
            .Where(r => r.UserId == u.Id).Select(r => r.Role).ToArrayAsync();
        return new IdentityUserDto
        {
            UserId = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            IsActive = u.IsActive,
            Roles = roles,
            CreatedAt = u.CreatedAt,
        };
    }

    // ── Login / token validation ────────────────────────────────────────

    public async Task<(string PlainToken, IdentityToken Stored, HorizonIdentity Identity)?> LoginAsync(
        string userName, string password, TimeSpan? lifetime = null)
    {
        await using var ctx = await Factory.CreateDbContextAsync();

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user is null || !user.IsActive) return null;

        var verify = Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Failed)
        {
            Logger.LogWarning("Login fallido para '{UserName}'", userName);
            return null;
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = Hasher.HashPassword(user, password);
            await ctx.SaveChangesAsync();
        }

        var plain = GeneratePlainToken();
        var token = new IdentityToken
        {
            UserId = user.Id,
            TokenHash = HashToken(plain),
            Label = "session",
            Kind = TokenKind.Session,
            ExpiresAt = DateTime.UtcNow.Add(lifetime ?? DefaultSessionLifetime),
        };
        ctx.Tokens.Add(token);
        await ctx.SaveChangesAsync();

        var roles = await ctx.UserRoles.AsNoTracking()
            .Where(r => r.UserId == user.Id).Select(r => r.Role).ToArrayAsync();

        var identity = new HorizonIdentity
        {
            UserId = user.Id,
            UserName = user.DisplayName.Length > 0 ? user.DisplayName : user.UserName,
            Roles = roles,
            Token = plain,
        };
        Logger.LogInformation("Login OK '{UserName}' (id={UserId}) — sesión {TokenId}", user.UserName, user.Id, token.Id);
        return (plain, token, identity);
    }

    public async Task<HorizonIdentity?> ValidateTokenAsync(string plainToken)
    {
        if (string.IsNullOrEmpty(plainToken)) return null;

        await using var ctx = await Factory.CreateDbContextAsync();
        var hash = HashToken(plainToken);

        var token = await ctx.Tokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (token is null || token.IsRevoked) return null;
        if (token.ExpiresAt is { } exp && exp < DateTime.UtcNow) return null;

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);
        if (user is null || !user.IsActive) return null;

        token.LastUsedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        var roles = await ctx.UserRoles.AsNoTracking()
            .Where(r => r.UserId == user.Id).Select(r => r.Role).ToArrayAsync();

        return new HorizonIdentity
        {
            UserId = user.Id,
            UserName = user.DisplayName.Length > 0 ? user.DisplayName : user.UserName,
            Roles = roles,
            Token = plainToken,
        };
    }

    public async Task<bool> RevokeByPlainAsync(string plainToken)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var hash = HashToken(plainToken);
        var token = await ctx.Tokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (token is null) return false;
        token.IsRevoked = true;
        await ctx.SaveChangesAsync();
        Logger.LogInformation("Token revocado por plain (id={TokenId} kind={Kind} user={UserId})", token.Id, token.Kind, token.UserId);
        return true;
    }

    public async Task<bool> RevokeByIdAsync(string tokenId)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var token = await ctx.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId);
        if (token is null) return false;
        token.IsRevoked = true;
        await ctx.SaveChangesAsync();
        Logger.LogInformation("Token revocado por id (id={TokenId} kind={Kind} user={UserId})", token.Id, token.Kind, token.UserId);
        return true;
    }

    // ── Cambio de password (autoservicio) ──────────────────────────────

    public async Task<bool> ChangePasswordAsync(string userId, string current, string @new)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return false;

        var verify = Hasher.VerifyHashedPassword(user, user.PasswordHash, current);
        if (verify == PasswordVerificationResult.Failed)
        {
            Logger.LogWarning("Cambio de password fallido (current incorrecta) user={UserId}", userId);
            return false;
        }

        user.PasswordHash = Hasher.HashPassword(user, @new);
        await ctx.SaveChangesAsync();
        Logger.LogInformation("Password cambiada para '{UserName}' (id={UserId})", user.UserName, userId);
        return true;
    }

    // ── Usuarios (admin) ────────────────────────────────────────────────

    public async Task<List<IdentityUserDto>> ListUsersAsync()
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var users = await ctx.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
        var allRoles = await ctx.UserRoles.AsNoTracking().ToListAsync();
        return users.Select(u => new IdentityUserDto
        {
            UserId = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            IsActive = u.IsActive,
            Roles = allRoles.Where(r => r.UserId == u.Id).Select(r => r.Role).ToArray(),
            CreatedAt = u.CreatedAt,
        }).ToList();
    }

    public async Task<IdentityUserDto?> CreateUserAsync(string userName, string displayName, string password, string[] roles)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        if (await ctx.Users.AnyAsync(u => u.UserName == userName)) return null;

        var user = new IdentityUser
        {
            UserName = userName,
            DisplayName = displayName,
        };
        user.PasswordHash = Hasher.HashPassword(user, password);
        ctx.Users.Add(user);

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            ctx.UserRoles.Add(new IdentityUserRole { UserId = user.Id, Role = role });

        await ctx.SaveChangesAsync();
        Logger.LogInformation("Usuario creado '{UserName}' (id={UserId}) con roles=[{Roles}]",
            user.UserName, user.Id, string.Join(",", roles));
        return await ToDtoAsync(user, ctx);
    }

    public async Task<IdentityUserDto?> UpdateUserAsync(string userId, string? displayName, bool? isActive, string? newPassword)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return null;

        if (displayName is not null) user.DisplayName = displayName;
        if (isActive is { } a) user.IsActive = a;
        if (!string.IsNullOrEmpty(newPassword))
        {
            user.PasswordHash = Hasher.HashPassword(user, newPassword);
            Logger.LogInformation("Password reset por admin para '{UserName}' (id={UserId})", user.UserName, userId);
        }

        await ctx.SaveChangesAsync();
        Logger.LogInformation("Usuario actualizado '{UserName}' (id={UserId})", user.UserName, userId);
        return await ToDtoAsync(user, ctx);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return false;

        var tokens = await ctx.Tokens.Where(t => t.UserId == userId).ToListAsync();
        ctx.Tokens.RemoveRange(tokens);
        var userRoles = await ctx.UserRoles.Where(r => r.UserId == userId).ToListAsync();
        ctx.UserRoles.RemoveRange(userRoles);
        ctx.Users.Remove(user);

        await ctx.SaveChangesAsync();
        Logger.LogInformation("Usuario eliminado '{UserName}' (id={UserId})", user.UserName, userId);
        return true;
    }

    public async Task<string[]?> SetRolesAsync(string userId, string[] roles)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return null;

        var existing = await ctx.UserRoles.Where(r => r.UserId == userId).ToListAsync();
        ctx.UserRoles.RemoveRange(existing);

        var distinct = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var role in distinct)
            ctx.UserRoles.Add(new IdentityUserRole { UserId = userId, Role = role });

        await ctx.SaveChangesAsync();
        Logger.LogInformation("Roles de '{UserName}' (id={UserId}) actualizados → [{Roles}]",
            user.UserName, userId, string.Join(",", distinct));
        return distinct;
    }

    public async Task<List<string>> ListRolesAsync()
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        return await ctx.UserRoles.AsNoTracking()
            .Select(r => r.Role).Distinct().OrderBy(r => r).ToListAsync();
    }

    // ── Tokens / API keys (gestión explícita) ──────────────────────────

    public async Task<(string Plain, IdentityTokenDto Info)?> CreateTokenAsync(
        string userId, string label, TokenKind kind, DateTime? expiresAt)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        if (!await ctx.Users.AnyAsync(u => u.Id == userId)) return null;

        var plain = GeneratePlainToken();
        var token = new IdentityToken
        {
            UserId = userId,
            TokenHash = HashToken(plain),
            Label = label,
            Kind = kind,
            ExpiresAt = expiresAt,
        };
        ctx.Tokens.Add(token);
        await ctx.SaveChangesAsync();
        Logger.LogInformation("Token creado (id={TokenId} kind={Kind} user={UserId} label='{Label}')",
            token.Id, token.Kind, userId, label);
        return (plain, ToDto(token));
    }

    public async Task<List<IdentityTokenDto>> ListTokensAsync(string? userId, TokenKind? kind, string? currentPlainToken = null)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var query = ctx.Tokens.AsNoTracking().AsQueryable();
        if (userId is not null) query = query.Where(t => t.UserId == userId);
        if (kind is { } k) query = query.Where(t => t.Kind == k);
        var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        var currentHash = string.IsNullOrEmpty(currentPlainToken) ? null : HashToken(currentPlainToken);
        return list.Select(t =>
        {
            var dto = ToDto(t);
            if (currentHash != null && t.TokenHash == currentHash)
                dto.IsCurrent = true;
            return dto;
        }).ToList();
    }

    public async Task<IdentityToken?> FindTokenAsync(string tokenId)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        return await ctx.Tokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tokenId);
    }

    /// <summary>
    /// Borra físicamente los tokens revocados o expirados desde hace más de
    /// <paramref name="olderThan"/>. Devuelve cuántos se han eliminado.
    /// </summary>
    public async Task<int> PurgeOldTokensAsync(TimeSpan olderThan)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var threshold = DateTime.UtcNow.Subtract(olderThan);
        var stale = await ctx.Tokens.Where(t =>
            (t.IsRevoked && t.CreatedAt < threshold) ||
            (t.ExpiresAt != null && t.ExpiresAt < threshold)).ToListAsync();
        if (stale.Count == 0) return 0;
        ctx.Tokens.RemoveRange(stale);
        await ctx.SaveChangesAsync();
        Logger.LogInformation("Purga de tokens: eliminados {Count}", stale.Count);
        return stale.Count;
    }

    // ── Bootstrap ───────────────────────────────────────────────────────

    /// <summary>
    /// Crea un usuario admin si la base de datos está vacía. Pensado para
    /// llamarse en el arranque del proceso Identity.
    /// </summary>
    public async Task EnsureSeedAsync(string userName, string password, string displayName = "Admin")
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        if (await ctx.Users.AnyAsync()) return;

        var user = new IdentityUser { UserName = userName, DisplayName = displayName };
        user.PasswordHash = Hasher.HashPassword(user, password);
        ctx.Users.Add(user);
        ctx.UserRoles.Add(new IdentityUserRole { UserId = user.Id, Role = IdentityChannels.AdminRole });
        await ctx.SaveChangesAsync();
    }
}