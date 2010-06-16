using Truesight.Playground.InAction.Domain;

namespace Truesight.Playground.InAction.Runtime
{
    internal interface IBlockRunner<T1, T2, T3>
    {
        void RunBlock(int3 blockIdx, T1 arg1, T2 arg2, T3 arg3);
    }
}