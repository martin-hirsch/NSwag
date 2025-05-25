using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;
using NJsonSchema;

namespace NSwag.ConsoleApplication;

public class SuffixTypeNameGenerator(string suffix)
    : ITypeNameGenerator
{
    private readonly string _suffix = suffix;

    private readonly Dictionary<string, int> _typeNameCounts = [];

    protected virtual string Generate(JsonSchema schema, string? typeNameHint)
    {
        if (string.IsNullOrEmpty(typeNameHint) && schema.HasTypeNameTitle)
        {
            typeNameHint = schema.Title;
        }

        var lastSegment = GetLastSegment(typeNameHint);

        return ConversionUtilities.ConvertToUpperCamelCase(lastSegment ?? "Anonymous", true);
    }

    public virtual string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        if (string.IsNullOrEmpty(typeNameHint) && !string.IsNullOrEmpty(schema.DocumentPath))
        {
            var parts = schema.DocumentPath!.Replace("\\", "/").Split('/');
            typeNameHint = parts[^1];
        }

        typeNameHint ??= "";

        var requiresCleanup = typeNameHint.AsSpan().IndexOfAny(TypeNameHintCleanupChars) != -1;
        if (requiresCleanup)
        {
            typeNameHint = typeNameHint
                .Replace("[", " Of ")
                .Replace("]", " ")
                .Replace("<", " Of ")
                .Replace(">", " ")
                .Replace(",", " And ")
                .Replace("  ", " ");

            var parts = typeNameHint.Split(' ');
            typeNameHint = string.Join(separator: string.Empty, parts.Select(p => Generate(schema: schema, typeNameHint: p)));
        }

        var typeName = Generate(schema: schema, typeNameHint: typeNameHint);
        typeName = RemoveIllegalCharacters(typeName);

        if (string.IsNullOrEmpty(typeName) || reservedTypeNames.Contains(typeName))
        {
            typeName = GenerateAnonymousTypeName(typeNameHint: typeNameHint, reservedTypeNames: reservedTypeNames);
        }

        if (!typeName.EndsWith(value: suffix, comparisonType: StringComparison.Ordinal) && !typeName.EndsWith("Request", comparisonType: StringComparison.Ordinal))
        {
            typeName += suffix;
        }

        var baseTypeName = typeName;
        var finalTypeName = baseTypeName;

        if (reservedTypeNames.Contains(finalTypeName) || _reservedTypeNames.Contains(finalTypeName))
        {
            var count = _typeNameCounts.GetValueOrDefault(key: baseTypeName, 1);

            do
            {
                count++;
                finalTypeName = baseTypeName + count;
            } while (reservedTypeNames.Contains(finalTypeName) || _reservedTypeNames.Contains(finalTypeName));

            _typeNameCounts[baseTypeName] = count;
        }
        else
        {
            _typeNameCounts[baseTypeName] = 1;
        }

        return finalTypeName;
    }

    private static readonly char[] _typeNameHintCleanupChars = ['[', ']', '<', '>', ',', ' '];

#if NET8_0_OR_GREATER
    private static readonly SearchValues<char> TypeNameHintCleanupChars = SearchValues.Create(_typeNameHintCleanupChars);
#else
        private static readonly char[] TypeNameHintCleanupChars = _typeNameHintCleanupChars;
#endif

    private readonly Dictionary<string, string> _typeNameMappings = [];
    private string[] _reservedTypeNames = ["object"];

    // TODO: Expose as options to UI and cmd line?

    /// <summary>Gets or sets the reserved names.</summary>
    public IEnumerable<string> ReservedTypeNames
    {
        get => _reservedTypeNames;
        set => _reservedTypeNames = value.ToArray();
    }

    /// <summary>Gets the name mappings.</summary>
    public IDictionary<string, string> TypeNameMappings => _typeNameMappings;

    private string GenerateAnonymousTypeName(string typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        if (!string.IsNullOrEmpty(typeNameHint))
        {
            if (_typeNameMappings.TryGetValue(key: typeNameHint, out var mapping))
            {
                typeNameHint = mapping;
            }

            typeNameHint = GetLastSegment(typeNameHint)!;

            if (typeNameHint != null &&
                !reservedTypeNames.Contains(typeNameHint) &&
                Array.IndexOf(array: _reservedTypeNames, value: typeNameHint) == -1)
            {
                return typeNameHint;
            }

            var count = 1;
            string typeName;
            do
            {
                count++;
                typeName = ConversionUtilities.ConvertToUpperCamelCase(typeNameHint + count, true);
            } while (reservedTypeNames.Contains(typeName));

            return typeName;
        }

        return GenerateAnonymousTypeName("Anonymous", reservedTypeNames: reservedTypeNames);
    }

    private static string? GetLastSegment(string? input)
    {
        var lastSegment = input;
        if (input != null)
        {
            var index = input.LastIndexOf('.');
            if (index != -1)
            {
                lastSegment = input.Substring(index + 1);
            }
        }

        return lastSegment;
    }

    /// <summary>
    ///     Replaces all characters that are not normals letters, numbers or underscore, with an underscore.
    ///     Will prepend an underscore if the first characters is a number.
    ///     In case there are this would result in multiple underscores in a row, strips down to one underscore.
    ///     Will trim any underscores at the end of the type name.
    /// </summary>
    private static string RemoveIllegalCharacters(string typeName)
    {
        // TODO: Find a way to support unicode characters up to 3.0

        // first check if all are valid and we skip altogether
        var invalid = false;
        for (var i = 0; i < typeName.Length; i++)
        {
            var c = typeName[i];
            if (i == 0 && (!IsEnglishLetterOrUnderScore(c) || char.IsDigit(c)))
            {
                invalid = true;
                break;
            }

            if (!IsEnglishLetterOrUnderScore(c) && !char.IsDigit(c))
            {
                invalid = true;
                break;
            }
        }

        if (!invalid)
        {
            return typeName;
        }

        return DoRemoveIllegalCharacters(typeName);
    }

    private static string DoRemoveIllegalCharacters(string typeName)
    {
        var firstCharacter = typeName[0];
        var regexInvalidCharacters = new Regex("\\W");

        var legalTypeName = new StringBuilder(typeName);
        if (!IsEnglishLetterOrUnderScore(firstCharacter) || firstCharacter == '_')
        {
            if (!regexInvalidCharacters.IsMatch(firstCharacter.ToString()))
            {
                legalTypeName.Insert(0, "_");
            }
            else
            {
                legalTypeName[0] = '_';
            }
        }

        var illegalMatches = regexInvalidCharacters.Matches(legalTypeName.ToString());

        for (var i = illegalMatches.Count - 1; i >= 0; i--)
        {
            var illegalMatchIndex = illegalMatches[i].Index;
            legalTypeName[illegalMatchIndex] = '_';
        }

        var regexMoreThanOneUnderscore = new Regex("[_]{2,}");

        var legalTypeNameString = regexMoreThanOneUnderscore.Replace(legalTypeName.ToString(), "_");
        return legalTypeNameString.TrimEnd('_');
    }

    private static bool IsEnglishLetterOrUnderScore(char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
    }
}