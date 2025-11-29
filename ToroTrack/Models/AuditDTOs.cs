using System;

namespace ToroTrack.Models
{
    // View Model for displaying Audit Logs
    public class AuditLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = "System";
        public string UserRole { get; set; } = "System"; // e.g. Admin, Client
        public string Category { get; set; } = "System"; // Security, DataChange, System
        public string ActionType { get; set; } = "";
        public string Details { get; set; } = "";
        public string IpAddress { get; set; } = "";
    }
}