using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinTail.Services
{
    /// <summary>
    /// Serviço para monitorizar alterações em ficheiros de log e ler as últimas linhas.
    /// </summary>
    public sealed class FileWatcherService : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private readonly string _filePath;
        private long _lastPosition;

        public event EventHandler<string>? NewLinesAdded;

        public FileWatcherService(string filePath)
        {
            _filePath = filePath;
            _lastPosition = 0;
        }

        /// <summary>
        /// Lê as últimas N linhas do ficheiro.
        /// </summary>
        public List<string> ReadLastLines(int lineCount = 100)
        {
            var lines = new List<string>();

            try
            {
                if (!File.Exists(_filePath))
                    return lines;

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                var allLines = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                        allLines.Add(line);
                }

                _lastPosition = stream.Position;

                // Retorna as últimas N linhas
                lines = allLines.TakeLast(lineCount).ToList();
            }
            catch (Exception)
            {
                // Log error if needed
            }

            return lines;
        }

        /// <summary>
        /// Inicia a monitorização do ficheiro.
        /// </summary>
        public void StartWatching()
        {
            if (!File.Exists(_filePath))
                return;

            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);

            if (string.IsNullOrEmpty(directory))
                return;

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Pequeno delay para garantir que o ficheiro foi escrito completamente
                System.Threading.Thread.Sleep(100);

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                
                if (stream.Length <= _lastPosition)
                {
                    // Ficheiro foi truncado ou recriado
                    _lastPosition = 0;
                }

                stream.Seek(_lastPosition, SeekOrigin.Begin);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                var newContent = reader.ReadToEnd();
                _lastPosition = stream.Position;

                if (!string.IsNullOrEmpty(newContent))
                {
                    NewLinesAdded?.Invoke(this, newContent);
                }
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.Changed -= OnFileChanged;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
