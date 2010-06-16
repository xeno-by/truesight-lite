namespace Truesight.Playground.InAction.Domain
{
    internal interface IKernel<T1, T2, T3> : IGridApi, ISyncApi
    {
        void RunKernel(T1 arg1, T2 arg2, T3 arg3);
    }
}