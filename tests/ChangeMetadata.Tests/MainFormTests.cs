using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace ChangeMetadata.Tests;

/// <summary>The window itself, run off the screen in a message loop of its own, on real files.</summary>
public sealed class MainFormTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("change-title-window-").FullName;

    public void Dispose() => Directory.Delete(folder, recursive: true);

    [Fact]
    public void The_window_lists_a_folder_sets_and_clears_its_titles_and_says_what_failed()
    {
        var holiday = Touch("Holiday 2020.mp4", SampleVideo.Mp4());
        Touch("broken.MP4", SampleVideo.Garbage());
        Touch("notes.txt", []);
        var deep = Touch(Path.Combine("sub", "deep.m4v"), SampleVideo.Mp4());
        var asked = new List<string>();
        var warned = new List<string>();

        InWindow(async form =>
        {
            form.Ask = question =>
            {
                asked.Add(question);
                return true;
            };
            form.Warn = warned.Add;

            await form.LoadFolderAsync(folder);
            Assert.Equal([("broken.MP4", "", "Cannot be read"), ("Holiday 2020.mp4", "", "")], form.Rows);
            Assert.Equal("2 videos.", form.Status);

            // Ticking the box loads the folder again by itself, this time with what is under it.
            form.IncludeSubfolders = true;
            await Idle(form);
            Assert.Equal(["broken.MP4", "Holiday 2020.mp4", Path.Combine("sub", "deep.m4v")], form.Rows.Select(row => row.File));

            await form.ChangeTitlesAsync(TitleMode.FromFileName, "Set the title of {0} to its file name?");
            Assert.Equal(["Set the title of 3 videos to its file name?"], asked);
            Assert.StartsWith("Failed: ", form.Rows[0].Result);
            Assert.Equal(("Holiday 2020.mp4", "Holiday 2020", "Changed"), form.Rows[1]);
            Assert.Equal((Path.Combine("sub", "deep.m4v"), "deep", "Changed"), form.Rows[2]);
            Assert.Equal("2 changed, 0 already right, 1 failed.", form.Status);
            Assert.Single(warned);
            Assert.Equal("Holiday 2020", TitleTagger.ReadTitle(holiday));
            Assert.Equal("deep", TitleTagger.ReadTitle(deep));
            Assert.False(form.IsBusy);

            await form.ChangeTitlesAsync(TitleMode.FromFileName, "{0}");
            Assert.Equal("0 changed, 2 already right, 1 failed.", form.Status);

            await form.ChangeTitlesAsync(TitleMode.Clear, "{0}");
            Assert.Equal(["", ""], form.Rows.Skip(1).Select(row => row.Title));
            Assert.Equal("", TitleTagger.ReadTitle(holiday));
        });
    }

    [Fact]
    public void Nothing_is_changed_when_the_person_says_no()
    {
        var path = Touch("Keep me.mp4", SampleVideo.Mp4());

        InWindow(async form =>
        {
            form.Ask = _ => false;
            await form.LoadFolderAsync(folder);
            await form.ChangeTitlesAsync(TitleMode.FromFileName, "{0}");
            Assert.Equal([("Keep me.mp4", "", "")], form.Rows);
            Assert.Equal("1 video.", form.Status);
        });

        Assert.Equal("", TitleTagger.ReadTitle(path));
    }

    private string Touch(string relative, byte[] content)
    {
        var path = Path.Combine(folder, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        return path;
    }

    private static async Task Idle(MainForm form)
    {
        while (form.IsBusy)
        {
            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Runs a script against the window inside a real message loop, on a thread of the kind a window needs,
    /// with the window off the screen. What the script throws is thrown again here.
    /// </summary>
    private static void InWindow(Func<MainForm, Task> script)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            using var form = new MainForm { StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), ShowInTaskbar = false };
            form.Shown += async (_, _) =>
            {
                try
                {
                    await script(form);
                }
                catch (Exception problem)
                {
                    failure = problem;
                }
                finally
                {
                    form.Close();
                }
            };
            Application.Run(form);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
