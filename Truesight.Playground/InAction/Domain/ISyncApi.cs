using System;

namespace Truesight.Playground.InAction.Domain
{
    internal interface ISyncApi
    {
        void SyncThreads(params Object[] keys);
    }
}