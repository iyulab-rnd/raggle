﻿using Raggle.Abstractions.Json;

namespace Raggle.Abstractions.ChatCompletion.Tools;

public interface ITool
{
    string Name { get; set; }
    string? Description { get; set; }
    IDictionary<string, JsonSchema>? Parameters { get; set; }
    IEnumerable<string>? Required { get; set; }

    Task<ToolResult> InvokeAsync(ToolArguments? arguments);
}
