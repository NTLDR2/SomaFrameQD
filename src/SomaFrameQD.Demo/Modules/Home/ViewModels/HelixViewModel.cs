using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SomaFrameQD.Demo.Modules.Home.Views;
using SomaFrameQD.Framework;
using SomaFrameQD.Modules.CodeCompiler;

namespace SomaFrameQD.Demo.Modules.Home.ViewModels
{
    /// <summary>
    /// Interactive 3D HelixToolkit viewport document paired with a live Roslyn C# script editor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Roslyn Assembly Lifecycle on .NET Framework 4.8 (<c>net48</c>):</b>
    /// Each successful call to <see cref="ICodeCompiler.Compile"/> emits an in-memory PE assembly and loads it
    /// into <see cref="AppDomain.CurrentDomain"/> via <see cref="Assembly.Load(byte[])"/> so <see cref="IDemoScript"/>
    /// instances can mutate <see cref="HelixViewModel"/> properties directly at VSync frame rates without
    /// cross-<see cref="AppDomain"/> remoting overhead. Because the .NET Framework 4.8 CLR retains assemblies
    /// loaded into the default <see cref="AppDomain"/> until process exit, compiling synchronously on every raw
    /// keystroke would both stall the UI thread and accumulate unnecessary dynamic assemblies.
    /// </para>
    /// <para>
    /// To keep live scripting responsive and memory-frugal across Windows 7 SP1 through Windows 11, this ViewModel:
    /// (1) caches all Roslyn <see cref="MetadataReference"/> objects once per process in <see cref="CachedScriptReferences"/>,
    /// (2) debounces <c>TextEditor.TextChanged</c> events through a <c>350ms</c> <see cref="DispatcherTimer"/>,
    /// (3) skips recompilation whenever the source text matches <c>_lastCompiledSource</c>, and
    /// (4) subscribes to <see cref="CompositionTarget.Rendering"/> only while the document tab is active.
    /// </para>
    /// </remarks>
    [Export(typeof(HelixViewModel))]
    public class HelixViewModel : Document
    {
        private static readonly TimeSpan ScriptCompileDebounceInterval = TimeSpan.FromMilliseconds(350);

        /// <summary>
        /// Process-wide cache of immutable Roslyn <see cref="MetadataReference"/> descriptors for live demo scripts,
        /// avoiding repeated PE header disk reads and duplicate <c>mscorlib.dll</c> entries across compilations.
        /// </summary>
        private static readonly Lazy<MetadataReference[]> CachedScriptReferences =
            new Lazy<MetadataReference[]>(CreateScriptMetadataReferences);

        private readonly ICodeCompiler _codeCompiler;
        private readonly List<IDemoScript> _scripts;
        private readonly DispatcherTimer _compileDebounceTimer;

        private IHelixView _helixView;
        private string _lastCompiledSource;
        private bool _isActiveDocument;
        private bool _isRenderingSubscribed;
        private int _scriptRevision;

