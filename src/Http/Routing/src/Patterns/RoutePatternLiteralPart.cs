﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    /// <summary>
    /// Resprents a literal text part of a route pattern. Instances of <see cref="RoutePatternLiteralPart"/>
    /// are immutable.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public sealed class RoutePatternLiteralPart : RoutePatternPart
    {
        internal RoutePatternLiteralPart(string content)
            : base(RoutePatternPartKind.Literal)
        {
            Debug.Assert(!string.IsNullOrEmpty(content));
            Content = content;
        }

        /// <summary>
        /// Gets the text content.
        /// </summary>
        public string Content { get; }

        internal override string DebuggerToString()
        {
            return Content;
        }
    }
}
