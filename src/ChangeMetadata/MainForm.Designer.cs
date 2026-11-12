namespace ChangeMetadata;

partial class MainForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        lblFolder = new Label();
        txtFolder = new TextBox();
        btnBrowse = new Button();
        chkSubfolders = new CheckBox();
        listVideos = new ListView();
        colFile = new ColumnHeader();
        colTitle = new ColumnHeader();
        colResult = new ColumnHeader();
        progress = new ProgressBar();
        lblStatus = new Label();
        btnSetTitles = new Button();
        btnClearTitles = new Button();
        SuspendLayout();
        //
        // lblFolder
        //
        lblFolder.AutoSize = true;
        lblFolder.Location = new Point(12, 16);
        lblFolder.Name = "lblFolder";
        lblFolder.Size = new Size(43, 15);
        lblFolder.TabIndex = 0;
        lblFolder.Text = "Folder:";
        //
        // txtFolder
        //
        txtFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtFolder.Location = new Point(64, 12);
        txtFolder.Name = "txtFolder";
        txtFolder.ReadOnly = true;
        txtFolder.Size = new Size(726, 23);
        txtFolder.TabIndex = 1;
        //
        // btnBrowse
        //
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(798, 11);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(90, 25);
        btnBrowse.TabIndex = 2;
        btnBrowse.Text = "Browse...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;
        //
        // chkSubfolders
        //
        chkSubfolders.AutoSize = true;
        chkSubfolders.Location = new Point(64, 43);
        chkSubfolders.Name = "chkSubfolders";
        chkSubfolders.Size = new Size(124, 19);
        chkSubfolders.TabIndex = 3;
        chkSubfolders.Text = "Include subfolders";
        chkSubfolders.UseVisualStyleBackColor = true;
        chkSubfolders.CheckedChanged += chkSubfolders_CheckedChanged;
        //
        // listVideos
        //
        listVideos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listVideos.Columns.AddRange(new ColumnHeader[] { colFile, colTitle, colResult });
        listVideos.FullRowSelect = true;
        listVideos.Location = new Point(12, 72);
        listVideos.Name = "listVideos";
        listVideos.Size = new Size(876, 398);
        listVideos.TabIndex = 4;
        listVideos.UseCompatibleStateImageBehavior = false;
        listVideos.View = View.Details;
        //
        // colFile
        //
        colFile.Text = "File";
        colFile.Width = 360;
        //
        // colTitle
        //
        colTitle.Text = "Title now";
        colTitle.Width = 300;
        //
        // colResult
        //
        colResult.Text = "Result";
        colResult.Width = 190;
        //
        // progress
        //
        progress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progress.Location = new Point(12, 480);
        progress.Name = "progress";
        progress.Size = new Size(876, 12);
        progress.TabIndex = 5;
        //
        // lblStatus
        //
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 517);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(200, 15);
        lblStatus.TabIndex = 6;
        lblStatus.Text = "Choose a folder to list its videos.";
        //
        // btnSetTitles
        //
        btnSetTitles.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSetTitles.Location = new Point(566, 506);
        btnSetTitles.Name = "btnSetTitles";
        btnSetTitles.Size = new Size(190, 36);
        btnSetTitles.TabIndex = 7;
        btnSetTitles.Text = "Set titles to file names";
        btnSetTitles.UseVisualStyleBackColor = true;
        btnSetTitles.Click += btnSetTitles_Click;
        //
        // btnClearTitles
        //
        btnClearTitles.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClearTitles.Location = new Point(764, 506);
        btnClearTitles.Name = "btnClearTitles";
        btnClearTitles.Size = new Size(124, 36);
        btnClearTitles.TabIndex = 8;
        btnClearTitles.Text = "Clear titles";
        btnClearTitles.UseVisualStyleBackColor = true;
        btnClearTitles.Click += btnClearTitles_Click;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 554);
        Controls.Add(btnClearTitles);
        Controls.Add(btnSetTitles);
        Controls.Add(lblStatus);
        Controls.Add(progress);
        Controls.Add(listVideos);
        Controls.Add(chkSubfolders);
        Controls.Add(btnBrowse);
        Controls.Add(txtFolder);
        Controls.Add(lblFolder);
        MinimumSize = new Size(640, 400);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Change Title Metadata";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblFolder;
    private TextBox txtFolder;
    private Button btnBrowse;
    private CheckBox chkSubfolders;
    private ListView listVideos;
    private ColumnHeader colFile;
    private ColumnHeader colTitle;
    private ColumnHeader colResult;
    private ProgressBar progress;
    private Label lblStatus;
    private Button btnSetTitles;
    private Button btnClearTitles;
}
