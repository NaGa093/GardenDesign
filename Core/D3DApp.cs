﻿namespace Core
{
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Device = SharpDX.Direct3D12.Device;
    using RectangleF = SharpDX.RectangleF;
    using Resource = SharpDX.Direct3D12.Resource;

    public class D3DApp : IDisposable
    {
        private IntPtr _hInstance;

        private Factory _factory;
        private Device _device;
        private SwapChain3 _swapChain;
        private Fence _fence;
        private AutoResetEvent _fenceEvent;
        private CommandQueue _commandQueue;
        private CommandAllocator _commandAllocator;
        private GraphicsCommandList _commandList;
        private Resource _depthStencilBuffer;
        private DescriptorHeap _rtvHeap;
        private DescriptorHeap _dsvHeap;
        private ViewportF _viewport;
        private RectangleF _scissorRectangle;

        private Resource _currentBackBuffer => _swapChainBuffers[_swapChain.CurrentBackBufferIndex];
        private CpuDescriptorHandle _currentBackBufferView => _rtvHeap.CPUDescriptorHandleForHeapStart + _swapChain.CurrentBackBufferIndex * _rtvDescriptorSize;
        private CpuDescriptorHandle _depthStencilView => _dsvHeap.CPUDescriptorHandleForHeapStart;
        private readonly Resource[] _swapChainBuffers = new Resource[_swapChainBufferCount];
        private const Format _depthStencilFormat = Format.D24_UNorm_S8_UInt;
        private const Format _backBufferFormat = Format.R8G8B8A8_UNorm;

        private int _rtvDescriptorSize;
        private int _dsvDescriptorSize;
        private int _cbvSrvUavDescriptorSize;
        private int _clientWidth;
        private int _clientHeight;
        private int _m4xMsaaQuality;
        private int _frameCount;
        private bool _m4xMsaaState;
        private bool _paused;
        private bool _running;
        private long _currentFence;

        private int _msaaCount => _m4xMsaaState ? 4 : 1;
        private int _msaaQuality => _m4xMsaaState ? _m4xMsaaQuality - 1 : 0;
        private int _rtvDescriptorCount => _swapChainBufferCount;
        private int _dsvDescriptorCount => 1;

        private const int _swapChainBufferCount = 2;
        private readonly Stopwatch _fpsTimer = new Stopwatch();

        public D3DApp(IntPtr hInstance, int clientWidth, int clientHeight)
        {
            _hInstance = hInstance;
            _clientWidth = clientWidth;
            _clientHeight = clientHeight;

            InitDirect3D();
            CreateCommandObjects();
            CreateSwapChain();
            CreateRtvAndDsvDescriptorHeaps();

            _running = true;
            Resize();
        }

        public bool M4xMsaaState
        {
            get { return _m4xMsaaState; }
            set
            {
                if (_m4xMsaaState != value)
                {
                    _m4xMsaaState = value;

                    if (_running)
                    {
                        CreateSwapChain();
                        Resize();
                    }
                }
            }
        }

        private void InitDirect3D()
        {
            InitDevice();
            InitMultisampleQualityLevels();
        }

        private void InitDevice()
        {
            _factory = new Factory4();

            try
            {
                _device = new Device(_factory.GetAdapter(1), FeatureLevel.Level_11_0);
            }
            catch (SharpDXException)
            {
                Adapter warpAdapter = _factory.CreateSoftwareAdapter(_hInstance);
                _device = new Device(warpAdapter, FeatureLevel.Level_11_0);
            }

            _fence = _device.CreateFence(0, FenceFlags.None);
            _fenceEvent = new AutoResetEvent(false);

            _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
            _dsvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.DepthStencilView);
            _cbvSrvUavDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
        }

        private void InitMultisampleQualityLevels()
        {
            FeatureDataMultisampleQualityLevels msQualityLevels = new FeatureDataMultisampleQualityLevels();
            msQualityLevels.Format = _backBufferFormat;
            msQualityLevels.SampleCount = 4;
            msQualityLevels.Flags = MultisampleQualityLevelFlags.None;
            msQualityLevels.QualityLevelCount = 0;
            _device.CheckFeatureSupport(SharpDX.Direct3D12.Feature.MultisampleQualityLevels, ref msQualityLevels);
            _m4xMsaaQuality = msQualityLevels.QualityLevelCount;
        }

        private void CreateCommandObjects()
        {
            CommandQueueDescription queueDesc = new CommandQueueDescription(CommandListType.Direct);

            _commandQueue = _device.CreateCommandQueue(queueDesc);

            _commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);

            _commandList = _device.CreateCommandList(
                0,
                CommandListType.Direct,
                _commandAllocator,
                null);

            _commandList.Close();
        }

        private void CreateSwapChain()
        {
            _swapChain?.Dispose();

            var sd = new SwapChainDescription
            {
                ModeDescription = new ModeDescription
                {
                    Width = _clientWidth,
                    Height = _clientHeight,
                    Format = _backBufferFormat,
                    RefreshRate = new Rational(60, 1),
                    Scaling = DisplayModeScaling.Unspecified,
                    ScanlineOrdering = DisplayModeScanlineOrder.Unspecified
                },
                SampleDescription = new SampleDescription
                {
                    Count = _msaaCount,
                    Quality = _msaaQuality
                },
                Usage = Usage.RenderTargetOutput,
                BufferCount = _swapChainBufferCount,
                SwapEffect = SwapEffect.FlipDiscard,
                Flags = SwapChainFlags.AllowModeSwitch,
                OutputHandle = _hInstance,
                IsWindowed = true
            };

            using (var tempSwapChain = new SwapChain(_factory, _commandQueue, sd))
            {
                _swapChain = tempSwapChain.QueryInterface<SwapChain3>();
            }
        }

        private void CreateRtvAndDsvDescriptorHeaps()
        {
            var rtvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = _rtvDescriptorCount,
                Type = DescriptorHeapType.RenderTargetView
            };
            _rtvHeap = _device.CreateDescriptorHeap(rtvHeapDesc);

            var dsvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = _dsvDescriptorCount,
                Type = DescriptorHeapType.DepthStencilView
            };
            _dsvHeap = _device.CreateDescriptorHeap(dsvHeapDesc);
        }

        public void Resize(int clientWidth, int clientHeight)
        {
            _paused = true;

            _clientWidth = clientWidth;
            _clientHeight = clientHeight;

            Resize();

            _paused = false;
        }

        private void Resize()
        {
            lock (_commandList)
            {
                FlushCommandQueue();

                _commandList.Reset(_commandAllocator, null);

                foreach (Resource buffer in _swapChainBuffers)
                {
                    buffer?.Dispose();
                }

                _depthStencilBuffer?.Dispose();

                _swapChain.ResizeBuffers(
                    _swapChainBufferCount,
                    _clientWidth, _clientHeight,
                    _backBufferFormat,
                    SwapChainFlags.AllowModeSwitch);

                CpuDescriptorHandle rtvHeapHandle = _rtvHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < _swapChainBufferCount; i++)
                {
                    Resource backBuffer = _swapChain.GetBackBuffer<Resource>(i);
                    _swapChainBuffers[i] = backBuffer;
                    _device.CreateRenderTargetView(backBuffer, null, rtvHeapHandle);
                    rtvHeapHandle += _rtvDescriptorSize;
                }

                var depthStencilDesc = new ResourceDescription
                {
                    Dimension = ResourceDimension.Texture2D,
                    Alignment = 0,
                    Width = _clientWidth,
                    Height = _clientHeight,
                    DepthOrArraySize = 1,
                    MipLevels = 1,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription
                    {
                        Count = _msaaCount,
                        Quality = _msaaQuality
                    },
                    Layout = TextureLayout.Unknown,
                    Flags = ResourceFlags.AllowDepthStencil
                };
                var optClear = new ClearValue
                {
                    Format = _depthStencilFormat,
                    DepthStencil = new DepthStencilValue
                    {
                        Depth = 1.0f,
                        Stencil = 0
                    }
                };
                _depthStencilBuffer = _device.CreateCommittedResource(
                    new HeapProperties(HeapType.Default),
                    HeapFlags.None,
                    depthStencilDesc,
                    ResourceStates.Common,
                    optClear);

                var depthStencilViewDesc = new DepthStencilViewDescription
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Format = _depthStencilFormat
                };

                CpuDescriptorHandle dsvHeapHandle = _dsvHeap.CPUDescriptorHandleForHeapStart;
                _device.CreateDepthStencilView(_depthStencilBuffer, depthStencilViewDesc, dsvHeapHandle);

                _commandList.ResourceBarrierTransition(_depthStencilBuffer, ResourceStates.Common, ResourceStates.DepthWrite);

                _commandList.Close();
                _commandQueue.ExecuteCommandList(_commandList);

                FlushCommandQueue();

                _viewport = new ViewportF(0, 0, _clientWidth, _clientHeight, 0.0f, 1.0f);
                _scissorRectangle = new RectangleF(0, 0, _clientWidth, _clientHeight);
            }
        }

        private void FlushCommandQueue()
        {
            _currentFence++;

            _commandQueue.Signal(_fence, _currentFence);

            if (_fence.CompletedValue < _currentFence)
            {
                _fence.SetEventOnCompletion(_currentFence, _fenceEvent.SafeWaitHandle.DangerousGetHandle());

                _fenceEvent.WaitOne();
            }
        }

        private void CalculateFrameRateStats()
        {
            _frameCount++;
            if (_fpsTimer.ElapsedMilliseconds > 1000L)
            {
                var fps = 1000.0 * (double)_frameCount / (double)_fpsTimer.ElapsedMilliseconds;
                var ms = (float)_fpsTimer.ElapsedMilliseconds / (float)_frameCount;
                var text = string.Format("FPS: {0:F2} ({1:F2}ms)", fps, ms);
                Console.WriteLine(text);
                _fpsTimer.Reset();
                _fpsTimer.Stop();
                _fpsTimer.Start();
                _frameCount = 0;
            }
        }

        public void Run()
        {
            _fpsTimer.Start();
            Task.Factory.StartNew(() =>
            {
                while (_running)
                {
                    if (!_paused)
                    {
                        CalculateFrameRateStats();
                        Update();
                        Draw();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private void Update()
        {
        }

        private void Draw()
        {
            lock (_commandList)
            {
                if (_paused)
                {
                    return;
                }

                _commandAllocator.Reset();

                _commandList.Reset(_commandAllocator, null);

                _commandList.ResourceBarrierTransition(_currentBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);
                _commandList.SetViewport(_viewport);
                _commandList.SetScissorRectangles(_scissorRectangle);
                _commandList.ClearRenderTargetView(_currentBackBufferView, Color.LightSteelBlue);
                _commandList.ClearDepthStencilView(_depthStencilView, ClearFlags.FlagsDepth | ClearFlags.FlagsStencil, 1.0f, 0);
                _commandList.SetRenderTargets(_currentBackBufferView, _depthStencilView);
                _commandList.ResourceBarrierTransition(_currentBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);
                _commandList.Close();

                _commandQueue.ExecuteCommandList(_commandList);

                _swapChain.Present(0, PresentFlags.None);

                FlushCommandQueue();
            }
        }

        public void Dispose()
        {
            _running = false;
            _paused = true;

            FlushCommandQueue();

            _rtvHeap?.Dispose();
            _dsvHeap?.Dispose();
            _swapChain?.Dispose();

            foreach (Resource buffer in _swapChainBuffers)
            {
                buffer?.Dispose();
            }

            _depthStencilBuffer?.Dispose();
            _commandList?.Dispose();
            _commandAllocator?.Dispose();
            _commandQueue?.Dispose();
            _fence?.Dispose();
            _device?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}