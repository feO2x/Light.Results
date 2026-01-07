using System.Collections.Generic;

namespace Light.Results;

public readonly record struct Error(
    string Message,
    string? Code = null,
    string? Target = null,
    IReadOnlyDictionary<string, object?>? Meta = null
);
