namespace Truesight.Decompiler.Domains
{
    // todo. when adding new language here be sure to:
    // 1) check what steps of the pipeline no longer work
    // 2) implement mechanism to mark pipeline steps as semantics-specific
    // 3) reimplement certain steps of the pipeline for the new language

    public enum Language
    {
        CSharp35,
    }
}
