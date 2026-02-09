#!/usr/bin/env bun

import sharp from 'sharp';
import { readdir, mkdir } from 'fs/promises';
import { join, parse, extname } from 'path';
import { existsSync } from 'fs';

const INPUT_DIR = '../wwwroot/img';
const OUTPUT_DIR = '../wwwroot/img/responsive';
const WIDTHS = [576, 768, 992, 1200, 1400];
const FORMATS = ['jpg', 'webp', 'avif'] as const;

interface ImageInfo {
    path: string;
    name: string;
    ext: string;
    width: number;
    height: number;
}

async function getImageInfo(imagePath: string): Promise<ImageInfo | null> {
    try {
        const metadata = await sharp(imagePath).metadata();
        const { name, ext } = parse(imagePath);

        if (!metadata.width || !metadata.height) {
            console.warn(`⚠️  Could not read dimensions for ${imagePath}`);
            return null;
        }

        return {
            path: imagePath,
            name,
            ext,
            width: metadata.width,
            height: metadata.height,
        };
    } catch (error) {
        console.error(`❌ Error reading ${imagePath}:`, error);
        return null;
    }
}

async function processImage(imageInfo: ImageInfo): Promise<void> {
    console.log(`\n📸 Processing: ${imageInfo.name}${imageInfo.ext}`);

    // Check if original image already exists in generated folder
    const standardOutputPath = join(OUTPUT_DIR, `${imageInfo.name}${imageInfo.ext}`);
    if (existsSync(standardOutputPath)) {
        console.log(`  ⏭️  Skipped - ${imageInfo.name}${imageInfo.ext} already exists in generated folder`);
        return;
    }

    const aspectRatio = imageInfo.height / imageInfo.width;

    // Process responsive images
    for (const width of WIDTHS) {
        const height = Math.round(width * aspectRatio);

        for (const format of FORMATS) {
            const outputFilename = `${imageInfo.name}-${width}.${format}`;
            const outputPath = join(OUTPUT_DIR, outputFilename);

            try {
                await sharp(imageInfo.path)
                    .resize(width, height, {
                        fit: 'inside',
                        withoutEnlargement: true,
                    })
                    .toFormat(format, {
                        quality: format === 'jpg' ? 85 : 80,
                    })
                    .toFile(outputPath);

                console.log(`  ✓ ${outputFilename} (${width}×${height})`);
            } catch (error) {
                console.error(`  ❌ Failed to create ${outputFilename}:`, error);
            }
        }
    }

    // Create 1024px JPG with original name
    const standardWidth = 1024;
    const standardHeight = Math.round(standardWidth * aspectRatio);

    try {
        await sharp(imageInfo.path)
            .resize(standardWidth, standardHeight, {
                fit: 'inside',
                withoutEnlargement: true,
            })
            .jpeg({ quality: 85 })
            .toFile(standardOutputPath);

        console.log(`  ✓ ${imageInfo.name}${imageInfo.ext} (${standardWidth}×${standardHeight}) - standard size`);
    } catch (error) {
        console.error(`  ❌ Failed to create standard size:`, error);
    }
}

async function main() {
    console.log('🖼️  Image Processing CLI\n');

    // Check if input directory exists
    if (!existsSync(INPUT_DIR)) {
        console.error(`❌ Input directory not found: ${INPUT_DIR}`);
        process.exit(1);
    }

    // Create output directory if it doesn't exist
    if (!existsSync(OUTPUT_DIR)) {
        console.log(`📁 Creating output directory: ${OUTPUT_DIR}`);
        await mkdir(OUTPUT_DIR, { recursive: true });
    }

    // Read all files from input directory
    const files = await readdir(INPUT_DIR);

    // Filter for jpg and png images
    const imageFiles = files.filter(file => {
        const ext = extname(file).toLowerCase();
        return ext === '.jpg' || ext === '.jpeg' || ext === '.png';
    });

    if (imageFiles.length === 0) {
        console.log('⚠️  No JPG or PNG images found in the input directory.');
        return;
    }

    console.log(`Found ${imageFiles.length} image(s) to process:\n`);

    // Get image info for all files
    const imageInfos: ImageInfo[] = [];
    for (const file of imageFiles) {
        const imagePath = join(INPUT_DIR, file);
        const info = await getImageInfo(imagePath);
        if (info) {
            imageInfos.push(info);
        }
    }

    // Process each image
    for (const imageInfo of imageInfos) {
        await processImage(imageInfo);
    }

    console.log('\n✅ Image processing complete!\n');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});