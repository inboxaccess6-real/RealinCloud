namespace RealinApi.Common;

public static class ValidationExtensions
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Basic validation - starts with + and has 10-15 digits
        return phoneNumber.StartsWith('+') && 
               phoneNumber.Length >= 11 && 
               phoneNumber.Length <= 16 &&
               phoneNumber[1..].All(char.IsDigit);
    }
}
