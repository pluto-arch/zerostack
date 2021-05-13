﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace ZeroStack.DeviceCenter.Domain.Aggregates.TenantAggregate
{
    public class CurrentTenant : ICurrentTenant
    {
        private readonly ICurrentTenantAccessor _currentTenantAccessor;

        public virtual bool IsAvailable => Id.HasValue;

        public CurrentTenant(ICurrentTenantAccessor currentTenantAccessor) => _currentTenantAccessor = currentTenantAccessor;

        public virtual Guid? Id => _currentTenantAccessor.Current?.TenantId;

        public string? Name => _currentTenantAccessor.Current?.Name;

        public IDisposable Change(Guid? id, string? name = null)
        {
            var parentScope = _currentTenantAccessor.Current;
            _currentTenantAccessor.Current = new TenantInfo(id, name);

            return new DisposeAction(() =>
            {
                _currentTenantAccessor.Current = parentScope;
            });
        }

        public class DisposeAction : IDisposable
        {
            private readonly Action _action;

            public DisposeAction([NotNull] Action action) => _action = action;

            void IDisposable.Dispose()
            {
                _action();
                GC.SuppressFinalize(this);
            }
        }
    }
}
