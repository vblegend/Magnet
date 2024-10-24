using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace Magnet
{
    /// <summary>
    /// compilation result
    /// </summary>
    public interface ICompileResult
    {
        /// <summary>
        /// Get compilation result
        /// </summary>
        bool Success { get; }


        /// <summary>
        /// Gets diagnostic information generated during compilation
        /// </summary>
        ImmutableArray<Diagnostic> Diagnostics { get; }
    }



    internal class CompileResult: ICompileResult
    {
        internal CompileResult(Boolean success, IEnumerable<Diagnostic> diagnostics)
        {
            this.Success = success;
            this.Diagnostics = diagnostics.ToImmutableArray();
        }

        public bool Success { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}
