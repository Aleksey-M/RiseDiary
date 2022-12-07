using Microsoft.JSInterop;
using RiseDiary.Shared.Images;

namespace RiseDiary.Front.JsComponents;

public sealed class ImageSelection
{
    public int SelLeft { get; set; }

    public int SelTop { get; set; }

    public int SelWidth { get; set; }

    public int SelHeight { get; set; }

    public int ImageNaturalHeight { get; set; }

    public int ImageNaturalWidth { get; set; }

    public int ImageHeight { get; set; }

    public int ImageWidth { get; set; }

    public CropImageDto CreateDto(Guid imageId)
    {
        var biggestDimensionSize = (double)Math.Max(ImageNaturalHeight, ImageNaturalWidth);
        var biggestViewDimensionSize = (double)Math.Max(ImageHeight, ImageWidth);
        var coef = biggestDimensionSize / biggestViewDimensionSize;

        var dto = new CropImageDto
        {
            ImageId = imageId,
            SourceImageHeight = ImageNaturalHeight,
            SourceImageWidth = ImageNaturalWidth,
            Left = (int)Math.Floor(SelLeft * coef),
            Top = (int)Math.Floor(SelTop * coef),
            Width = (int)Math.Floor(SelWidth * coef),
            Height = (int)Math.Floor(SelHeight * coef)
        };

        if (dto.Top + dto.Height >= dto.SourceImageHeight)
        {
            dto.Height = dto.SourceImageHeight - dto.Top;
        }

        if (dto.Left + dto.Width >= dto.SourceImageWidth)
        {
            dto.Width = dto.SourceImageWidth - dto.Left;
        }

        return dto;
    }
}

public sealed class JcropInstance
{
    public IJSObjectReference JcropRef { get; set; } = null!;

    public string ImageCssId { get; set; } = string.Empty;
}

public sealed class Jcrop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public Jcrop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/Jcrop.cs.js").AsTask());
    }

    private async Task<IJSObjectReference> LoadModule()
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();
        await module.InvokeVoidAsync("loadLib", callbacksJsRef);
        await callbacks.WaitTask;

        return module;
    }

    public async Task<JcropInstance> Init(string imageElementId)
    {        
        var module = await LoadModule();

        var callbacks = new CallbacksTask<IJSObjectReference>();
        using var callbacksJsRef = callbacks.CreateRefForJs();
        await module.InvokeVoidAsync("initJcrop", imageElementId, callbacksJsRef);

        var jcropRef = await callbacks.WaitTask;

        if (jcropRef == null) throw new JsException("Can't create jcrop js instance");

        return new JcropInstance
        {
            JcropRef = jcropRef,
            ImageCssId = imageElementId
        };
    }

    public async Task<ImageSelection> GetSelection(JcropInstance jcrop)
    {
        var module = await LoadModule();

        var callbacks = new CallbacksTask<ImageSelection>();
        using var callbacksJsRef = callbacks.CreateRefForJs();
        await module.InvokeVoidAsync("getSelection", jcrop.JcropRef, jcrop.ImageCssId, callbacksJsRef);

        return await callbacks.WaitTask;
    }

    public async Task DisposeJcropInstance(JcropInstance jcrop)
    {
        var module = await LoadModule();

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("disposeJcrop", jcrop.JcropRef, callbacksJsRef);
        await callbacks.WaitTask;
        await jcrop.JcropRef.DisposeAsync();
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
