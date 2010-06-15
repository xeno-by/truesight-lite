namespace Truesight.Decompiler.Pipeline
{
    internal enum Stages
    {
        ParseCIL = 100,
        BuildEarlyHirAndCfg = 200,
        PostprocessEarlyHir = 300,
        PostprocessCfg = 400,
        DecompileScopes = 500,
        PostprocessHir = 600,
    }
}
