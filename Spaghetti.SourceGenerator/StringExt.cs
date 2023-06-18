using System.Text;

namespace Spaghetti.SourceGenerator;

internal static class StringExt
{
    private static readonly char[] s_Delimeters = { ' ', '-', '_' };

    private static string SymbolsPipe(
        string source,
        char mainDelimeter,
        Func<char, bool, char[]> newWordSymbolHandler)
    {
        var builder = new StringBuilder();

        var nextSymbolStartsNewWord = true;
        var disableFrontDelimeter = true;

        for (var i = 0; i < source.Length; i++)
        {
            var symbol = source[i];

            if (s_Delimeters.Contains(symbol))
            {
                if (symbol == mainDelimeter)
                {
                    builder.Append(symbol);
                    disableFrontDelimeter = true;
                }

                nextSymbolStartsNewWord = true;
            }
            else if (!char.IsLetter(symbol))
            {
                builder.Append(symbol);
                disableFrontDelimeter = true;
                nextSymbolStartsNewWord = true;
            }
            else
            {
                if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                {
                    builder.Append(newWordSymbolHandler(symbol, disableFrontDelimeter));
                    disableFrontDelimeter = false;
                    nextSymbolStartsNewWord = false;
                }
                else
                {
                    builder.Append(symbol);
                }
            }
        }

        return builder.ToString();
    }

    public static string ToCamelCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '\0',
            (s, disableFrontDelimeter) =>
            {
                return disableFrontDelimeter ? new[] { char.ToLowerInvariant(s) } : new[] { char.ToUpperInvariant(s) };
            });
    }

    public static string ToKebabCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '-',
            (s, disableFrontDelimeter) =>
            {
                return disableFrontDelimeter ? new[] { char.ToLowerInvariant(s) } : new[] { '-', char.ToLowerInvariant(s) };
            });
    }

    public static string ToPascalCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '\0',
            (s, _) => new[] { char.ToUpperInvariant(s) });
    }

    public static string ToSnakeCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '_',
            (s, disableFrontDelimeter) =>
            {
                return disableFrontDelimeter ? new[] { char.ToLowerInvariant(s) } : new[] { '_', char.ToLowerInvariant(s) };
            });
    }

    public static string ToTrainCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '-',
            (s, disableFrontDelimeter) =>
            {
                return disableFrontDelimeter ? new[] { char.ToUpperInvariant(s) } : new[] { '-', char.ToUpperInvariant(s) };
            });
    }
}
