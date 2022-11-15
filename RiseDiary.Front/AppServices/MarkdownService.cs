using Markdig;
using Microsoft.AspNetCore.Components;

namespace RiseDiary.Front.AppServices;

public sealed class MarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public MarkupString ToHtml(string? markdownString) => string.IsNullOrWhiteSpace(markdownString)
        ? (MarkupString)string.Empty
        : (MarkupString)Markdown.ToHtml(markdownString, _pipeline);
}