        private Point3D _cameraPosition;
        [DisplayName("Camera Position"), Description("Position of the camera in 3D space"), Category("Camera")]
        public Point3D CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                NotifyOfPropertyChange(() => CameraPosition);
            }
        }

        private double _cameraFieldOfView;
        [DisplayName("Field of View"), Range(1.0, 180.0), Category("Camera")]
        public double CameraFieldOfView
        {
            get { return _cameraFieldOfView; }
            set
            {
                _cameraFieldOfView = value;
                NotifyOfPropertyChange(() => CameraFieldOfView);
            }
        }

        private Point3D _lightPosition;
        [DisplayName("Light Position")]
        public Point3D LightPosition
        {
            get { return _lightPosition; }
            set
            {
                _lightPosition = value;
                NotifyOfPropertyChange(() => LightPosition);
            }
        }

        private double _rotationAngle;
        [DisplayName("Rotation Angle"), ReadOnly(true)]
        public double RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                _rotationAngle = value;
                NotifyOfPropertyChange(() => RotationAngle);
            }
        }

        [ImportingConstructor]
        public HelixViewModel(ICodeCompiler codeCompiler)
        {
            DisplayName = "Helix";

            _codeCompiler = codeCompiler;
            _scripts = new List<IDemoScript>();
            _compileDebounceTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = ScriptCompileDebounceInterval
            };
            _compileDebounceTimer.Tick += OnCompileDebounceTimerTick;

            CameraPosition = new Point3D(6, 5, 4);
            CameraFieldOfView = 45;
            LightPosition = new Point3D(0, 5, 0);
            RotationAngle = 0;
        }

        protected override void OnViewLoaded(object view)
        {
            if (_helixView?.TextEditor != null)
            {
                _helixView.TextEditor.TextChanged -= OnScriptTextChanged;
            }

            _helixView = (IHelixView)view;

            _helixView.TextEditor.Text = @"public class MyClass : SomaFrameQD.Demo.Modules.Home.ViewModels.IDemoScript
{
    public void Execute(SomaFrameQD.Demo.Modules.Home.ViewModels.HelixViewModel viewModel)
    {
        viewModel.RotationAngle += 0.1;
    }
}
";

            _helixView.TextEditor.TextChanged += OnScriptTextChanged;
            CompileScripts();
            UpdateRenderingSubscription();

            base.OnViewLoaded(view);
        }

        protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            await base.OnActivatedAsync(cancellationToken);
            _isActiveDocument = true;
            UpdateRenderingSubscription();
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            // Detach CompositionTarget.Rendering on both tab switches (close == false) and closes (close == true)
            // so backgrounded 3D tabs consume zero VSync CPU/GPU cycles.
            _isActiveDocument = false;
            _compileDebounceTimer.Stop();
            UpdateRenderingSubscription();

            if (close && _helixView?.TextEditor != null)
            {
                _helixView.TextEditor.TextChanged -= OnScriptTextChanged;
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        private void OnScriptTextChanged(object sender, EventArgs e)
        {
            _compileDebounceTimer.Stop();
            _compileDebounceTimer.Start();
        }

        private void OnCompileDebounceTimerTick(object sender, EventArgs e)
        {
            _compileDebounceTimer.Stop();
            CompileScripts();
        }

        /// <summary>
        /// Compiles the current script buffer into an in-memory assembly using <see cref="CachedScriptReferences"/>
        /// when the editor text has changed since the last compilation, replacing the active <see cref="IDemoScript"/>
        /// instances in <c>_scripts</c>.
        /// </summary>
        private void CompileScripts()
        {
            var sourceText = _helixView?.TextEditor?.Text;
            if (sourceText == null || string.Equals(sourceText, _lastCompiledSource, StringComparison.Ordinal))
            {
                return;
            }

            _lastCompiledSource = sourceText;
            int revision = Interlocked.Increment(ref _scriptRevision);

            lock (_scripts)
            {
                _scripts.Clear();

                var newAssembly = _codeCompiler.Compile(
                    new[] { CSharpSyntaxTree.ParseText(sourceText) },
                    CachedScriptReferences.Value,
                    "SomaFrameQDDemoScript_" + revision);

                if (newAssembly != null)
                {
                    _scripts.AddRange(newAssembly.GetTypes()
                        .Where(x => typeof(IDemoScript).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
                        .Select(x => (IDemoScript)Activator.CreateInstance(x)));
                }
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            lock (_scripts)
            {
                for (int i = 0; i < _scripts.Count; i++)
                {
                    _scripts[i].Execute(this);
                }
            }
        }

        private void UpdateRenderingSubscription()
        {
            bool shouldSubscribe = _isActiveDocument && _helixView != null;
            if (shouldSubscribe == _isRenderingSubscribed)
                return;

            if (shouldSubscribe)
            {
                CompositionTarget.Rendering += OnRendering;
                _isRenderingSubscribed = true;
            }
            else
            {
                CompositionTarget.Rendering -= OnRendering;
                _isRenderingSubscribed = false;
            }
        }

        private static MetadataReference[] CreateScriptMetadataReferences()
        {
            var bclDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
            var candidatePaths = new List<string>
            {
                typeof(object).Assembly.Location,
                Path.Combine(bclDirectory, "System.dll"),
                Path.Combine(bclDirectory, "System.Core.dll"),
                typeof(IResult).Assembly.Location,
                typeof(AppBootstrapper).Assembly.Location,
                typeof(HelixViewModel).Assembly.Location
            };

            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrWhiteSpace(entryLocation))
            {
                candidatePaths.Add(entryLocation);
            }

            return candidatePaths
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                .ToArray();
        }
    }
}
