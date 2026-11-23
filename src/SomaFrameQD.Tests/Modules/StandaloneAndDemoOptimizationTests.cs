using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SomaFrameQD.Demo.Modules.FilterDesigner.Util;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.GraphEditor.Controls;
using SomaFrameQD.Modules.Inspector.Controls;
using SomaFrameQD.Modules.Inspector.Util;
using SomaFrameQD.Modules.MainMenu;
using SomaFrameQD.Modules.PropertyGrid.ViewModels;
using SomaFrameQD.Modules.Shell.Commands;
using SomaFrameQD.Modules.StatusBar;
using SomaFrameQD.Modules.ToolBars;
using SomaFrameQD.Modules.UndoRedo.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingRectangle = System.Drawing.Rectangle;

namespace SomaFrameQD.Tests.Modules
{
    [STATestClass]
    [DoNotParallelize]
    public class StandaloneAndDemoOptimizationTests
    {
        [STATestMethod]
        public void InspectorVirtualScreenPhysicalBounds_AndHighDpiMultiMonitorSampling_WorkCorrectly()
        {
            var bounds = ScreenShotUtility.GetVirtualScreenPhysicalBounds();
            Assert.IsTrue(bounds.Width > 0, "Expected positive physical virtual-screen width.");
            Assert.IsTrue(bounds.Height > 0, "Expected positive physical virtual-screen height.");

            try
            {
                using (var screenshot = ScreenShotUtility.Take())
                {
                    Assert.AreEqual(bounds.Width, screenshot.Width);
                    Assert.AreEqual(bounds.Height, screenshot.Height);
                }
            }
            catch (Win32Exception)
            {
                // Locked desktop / sandboxed non-interactive Win32 station disallows GDI BitBlt from screen DC.
            }

            // Deterministic High-DPI + negative multi-monitor physical coordinate test (including x==0, y==0 top-left edge).
            var simulatedVirtualBounds = new DrawingRectangle(x: -1920, y: -240, width: 3840, height: 1200);
            using (var syntheticDesktop = new DrawingBitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                syntheticDesktop.SetPixel(0, 0, DrawingColor.FromArgb(255, 10, 20, 30));
                syntheticDesktop.SetPixel(7, 7, DrawingColor.FromArgb(255, 200, 150, 100));

                Assert.IsTrue(ScreenColorPicker.TrySamplePixel(
                    syntheticDesktop,
                    simulatedVirtualBounds,
                    physicalCursorX: -1920,
                    physicalCursorY: -240,
                    out Color topLeftSample));
                Assert.AreEqual(Color.FromArgb(255, 10, 20, 30), topLeftSample);

                Assert.IsTrue(ScreenColorPicker.TrySamplePixel(
                    syntheticDesktop,
                    simulatedVirtualBounds,
                    physicalCursorX: -1913,
                    physicalCursorY: -233,
                    out Color innerSample));
                Assert.AreEqual(Color.FromArgb(255, 200, 150, 100), innerSample);

                Assert.IsFalse(ScreenColorPicker.TrySamplePixel(
                    syntheticDesktop,
                    simulatedVirtualBounds,
                    physicalCursorX: -1921,
                    physicalCursorY: -240,
                    out _));
            }

            // Verify ScreenColorPicker handles lost capture gracefully even when no capture bitmap is active.
            var picker = new ScreenColorPicker();
            picker.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = UIElement.LostMouseCaptureEvent
            });
        }

        [STATestMethod]
        public void ConnectorItem_UnloadedFromVisualTree_DetachesLayoutUpdatedAndSkipsRedundantPositionWrites()
        {
            var window = new Window
            {
                Width = 300,
                Height = 220,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000
            };

            try
            {
                window.Show();

                var weakConnector = AttachVerifyAndDetachConnector(window);
                PumpDispatcherBackground();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Assert.IsFalse(
                    weakConnector.IsAlive,
                    "Expected unloaded ConnectorItem to be collected without leaking via ContextLayoutManager.LayoutUpdated.");
            }
            finally
            {
                window.Close();
            }
        }

        [STATestMethod]
        public void PropertyGridViewModel_CommandRerouter_ForwardsUndoRedoToActiveDocumentOnX64()
        {
            var activeDoc = new StubDocument();
            var shell = new StubShell { ActiveItem = activeDoc };
            var propertyGrid = new PropertyGridViewModel(shell);
            var rerouter = (ICommandRerouter)propertyGrid;

            Assert.AreSame(activeDoc, rerouter.GetHandler(new UndoCommandDefinition()));
            Assert.AreSame(activeDoc, rerouter.GetHandler(new RedoCommandDefinition()));
            Assert.IsNull(rerouter.GetHandler(new OpenFileCommandDefinition()));
        }

        [STATestMethod]
        public void FastBitmap_64BitPixelReadWriteAndFinalizerSafeDisposal_SucceedWithoutCrash()
        {
            using (var fastBitmap = new FastBitmap(32, 24))
            {
                Assert.AreEqual(32, fastBitmap.Width);
                Assert.AreEqual(24, fastBitmap.Height);

                var topLeft = Color.FromArgb(255, 12, 34, 56);
                var bottomRight = Color.FromArgb(255, 210, 140, 70);

                fastBitmap.Lock();
                fastBitmap[0, 0] = topLeft;
                fastBitmap[31, 23] = bottomRight;

                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = fastBitmap[-1, 0]);
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = fastBitmap[32, 0]);
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = fastBitmap[0, 24]);

                fastBitmap.Unlock();

                fastBitmap.Lock();
                Assert.AreEqual(topLeft, fastBitmap[0, 0]);
                Assert.AreEqual(bottomRight, fastBitmap[31, 23]);
                fastBitmap.Unlock();
            }

            // Allocate and drop a locked FastBitmap without calling Dispose(), then force finalization on
            // the CLR 4.8 GC Finalizer Thread to prove no cross-thread WriteableBitmap.Unlock() crash occurs.
            AllocateDroppedFastBitmap();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference AttachVerifyAndDetachConnector(Window window)
        {
            var canvas = new Canvas
            {
                Width = 200,
                Height = 150,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            var connector = new ConnectorItem { Width = 20, Height = 20 };
            Canvas.SetLeft(connector, 40);
            Canvas.SetTop(connector, 30);
            canvas.Children.Add(connector);

            // Host inside a GraphControl visual subtree so ConnectorItem.UpdatePosition transforms against GraphControl.
            var graphHost = new GraphHostControl(canvas);
            window.Content = graphHost;
            window.UpdateLayout();
            PumpDispatcherBackground();

            Assert.AreEqual(new Point(50, 40), connector.Position);

            int redundantChangeCount = 0;
            var descriptor = DependencyPropertyDescriptor.FromProperty(ConnectorItem.PositionProperty, typeof(ConnectorItem));
            EventHandler handler = (_, __) => redundantChangeCount++;
            descriptor.AddValueChanged(connector, handler);
            try
            {
                // Forcing a layout pass without coordinate changes must not re-write ConnectorItem.Position.
                connector.InvalidateArrange();
                window.UpdateLayout();
                Assert.AreEqual(0, redundantChangeCount);
            }
            finally
            {
                descriptor.RemoveValueChanged(connector, handler);
            }

            // Detach from visual tree so Unloaded fires and detaches LayoutUpdated.
            canvas.Children.Clear();
            window.Content = null;
            window.UpdateLayout();

            return new WeakReference(connector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AllocateDroppedFastBitmap()
        {
            var bitmap = new FastBitmap(8, 8);
            bitmap.Lock();
            bitmap[0, 0] = Colors.CornflowerBlue;
        }

        private static void PumpDispatcherBackground()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(_ =>
                {
                    frame.Continue = false;
                    return null;
                }),
                null);
            Dispatcher.PushFrame(frame);
        }

        private sealed class GraphHostControl : GraphControl
        {
            private readonly UIElement _child;

            public GraphHostControl(UIElement child)
            {
                _child = child;
                AddVisualChild(_child);
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => _child;

            protected override Size MeasureOverride(Size constraint)
            {
                _child.Measure(constraint);
                return _child.DesiredSize;
            }

            protected override Size ArrangeOverride(Size arrangeBounds)
            {
                _child.Arrange(new Rect(arrangeBounds));
                return arrangeBounds;
            }
        }

        private sealed class StubDocument : Document
        {
        }

        private sealed class StubShell : Screen, IShell
        {
            public event EventHandler ActiveDocumentChanging { add { } remove { } }
            public event EventHandler ActiveDocumentChanged { add { } remove { } }

            public bool ShowFloatingWindowsInTaskbar { get; set; }
            public IMenu MainMenu => null;
            public IToolBars ToolBars => null;
            public IStatusBar StatusBar => null;
            public ILayoutItem ActiveLayoutItem { get; set; }
            public IDocument ActiveItem { get; set; }
            public IObservableCollection<IDocument> Documents => null;
            public IObservableCollection<ITool> Tools => null;
            public Task InitializationTask => Task.CompletedTask;

            public bool RegisterTool(ITool tool) => true;
            public Task ShowToolAsync<TTool>() where TTool : ITool => Task.CompletedTask;
            public Task ShowToolAsync(ITool model) => Task.CompletedTask;
            public Task CloseToolAsync(ITool tool) => Task.CompletedTask;
            public Task OpenDocumentAsync(IDocument model) => Task.CompletedTask;
            public Task CloseDocumentAsync(IDocument document) => Task.CompletedTask;
            public void Close() { }
        }
    }
}
