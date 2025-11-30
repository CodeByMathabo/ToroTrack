using System.Text;

namespace ToroTrack.Business.Helpers
{
    // Why: Centralized password generation logic to be reused by InviteService and ForgotPassword
    public static class PasswordHelper
    {
        public static string GenerateRandomPassword()
        {
            // Why: Microsoft Identity requires at least 1 Upper, 1 Lower, 1 Digit, 1 Non-Alphanumeric
            const string lowers = "abcdefghijklmnopqrstuvwxyz";
            const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*";

            var allChars = lowers + uppers + digits + specials;
            var random = new Random();
            var password = new StringBuilder();

            // Ensure one of each requirement is met first
            password.Append(lowers[random.Next(lowers.Length)]);
            password.Append(uppers[random.Next(uppers.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specials[random.Next(specials.Length)]);

            // Fill the rest to reach length of 12
            while (password.Length < 12)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the result so the required chars aren't always at the start
            return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
        }
    }
}