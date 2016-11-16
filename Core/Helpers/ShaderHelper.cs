namespace Core.Helpers
{
    using SharpDX;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D;

    using System;
    using System.IO;

    using ShaderBytecode = SharpDX.Direct3D12.ShaderBytecode;

    public class ShaderHelper
    {
        public static ShaderBytecode CompileShader(string fileName, string entryPoint, string profile, ShaderMacro[] defines = null)
        {
            var shaderFlags = ShaderFlags.None;
#if DEBUG
            shaderFlags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#endif
            CompilationResult result = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(
                fileName,
                entryPoint,
                profile,
                shaderFlags,
                include: FileIncludeHandler.Default,
                defines: defines);
            return new ShaderBytecode(result);
        }
    }

    internal class FileIncludeHandler : CallbackBase, Include
    {
        public static FileIncludeHandler Default { get; } = new FileIncludeHandler();

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string filePath = fileName;

            if (!Path.IsPathRooted(filePath))
            {
                string selectedFile = Path.Combine(Environment.CurrentDirectory, fileName);
                if (File.Exists(selectedFile))
                    filePath = selectedFile;
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public void Close(Stream stream) => stream.Close();
    }
}