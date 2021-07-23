// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BasicTestApp
{
    public class PreserveStateService : IDisposable
    {
        private readonly ComponentApplicationState _componentApplicationState;

        private ServiceState _state = new();

        public PreserveStateService(ComponentApplicationState componentApplicationState)
        {
            _componentApplicationState = componentApplicationState;
            _componentApplicationState.OnPersisting += PersistState;
            TryRestoreState();
        }

        public Guid Guid => _state.TheState;

        private void TryRestoreState()
        {
            if (_componentApplicationState.TryTakeAsJson<ServiceState>("Service", out var state))
            {
                _state = state;
            }
            else
            {
                _state = new ServiceState { TheState = Guid.NewGuid() };
            }
        }

        public void NewState() => _state = new ServiceState { TheState = Guid.NewGuid() };

        private Task PersistState()
        {
            _componentApplicationState.PersistAsJson("Service", _state);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _componentApplicationState.OnPersisting -= PersistState;
        }

        private class ServiceState
        {
            public Guid TheState { get; set; }
        }
    }
}
