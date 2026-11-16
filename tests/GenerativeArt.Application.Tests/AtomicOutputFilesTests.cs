using System.Text;
using GenerativeArt.Application.Files;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class AtomicOutputFilesTests
{
    [Fact]
    public async Task SavesAndNeverOverwritesByDefault()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-atomic-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "test.json");
        try
        {
            await AtomicOutputFiles.WriteAsync(path, Encoding.UTF8.GetBytes("first"));
            await Assert.ThrowsAsync<IOException>(async () =>
                await AtomicOutputFiles.WriteAsync(path, Encoding.UTF8.GetBytes("second")));
            Assert.Equal("first", await File.ReadAllTextAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task ExplicitOverwritePublishesWholePayloadAndCleansPartialFile()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-atomic-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "test.png");
        try
        {
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 1, 2 });
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 3, 4, 5 }, overwrite: true);
            Assert.Equal(new byte[] { 3, 4, 5 }, await File.ReadAllBytesAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task CancellationDoesNotCreateDestinationOrLeavePartials()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-atomic-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "test.png");
        Directory.CreateDirectory(folder);
        try
        {
            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await AtomicOutputFiles.WriteAsync(path, new byte[] { 1, 2, 3 }, cancellationToken: canceled.Token));
            Assert.Empty(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task StreamingWriterPublishesEntireOutputWithoutMemoryBuffer()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-generated-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "art.png");
        try
        {
            byte[] bytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            await AtomicOutputFiles.WriteGeneratedAsync(path, (output, token) =>
            {
                Assert.False(output is MemoryStream);
                for (int k = 0; k < 8; k++)
                {
                    token.ThrowIfCancellationRequested();
                    output.Write(bytes);
                }
            });
            Assert.Equal(2048L, new FileInfo(path).Length);
            Assert.Equal(bytes, (await File.ReadAllBytesAsync(path)).AsSpan(0, 256).ToArray());
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task GeneratedFileFailureKeepsExistingFileAndCleansTemporaryFile()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-generated-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "art.png");
        try
        {
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 11, 22, 33 });
            await Assert.ThrowsAsync<InvalidOperationException>(() => AtomicOutputFiles.WriteGeneratedAsync(
                path, (stream, _) =>
                {
                    stream.WriteByte(99);
                    throw new InvalidOperationException("Errore simulato dal renderer");
                }, overwrite: true));
            Assert.Equal(new byte[] { 11, 22, 33 }, await File.ReadAllBytesAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task GeneratedFileCancellationPreservesOriginalAndCleansPartials()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-generated-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "art.png");
        using var cancellation = new CancellationTokenSource();
        try
        {
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 7, 8 });
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AtomicOutputFiles.WriteGeneratedAsync(
                path, (stream, token) =>
                {
                    stream.WriteByte(66);
                    cancellation.Cancel();
                    token.ThrowIfCancellationRequested();
                }, overwrite: true, cancellationToken: cancellation.Token));
            Assert.Equal(new byte[] { 7, 8 }, await File.ReadAllBytesAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task GeneratedFileMustNotOverwriteWithoutPermission()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-generated-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "art.png");
        try
        {
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 1 });
            await Assert.ThrowsAsync<IOException>(() => AtomicOutputFiles.WriteGeneratedAsync(
                path, (stream, _) => stream.WriteByte(2)));
            Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public async Task GeneratedFileExplicitOverwritePublishesNewContent()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ga-generated-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "art.png");
        try
        {
            await AtomicOutputFiles.WriteAsync(path, new byte[] { 1, 2 });
            await AtomicOutputFiles.WriteGeneratedAsync(path, (stream, _) =>
            {
                stream.WriteByte(3);
                stream.WriteByte(4);
                stream.WriteByte(5);
            }, overwrite: true);
            Assert.Equal(new byte[] { 3, 4, 5 }, await File.ReadAllBytesAsync(path));
            Assert.Single(Directory.GetFiles(folder));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }
}
