using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Respond.TagHelpers;

/// <summary>
/// Tag Helper that generates responsive picture elements with multiple image formats and sizes
/// </summary>
[HtmlTargetElement("picture", Attributes = "src")]
public class PictureElementTagHelper : TagHelper
{
    private static readonly int[] Widths = { 1400, 1200, 992, 768, 576 };
    private static readonly string[] Formats = { "avif", "webp" };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public PictureElementTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
        
    /// <summary>
    /// The source image filename (e.g., "growth.jpg")
    /// </summary>
    [HtmlAttributeName("src")]
    public string Src { get; set; } = string.Empty;

    /// <summary>
    /// Alt text for the image
    /// </summary>
    [HtmlAttributeName("alt")]
    public string Alt { get; set; } = string.Empty;

    /// <summary>
    /// CSS class(es) to apply to the img element
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// ID to apply to the img element
    /// </summary>
    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Relative path for generated images (default: "img/responsive")
    /// Will be combined with Request.PathBase automatically
    /// </summary>
    [HtmlAttributeName("base-path")]
    public string BasePath { get; set; } = "img/responsive";

    /// <summary>
    /// Loading attribute for the img element (lazy, eager, auto)
    /// </summary>
    [HtmlAttributeName("loading")]
    public string? Loading { get; set; }

    /// <summary>
    /// Whether to include AVIF format (default: true)
    /// </summary>
    [HtmlAttributeName("include-avif")]
    public bool IncludeAvif { get; set; } = true;

    /// <summary>
    /// Whether to include WebP format (default: true)
    /// </summary>
    [HtmlAttributeName("include-webp")]
    public bool IncludeWebp { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Src))
        {
            output.SuppressOutput();
            return;
        }

        // Extract filename without extension
        var lastDot = Src.LastIndexOf('.');
        var fileName = lastDot > 0 ? Src.Substring(0, lastDot) : Src;
        var extension = lastDot > 0 ? Src.Substring(lastDot) : ".jpg";

        // Build the full base path using Request.PathBase
        var pathBase = _httpContextAccessor.HttpContext?.Request.PathBase.Value ?? "";
        var normalizedBasePath = BasePath.TrimStart('/').TrimEnd('/');
        var basePath = string.IsNullOrEmpty(pathBase) 
            ? $"/{normalizedBasePath}" 
            : $"{pathBase}/{normalizedBasePath}";

        output.TagName = "picture";
        output.TagMode = TagMode.StartTagAndEndTag;
            
        // Remove the attributes we've processed
        output.Attributes.RemoveAll("src");
        output.Attributes.RemoveAll("alt");
        output.Attributes.RemoveAll("class");
        output.Attributes.RemoveAll("id");
        output.Attributes.RemoveAll("base-path");
        output.Attributes.RemoveAll("loading");
        output.Attributes.RemoveAll("include-avif");
        output.Attributes.RemoveAll("include-webp");

        var contentBuilder = new StringBuilder();

        // Add AVIF source if enabled
        if (IncludeAvif)
        {
            contentBuilder.AppendLine();
            contentBuilder.Append("    <source");
            contentBuilder.AppendLine();
            contentBuilder.Append($"        srcset=\"{BuildSrcset(basePath, fileName, "avif")}\"");
            contentBuilder.AppendLine();
            contentBuilder.Append("        type=\"image/avif\">");
        }

        // Add WebP source if enabled
        if (IncludeWebp)
        {
            contentBuilder.AppendLine();
            contentBuilder.Append("    <source");
            contentBuilder.AppendLine();
            contentBuilder.Append($"        srcset=\"{BuildSrcset(basePath, fileName, "webp")}\"");
            contentBuilder.AppendLine();
            contentBuilder.Append("        type=\"image/webp\">");
        }

        // Add img element with JPG fallback
        contentBuilder.AppendLine();
        contentBuilder.Append("    <img");
            
        if (!string.IsNullOrWhiteSpace(Alt))
        {
            contentBuilder.Append($" alt=\"{System.Net.WebUtility.HtmlEncode(Alt)}\"");
        }
            
        contentBuilder.Append($" src=\"{basePath}/{fileName}{extension}\"");
        contentBuilder.AppendLine();
        contentBuilder.Append($"        srcset=\"{BuildSrcset(basePath, fileName, "jpg")}\"");

        if (!string.IsNullOrWhiteSpace(Id))
        {
            contentBuilder.AppendLine();
            contentBuilder.Append($"        id=\"{System.Net.WebUtility.HtmlEncode(Id)}\"");
        }

        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            contentBuilder.AppendLine();
            contentBuilder.Append($"        class=\"{System.Net.WebUtility.HtmlEncode(CssClass)}\"");
        }

        if (!string.IsNullOrWhiteSpace(Loading))
        {
            contentBuilder.AppendLine();
            contentBuilder.Append($"        loading=\"{System.Net.WebUtility.HtmlEncode(Loading)}\"");
        }

        contentBuilder.Append(">");
        contentBuilder.AppendLine();

        output.Content.SetHtmlContent(contentBuilder.ToString());
    }

    private string BuildSrcset(string basePath, string fileName, string format)
    {
        var srcsetItems = new List<string>();

        foreach (var width in Widths)
        {
            srcsetItems.Add($"{basePath}/{fileName}-{width}.{format} {width}w");
        }

        return string.Join(", ", srcsetItems);
    }
}