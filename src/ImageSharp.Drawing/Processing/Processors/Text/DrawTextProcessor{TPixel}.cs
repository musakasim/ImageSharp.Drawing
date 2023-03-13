// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace SixLabors.ImageSharp.Drawing.Processing.Processors.Text
{
    /// <summary>
    /// Using the brush as a source of pixels colors blends the brush color with source.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class DrawTextProcessor<TPixel> : ImageProcessor<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private CachingGlyphRenderer textRenderer;
        private readonly DrawTextProcessor definition;

        public DrawTextProcessor(Configuration configuration, DrawTextProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
            : base(configuration, source, sourceRectangle)
            => this.definition = definition;

        protected override void BeforeImageApply()
        {
            base.BeforeImageApply();

            // Do everything at the image level as we are delegating the processing down to other processors
            this.textRenderer = new CachingGlyphRenderer(
                this.Configuration.MemoryAllocator,
                this.definition.Text.GetGraphemeCount(),
                this.definition.TextOptions,
                this.definition.Pen,
                this.definition.Brush,
                this.definition.DrawingOptions.Transform)
            {
                Options = this.definition.DrawingOptions
            };

            TextRenderer renderer = new(this.textRenderer);
            renderer.RenderText(this.definition.Text, this.definition.TextOptions);
        }

        protected override void AfterImageApply()
        {
            base.AfterImageApply();
            this.textRenderer?.Dispose();
            this.textRenderer = null;
        }

        /// <inheritdoc/>
        protected override void OnFrameApply(ImageFrame<TPixel> source)
        {
            void Draw(IEnumerable<DrawingOperation> operations)
            {
                foreach (DrawingOperation operation in operations)
                {
                    using BrushApplicator<TPixel> app = operation.Brush.CreateApplicator(
                             this.Configuration,
                             this.textRenderer.Options.GraphicsOptions,
                             source,
                             this.SourceRectangle);

                    Buffer2D<float> buffer = operation.Map;
                    int startY = operation.Location.Y;
                    int startX = operation.Location.X;
                    int offsetSpan = 0;

                    if (startX + buffer.Height < 0)
                    {
                        continue;
                    }

                    if (startX + buffer.Width < 0)
                    {
                        continue;
                    }

                    if (startX < 0)
                    {
                        offsetSpan = -startX;
                        startX = 0;
                    }

                    if (startX >= source.Width)
                    {
                        continue;
                    }

                    int firstRow = 0;
                    if (startY < 0)
                    {
                        firstRow = -startY;
                    }

                    int maxHeight = source.Height - startY;
                    int end = Math.Min(operation.Map.Height, maxHeight);

                    for (int row = firstRow; row < end; row++)
                    {
                        int y = startY + row;
                        Span<float> span = buffer.DangerousGetRowSpan(row).Slice(offsetSpan);
                        app.Apply(span, startX, y);
                    }
                }
            }

            if (this.textRenderer.DrawingOperations.Count > 0)
            {
                Draw(this.textRenderer.DrawingOperations.OrderBy(x => x.RenderPass));
            }
        }
    }
}
