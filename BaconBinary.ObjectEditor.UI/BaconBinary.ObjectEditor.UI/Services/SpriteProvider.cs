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
        private readonly SprReader _sprReader;

        public SpriteProvider(SprFile sprFile)
        {
            _sprFile = sprFile;
            _sprReader = new SprReader();
        }

        public WriteableBitmap GetSpriteBitmap(uint spriteId)
        {
            if (spriteId == 0) return null;

            var bitmap = new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            
            using (var buffer = bitmap.Lock())
            {
                ClearBuffer(buffer, 32);
                byte[] compressedPixels = _sprReader.ExtractPixels(_sprFile, (int)spriteId);
                if (compressedPixels != null && compressedPixels.Length > 0)
                {
                    DrawSpriteLegacy(compressedPixels, buffer, 0, 0);
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

            var bitmap = new WriteableBitmap(new PixelSize(totalWidth, totalHeight), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);

            using (var buffer = bitmap.Lock())
            {
                ClearBuffer(buffer, totalHeight);

                for (int h = 0; h < group.Height; h++)
                {
                    for (int w = 0; w < group.Width; w++)
                    {
                        int indexW = group.Width - 1 - w;
                        int indexH = group.Height - 1 - h;

                        uint spriteId = group.GetSpriteId(frameIndex, patternX, patternY, patternZ, 0, indexW, indexH);

                        if (spriteId > 0)
                        {
                            byte[] compressedPixels = _sprReader.ExtractPixels(_sprFile, (int)spriteId);
                            
                            if (compressedPixels != null && compressedPixels.Length > 0)
                            {
                                DrawSpriteLegacy(compressedPixels, buffer, w * 32, h * 32);
                            }
                        }
                    }
                }
            }
            return bitmap;
        }

        private unsafe void ClearBuffer(ILockedFramebuffer buffer, int height)
        {
            byte* ptr = (byte*)buffer.Address;
            long totalBytes = buffer.RowBytes * height;
            new Span<byte>(ptr, (int)totalBytes).Clear();
        }

        private unsafe void DrawSpriteLegacy(byte[] data, ILockedFramebuffer buffer, int startX, int startY)
        {
            int pos = 0; 
            
            if (data.Length > 5 && data[0] == 0xFF && data[1] == 0x00 && data[2] == 0xFF)
            {
                ushort sizeCheck = BitConverter.ToUInt16(data, 3);
                if (sizeCheck == data.Length - 5)
                {
                    pos = 5; 
                }
            }

            bool useAlpha = ClientFeatures.Transparency; 
            int bitPerPixel = useAlpha ? 4 : 3; 

            int writeIndex = 0; 
            int length = data.Length;

            byte* destBase = (byte*)buffer.Address;
            int destStride = buffer.RowBytes;

            while (pos < length)
            {
                if (pos + 4 > length) break;

                ushort transparentPixels = BitConverter.ToUInt16(data, pos);
                pos += 2;
                ushort coloredPixels = BitConverter.ToUInt16(data, pos);
                pos += 2;

                writeIndex += transparentPixels;

                for (int i = 0; i < coloredPixels; i++)
                {
                    if (pos + 3 > length) break; 

                    byte red = data[pos++];
                    byte green = data[pos++];
                    byte blue = data[pos++];
                    byte alpha = useAlpha ? data[pos++] : (byte)0xFF;

                    WritePixel(destBase, destStride, startX, startY, writeIndex, blue, green, red, alpha);
                    writeIndex++;
                }
            }
        }

        private unsafe void WritePixel(byte* basePtr, int stride, int startX, int startY, int linearIndex, byte b, byte g, byte r, byte a)
        {
            if (linearIndex >= 1024) return;

            int localX = linearIndex % 32;
            int localY = linearIndex / 32;

            int globalX = startX + localX;
            int globalY = startY + localY;

            byte* pixelPtr = basePtr + (globalY * stride) + (globalX * 4);

            pixelPtr[0] = b; 
            pixelPtr[1] = g; 
            pixelPtr[2] = r; 
            pixelPtr[3] = a;
        }
    }
}
