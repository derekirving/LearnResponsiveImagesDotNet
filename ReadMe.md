# LearnResponsiveImagesDotNet

This provides a tag helper for creating a `srcset` of responsive images in **webp** and **jpg**. The node version will also create **avif** files.

The ideal image size is 2048px width by 1152px height. This creates a 16:9 widescreen ratio (2048 × 1152 = 16:9).

**There are 2 versions of this and the first is recommended.**

## Version 1 - Node

Images are created at build time based on the breakpoints of '576, 768, 992, 1200, 1400' (Bootstrap's default).

```bash
cd client
bun install
bun run start
```

This will create responsive images for all png and jpg files in `wwwroot/img` into a `wwwroot/img/responsive` folder.

Use the tag helper:

```bash
<picture class="img-fluid" src="growth.jpg" alt="An image depicting growth"></picture>
```

Include Child content

```bash
<figure class="figure">
    <picture class="img-fluid" src="growth.jpg" alt="An image depicting growth">
        <figcaption class="figure-caption">A caption for the above image.</figcaption>
    </picture>
</figure>

```

## Version 2 - .NET

Images are created at runtime. **Only use this version if you can't use the node version**.

The tag helper will create a subset of images based on the breakpoints of '576, 768, 992, 1200, 1400' (Bootstrap's default).

To use the tag helper:

```csharp
<pic class="img-fluid" src="growth.jpg" alt="An image depicting growth"/>
```

This will create a responsive image for `wwwroot/img/growth.jpg` into a `wwwroot/img/responsive` folder. If the responsive image alreadt exists, the creation process is skipped.

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

In both cases, `IHttpContextAccessor` will need to be available:

```bash
builder.Services.AddHttpContextAccessor();
```

### Cachebusting

In node, the image file content is hashed and included in  wwwroot/img/responsive/img-manifest.json file.

In the taghelper, the hash lookup takes 0.02ms per request.

This is the most performant approach, perfect for production sites of any scale.


