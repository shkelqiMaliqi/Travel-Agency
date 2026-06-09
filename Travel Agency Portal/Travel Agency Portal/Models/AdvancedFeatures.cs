using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class UserMfaCode
{
    [Key]
    public int Mfa_Id { get; set; }
    public int U_Id { get; set; }
    public string U_Email { get; set; } = string.Empty;
    public string Code_Hash { get; set; } = string.Empty;
    public DateTime Expires_At { get; set; }
    public bool Is_Used { get; set; }
    public DateTime Created_At { get; set; }
}

public class AuditLogEntry
{
    [Key]
    public int Audit_Id { get; set; }
    public string Event_Type { get; set; } = string.Empty;
    public string? User_Email { get; set; }
    public int? U_Id { get; set; }
    public string Request_Path { get; set; } = string.Empty;
    public string Http_Method { get; set; } = string.Empty;
    public int Status_Code { get; set; }
    public string? Details { get; set; }
    public DateTime Created_At { get; set; }
}

public class MetricsSnapshot
{
    [Key]
    public int Snapshot_Id { get; set; }
    public int Users_Count { get; set; }
    public int Bookings_Count { get; set; }
    public int Packages_Count { get; set; }
    public int Unread_Messages_Count { get; set; }
    public DateTime Recorded_At { get; set; }
}

public class AnalyticsEventRequest
{
    [Required]
    public string EventName { get; set; } = string.Empty;

    public string? Category { get; set; }
    public string? Metadata { get; set; }
}

public class StorageUploadRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}

public class MfaDeliveryMessage
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
