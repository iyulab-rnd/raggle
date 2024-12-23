﻿using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Tools;
using Raggle.Server.Services;
using System;
using System.ComponentModel;

namespace Raggle.Server.ToolKits;

public class VectorSearchTool
{
    private readonly IServiceProvider _provider;

    public VectorSearchTool(IServiceProvider provider)
    {
        _provider = provider;
    }

    [FunctionTool("vector_search", "search vectordb for internel information")]
    public async Task<IEnumerable<ScoredVectorPoint>> SearchVectorAsync(
        [Description("memory collectionName")] string collectionName,
        [Description("search query")] string query)
    {
        using var scope = _provider.CreateScope();
        var memory = scope.ServiceProvider.GetRequiredService<MemoryService>();
        var points = await memory.SearchDocumentAsync(collectionName, query);
        return points;
    }
}
