// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Drawing.Processing
{
    /// <summary>
    /// Provides a pen that can apply a pattern to a line with a set brush and thickness
    /// </summary>
    /// <remarks>
    /// The pattern will be in to the form of new float[]{ 1f, 2f, 0.5f} this will be
    /// converted into a pattern that is 3.5 times longer that the width with 3 sections
    /// section 1 will be width long (making a square) and will be filled by the brush
    /// section 2 will be width * 2 long and will be empty
    /// section 3 will be width/2 long and will be filled
    /// the pattern will immediately repeat without gap.
    /// </remarks>
    public sealed class Pen
    {
        private readonly float[] pattern;

        public Pen(Brush strokeFill)
            : this(strokeFill, 0)
        {
        }

        public Pen(Brush strokeFill, float strokeWidth)
            : this(strokeFill, strokeWidth, Pens.EmptyPattern)
        {
        }

        public Pen(PenOptions options)
            : this(options.StrokeFill, options.StrokeWidth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pen"/> class.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="width">The width.</param>
        /// <param name="pattern">The pattern.</param>
        private Pen(Brush brush, float width, float[] pattern)
        {
            Guard.MustBeGreaterThan(width, 0, nameof(width));

            this.StrokeFill = brush;
            this.StrokeWidth = width;
            this.pattern = pattern;
        }

        /// <inheritdoc/>
        public Brush StrokeFill { get; }

        /// <inheritdoc/>
        public float StrokeWidth { get; }

        /// <inheritdoc/>
        public ReadOnlySpan<float> StrokePattern => this.pattern;

        /// <inheritdoc/>
        public JointStyle JointStyle { get; set; }

        /// <inheritdoc/>
        public EndCapStyle EndCapStyle { get; set; }
    }
}
