namespace Core
{
    using Core.Cameras;
    using Core.Helpers;
    using Core.Meshes;
    using Core.Primitives;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    using System;
    using System.Threading;

    using Device = SharpDX.Direct3D12.Device;
    using RectangleF = SharpDX.RectangleF;
    using Resource = SharpDX.Direct3D12.Resource;

    public class D3DApp : IDisposable
    {
        private IntPtr handleIntPtr;

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

        private int _m4xMsaaQuality;
        
        private bool _m4xMsaaState;
        
        
        private long _currentFence;

        private int _msaaCount => _m4xMsaaState ? 4 : 1;
        private int _msaaQuality => _m4xMsaaState ? _m4xMsaaQuality - 1 : 0;
        private int _rtvDescriptorCount => _swapChainBufferCount;
        private int _dsvDescriptorCount => 1;

        private const int _swapChainBufferCount = 2;
        

        private PipelineState _pso;
        private RootSignature _rootSignature;

        private Utilities.UploadBuffer<ObjectConstants> _objectCB;
        private DescriptorHeap _cbvHeap;
        private DescriptorHeap[] _descriptorHeaps;
        private Grid _grid;
        private Sphere _sphere;
        private Cylinder _cylinder;

        public D3DApp(IntPtr handleIntPtr)
        {
            this.handleIntPtr = handleIntPtr;

            InitDirect3D();

            CreateCommandObjects();
            CreateSwapChain();
            CreateRtvAndDsvDescriptorHeaps();

            _commandList.Reset(_commandAllocator, null);

            BuildDescriptorHeaps();
            BuildConstantBuffers();
            BuildRootSignature();
            BuildMesh();

            _pso = _device.CreateGraphicsPipelineState(PipelinesStateObject.New(_rootSignature, _msaaCount, _msaaQuality, _depthStencilFormat, _backBufferFormat));

            _commandList.Close();
            _commandQueue.ExecuteCommandList(_commandList);

            FlushCommandQueue();

            Camera = new OrbitCamera();
        }

        public OrbitCamera Camera
        {
            get;
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
                var warpAdapter = _factory.CreateSoftwareAdapter(this.handleIntPtr);
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
            var msQualityLevels = new FeatureDataMultisampleQualityLevels();
            msQualityLevels.Format = _backBufferFormat;
            msQualityLevels.SampleCount = 4;
            msQualityLevels.Flags = MultisampleQualityLevelFlags.None;
            msQualityLevels.QualityLevelCount = 0;
            _device.CheckFeatureSupport(SharpDX.Direct3D12.Feature.MultisampleQualityLevels, ref msQualityLevels);
            _m4xMsaaQuality = msQualityLevels.QualityLevelCount;
        }

        private void CreateCommandObjects()
        {
            var queueDesc = new CommandQueueDescription(CommandListType.Direct);

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
                OutputHandle = this.handleIntPtr,
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

        private void BuildRootSignature()
        {
            var cbvTable = new DescriptorRange(DescriptorRangeType.ConstantBufferView, 1, 0);

            var rootSigDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout, new[]
            {
                new RootParameter(ShaderVisibility.Vertex, cbvTable)
            });

            _rootSignature = _device.CreateRootSignature(rootSigDesc.Serialize());
        }

        private void BuildDescriptorHeaps()
        {
            var cbvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = 1,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                Flags = DescriptorHeapFlags.ShaderVisible,
                NodeMask = 0
            };
            _cbvHeap = _device.CreateDescriptorHeap(cbvHeapDesc);
            _descriptorHeaps = new[] { _cbvHeap };
        }

        private void BuildConstantBuffers()
        {
            var sizeInBytes = BufferHelper.CalcConstantBufferByteSize<ObjectConstants>();

            _objectCB = new Utilities.UploadBuffer<ObjectConstants>(_device, 1, true);

            var cbvDesc = new ConstantBufferViewDescription
            {
                BufferLocation = _objectCB.Resource.GPUVirtualAddress,
                SizeInBytes = sizeInBytes
            };
            CpuDescriptorHandle cbvHeapHandle = _cbvHeap.CPUDescriptorHandleForHeapStart;
            _device.CreateConstantBufferView(cbvDesc, cbvHeapHandle);
        }

