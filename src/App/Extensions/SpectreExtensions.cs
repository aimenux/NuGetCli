using Spectre.Console;

namespace App.Extensions;

public static class SpectreExtensions
{
    public static readonly Markup ErrorMarkup = new(Emoji.Known.CrossMark);

    public static Markup ToMarkup(this string text)
    {
        try
        {
            return new Markup(text ?? string.Empty);
        }
        catch
        {
            return ErrorMarkup;
        }
    }

    public static T WithStyle<T>(this T obj, bool anyFailure) where T : class, IAlignable
    {
        return anyFailure ? obj.LeftAligned() : obj.Centered();
    }
}