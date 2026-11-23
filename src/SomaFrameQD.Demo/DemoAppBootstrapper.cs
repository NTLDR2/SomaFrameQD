namespace SomaFrameQD.Demo
{
    /// <summary>
    /// Showcase bootstrapper for <c>SomaFrameQD.Demo</c> on .NET Framework 4.8 (<c>x64</c>).
    /// Inherits two-tier MEF priority resolution, filtered <c>BaseDirectory</c> plugin discovery,
    /// and Multi-Core Background JIT startup profiling from <see cref="AppBootstrapper"/>.
    /// </summary>
    public class DemoAppBootstrapper : AppBootstrapper
    {
        protected override string JitProfileFileName => "SomaFrameQD.Demo.Startup.net48.jitprofile";
    }
}
