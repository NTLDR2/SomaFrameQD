using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace SomaFrameQD.Modules.CodeCompiler
{
    /// <summary>
    /// Compiles C# syntax trees in-process using Roslyn (<c>Microsoft.CodeAnalysis.CSharp</c>) and loads
    /// the emitted assembly directly into the current <see cref="System.AppDomain"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>In-Process Execution &amp; .NET Framework 4.8 <c>AppDomain</c> Assembly Retention:</b>
    /// Implementations emit compiled IL into an in-memory PE buffer and call <see cref="Assembly.Load(byte[])"/>
    /// on <see cref="System.AppDomain.CurrentDomain"/> so generated types can interact directly with host WPF
    /// ViewModels, controls, and 3D/graphics objects without cross-domain <see cref="System.MarshalByRefObject"/>
    /// serialization boundaries.
    /// </para>
    /// <para>
    /// Unlike .NET Core's collectible <c>AssemblyLoadContext</c>, the .NET Framework 4.8 CLR pins assemblies loaded
    /// into the default <see cref="System.AppDomain"/> until process exit (forcing <see cref="System.GC.Collect()"/>
    /// cannot unload them). Callers that recompile frequently (such as live script or shader/3D editors) should
    /// cache <see cref="MetadataReference"/> instances, debounce keystroke-driven compilations, and skip recompiling
    /// unchanged source text.
    /// </para>
    /// </remarks>
    public interface ICodeCompiler
    {
        /// <summary>
        /// Compiles the specified C# <paramref name="syntaxTrees"/> against <paramref name="references"/> into an
        /// in-memory dynamic library named <paramref name="outputName"/>, reporting build status to <c>IOutput</c>
        /// and diagnostics to <c>IErrorList</c>.
        /// </summary>
        /// <param name="syntaxTrees">The Roslyn C# syntax trees to compile.</param>
        /// <param name="references">Metadata references required by the compilation.</param>
        /// <param name="outputName">Logical assembly name for the emitted dynamic library.</param>
        /// <returns>
        /// The loaded <see cref="Assembly"/> when compilation succeeds; otherwise, <see langword="null"/> after
        /// populating <c>IErrorList</c> with compiler diagnostics.
        /// </returns>
        Assembly Compile(
            IEnumerable<SyntaxTree> syntaxTrees,
            IEnumerable<MetadataReference> references,
            string outputName);
    }
}
