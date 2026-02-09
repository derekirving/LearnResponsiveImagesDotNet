using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Respond.TagHelpers;

[HtmlTargetElement("pic")]
public class ResponsiveImageTagHelper(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    : TagHelper
{
    public required string Src { get; set; }
    public required string Alt { get; set; }

    private readonly int[] _breakpoints = [576, 768, 992, 1200, 1400]; // Bootstrap 5 default breakpoints
    private const int MainImageWidth = 1024;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var responsiveDir = Path.Combine(environment.WebRootPath, "responsive");
        Directory.CreateDirectory(responsiveDir);
        
        output.TagName = "picture";
        output.TagMode = TagMode.StartTagAndEndTag;

        var pathBase = httpContextAccessor.HttpContext?.Request.PathBase.Value ?? "";
        var imagePath = Path.Combine(environment.WebRootPath, Src);
        var baseFileName = Path.GetFileNameWithoutExtension(Src);

        EnsureResponsiveImagesExist(imagePath, responsiveDir, baseFileName);

        var webpSource = BuildWebPSource(pathBase, baseFileName);
        var imgTag = BuildImgTag(pathBase, baseFileName, Alt, output);

        output.Content.AppendHtml(webpSource);
        output.Content.AppendHtml(imgTag);
        
        output.Attributes.RemoveAll("id");
        output.Attributes.RemoveAll("class");
    }

    private void EnsureResponsiveImagesExist(string sourcePath, string outputDir, string baseFileName)
    {
        if (AllBreakpointImagesExist(outputDir, baseFileName))
            return;

        using var image = Image.Load(sourcePath);
        
        // Create main 1024px wide JPG version
        var mainJpgPath = Path.Combine(outputDir, $"{baseFileName}.jpg");
        if (!File.Exists(mainJpgPath))
        {
            ResizeAndSaveImage(image, mainJpgPath, MainImageWidth, isJpg: true);
        }
        
        // Create breakpoint images in both WebP and JPG formats
        foreach (var breakpoint in _breakpoints.Reverse())
        {
            var fileName = GetBreakpointFileName(baseFileName, breakpoint);
            var webpPath = Path.Combine(outputDir, $"{fileName}.webp");
            var jpgPath = Path.Combine(outputDir, $"{fileName}.jpg");

            if (!File.Exists(webpPath))
            {
                ResizeAndSaveImage(image, webpPath, breakpoint, isJpg: false);
            }
            
            if (!File.Exists(jpgPath))
            {
                ResizeAndSaveImage(image, jpgPath, breakpoint, isJpg: true);
            }
        }
    }

    private bool AllBreakpointImagesExist(string outputDir, string baseFileName)
    {
        // Check main JPG
        var mainJpgPath = Path.Combine(outputDir, $"{baseFileName}.jpg");
        if (!File.Exists(mainJpgPath))
            return false;
            
        foreach (var breakpoint in _breakpoints)
        {
            var fileName = GetBreakpointFileName(baseFileName, breakpoint);
            var webpPath = Path.Combine(outputDir, $"{fileName}.webp");
            var jpgPath = Path.Combine(outputDir, $"{fileName}.jpg");
            
            if (!File.Exists(webpPath) || !File.Exists(jpgPath))
                return false;
        }
        
        return true;
    }

    private static void ResizeAndSaveImage(Image image, string outputPath, int width, bool isJpg)
    {
        var clonedImage = image.Clone(x => x.Resize(width, 0));
        
        if (isJpg)
        {
            clonedImage.Save(outputPath, new JpegEncoder());
        }
        else
        {
            clonedImage.Save(outputPath, new WebpEncoder());
        }
    }

    private string BuildWebPSource(string pathBase, string baseFileName)
    {
        var sb = new StringBuilder("<source srcset=\"");
        
        foreach (var breakpoint in _breakpoints.Reverse())
        {
            var fileName = GetBreakpointFileName(baseFileName, breakpoint);
            sb.Append($"{pathBase}/responsive/{fileName}.webp {breakpoint}w, ");
        }

        // Remove trailing comma and space
        sb.Length -= 2;
        sb.Append("\" type=\"image/webp\">");
        
        return sb.ToString();
    }

    private string BuildImgTag(string pathBase, string baseFileName, string alt, TagHelperOutput output)
    {
        var idAttr = output.Attributes["id"];
        var classAttr = output.Attributes["class"];
        
        var existingId = idAttr?.Value?.ToString();
        var existingClass = classAttr?.Value?.ToString();

        var id = "";
        var @class = "";
        
        var sb = new StringBuilder("<img");

        if (!string.IsNullOrEmpty(existingId))
        {
            id= $" id=\"{existingId}\"";
        }

        if (!string.IsNullOrEmpty(existingClass))
        {
            @class = $" class=\"{existingClass}\"";
        }
        
        sb.Append($"{id}{@class} alt=\"{alt}\" src=\"{pathBase}/responsive/{baseFileName}.jpg\" srcset=\"");
        
        foreach (var breakpoint in _breakpoints.Reverse())
        {
            var fileName = GetBreakpointFileName(baseFileName, breakpoint);
            sb.Append($"{pathBase}/responsive/{fileName}.jpg {breakpoint}w, ");
        }
        
        sb.Length -= 2;
        sb.Append("\">");
        
        return sb.ToString();
    }

    private static string GetBreakpointFileName(string baseFileName, int breakpoint)
    {
        return $"{baseFileName}-{breakpoint}";
    }
}