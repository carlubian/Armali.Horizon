using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Contracts.Identity;

namespace Armali.Horizon.Identity.Model;

/// <summary>
/// Usuario local del sistema. Las credenciales se almacenan hasheadas con
/// PBKDF2 (a través de <c>Microsoft.AspNetCore.Identity.PasswordHasher</c>).
/// </summary>
public class IdentityUser
{
  [Key]
  public string Id { get; set; } = Guid.NewGuid().ToString("N");

  [Required, MaxLength(64)]
  public string UserName { get; set; } = string.Empty;

  [MaxLength(128)]
  public string DisplayName { get; set; } = string.Empty;

  /// <summary>Hash PBKDF2 + salt + iter (formato de PasswordHasher).</summary>
  [Required]
  public string PasswordHash { get; set; } = string.Empty;

  public bool IsActive { get; set; } = true;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Asociación usuario ↔ rol. Los roles son strings libres validados sólo por
/// convención (no hay tabla maestra para mantenerlo simple).
/// </summary>
public class IdentityUserRole
{
  public int Id { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;
  [ForeignKey(nameof(UserId))]
  public IdentityUser? User { get; set; }

  [Required, MaxLength(64)]
  public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Token bearer emitido por Identity. El campo <see cref="TokenHash"/> guarda
/// SHA-256(token); el valor en claro sólo se devuelve al crearlo.
/// <para>
/// Se distinguen sesiones (volátiles, generadas en login) de API keys
/// (permanentes, creadas explícitamente) mediante <see cref="Kind"/>.
/// </para>
/// </summary>
public class IdentityToken
{
  [Key]
  public string Id { get; set; } = Guid.NewGuid().ToString("N");

  [Required]
  public string UserId { get; set; } = string.Empty;
  [ForeignKey(nameof(UserId))]
  public IdentityUser? User { get; set; }

  /// <summary>SHA-256 hex del token en claro.</summary>
  [Required, MaxLength(64)]
  public string TokenHash { get; set; } = string.Empty;

  [MaxLength(128)]
  public string Label { get; set; } = string.Empty;

  public TokenKind Kind { get; set; } = TokenKind.Session;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? LastUsedAt { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public bool IsRevoked { get; set; }
}
