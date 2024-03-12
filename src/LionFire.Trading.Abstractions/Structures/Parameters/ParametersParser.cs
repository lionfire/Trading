#nullable enable

using System.Text;

namespace LionFire.Trading;

public static class ParametersParser
{
    public static int ParseIntParameter(string key, char startSeparator = '(', char endSeparator = ')', char escapeCharacter = '\\')
        => int.Parse(TryGetParameters(key, startSeparator, endSeparator, escapeCharacter) ?? throw new ArgumentException());

    public static uint ParseUintParameter(string key, char startSeparator = '(', char endSeparator = ')', char escapeCharacter = '\\')
        => uint.Parse(TryGetParameters(key, startSeparator, endSeparator, escapeCharacter) ?? throw new ArgumentException());

    public static double ParseDoubleParameter(string key, char startSeparator = '(', char endSeparator = ')', char escapeCharacter = '\\')
        => double.Parse(TryGetParameters(key, startSeparator, endSeparator, escapeCharacter) ?? throw new ArgumentException());

    public static string[] GetParametersArray(string key, char startSeparator = '(', char endSeparator = ')', char escapeCharacter = '\\', int minCount = 0, int maxCount = int.MaxValue)
    {
        var parametersString = TryGetParameters(key, startSeparator, endSeparator, escapeCharacter);
        if (parametersString == null)
        {
            if (minCount > 0) throw new ArgumentException($"Got 0 parameters, but minCount is {minCount}");
            return [];
        }
        var parameters = parametersString.Split(',');
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = parameters[i].Trim();
        }
        if (parameters.Length < minCount) throw new ArgumentException($"Got {parameters.Length} parameters, but minCount is {minCount}");
        if (parameters.Length > maxCount) throw new ArgumentException($"Got {parameters.Length} parameters, but maxCount is {maxCount}");
        return parameters;
    }

    public static string? TryGetParameters(string key, char startSeparator = '(', char endSeparator = ')', char escapeCharacter = '\\')
    {
        var index = key.IndexOf(startSeparator);

        int depth = 1;

        var sb = new StringBuilder();

        bool escaped = false;
        for (int i = index + 1; i < key.Length; i++)
        {
            if (key[i] == startSeparator && !escaped) depth++;
            if (key[i] == endSeparator && !escaped) depth--;
            if (depth == 0) break;
            if (key[i] == escapeCharacter)
            {
                if (escaped) sb.Append(escapeCharacter);
                else escaped = true;
                continue;
            }
            sb.Append(key[i]);
        }

        return sb.ToString();
    }

}
