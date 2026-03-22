using System.ComponentModel.DataAnnotations;

namespace SentinelAssetsAPI.Models;

public class Asset
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(45)]
    public string? IPAddress { get; set; }

    [StringLength(100)]
    public string? VulnerabilityStatus { get; set; } = "OK"; // OK, LOW, MEDIUM, HIGH, CRITICAL

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastScanned { get; set; }
}

