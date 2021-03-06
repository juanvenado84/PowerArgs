﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerArgs.Cli
{
    /// <summary>
    /// An object that has a beginning and and end  that can be used to define the lifespan of event and observable subscriptions.
    /// </summary>
    public class Lifetime : Disposable, ILifetimeManager
    {
        private LifetimeManager _manager;

        private static Lifetime forever = new Lifetime();

        /// <summary>
        /// The forever lifetime manager that will never end. Any subscriptions you intend to keep forever should use this lifetime so it's easy to spot leaks.
        /// </summary>
        public static LifetimeManager Forever => forever._manager;

        /// <summary>
        /// If true then this lifetime has already ended
        /// </summary>
        public bool IsExpired
        {
            get
            {
                return _manager == null;
            }
        }
        
        public Lifetime()
        {
            _manager = new LifetimeManager();
        }

        public async Task AwaitEndOfLifetime()
        {
            while(IsExpired == false)
            {
                await Task.Delay(10);
            }
        }

        public Promise OnDisposed(Action cleanupCode)
        {
            if (IsExpired == false)
            {
                return _manager.OnDisposed(cleanupCode);
            }
            else
            {
                cleanupCode();
                var d = Deferred.Create();
                d.Resolve();
                return d.Promise;
            }
        }
        public Promise OnDisposed(IDisposable cleanupCode)
        {
            if (IsExpired == false)
            {
                return _manager.OnDisposed(cleanupCode);
            }
            else
            {
                cleanupCode.Dispose();
                var d = Deferred.Create();
                d.Resolve();
                return d.Promise;
            }
        }

        public static Lifetime EarliestOf(params Lifetime[] others)
        {
            return EarliestOf((IEnumerable<Lifetime>)others);
        }

        public static Lifetime EarliestOf(IEnumerable<Lifetime> others)
        {
            Lifetime ret = new Lifetime();
            foreach (var other in others)
            {
                other.OnDisposed(() =>
                {
                    if(ret.IsExpired == false)
                    {
                        ret.Dispose();
                    }
                });
            }
            return ret;
        }

        public Lifetime CreateChildLifetime()
        {
            var ret = new Lifetime();
            _manager.OnDisposed(()=>
            {
                if(ret.IsExpired == false)
                {
                    ret.Dispose();
                }
            });
            return ret;
        }

        protected override void DisposeManagedResources()
        {
            if (!IsExpired)
            {
                foreach (var item in _manager.ManagedItems)
                {
                    item.Dispose();
                }
                _manager = null;
            }
        }
    }
}
