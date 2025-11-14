using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogTailPlus.Services
{
    /// <summary>
    /// Service to ensure only one instance of the application is running.
    /// New instances communicate with the main instance via Named Pipes.
    /// </summary>
    public sealed class SingleInstanceService : IDisposable
    {
        private const string PipeName = "WinTail_SingleInstance_Pipe";
        private readonly Mutex _mutex;
        private readonly bool _isFirstInstance;
        private NamedPipeServerStream? _pipeServer;
        private CancellationTokenSource? _cancellationTokenSource;
        
        public event EventHandler<string>? FilePathReceived;

        public SingleInstanceService()
        {
            _mutex = new Mutex(true, "WinTail_SingleInstance_Mutex", out _isFirstInstance);
        }

        public bool IsFirstInstance => _isFirstInstance;

        /// <summary>
        /// Starts the Named Pipe server to receive messages from other instances.
        /// </summary>
        public void StartListening()
        {
            if (!_isFirstInstance)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ListenForConnectionsAsync(_cancellationTokenSource.Token));
        }

        private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await _pipeServer.WaitForConnectionAsync(cancellationToken);

                    using var reader = new StreamReader(_pipeServer, Encoding.UTF8);
                    var filePath = await reader.ReadToEndAsync();
                    
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        FilePathReceived?.Invoke(this, filePath.Trim());
                    }

                    _pipeServer.Disconnect();
                    _pipeServer.Dispose();
                    _pipeServer = null;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Log error if needed
                }
            }
        }

        /// <summary>
        /// Sends the file path to the main instance.
        /// </summary>
        public static async Task<bool> SendFilePathToMainInstanceAsync(string filePath)
        {
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                await pipeClient.ConnectAsync(5000);

                using var writer = new StreamWriter(pipeClient, Encoding.UTF8);
                await writer.WriteAsync(filePath);
                await writer.FlushAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _pipeServer?.Dispose();
            _mutex?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
