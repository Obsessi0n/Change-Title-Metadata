namespace ChangeMetadata.Tests;

public sealed class TitleTaggerTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("change-title-").FullName;

    public void Dispose()
    {
        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(folder, recursive: true);
    }

    [Theory]
    [InlineData("a.mp4", true)]
    [InlineData("A.MP4", true)]
    [InlineData("b.Wmv", true)]
    [InlineData("c.m4v", true)]
    [InlineData("d.mov", false)]
    [InlineData("e.mkv", false)]
    [InlineData("notes.txt", false)]
    [InlineData("mp4", false)]
    public void A_video_is_told_by_its_extension_whatever_its_capitals(string name, bool expected)
    {
        Assert.Equal(expected, TitleTagger.IsVideo(name));
    }

    [Fact]
    public void Every_extension_is_one_the_tag_library_knows()
    {
        foreach (var extension in TitleTagger.Extensions)
        {
            Assert.True(TagLib.FileTypes.AvailableTypes.ContainsKey("taglib/" + extension.TrimStart('.')), extension);
        }
    }

    [Fact]
    public void The_videos_of_a_folder_are_listed_by_name_with_its_subfolders_only_when_asked()
    {
        var b = Touch("b.MP4");
        var a = Touch("a.mp4");
        Touch("notes.txt");
        var deep = Touch(Path.Combine("season 1", "c.wmv"));

        Assert.Equal([a, b], TitleTagger.FindVideos(folder, includeSubfolders: false));
        Assert.Equal([a, b, deep], TitleTagger.FindVideos(folder, includeSubfolders: true));
    }

    [Fact]
    public void The_title_asked_for_is_the_file_name_without_its_extension_or_nothing()
    {
        var path = Path.Combine(folder, "Holiday.in.Porto.2020.mp4");
        Assert.Equal("Holiday.in.Porto.2020", TitleTagger.TitleFor(path, TitleMode.FromFileName));
        Assert.Equal("", TitleTagger.TitleFor(path, TitleMode.Clear));
    }

    [Fact]
    public void A_title_is_set_from_the_file_name_left_alone_when_already_right_and_cleared()
    {
        var path = Touch("Holiday 2020.mp4", SampleVideo.Mp4());
        Assert.Equal("", TitleTagger.ReadTitle(path));

        Assert.Equal(TagOutcome.Changed, TitleTagger.Apply(path, TitleMode.FromFileName).Outcome);
        Assert.Equal("Holiday 2020", TitleTagger.ReadTitle(path));

        // A second pass has nothing to do, and does not write the file again.
        File.SetLastWriteTimeUtc(path, new DateTime(2020, 10, 4, 12, 0, 0, DateTimeKind.Utc));
        Assert.Equal(TagOutcome.Unchanged, TitleTagger.Apply(path, TitleMode.FromFileName).Outcome);
        Assert.Equal(new DateTime(2020, 10, 4, 12, 0, 0, DateTimeKind.Utc), File.GetLastWriteTimeUtc(path));

        Assert.Equal(TagOutcome.Changed, TitleTagger.Apply(path, TitleMode.Clear).Outcome);
        Assert.Equal("", TitleTagger.ReadTitle(path));
        Assert.Equal(TagOutcome.Unchanged, TitleTagger.Apply(path, TitleMode.Clear).Outcome);
    }

    [Fact]
    public void A_file_that_is_no_video_fails_by_itself_and_the_rest_go_on()
    {
        var broken = Touch("a broken.mp4", SampleVideo.Garbage());
        var fine = Touch("b fine.mp4", SampleVideo.Mp4());
        var heard = new List<TagResult>();

        var results = TitleTagger.ApplyAll([broken, fine], TitleMode.FromFileName, new Listener(heard));

        Assert.Equal([TagOutcome.Failed, TagOutcome.Changed], results.Select(result => result.Outcome));
        Assert.False(string.IsNullOrWhiteSpace(results[0].Error));
        Assert.Equal(results, heard);
        Assert.Null(TitleTagger.ReadTitle(broken));
        Assert.Equal(SampleVideo.Garbage(), File.ReadAllBytes(broken));
        Assert.Equal("b fine", TitleTagger.ReadTitle(fine));
    }

    [Fact]
    public void A_file_that_may_not_be_written_is_reported_and_left_as_it_was()
    {
        var path = Touch("locked.mp4", SampleVideo.Mp4());
        File.SetAttributes(path, FileAttributes.ReadOnly);

        var result = TitleTagger.Apply(path, TitleMode.FromFileName);

        Assert.Equal(TagOutcome.Failed, result.Outcome);
        Assert.Equal(SampleVideo.Mp4(), File.ReadAllBytes(path));
    }

    [Fact]
    public void A_run_that_was_called_off_changes_nothing()
    {
        var path = Touch("later.mp4", SampleVideo.Mp4());
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();

        Assert.Throws<OperationCanceledException>(() => TitleTagger.ApplyAll([path], TitleMode.FromFileName, cancel: cancel.Token));
        Assert.Equal("", TitleTagger.ReadTitle(path));
    }

    private string Touch(string relative, byte[]? content = null)
    {
        var path = Path.Combine(folder, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content ?? []);
        return path;
    }

    private sealed class Listener(List<TagResult> heard) : IProgress<TagResult>
    {
        public void Report(TagResult value) => heard.Add(value);
    }
}
