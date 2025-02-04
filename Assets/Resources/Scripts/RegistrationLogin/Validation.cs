using System.Text.RegularExpressions;
using UnityEngine;


public class Validation : MonoBehaviour
{
    private static string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false; // Minimum 8 characters
        if (!Regex.IsMatch(password, @"\d")) return false; // At least one number
        if (!Regex.IsMatch(password, @"[\W_]")) return false; // At least one special character
        return true;
    }

    public static bool IsMatchingPassword(string password, string confirmPassword)
    {
        return password == confirmPassword;
    }

    public static bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= 2;
    }

    public static bool IsValidYearSection(string yearSection)
    {
        return !string.IsNullOrEmpty(yearSection);
    }

    public static bool IsDropdownSelected(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "Select Department" && value != "Select Program";
    }

    public static string ValidateRegistrationInputs(string firstName, string lastName, string email, string password, string confirmPassword, string yearSection, string department, string program)
    {
        if (!IsValidName(firstName) || !IsValidName(lastName))
            return "First name and last name must be at least 2 characters long.";

        if (!IsValidEmail(email))
            return "Invalid email format.";

        if (!IsValidPassword(password))
            return "Password must be at least 8 characters long, contain a number, and a special character.";

        if (!IsMatchingPassword(password, confirmPassword))
            return "Passwords do not match.";

        if (!IsValidYearSection(yearSection))
            return "Please enter a valid year section.";

        if (!IsDropdownSelected(department) || !IsDropdownSelected(program))
            return "Please select a valid department and program.";

        return null; 
    }
}
