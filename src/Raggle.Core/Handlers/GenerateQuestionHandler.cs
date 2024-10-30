﻿using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Messages;
using Raggle.Core.Document;
using Raggle.Core.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace Raggle.Core.Handlers;

public class GenerateQuestionHandler : IPipelineHandler
{
    private static readonly Regex QuestionRegex = new Regex(@"<question>\s*(.*?)\s*</question>", RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IDocumentStorage _documentStorage;
    private readonly IChatCompletionService _chatService;
    private readonly ChatCompletionOptions _chatOptions;

    public GenerateQuestionHandler(
        IDocumentStorage documentStorage,
        IChatCompletionService chatService,
        ChatCompletionOptions chatOptions)
    {
        _documentStorage = documentStorage;
        _chatService = chatService;
        _chatOptions = chatOptions;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        foreach (var chunk in chunks)
        {
            string fullText;
            if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
                fullText = chunk.SummarizedText;
            else if (!string.IsNullOrWhiteSpace(chunk.RawText))
                fullText = chunk.RawText;
            else
                throw new InvalidOperationException("No text content found in the document chunk.");

            var questions = await GenerateQuestionsAsync(fullText, cancellationToken);
            chunk.ExtractedQuestions = questions;
            await UpdateDocumentChunkAsync(pipeline, chunk, cancellationToken);
        }

        return pipeline;
    }

    #region Private Methods

    private async Task<IEnumerable<DocumentChunk>> GetDocumentChunksAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            pipeline.Document.CollectionName,
            pipeline.Document.DocumentId,
            cancellationToken);
        var chunkFilePaths = filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));

        var chunks = new List<DocumentChunk>();
        foreach (var chunkFilePath in chunkFilePaths)
        {
            var chunkStream = await _documentStorage.ReadDocumentFileAsync(
                pipeline.Document.CollectionName,
                pipeline.Document.DocumentId,
                chunkFilePath,
                cancellationToken);

            var chunk = JsonDocumentSerializer.Deserialize<DocumentChunk>(chunkStream);
            chunks.Add(chunk);
        }
        return chunks;
    }

    private async Task UpdateDocumentChunkAsync(DataPipeline pipeline, DocumentChunk chunk, CancellationToken cancellationToken)
    {
        var filename = DocumentFileHelper.GetChunkedFileName(pipeline.Document.FileName, chunk.ChunkIndex);
        var chunkStream = JsonDocumentSerializer.SerializeToStream(chunk);
        await _documentStorage.WriteDocumentFileAsync(
            pipeline.Document.CollectionName,
            pipeline.Document.DocumentId,
            filename,
            chunkStream,
            overwrite: true,
            cancellationToken);
    }

    private async Task<IEnumerable<string>> GenerateQuestionsAsync(string text, CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddUserMessage(new TextContentBlock
        {
            Text = $"Generate questions for this information: \n\n{text}",
        });
        _chatOptions.System = GetSystemInstruction();
        var response = await _chatService.ChatCompletionAsync(history, _chatOptions);
        if (response.State == ChatResponseState.Stop)
        {
            var textAnswer = new StringBuilder();
            foreach(var content in response.Contents)
            {
                if (content is TextContentBlock textContent)
                {
                    textAnswer.AppendLine(textContent.Text);
                }
            }
            var answer = textAnswer.ToString();
            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException("Failed to generate questions.");
            }
            return ParseQuestionsFromTags(answer);
        }
        else
        {
            throw new InvalidOperationException("Failed to generate questions.");
        }
    }

    private IEnumerable<string> ParseQuestionsFromTags(string text)
    {
        var questions = new List<string>();
        var matches = QuestionRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var question = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(question))
                {
                    questions.Add(question);
                }
            }
        }

        if (!questions.Any())
        {
            throw new FormatException("No questions found within <question> tags.");
        }

        return questions;
    }

    private string GetSystemInstruction()
    {
        return """
        You are an expert assistant specialized in generating insightful and objective questions based on the provided information.
        Please analyze the given text and generate a series of relevant questions that can be used for further analysis or discussion. 
        Each question should be enclosed within <question> and </question> tags.
        Ensure that the questions are clear, concise, and directly related to the information provided. Avoid ambiguous or overly broad questions.
        
        ### Example Information
        [information]
        On July 20, 1969, Apollo 11 successfully landed the first humans on the Moon, marking a significant achievement in space exploration.

        ### Example Response:
        
        <question>When did Apollo 11 land on the Moon?</question>
        <question>What was the significance of Apollo 11's Moon landing?</question>
        <question>Who were the first humans to walk on the Moon?</question>
        """;
    }

    #endregion
}
