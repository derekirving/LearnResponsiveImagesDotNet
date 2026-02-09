# LearnResponsiveImagesDotNet

This provides a tag helper for creating a `srcset` of responsive images in webp and jpg.

The ideal image size is 2048px width by 1152px height. This creates a 16:9 widescreen ratio (2048 × 1152 = 16:9).

The tag helper will create a subset of images based on the breakpoints of '576, 768, 992, 1200, 1400' (Bootstrap's default).

To use the tag helper:

```csharp
<pic class="img-fluid" src="growth.jpg" alt="An image depicting growth"/>
```

The output will match the generated images: 

```html
<picture>
    <source
        srcset="/responsive/growth-1400.webp 1400w, /responsive/growth-1200.webp 1200w, /responsive/growth-992.webp 992w, /responsive/growth-768.webp 768w,  /responsive/growth-576.webp 576w"
        type="image/webp">
    <img alt="An image depicting growth" src="/responsive/growth.jpg"
        srcset="/responsive/growth-1400.jpg 1400w, /responsive/growth-1200.jpg 1200w, /responsive/growth-992.jpg 992w, /responsive/growth-768.jpg 768w, /responsive/growth-576.jpg 576w">
</picture>
```

