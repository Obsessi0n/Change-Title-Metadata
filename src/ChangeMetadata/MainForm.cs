using System.ComponentModel;

namespace ChangeMetadata;

/// <summary>
/// The window: a folder is chosen, its videos are listed with the title each has now, and two buttons
/// set every title to the file's name or clear it. The work itself is <see cref="TitleTagger"/>'s.
/// </summary>
public partial class MainForm : Form
{
    private readonly Dictionary<string, ListViewItem> rows = new(StringComparer.OrdinalIgnoreCase);
    private string? folder;
    private bool isBusy;

    public MainForm()
    {
        InitializeComponent();
        Ask = question => MessageBox.Show(this, question, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        Warn = message => MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        ShowIdle();
    }

    // None of these is for the designer to write into its file, which is what the attribute says.

    /// <summary>How the window asks before it changes anything, and how it warns. A test puts its own in their place.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal Func<string, bool> Ask { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal Action<string> Warn { get; set; }

    internal bool IsBusy => isBusy;

    internal string Status => lblStatus.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool IncludeSubfolders
    {
        get => chkSubfolders.Checked;
        set => chkSubfolders.Checked = value;
    }

    /// <summary>The list as it reads: each video, the title it has now, and what was last done to it.</summary>
    internal IReadOnlyList<(string File, string Title, string Result)> Rows =>
        listVideos.Items.Cast<ListViewItem>().Select(row => (row.SubItems[0].Text, row.SubItems[1].Text, row.SubItems[2].Text)).ToList();

    private async void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog { Description = "Choose the folder with your videos.", UseDescriptionForTitle = true };
        if (folder is not null)
        {
            dialog.InitialDirectory = folder;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await LoadFolderAsync(dialog.SelectedPath);
        }
    }

    private async void chkSubfolders_CheckedChanged(object? sender, EventArgs e)
    {
        if (folder is not null)
        {
            await LoadFolderAsync(folder);
        }
    }

    private async void btnSetTitles_Click(object? sender, EventArgs e) =>
        await ChangeTitlesAsync(TitleMode.FromFileName, "Set the title of {0} to its file name?");

    private async void btnClearTitles_Click(object? sender, EventArgs e) =>
        await ChangeTitlesAsync(TitleMode.Clear, "Clear the title of {0}?");

    /// <summary>Lists the folder's videos, then reads the title of each without holding the window up.</summary>
    internal async Task LoadFolderAsync(string path)
    {
        if (isBusy)
        {
            return;
        }

        folder = path;
        txtFolder.Text = path;
        SetBusy(true, "Looking for videos...");
        try
        {
            var includeSubfolders = chkSubfolders.Checked;
            var files = await Task.Run(() => TitleTagger.FindVideos(path, includeSubfolders));

            listVideos.BeginUpdate();
            listVideos.Items.Clear();
            rows.Clear();
            foreach (var file in files)
            {
                var row = new ListViewItem(new[] { Path.GetRelativePath(path, file), "", "" }) { Tag = file };
                rows[file] = row;
                listVideos.Items.Add(row);
            }

            listVideos.EndUpdate();

            progress.Maximum = Math.Max(1, files.Count);
            var read = new Progress<(string File, string? Title)>(one =>
            {
                rows[one.File].SubItems[1].Text = one.Title ?? "";
                rows[one.File].SubItems[2].Text = one.Title is null ? "Cannot be read" : "";
                progress.Value = Math.Min(progress.Maximum, progress.Value + 1);
            });
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    ((IProgress<(string, string?)>)read).Report((file, TitleTagger.ReadTitle(file)));
                }
            });
        }
        catch (Exception problem) when (problem is IOException or UnauthorizedAccessException)
        {
            Warn(problem.Message);
        }
        finally
        {
            SetBusy(false);
            ShowIdle();
        }
    }

    internal async Task ChangeTitlesAsync(TitleMode mode, string question)
    {
        var files = rows.Keys.ToList();
        if (isBusy || files.Count == 0)
        {
            return;
        }

        var what = files.Count == 1 ? "1 video" : $"{files.Count} videos";
        if (!Ask(string.Format(question, what)))
        {
            return;
        }

        SetBusy(true, "Working...");
        progress.Maximum = files.Count;
        var done = new Progress<TagResult>(result =>
        {
            var row = rows[result.Path];
            row.SubItems[2].Text = result.Outcome switch
            {
                TagOutcome.Changed => "Changed",
                TagOutcome.Unchanged => "Already right",
                _ => $"Failed: {result.Error}",
            };
            if (result.Outcome != TagOutcome.Failed)
            {
                row.SubItems[1].Text = TitleTagger.TitleFor(result.Path, mode);
            }

            progress.Value = Math.Min(progress.Maximum, progress.Value + 1);
        });

        var results = await Task.Run(() => TitleTagger.ApplyAll(files, mode, done));
        SetBusy(false);

        var changed = results.Count(r => r.Outcome == TagOutcome.Changed);
        var unchanged = results.Count(r => r.Outcome == TagOutcome.Unchanged);
        var failed = results.Count(r => r.Outcome == TagOutcome.Failed);
        lblStatus.Text = $"{changed} changed, {unchanged} already right, {failed} failed.";
        if (failed > 0)
        {
            Warn($"{failed} of {results.Count} could not be changed. The Result column says why for each.");
        }
    }

    /// <summary>While work is going on nothing else may be started: one folder is loaded or changed at a time.</summary>
    private void SetBusy(bool busy, string? status = null)
    {
        isBusy = busy;
        btnBrowse.Enabled = chkSubfolders.Enabled = !busy;
        btnSetTitles.Enabled = btnClearTitles.Enabled = !busy && rows.Count > 0;
        progress.Value = 0;
        if (status is not null)
        {
            lblStatus.Text = status;
        }
    }

    private void ShowIdle()
    {
        btnSetTitles.Enabled = btnClearTitles.Enabled = rows.Count > 0;
        lblStatus.Text = folder is null
            ? "Choose a folder to list its videos."
            : rows.Count == 1 ? "1 video." : $"{rows.Count} videos.";
    }
}
