using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LogTailPlus.Services
{
    /// <summary>
    /// Service to monitor log file changes and read the last lines.
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
            Debug.WriteLine($"[FileWatcher] Created for: {filePath}");
        }

        /// <summary>
        /// Reads the last N lines of the file.
        /// </summary>
        public List<string> ReadLastLines(int lineCount = 100)
        {
            var lines = new List<string>();

            try
            {
                Debug.WriteLine($"[FileWatcher] ReadLastLines: Checking file exists: {_filePath}");
                
                if (!File.Exists(_filePath))
                {
                    Debug.WriteLine($"[FileWatcher] ReadLastLines: File does not exist!");
                    return lines;
                }

                Debug.WriteLine($"[FileWatcher] ReadLastLines: Opening file...");
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

                Debug.WriteLine($"[FileWatcher] ReadLastLines: Read {allLines.Count} total lines");

                // Return the last N lines
                lines = allLines.TakeLast(lineCount).ToList();
                
                Debug.WriteLine($"[FileWatcher] ReadLastLines: Returning {lines.Count} lines");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FileWatcher] ReadLastLines ERROR: {ex.Message}");
            }

            return lines;
        }

        /// <summary>
        /// Starts monitoring the file.
        /// </summary>
        public void StartWatching()
        {
            Debug.WriteLine($"[FileWatcher] StartWatching: Starting for {_filePath}");
            
            if (!File.Exists(_filePath))
            {
                Debug.WriteLine($"[FileWatcher] StartWatching: File does not exist, cannot watch");
                return;
            }

            var directory = Path.GetDirectoryName(_filePath);
            var fileName = Path.GetFileName(_filePath);

            Debug.WriteLine($"[FileWatcher] StartWatching: Directory={directory}, FileName={fileName}");

            if (string.IsNullOrEmpty(directory))
            {
                Debug.WriteLine($"[FileWatcher] StartWatching: Directory is empty!");
                return;
            }

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            Debug.WriteLine($"[FileWatcher] StartWatching: Watcher started successfully");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"[FileWatcher] OnFileChanged: File changed event received");
            
            try
            {
                // Small delay to ensure the file has been written completely
                System.Threading.Thread.Sleep(100);

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                
                if (stream.Length <= _lastPosition)
                {
                    // File was truncated or recreated
                    Debug.WriteLine($"[FileWatcher] OnFileChanged: File truncated, resetting position");
                    _lastPosition = 0;
                }

                stream.Seek(_lastPosition, SeekOrigin.Begin);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                var newContent = reader.ReadToEnd();
                _lastPosition = stream.Position;

                Debug.WriteLine($"[FileWatcher] OnFileChanged: Read {newContent.Length} new characters");

                if (!string.IsNullOrEmpty(newContent))
                {
                    Debug.WriteLine($"[FileWatcher] OnFileChanged: Invoking NewLinesAdded event");
                    NewLinesAdded?.Invoke(this, newContent);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FileWatcher] OnFileChanged ERROR: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Debug.WriteLine($"[FileWatcher] Disposing watcher for: {_filePath}");
            
            if (_watcher != null)
            {
                _watcher.Changed -= OnFileChanged;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
