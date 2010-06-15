using System;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Framework.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal abstract class PipelineStepAttribute : PipelineElementAttribute
    {
        private bool _logBefore;
        public bool LogBefore
        {
            get { return _logBefore; }
            set { _logBefore = value; }
        }

        private bool _logAfter;
        public bool LogAfter
        {
            get { return _logAfter; }
            set { _logAfter = value; }
        }

        public override bool LogDisabled
        {
            get { return !_logBefore && !_logAfter; }
            set
            {
                value.AssertTrue();
                _logBefore = false;
                _logAfter = false;
            }
        }

        public override bool LogEnabled
        {
            get { return _logBefore || _logAfter; }
            set
            {
                value.AssertTrue();
                _logBefore = true;
                _logAfter = true;
            }
        }
    }
}