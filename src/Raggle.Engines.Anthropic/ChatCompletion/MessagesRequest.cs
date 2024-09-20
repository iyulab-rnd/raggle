﻿using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class MessagesRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public required Message[] Messages { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("tool_choice")]
    public ToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stop_sequences")]
    public string[]? StopSequences { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
}
