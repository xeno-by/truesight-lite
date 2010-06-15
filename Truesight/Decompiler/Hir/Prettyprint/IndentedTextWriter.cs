using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    [DebuggerNonUserCode]
    public class IndentedTextWriter : TextWriter
    {
        public const String DefaultTabString = "    ";
        private int indentLevel;
        private bool tabsPending;
        private String tabString;
        private TextWriter writer;

        public IndentedTextWriter(TextWriter writer) 
            : this(writer, "    ")
        {
        }

        public IndentedTextWriter(TextWriter writer, String tabString) 
            : base(CultureInfo.InvariantCulture)
        {
            this.writer = writer;
            this.tabString = tabString;
            this.indentLevel = 0;
            this.tabsPending = true;
        }

        public override void Close()
        {
            this.writer.Close();
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        internal void InternalOutputTabs()
        {
            for (var i = 0; i < this.indentLevel; i++)
            {
                this.writer.Write(this.tabString);
            }
        }

        protected virtual void OutputTabs()
        {
            if (this.tabsPending)
            {
                for (var i = 0; i < this.indentLevel; i++)
                {
                    this.writer.Write(this.tabString);
                }
                this.tabsPending = false;
            }
        }

        public override void Write(bool value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(char value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            this.OutputTabs();
            this.writer.Write(buffer);
        }

        public override void Write(double value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(int value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(long value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(Object value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(float value)
        {
            this.OutputTabs();
            this.writer.Write(value);
        }

        public override void Write(String s)
        {
            this.OutputTabs();
            this.writer.Write(s);
        }

        public override void Write(String format, Object arg0)
        {
            this.OutputTabs();
            this.writer.Write(format, arg0);
        }

        public override void Write(String format, params Object[] arg)
        {
            this.OutputTabs();
            this.writer.Write(format, arg);
        }

        public override void Write(String format, Object arg0, Object arg1)
        {
            this.OutputTabs();
            this.writer.Write(format, arg0, arg1);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.OutputTabs();
            this.writer.Write(buffer, index, count);
        }

        public override void WriteLine()
        {
            this.OutputTabs();
            this.writer.WriteLine();
            this.tabsPending = true;
        }

        public override void WriteLine(bool value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(char value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(double value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer)
        {
            this.OutputTabs();
            this.writer.WriteLine(buffer);
            this.tabsPending = true;
        }

        public override void WriteLine(int value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(long value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(Object value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(float value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(String s)
        {
            this.OutputTabs();
            this.writer.WriteLine(s);
            this.tabsPending = true;
        }

        public override void WriteLine(uint value)
        {
            this.OutputTabs();
            this.writer.WriteLine(value);
            this.tabsPending = true;
        }

        public override void WriteLine(String format, Object arg0)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg0);
            this.tabsPending = true;
        }

        public override void WriteLine(String format, params Object[] arg)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg);
            this.tabsPending = true;
        }

        public override void WriteLine(String format, Object arg0, Object arg1)
        {
            this.OutputTabs();
            this.writer.WriteLine(format, arg0, arg1);
            this.tabsPending = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this.OutputTabs();
            this.writer.WriteLine(buffer, index, count);
            this.tabsPending = true;
        }

        public void WriteLineNoTabs(String s)
        {
            this.writer.WriteLine(s);
        }

        public override Encoding Encoding
        {
            get
            {
                return this.writer.Encoding;
            }
        }

        public int Indent
        {
            get { return this.indentLevel; }
            set { if (value < 0) { value = 0; } this.indentLevel = value; }
        }

        public TextWriter InnerWriter
        {
            get
            {
                return this.writer;
            }
        }

        public override String NewLine
        {
            get
            {
                return this.writer.NewLine;
            }
            set
            {
                this.writer.NewLine = value;
            }
        }

        internal String TabString
        {
            get
            {
                return this.tabString;
            }
        }
    }
}
