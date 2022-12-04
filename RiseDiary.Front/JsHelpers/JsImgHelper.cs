using Microsoft.JSInterop;

namespace RiseDiary.Front.JsHelpers;

public class ImgDimensions
{
    public int Width { get; set; }

    public int Height { get; set; }

    public int GetBiggest() => Width > Height ? Width : Height;
}

public class JsImgHelper : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public JsImgHelper(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/images-helper.js").AsTask());
    }

    public async Task<ImgDimensions> GetImgDimensions(Stream imageDataStream)
    {
        var module = await moduleTask.Value;
        using var streamRef = new DotNetStreamReference(imageDataStream, true);

        return await module.InvokeAsync<ImgDimensions>("getImageDimensions", streamRef);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
