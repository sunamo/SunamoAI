namespace SunamoAI;

#if !NET5_0_OR_GREATER

internal static class Net48CollectionPolyfillExtensions
{
    internal static bool TryAdd<TKey, TValue>(this System.Collections.Generic.Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }
        dictionary.Add(key, value);
        return true;
    }
}

internal static class Net48StringPolyfillExtensions
{
    internal static string[] Split(this string text, string separator, System.StringSplitOptions options = System.StringSplitOptions.None)
    {
        return text.Split(new[] { separator }, options);
    }

    internal static string[] Split(this string text, char separator, System.StringSplitOptions options)
    {
        return text.Split(new[] { separator }, options);
    }

    internal static string Replace(this string text, string oldValue, string? newValue, System.StringComparison comparisonType)
    {
        if (oldValue == null)
        {
            throw new System.ArgumentNullException(nameof(oldValue));
        }
        if (oldValue.Length == 0)
        {
            throw new System.ArgumentException("String cannot be of zero length.", nameof(oldValue));
        }
        newValue ??= string.Empty;

        var result = new System.Text.StringBuilder(text.Length);
        int previousIndex = 0;
        int foundIndex = text.IndexOf(oldValue, comparisonType);
        while (foundIndex >= 0)
        {
            result.Append(text, previousIndex, foundIndex - previousIndex);
            result.Append(newValue);
            previousIndex = foundIndex + oldValue.Length;
            foundIndex = text.IndexOf(oldValue, previousIndex, comparisonType);
        }
        result.Append(text, previousIndex, text.Length - previousIndex);
        return result.ToString();
    }

    internal static bool StartsWith(this string text, char value)
    {
        return text.Length != 0 && text[0] == value;
    }

    internal static bool EndsWith(this string text, char value)
    {
        return text.Length != 0 && text[text.Length - 1] == value;
    }

    internal static bool Contains(this string text, char value)
    {
        return text.IndexOf(value) >= 0;
    }

    internal static bool Contains(this string text, string value, System.StringComparison comparisonType)
    {
        return text.IndexOf(value, comparisonType) >= 0;
    }

    internal static string ReplaceLineEndings(this string text)
    {
        return text.ReplaceLineEndings(System.Environment.NewLine);
    }

    internal static string ReplaceLineEndings(this string text, string replacementText)
    {
        string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        return replacementText == "\n" ? normalized : normalized.Replace("\n", replacementText);
    }
}

#endif
