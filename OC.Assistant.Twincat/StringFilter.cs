namespace OC.Assistant.Twincat;

/// <summary>
/// Matches against a wildcard filter.<br/><br/>
/// Supported patterns<br/>
/// foo   : contains<br/>
/// foo*  : starts with<br/>
/// *foo  : ends with<br/><br/>
///
/// Multiple filters can be combined with commas
/// </summary>
public sealed class StringFilter
{
    private enum Mode : byte { Contains, StartsWith, EndsWith }

    private readonly struct Part(string core, Mode mode)
    {
        public readonly string Core = core;
        public readonly Mode Mode = mode;
    }

    private readonly Part[] _parts;
    private readonly StringComparison _comparison;
    private readonly bool _matchAll;
    private readonly int _minLength;

    public StringFilter(string filter, bool ignoreCase = true)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _comparison = ignoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        
        var raw = filter.Split(',');
        var parts = new List<Part>(raw.Length);
        var minLength = int.MaxValue;

        foreach (var r in raw)
        {
            var p = r.Trim();
            if (p.Length == 0) continue;

            var star0 = p[0] == '*';
            var star1 = p.Length > 1 && p[^1] == '*';

            var core = p.Trim('*');
            if (core.Length == 0)
            {
                _matchAll = true;
                continue;
            }

            var mode = star1 ? Mode.StartsWith : star0 ? Mode.EndsWith : Mode.Contains;

            parts.Add(new Part(core, mode));
            if (core.Length < minLength) minLength = core.Length;
        }

        _parts = parts.ToArray();
        _minLength = _parts.Length > 0 ? minLength : 0;
    }

    /// <summary>
    /// Returns true if the input matches at least one filter part.
    /// </summary>
    public bool IsMatch(string? input)
    {
        if (input is null) return false;
        if (_matchAll) return true;
        if (input.Length < _minLength) return false;

        var parts = _parts;
        for (var i = 0; i < parts.Length; i++)
        {
            var core = parts[i].Core;
            
            switch (parts[i].Mode)
            {
                case Mode.Contains:
                    if (input.Length >= core.Length && input.IndexOf(core, _comparison) >= 0) return true;
                    break;

                case Mode.StartsWith:
                    if (input.Length >= core.Length && input.StartsWith(core, _comparison)) return true;
                    break;

                case Mode.EndsWith:
                    if (input.Length >= core.Length && input.EndsWith(core, _comparison)) return true;
                    break;
            }
        }
        return false;
    }
}