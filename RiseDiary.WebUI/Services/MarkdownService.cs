using Markdig;
using Microsoft.AspNetCore.Components;

namespace RiseDiary.WebUI.Services;

public sealed class MarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public MarkupString ToHtml(string? markdownString) => string.IsNullOrWhiteSpace(markdownString)
        ? (MarkupString)string.Empty
        : (MarkupString)Markdown.ToHtml(markdownString, _pipeline);
}
