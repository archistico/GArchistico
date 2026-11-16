namespace GenerativeArt.Application.Files;

/// <summary>
/// Scrive nella stessa directory di destinazione e pubblica il file soltanto dopo
/// il completamento dello stream. Non elimina mai un file esistente in caso di errore.
/// L'eventuale sovrascrittura richiede una decisione esplicita del chiamante.
/// </summary>
public static class AtomicOutputFiles
{
    /// <summary>
    /// Genera un file direttamente su uno stream temporaneo, senza richiedere un
    /// buffer byte[] dell'intero risultato. La produzione è eseguita sul ThreadPool,
    /// così anche i renderer sincroni non bloccano il thread dell'interfaccia.
    /// Un errore o una cancellazione prima del rename preservano la destinazione.
    /// </summary>
    public static Task WriteGeneratedAsync(string destination,
        Action<Stream, CancellationToken> generator, bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(generator);
        return Task.Run(() => WriteGeneratedCore(destination, generator, overwrite, cancellationToken),
            cancellationToken);
    }

    private static void WriteGeneratedCore(string destination,
        Action<Stream, CancellationToken> generator, bool overwrite, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string full = Path.GetFullPath(destination);
        if (Directory.Exists(full))
            throw new IOException("La destinazione è una directory.");
        if (!overwrite && File.Exists(full))
            throw new IOException("Il file esiste già e non può essere sovrascritto implicitamente.");
        string folder = Path.GetDirectoryName(full) ?? throw new IOException("Directory destinazione non valida.");
        Directory.CreateDirectory(folder);
        string partial = Path.Combine(folder, "." + Path.GetFileName(full) + "." +
            Guid.NewGuid().ToString("N") + ".partial");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var stream = new FileStream(partial, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, bufferSize: 81920))
            {
                generator(stream, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                stream.Flush(flushToDisk: true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(partial, full, overwrite);
        }
        finally
        {
            if (File.Exists(partial)) File.Delete(partial);
        }
    }

    public static async Task WriteAsync(string destination, ReadOnlyMemory<byte> content,
        bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        string full = Path.GetFullPath(destination);
        if (Directory.Exists(full))
            throw new IOException("La destinazione è una directory.");
        if (!overwrite && File.Exists(full))
            throw new IOException("Il file esiste già e non può essere sovrascritto implicitamente.");
        string folder = Path.GetDirectoryName(full) ?? throw new IOException("Directory destinazione non valida.");
        Directory.CreateDirectory(folder);
        string partial = Path.Combine(folder, "." + Path.GetFileName(full) + "." +
            Guid.NewGuid().ToString("N") + ".partial");
        try
        {
            await using (var stream = new FileStream(partial, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, bufferSize: 81920, FileOptions.Asynchronous))
            {
                await stream.WriteAsync(content, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(partial, full, overwrite);
        }
        finally
        {
            if (File.Exists(partial)) File.Delete(partial);
        }
    }
}
