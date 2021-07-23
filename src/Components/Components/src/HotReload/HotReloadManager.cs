// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components.HotReload;

[assembly: MetadataUpdateHandler(typeof(HotReloadManager))]

namespace Microsoft.AspNetCore.Components.HotReload
{
    internal static class HotReloadManager
    {
       internal static event Action? OnDeltaApplied;

        public static void DeltaApplied()
        {
            OnDeltaApplied?.Invoke();
        }

        public static void UpdateApplication(Type[]? _) => OnDeltaApplied?.Invoke();
    }
}
