﻿namespace Raggle.Abstractions.Embedding;

public class EmbeddingsRequest
{
    /// <summary>
    /// The Embedding Model Name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The input text to embed.
    /// </summary>
    public IEnumerable<string> Input { get; set; } = [];
}
