using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.All)]
public sealed class ExperimentalAttribute : Attribute
{
    public string? Message { get; }

    public ExperimentalAttribute(string? message = null)
    {
        Message = message;

#if DEBUG
        Debug.WriteLine($"[Experimental]: {message}");
#endif
    }
}