namespace Truesight.Playground.InAction.Domain
{
    internal class Grid : IGrid
    {
        public dim3 GridDim { get; set; }
        public dim3 BlockDim { get; set; }
    }
}