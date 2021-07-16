// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Connections.Features
{
    /// <summary>
    /// Explicitly abort one direction of a connection stream.
    /// </summary>
    public interface IStreamAbortFeature
    {
        /// <summary>
        /// Abort reading from the connection stream.
        /// </summary>
        /// <param name="abortReason">An optional <see cref="ConnectionAbortedException"/> describing the reason to abort reading from the connection stream.</param>
        void AbortRead(ConnectionAbortedException abortReason);

        /// <summary>
        /// Abort writing to the connection stream.
        /// </summary>
        /// <param name="abortReason">An optional <see cref="ConnectionAbortedException"/> describing the reason to abort writing to the connection stream.</param>
        void AbortWrite(ConnectionAbortedException abortReason);
    }
}
