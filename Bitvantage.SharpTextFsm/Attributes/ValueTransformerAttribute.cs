using System.Text.RegularExpressions;

namespace Bitvantage.SharpTextFsm.Attributes;

public enum MatchDisposition
{
    Stop,
    Continue,
    Skip
}

public enum MatchMethod
{
    Full,
    Substring
}

public enum MatchMode
{
    Literal,
    Regex
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ValueTransformerAttribute : Attribute
{
    private readonly string _oldValue;
    private readonly string? _newValue;

    private readonly Regex? _regex;

    private readonly MatchMode _matchMode;
    private readonly MatchMethod _matchMethod;
    private readonly MatchDisposition _matchDisposition;

    public ValueTransformerAttribute(string oldValue, string? newValue, MatchMode matchMode = MatchMode.Literal, MatchMethod matchMethod = MatchMethod.Full, MatchDisposition matchDisposition = MatchDisposition.Stop)
    {
        if (matchMode == MatchMode.Regex)
            _regex = new Regex(oldValue, RegexOptions.Compiled);

        _matchDisposition = matchDisposition;
        _oldValue = oldValue;
        _newValue = newValue;
        _matchMode = matchMode;
        _matchMethod = matchMethod;
    }

    internal static string? Transform(ValueTransformerAttribute[] transforms, string value)
    {
        var currentValue = value;

        foreach (var transform in transforms)
            if (transform.Transform(currentValue, out currentValue))
            {
                if (transform._matchDisposition == MatchDisposition.Stop)
                    return currentValue;

                if (transform._matchDisposition == MatchDisposition.Skip)
                    return null;
            }

        return currentValue;
    }

    private bool Transform(string value, out string? newValue)
    {
        switch (_matchMode)
        {
            case MatchMode.Literal:
                switch (_matchMethod)
                {
                    case MatchMethod.Full:
                        if (value == _oldValue)
                        {
                            newValue = _newValue;
                            return true;
                        }

                        newValue = value;
                        return false;

                    case MatchMethod.Substring:
                        if (value.Contains(_oldValue))
                        {
                            newValue = value.Replace(_oldValue, _newValue);
                            return true;
                        }

                        newValue = value;
                        return false;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            case MatchMode.Regex:
                switch (_matchMethod)
                {
                    case MatchMethod.Full:
                        var match = _regex!.Match(value);
                        if (match.Success)
                        {
                            newValue = _newValue;
                            return true;
                        }

                        newValue = value;
                        return false;

                    case MatchMethod.Substring:
                        match = _regex!.Match(value);
                        if (match.Success)
                        {
                            newValue = _regex.Replace(value, _newValue ?? string.Empty);
                            return true;
                        }

                        newValue = value;
                        return false;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}