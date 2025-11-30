namespace ToroTrack.Models
{
    // Wrapper class to hold both sections of the settings page
    public class ClientSettingsViewModel
    {
        public ClientPreferencesDTO Preferences { get; set; } = new();
        public ClientAccountDTO Account { get; set; } = new();
    }

    public class ClientPreferencesDTO
    {
        public bool NotifyOnMilestone { get; set; }
        public bool NotifyOnQueryReply { get; set; }
        public bool NotifyOnMeeting { get; set; }
    }

    public class ClientAccountDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}