using System;
using System.Collections.Generic;
using System.Text;

namespace ModernLearn;

public static class IndentationHelper
{
    public static string RemoveBaseIndent(this string text, bool trimOuterEmptyLines = true)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');

        var baseIndent = "";
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
                i++;

            baseIndent = line[..i];
            break;
        }

        if (baseIndent.Length == 0)
            return JoinLines(lines, trimOuterEmptyLines);

        for (var idx = 0; idx < lines.Length; idx++)
        {
            var line = lines[idx];
            if (line.Length == 0)
                continue;

            var remove = 0;
            while (remove < baseIndent.Length &&
                   remove < line.Length &&
                   line[remove] == baseIndent[remove])
            {
                remove++;
            }

            lines[idx] = line[remove..];
        }

        return JoinLines(lines, trimOuterEmptyLines);
    }

    private static string JoinLines(string[] lines, bool trimOuterEmptyLines)
    {
        if (!trimOuterEmptyLines)
            return string.Join(Environment.NewLine, lines);

        var start = 0;
        while (start < lines.Length && string.IsNullOrWhiteSpace(lines[start]))
            start++;

        var end = lines.Length - 1;
        while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
            end--;

        if (end < start)
            return string.Empty;

        var count = end - start + 1;
        var slice = new string[count];
        Array.Copy(lines, start, slice, 0, count);

        return string.Join(Environment.NewLine, slice);
    }
}