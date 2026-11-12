namespace ChangeMetadata;

/// <summary>What a video's title is set to.</summary>
public enum TitleMode
{
    /// <summary>The file's own name, without its extension.</summary>
    FromFileName,

    /// <summary>Nothing: the title is taken out.</summary>
    Clear,
}

public enum TagOutcome
{
    Changed,

    /// <summary>The title was already what was asked for, so the file was left alone.</summary>
    Unchanged,

    Failed,
}

/// <summary>What became of one file. <see cref="Error"/> says why when it failed.</summary>
public readonly record struct TagResult(string Path, TagOutcome Outcome, string? Error = null);

/// <summary>
/// Finds the videos of a folder and sets or clears their title. Nothing here knows about the window,
/// so all of it can be tested and run off the window's thread.
/// </summary>
public static class TitleTagger
{
    /// <summary>
    /// The kinds of file whose title is changed: MPEG-4 and Windows Media. Each is one the tag library
    /// opens by its extension, which a test holds this list to.
    /// </summary>
    public static readonly IReadOnlySet<string> Extensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp4", ".m4v", ".wmv" };

    public static bool IsVideo(string path) => Extensions.Contains(Path.GetExtension(path));

    /// <summary>The videos of a folder, by name. A subfolder that cannot be opened is passed over.</summary>
    public static IReadOnlyList<string> FindVideos(string folder, bool includeSubfolders)
    {
        var options = new EnumerationOptions { RecurseSubdirectories = includeSubfolders, IgnoreInaccessible = true };
        return Directory.EnumerateFiles(folder, "*", options)
            .Where(IsVideo)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string TitleFor(string path, TitleMode mode) =>
        mode == TitleMode.Clear ? "" : Path.GetFileNameWithoutExtension(path);

    /// <summary>The title a file has now, empty when it has none, or null when the file cannot be read.</summary>
    public static string? ReadTitle(string path)
    {
        try
        {
            using var file = TagLib.File.Create(path);
            return file.Tag.Title ?? "";
        }
        catch (Exception e) when (IsOfTheFile(e))
        {
            return null;
        }
    }

    /// <summary>
    /// Sets or clears one file's title. A file that already has the title asked for is not written,
    /// which keeps its date and saves rewriting a large video for nothing.
    /// </summary>
    public static TagResult Apply(string path, TitleMode mode)
    {
        try
        {
            using var file = TagLib.File.Create(path);
            var wanted = TitleFor(path, mode);
            if ((file.Tag.Title ?? "") == wanted)
            {
                return new TagResult(path, TagOutcome.Unchanged);
            }

            file.Tag.Title = wanted.Length == 0 ? null : wanted;
            file.Save();
            return new TagResult(path, TagOutcome.Changed);
        }
        catch (Exception e) when (IsOfTheFile(e))
        {
            return new TagResult(path, TagOutcome.Failed, e.Message);
        }
    }

    /// <summary>
    /// Does every file in turn. One that fails (in use, read-only, not really a video) is reported
    /// and the rest go on; <paramref name="progress"/> hears of each as it is done.
    /// </summary>
    public static IReadOnlyList<TagResult> ApplyAll(
        IReadOnlyList<string> files,
        TitleMode mode,
        IProgress<TagResult>? progress = null,
        CancellationToken cancel = default)
    {
        var results = new List<TagResult>(files.Count);
        foreach (var file in files)
        {
            cancel.ThrowIfCancellationRequested();
            var result = Apply(file, mode);
            results.Add(result);
            progress?.Report(result);
        }

        return results;
    }

    /// <summary>A failure that belongs to the one file, whatever the tag library threw for it.</summary>
    private static bool IsOfTheFile(Exception e) => e is not (OutOfMemoryException or StackOverflowException or OperationCanceledException);
}
