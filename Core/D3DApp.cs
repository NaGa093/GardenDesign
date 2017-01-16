namespace Core
{
    using Core.Cameras;
    using Core.Helpers;
    using Core.Meshes;
    using Core.Primitives;
    using Core.Meshes.Base;

    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    using System;
    using System.Threading;
    using System.Collections.Generic;

    using Device = SharpDX.Direct3D12.Device;
    using RectangleF = SharpDX.RectangleF;
    using Resource = SharpDX.Direct3D12.Resource;

    public class D3DApp : IDisposable
    {
        private const int NumFrameResources = 3;
        private const Format DepthStencilFormat = Format.D24_UNorm_S8_UInt;
        private const Format BackBufferFormat = Format.R8G8B8A8_UNorm;
        private const int SwapChainBufferCount = 2;
        private Resource CurrentBackBuffer => swapChainBuffers[swapChain.CurrentBackBufferIndex];
        private CpuDescriptorHandle CurrentBackBufferView => descriptorHeapObjects.RenderTargetViewDescriptorHeap.CPUDescriptorHandleForHeapStart + swapChain.CurrentBackBufferIndex * rtvDescriptorSize;


        private Factory factory;
        private Device device;
        private Fence fence;
        private AutoResetEvent fenceEvent;
        private Resource depthStencilBuffer;
        private ViewportF viewport;
        private RectangleF scissorRectangle;
       
        private int currFrameResourceIndex;
        private int rtvDescriptorSize;
        private int m4XMsaaQuality;
        private long currentFence;
        private int MsaaCount => m4XMsaaState ? 4 : 1;
        private int MsaaQuality => m4XMsaaState ? m4XMsaaQuality - 1 : 0;
        
        private readonly List<Mesh> meshes; 
        private readonly PipelineState pso;
        private readonly bool m4XMsaaState;
        private readonly OrbitCamera camera;
        private readonly IntPtr handleIntPtr;
        private readonly SwapChain3 swapChain;
        private readonly RootSignature rootSignature;
        private readonly Resource[] swapChainBuffers;
        private readonly CommandObjects commandObjects;
        private readonly DescriptorHeapObjects descriptorHeapObjects;
        private readonly List<FrameResource> frameResources;
        private readonly List<AutoResetEvent> fenceEvents;
        private FrameResource CurrFrameResource => frameResources[currFrameResourceIndex];
        private AutoResetEvent CurrentFenceEvent => fenceEvents[currFrameResourceIndex];

        private PassConstants mainPassCb;

        public D3DApp(IntPtr handleIntPtr)
        {
            this.handleIntPtr = handleIntPtr;

            this.m4XMsaaState = false;
            this.swapChainBuffers = new Resource[SwapChainBufferCount];
            this.frameResources = new List<FrameResource>(NumFrameResources);
            this.fenceEvents = new List<AutoResetEvent>(NumFrameResources);
            this.mainPassCb = PassConstants.Default;
            meshes = new List<Mesh>();

            this.InitDirect3D();
            this.commandObjects = new CommandObjects(device);


            this.swapChain = SwapChainObject.New(this.handleIntPtr, this.factory, this.commandObjects.GetCommandQueue, BackBufferFormat, SwapChainBufferCount, MsaaCount, MsaaQuality);
            this.descriptorHeapObjects = new DescriptorHeapObjects(device, SwapChainBufferCount, 1);

            

            this.rootSignature = RootSignatureObject.New(device);
            

            this.BuildMesh();
            this.BuildFrameResources();
            this.BuildConstantBuffers();
            this.pso = PipelinesStateObject.New(device, rootSignature, MsaaCount, MsaaQuality, DepthStencilFormat, BackBufferFormat);

            this.FlushCommandQueue();

            this.camera = new OrbitCamera();
        }


        private void InitDirect3D()
        {
            this.InitDevice();
            this.InitMultisampleQualityLevels();
        }

        private void InitDevice()
        {
            this.factory = new Factory4();

            try
            {
                this.device = new Device(factory.GetAdapter(1), FeatureLevel.Level_11_0);
            }
            catch (SharpDXException)
            {
                var warpAdapter = factory.CreateSoftwareAdapter(this.handleIntPtr);
                this.device = new Device(warpAdapter, FeatureLevel.Level_11_0);
            }

            this.fence = device.CreateFence(0, FenceFlags.None);
            this.fenceEvent = new AutoResetEvent(false);

            this.rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
        }

        private void InitMultisampleQualityLevels()
        {
            var msQualityLevels = new FeatureDataMultisampleQualityLevels
            {
                Format = BackBufferFormat,
                SampleCount = 4,
                Flags = MultisampleQualityLevelFlags.None,
                QualityLevelCount = 0
            };

           this.device.CheckFeatureSupport(SharpDX.Direct3D12.Feature.MultisampleQualityLevels, ref msQualityLevels);
           this.m4XMsaaQuality = msQualityLevels.QualityLevelCount;
        }

        private void BuildConstantBuffers()
        {
            var cbvDesc = new ConstantBufferViewDescription
            {
                BufferLocation = CurrFrameResource.ObjectCB.Resource.GPUVirtualAddress,
                SizeInBytes = BufferHelper.CalcConstantBufferByteSize<ObjectConstants>()
            };

            this.device.CreateConstantBufferView(cbvDesc, descriptorHeapObjects.ConstantBufferViewShaderResourceViewUnorderedAccessViewDescriptorHeap.CPUDescriptorHandleForHeapStart);
        }

        private void BuildMesh()
        {
            this.commandObjects.GetGraphicsCommandList.Reset(commandObjects.GetCommandAllocator, null);

            this.meshes.AddRange(CoordinateAxis.New(device, commandObjects.GetGraphicsCommandList, PrimitiveTopology.LineList));
            //this.meshes.Add(new Grid(device, commandObjects.GetGraphicsCommandList, PrimitiveTopology.LineList, 10, 1.0f, Color.White));
            //this.meshes.Add(new Cylinder(device, commandObjects.GetGraphicsCommandList, PrimitiveTopology.TriangleList, new Vector3(0, 0, 0), new Vector3(4, 4, 4), 0, 1, 10, 10, Color.Black));
            //this.meshes.Add(new Sphere(device, commandObjects.GetGraphicsCommandList, PrimitiveTopology.TriangleList, 2, 10, 10, Color.Red));

            this.commandObjects.GetGraphicsCommandList.Close();
            this.commandObjects.GetCommandQueue.ExecuteCommandList(commandObjects.GetGraphicsCommandList);
        }

        private void BuildFrameResources()
        {
            for (int i = 0; i < NumFrameResources; i++)
            {
                frameResources.Add(new FrameResource(device, 1, this.meshes.Count));
                fenceEvents.Add(new AutoResetEvent(false));
            }
        }


        public void Resize(int clientWidth, int clientHeight)
        {
            lock (commandObjects.GetGraphicsCommandList)
            {
                this.FlushCommandQueue();

                this.commandObjects.GetGraphicsCommandList.Reset(commandObjects.GetCommandAllocator, null);

                foreach (var buffer in swapChainBuffers)
                {
                    buffer?.Dispose();
                }

                this.depthStencilBuffer?.Dispose();

                this.swapChain.ResizeBuffers(
                    SwapChainBufferCount,
                    clientWidth, clientHeight,
                    BackBufferFormat,
                    SwapChainFlags.AllowModeSwitch);

                var rtvHeapHandle = descriptorHeapObjects.RenderTargetViewDescriptorHeap.CPUDescriptorHandleForHeapStart;
                for (var i = 0; i < SwapChainBufferCount; i++)
                {
                    var backBuffer = swapChain.GetBackBuffer<Resource>(i);
                    this.swapChainBuffers[i] = backBuffer;
                    this.device.CreateRenderTargetView(backBuffer, null, rtvHeapHandle);
                    rtvHeapHandle += rtvDescriptorSize;
                }

                this.depthStencilBuffer = DepthStencilBufferResource.New(device, clientWidth, clientHeight, MsaaCount,
                    MsaaQuality, DepthStencilFormat);

                var depthStencilViewDesc = new DepthStencilViewDescription
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Format = DepthStencilFormat
                };

                var dsvHeapHandle = descriptorHeapObjects.DepthStencilViewDescriptorHeap.CPUDescriptorHandleForHeapStart;
                this.device.CreateDepthStencilView(depthStencilBuffer, depthStencilViewDesc, dsvHeapHandle);

                this.commandObjects.GetGraphicsCommandList.ResourceBarrierTransition(depthStencilBuffer, ResourceStates.Common, ResourceStates.DepthWrite);

                this.commandObjects.GetGraphicsCommandList.Close();
                this.commandObjects.GetCommandQueue.ExecuteCommandList(commandObjects.GetGraphicsCommandList);

                this.FlushCommandQueue();

                this.viewport = new ViewportF(0, 0, clientWidth, clientHeight, 0.0f, 1.0f);
                this.scissorRectangle = new RectangleF(0, 0, clientWidth, clientHeight);

                this.camera.SetPerspective(MathUtil.PiOverFour, (float)clientWidth / clientHeight, 1.0f, 1000.0f);
                this.camera.SetOrthographic((float)clientWidth / 100, (float)clientHeight / 100, -1500f, 1500.0f);
            }
        }

        private void FlushCommandQueue()
        {
            this.currentFence++;

            this.commandObjects.GetCommandQueue.Signal(fence, currentFence);

            if (this.fence.CompletedValue < this.currentFence)
            {
                this.fence.SetEventOnCompletion(currentFence, fenceEvent.SafeWaitHandle.DangerousGetHandle());
                this.fenceEvent.WaitOne();
            }
        }

        private int init = 3;

        public void Update(int clientWidth, int clientHeight)
        {
            currFrameResourceIndex = (currFrameResourceIndex + 1) % NumFrameResources;

            // Has the GPU finished processing the commands of the current frame resource?
            // If not, wait until the GPU has completed commands up to this fence point.
            if (CurrFrameResource.Fence != 0 && fence.CompletedValue < CurrFrameResource.Fence)
            {
                fence.SetEventOnCompletion(CurrFrameResource.Fence, CurrentFenceEvent.SafeWaitHandle.DangerousGetHandle());
                CurrentFenceEvent.WaitOne();
            }

            if (init > 0)
            {
                for (int i = 0; i < this.meshes.Count; i++)
                {
                    var objConstants = new ObjectConstants { World = Matrix.Transpose(this.meshes[i].World) };
                    CurrFrameResource.ObjectCB.CopyData(i, ref objConstants);
                }
                init--;
            }

            Matrix viewProj = this.camera.ViewMatrix * this.camera.ProjectionMatrix;
            Matrix invView = Matrix.Invert(this.camera.ViewMatrix);
            Matrix invProj = Matrix.Invert(this.camera.ProjectionMatrix);
            Matrix invViewProj = Matrix.Invert(viewProj);

            mainPassCb.View = Matrix.Transpose(this.camera.ViewMatrix);
            mainPassCb.InvView = Matrix.Transpose(invView);
            mainPassCb.Proj = Matrix.Transpose(this.camera.ProjectionMatrix);
            mainPassCb.InvProj = Matrix.Transpose(invProj);
            mainPassCb.ViewProj = Matrix.Transpose(viewProj);
            mainPassCb.InvViewProj = Matrix.Transpose(invViewProj);
            mainPassCb.EyePosW = this.camera.Eye;
            mainPassCb.RenderTargetSize = new Vector2(clientWidth, clientHeight);
            mainPassCb.InvRenderTargetSize = 1.0f / mainPassCb.RenderTargetSize;
        
            CurrFrameResource.PassCB.CopyData(0, ref mainPassCb);
        }

        public void Draw(bool paused)
        {
            lock (this.commandObjects.GetGraphicsCommandList)
            {
                if (paused || this.swapChain.IsDisposed)
                {
                    return;
                }

                this.CurrFrameResource.CmdListAlloc.Reset();

                this.commandObjects.GetGraphicsCommandList.Reset(CurrFrameResource.CmdListAlloc, pso);

                this.commandObjects.GetGraphicsCommandList.SetViewport(viewport);
                this.commandObjects.GetGraphicsCommandList.SetScissorRectangles(scissorRectangle);

                this.commandObjects.GetGraphicsCommandList.ResourceBarrierTransition(CurrentBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);

                this.commandObjects.GetGraphicsCommandList.ClearRenderTargetView(CurrentBackBufferView, Color.White);
                this.commandObjects.GetGraphicsCommandList.ClearDepthStencilView(descriptorHeapObjects.DepthStencilViewDescriptorHeap.CPUDescriptorHandleForHeapStart, ClearFlags.FlagsDepth | ClearFlags.FlagsStencil, 1.0f, 0);

                this.commandObjects.GetGraphicsCommandList.SetRenderTargets(CurrentBackBufferView, descriptorHeapObjects.DepthStencilViewDescriptorHeap.CPUDescriptorHandleForHeapStart);

                this.commandObjects.GetGraphicsCommandList.SetGraphicsRootSignature(rootSignature);

                this.commandObjects.GetGraphicsCommandList.SetGraphicsRootConstantBufferView(2, CurrFrameResource.PassCB.Resource.GPUVirtualAddress);
               
                this.DrawMeshes();

                this.commandObjects.GetGraphicsCommandList.ResourceBarrierTransition(CurrentBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);
                this.commandObjects.GetGraphicsCommandList.Close();

                this.commandObjects.GetCommandQueue.ExecuteCommandList(commandObjects.GetGraphicsCommandList);

                this.swapChain.Present(0, PresentFlags.None);

                this.CurrFrameResource.Fence = ++currentFence;

                this.commandObjects.GetCommandQueue.Signal(fence, currentFence);
            }
        }

        private void DrawMeshes()
        {
            int objCBByteSize = BufferHelper.CalcConstantBufferByteSize<ObjectConstants>();

            var objectCB = CurrFrameResource.ObjectCB.Resource;

            for (int i = 0; i < this.meshes.Count; i++)
            {
                this.commandObjects.GetGraphicsCommandList.SetVertexBuffer(0, this.meshes[i].VertexBufferView);
                this.commandObjects.GetGraphicsCommandList.SetIndexBuffer(this.meshes[i].IndexBufferView);
                this.commandObjects.GetGraphicsCommandList.PrimitiveTopology = this.meshes[i].PrimitiveTopology;

                var objCbAddress = objectCB.GPUVirtualAddress + i * objCBByteSize;

                this.commandObjects.GetGraphicsCommandList.SetGraphicsRootConstantBufferView(0, objCbAddress);

                this.commandObjects.GetGraphicsCommandList.DrawIndexedInstanced(meshes[i].IndexCount, 1, 0, 0, 0);
            }
        }

        public void Dispose()
        {
            this.FlushCommandQueue();

            this.descriptorHeapObjects?.Dispose();
            this.swapChain?.Dispose();

            foreach (var buffer in swapChainBuffers)
            {
                buffer?.Dispose();
            }

            this.depthStencilBuffer?.Dispose();
            this.commandObjects?.Dispose();
            this.fence?.Dispose();
            this.device?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void CameraZoom(int zoomValue)
        {
            this.camera.Zoom(zoomValue);
        }

        public void CameraRotationY(int zoomValue)
        {
            this.camera.RotateY(zoomValue);
        }

        public void CameraRotationOrtho(int zoomValue)
        {
            this.camera.RotateOrtho(zoomValue);
        }
    }
}