        private void BuildMesh()
        {
            //_boxMesh = Triangle.Create(_device, _commandList, new Vector3(-1.0f, -1.0f, 0f), new Vector3(-1.0f, +1.0f, 0f), new Vector3(+1.0f, +1.0f, 0f), Color.White);
            _grid = new Grid(_device, _commandList, PrimitiveTopology.LineList, 10, 1.0f, Color.White);
            _cylinder = new Cylinder(_device, _commandList, PrimitiveTopology.LineList, new Vector3(2, 2, 2), new Vector3(4, 4, 4), 1, 1, 10, Color.Black);
            _sphere = new Sphere(_device, _commandList, PrimitiveTopology.TriangleList, 2, 10, 10, Color.Red);
        }

        public void Resize(int clientWidth, int clientHeight)
        {
            lock (_commandList)
            {
                //this.CreateSwapChain(clientWidth, clientHeight);

                this.FlushCommandQueue();

                _commandList.Reset(_commandAllocator, null);

                foreach (Resource buffer in _swapChainBuffers)
                {
                    buffer?.Dispose();
                }

                _depthStencilBuffer?.Dispose();

                _swapChain.ResizeBuffers(
                    _swapChainBufferCount,
                    clientWidth, clientHeight,
                    _backBufferFormat,
                    SwapChainFlags.AllowModeSwitch);

                CpuDescriptorHandle rtvHeapHandle = _rtvHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < _swapChainBufferCount; i++)
                {
                    var backBuffer = _swapChain.GetBackBuffer<Resource>(i);
                    _swapChainBuffers[i] = backBuffer;
                    _device.CreateRenderTargetView(backBuffer, null, rtvHeapHandle);
                    rtvHeapHandle += _rtvDescriptorSize;
                }

                var depthStencilDesc = new ResourceDescription
                {
                    Dimension = ResourceDimension.Texture2D,
                    Alignment = 0,
                    Width = clientWidth,
                    Height = clientHeight,
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

                _viewport = new ViewportF(0, 0, clientWidth, clientHeight, 0.0f, 1.0f);
                _scissorRectangle = new RectangleF(0, 0, clientWidth, clientHeight);

                this.Camera.SetPerspective(MathUtil.PiOverFour, (float)clientWidth / clientHeight, 1.0f, 1000.0f);
                this.Camera.SetOrthographic((float)clientWidth / 100, (float)clientHeight / 100, -15000f, 15000.0f);
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

        public void Update()
        {
            var eye = new Vector3(1, 1, 20);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);

            this.Camera.SetView(eye, target, up);

            var cb = new ObjectConstants
            {
                WorldViewProj = Matrix.Transpose(this.Camera.ViewMatrix * this.Camera.ProjectionMatrix)
            };

            _objectCB.CopyData(0, ref cb);
        }

        public void Draw(bool paused)
        {
            lock (_commandList)
            {
                if (paused || _swapChain.IsDisposed)
                {
                    return;
                }

                _commandAllocator.Reset();

                _commandList.Reset(_commandAllocator, _pso);
                _commandList.SetViewport(_viewport);
                _commandList.SetScissorRectangles(_scissorRectangle);
                _commandList.ResourceBarrierTransition(_currentBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);
                _commandList.ClearRenderTargetView(_currentBackBufferView, Color.LightBlue);
                _commandList.ClearDepthStencilView(_depthStencilView, ClearFlags.FlagsDepth | ClearFlags.FlagsStencil, 1.0f, 0);
                _commandList.SetRenderTargets(_currentBackBufferView, _depthStencilView);
                _commandList.SetDescriptorHeaps(_descriptorHeaps.Length, _descriptorHeaps);
                _commandList.SetGraphicsRootSignature(_rootSignature);
                _commandList.SetGraphicsRootDescriptorTable(0, _cbvHeap.GPUDescriptorHandleForHeapStart);

                //Draw
                _grid.Draw();
                _cylinder.Draw();
                _sphere.Draw();

                _commandList.ResourceBarrierTransition(_currentBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);
                _commandList.Close();

                _commandQueue.ExecuteCommandList(_commandList);

                _swapChain.Present(0, PresentFlags.None);

                FlushCommandQueue();
            }
        }

        public void Dispose()
        {
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