namespace Truesight.Playground.InAction.Domain
{
    internal interface IGridApi
    {
        dim3 GridDim { get; }
        int3 BlockIdx { get; }
        dim3 BlockDim { get; }
        int3 ThreadIdx { get; }
    }
}