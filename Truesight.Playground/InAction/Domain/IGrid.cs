namespace Truesight.Playground.InAction.Domain
{
    internal interface IGrid
    {
        dim3 GridDim { get; }
        dim3 BlockDim { get; }
    }
}