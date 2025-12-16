using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BaconBinary.Core;
using BaconBinary.Core.Enum;
using BaconBinary.Core.IO.Spr;
using BaconBinary.Core.Models;

namespace BaconBinary.ObjectEditor.UI.Services
{
    public class SpriteProvider
    {
        private readonly SprFile _sprFile;

        public SpriteProvider(SprFile sprFile)
        {
            _sprFile = sprFile;
        }

        public WriteableBitmap GetSpriteBitmap(uint spriteId)
        {
            if (spriteId == 0 || !_sprFile.Sprites.TryGetValue(spriteId, out var sprite)) 
                return null;
            
            var bitmap = new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);
            
            using (var buffer = bitmap.Lock())
            {
                byte[] decompressedPixels = sprite.GetPixels();
                
                unsafe
                {
                    fixed (byte* p = decompressedPixels)
                    {
                        long bufferSize = buffer.Size.Height * buffer.RowBytes;
                        Buffer.MemoryCopy(p, (void*)buffer.Address, bufferSize, decompressedPixels.Length);
                    }
                }
            }
            return bitmap;
        }

        public WriteableBitmap GetThingBitmap(ThingType thing, FrameGroupType groupType = FrameGroupType.Default, int frameIndex = 0)
        {
            return GetThingBitmapWithPattern(thing, groupType, frameIndex, 0, 0, 0);
        }

        public WriteableBitmap GetThingBitmapWithPattern(ThingType thing, FrameGroupType groupType, int frameIndex, int patternX, int patternY, int patternZ)
        {
            if (thing == null || !thing.FrameGroups.ContainsKey(groupType))
                return null;

            var group = thing.FrameGroups[groupType];
            
            int totalWidth = group.Width * 32;
            int totalHeight = group.Height * 32;

            if (thing.FrameIndex != null)
                frameIndex = thing.FrameIndex.Value;

            var bitmap = new WriteableBitmap(new PixelSize(totalWidth, totalHeight), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);

            using (var buffer = bitmap.Lock())
            {
                long bufferSize = buffer.Size.Height * buffer.RowBytes;
                unsafe { new Span<byte>((void*)buffer.Address, (int)bufferSize).Clear(); }

                for (int h = 0; h < group.Height; h++)
                {
                    for (int w = 0; w < group.Width; w++)
                    {
                        int indexW = group.Width - 1 - w;
                        int indexH = group.Height - 1 - h;

                        uint spriteId = group.GetSpriteId(frameIndex, patternX, patternY, patternZ, 0, indexW, indexH);

                        if (spriteId > 0 && _sprFile.Sprites.TryGetValue(spriteId, out var sprite))
                        {
                            byte[] decompressedPixels = sprite.GetPixels();
                            DrawSprite(decompressedPixels, buffer, w * 32, h * 32);
                        }
                    }
                }
            }
            return bitmap;
        }

        private unsafe void DrawSprite(byte[] source, ILockedFramebuffer dest, int startX, int startY)
        {
            byte* destPtr = (byte*)dest.Address;
            int destStride = dest.RowBytes;

            fixed (byte* sourcePtr = source)
            {
                for (int y = 0; y < 32; y++)
                {
                    void* destRow = destPtr + ((startY + y) * destStride) + (startX * 4);
                    void* sourceRow = sourcePtr + (y * 32 * 4);
                    Buffer.MemoryCopy(sourceRow, destRow, 32 * 4, 32 * 4);
                }
            }
        }
    }
}
