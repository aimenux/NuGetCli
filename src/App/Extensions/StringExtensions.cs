namespace App.Extensions;

public static class StringExtensions
{
    public static bool IgnoreCaseEquals(this string input, string key)
    {
        return string.Equals(input, key, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IgnoreCaseContains(this string input, string key)
    {
        if (input is null || key is null) return input == key;
        return input.Contains(key, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IgnoreCaseStartWith(this string input, string key)
    {
        if (input is null || key is null) return input == key;
        return input.StartsWith(key, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IgnoreCaseEndsWith(this string input, string key)
    {
        if (input is null || key is null) return input == key;
        return input.EndsWith(key, StringComparison.OrdinalIgnoreCase);
    }
}