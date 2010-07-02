namespace Truesight.Decompiler.Hir
{
    public enum NodeType
    {
        // special
        Null,

        // control flow
        Block,
        Break,
        Catch,
        Continue,
        Finally,
        Goto,
        If,
        Iter,
        Label,
        Loop,
        Return,
        Throw,
        Try,
        Using,

        // expressions
        Addr,
        Assign,
        CollectionInit,
        Conditional,
        Const,
        Convert,
        Default,
        Deref,
        Fld,
        Loophole,
        ObjectInit,
        Operator,
        Prop,
        Ref,
        SizeOf,
        TypeAs,
        TypeIs,

        // functional
        Apply,
        Eval,
        Lambda,
    }
}