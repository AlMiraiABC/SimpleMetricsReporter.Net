namespace SimpleMetricsExporter.Helper
{
    internal static class Matcher
    {
        public static bool IsMatch(string input, string pattern)
        {
            return IsMatchCore(input.AsSpan(), pattern.AsSpan());
        }

        public static bool IsMatch(string input, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (pattern is not null && IsMatchCore(input.AsSpan(), pattern.AsSpan()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsMatch(string input, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            return IsIncluded(input, includePatterns, excludePatterns);
        }

        public static bool IsMatch(IEnumerable<string> inputs, string pattern)
        {
            return Execute(inputs, pattern) is not null;
        }

        public static bool IsMatch(IEnumerable<string> inputs, IEnumerable<string> patterns)
        {
            return Execute(inputs, patterns) is not null;
        }

        public static bool IsMatch(IEnumerable<string> inputs, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            return Execute(inputs, includePatterns, excludePatterns) is not null;
        }

        public static string? Execute(IEnumerable<string> inputs, string pattern)
        {
            foreach (var input in inputs)
            {
                if (input is not null && IsMatchCore(input.AsSpan(), pattern.AsSpan()))
                {
                    return input;
                }
            }

            return null;
        }

        public static string? Execute(IEnumerable<string> inputs, IEnumerable<string> patterns)
        {
            foreach (var input in inputs)
            {
                if (input is null)
                {
                    continue;
                }

                foreach (var pattern in patterns)
                {
                    if (pattern is not null && IsMatchCore(input.AsSpan(), pattern.AsSpan()))
                    {
                        return input;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<string> Execute(IEnumerable<string> inputs, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            foreach (var input in inputs)
            {
                if (input is not null && IsIncluded(input, includePatterns, excludePatterns))
                {
                    yield return input;
                }
            }
        }

        private static bool IsIncluded(string input, IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var included = IsMatchAny(input, includePatterns);
            if (!included)
            {
                return false;
            }

            return !IsMatchAny(input, excludePatterns);
        }

        private static bool IsMatchAny(string input, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (pattern is not null && IsMatchCore(input.AsSpan(), pattern.AsSpan()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatchCore(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
        {
            if (pattern.Length == 0)
            {
                return input.Length == 0;
            }

            if (pattern.Length == 1 && pattern[0] == '*')
            {
                return true;
            }

            var inputIndex = 0;
            var patternIndex = 0;
            var starIndex = -1;
            var starMatchInputIndex = 0;

            while (inputIndex < input.Length)
            {
                if (patternIndex < pattern.Length &&
                    (pattern[patternIndex] == '?' || pattern[patternIndex] == input[inputIndex]))
                {
                    inputIndex++;
                    patternIndex++;
                    continue;
                }

                if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex;
                    patternIndex++;
                    starMatchInputIndex = inputIndex;
                    continue;
                }

                if (starIndex != -1)
                {
                    patternIndex = starIndex + 1;
                    starMatchInputIndex++;
                    inputIndex = starMatchInputIndex;
                    continue;
                }

                return false;
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex == pattern.Length;
        }
    }
}
