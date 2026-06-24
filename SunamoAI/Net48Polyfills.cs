namespace SunamoAI;

    internal static class FileAsync
    {
#if NET5_0_OR_GREATER
        internal static System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllTextAsync(path, cancellationToken);

        internal static System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllTextAsync(path, encoding, cancellationToken);

        internal static System.Threading.Tasks.Task WriteAllTextAsync(string path, string? contents, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.WriteAllTextAsync(path, contents, cancellationToken);

        internal static System.Threading.Tasks.Task WriteAllTextAsync(string path, string? contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.WriteAllTextAsync(path, contents, encoding, cancellationToken);

        internal static System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllLinesAsync(path, cancellationToken);

        internal static System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllLinesAsync(path, encoding, cancellationToken);

        internal static System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.WriteAllLinesAsync(path, lines, cancellationToken);

        internal static System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.WriteAllLinesAsync(path, lines, encoding, cancellationToken);

        internal static System.Threading.Tasks.Task<byte[]> ReadAllBytesAsync(string path, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllBytesAsync(path, cancellationToken);

        internal static System.Threading.Tasks.Task WriteAllBytesAsync(string path, byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.WriteAllBytesAsync(path, bytes, cancellationToken);

        internal static System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.AppendAllTextAsync(path, contents, cancellationToken);

        internal static System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.AppendAllTextAsync(path, contents, encoding, cancellationToken);

        internal static System.Threading.Tasks.Task AppendAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => System.IO.File.AppendAllLinesAsync(path, lines, cancellationToken);
#else
        internal static async System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var reader = new System.IO.StreamReader(path);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var reader = new System.IO.StreamReader(path, encoding);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task WriteAllTextAsync(string path, string? contents, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, false);
            await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task WriteAllTextAsync(string path, string? contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, false, encoding);
            await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lines = new System.Collections.Generic.List<string>();
            using var reader = new System.IO.StreamReader(path);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        internal static async System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lines = new System.Collections.Generic.List<string>();
            using var reader = new System.IO.StreamReader(path, encoding);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        internal static async System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, false);
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
            }
        }

        internal static async System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, false, encoding);
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
            }
        }

        internal static async System.Threading.Tasks.Task<byte[]> ReadAllBytesAsync(string path, System.Threading.CancellationToken cancellationToken = default)
        {
            using var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 4096, useAsync: true);
            var bytes = new byte[stream.Length];
            int totalRead = 0;
            while (totalRead < bytes.Length)
            {
                int bytesRead = await stream.ReadAsync(bytes, totalRead, bytes.Length - totalRead, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    throw new System.IO.EndOfStreamException();
                }
                totalRead += bytesRead;
            }
            return bytes;
        }

        internal static async System.Threading.Tasks.Task WriteAllBytesAsync(string path, byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
        {
            using var stream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, useAsync: true);
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, true);
            await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, true, encoding);
            await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
        }

        internal static async System.Threading.Tasks.Task AppendAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new System.IO.StreamWriter(path, true);
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
            }
        }
#endif
    }

    internal static class PathPolyfill
    {
#if NET5_0_OR_GREATER
        internal static string GetRelativePath(string relativeTo, string path)
            => System.IO.Path.GetRelativePath(relativeTo, path);
#else
        internal static string GetRelativePath(string relativeTo, string path)
        {
            if (!relativeTo.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) &&
                !relativeTo.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
            {
                relativeTo += System.IO.Path.DirectorySeparatorChar;
            }
            var fromUri = new System.Uri(relativeTo);
            var toUri = new System.Uri(path);
            if (fromUri.Scheme != toUri.Scheme)
            {
                return path;
            }
            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());
            return relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
        }
#endif
    }

    internal static class FileCompat
    {
#if NET5_0_OR_GREATER
        internal static void Move(string sourceFileName, string destFileName, bool overwrite)
            => System.IO.File.Move(sourceFileName, destFileName, overwrite);
#else
        internal static void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            if (overwrite && System.IO.File.Exists(destFileName))
            {
                System.IO.File.Delete(destFileName);
            }
            System.IO.File.Move(sourceFileName, destFileName);
        }
#endif
    }

#if !NET5_0_OR_GREATER

    internal static class Net48ProcessPolyfillExtensions
    {
        internal static System.Threading.Tasks.Task WaitForExitAsync(this System.Diagnostics.Process process, System.Threading.CancellationToken cancellationToken = default)
        {
            var completionSource = new System.Threading.Tasks.TaskCompletionSource<object?>(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, eventArgs) => completionSource.TrySetResult(null);
            if (process.HasExited)
            {
                completionSource.TrySetResult(null);
            }
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => completionSource.TrySetCanceled(cancellationToken));
            }
            return completionSource.Task;
        }
    }

    internal static class Net48StreamPolyfillExtensions
    {
        internal static void ReadExactly(this System.IO.Stream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (bytesRead == 0)
                {
                    throw new System.IO.EndOfStreamException();
                }
                totalRead += bytesRead;
            }
        }

        internal static void ReadExactly(this System.IO.Stream stream, byte[] buffer)
        {
            stream.ReadExactly(buffer, 0, buffer.Length);
        }
    }

    internal static class Net48DbConnectionPolyfillExtensions
    {
        internal static System.Threading.Tasks.Task CloseAsync(this System.Data.Common.DbConnection connection)
        {
            connection.Close();
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

#endif