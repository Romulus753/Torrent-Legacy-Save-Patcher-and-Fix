using System;
using System.IO;
using System.Windows.Forms;

namespace TorrentLegacyFixGUI
{
    public partial class Form1 : Form
    {
        private Button selectGameFolderButton;
        private Button selectTargetButton;
        private Button installButton;
        private Button restoreButton;

        private Label gameFolderLabel;
        private Label targetLabel;
        private Label gameFolderPathLabel;
        private Label targetPathLabel;

        private TextBox statusBox;

        private string? selectedGameFolder;
        private string? selectedDonorFile;
        private string? selectedTargetFile;

        private bool gameFolderReady = false;
        private bool targetNeedsFix = false;

        public Form1()
        {
            InitializeComponent();

            Text = "Torrent Legacy Fix";
            Width = 800;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            // ---------------------------------------------------------
            // Elden Ring Game folder
            // ---------------------------------------------------------

            gameFolderLabel = new Label();
            gameFolderLabel.Text = "1. Elden Ring Game Folder";
            gameFolderLabel.Left = 30;
            gameFolderLabel.Top = 25;
            gameFolderLabel.Width = 350;
            gameFolderLabel.Height = 25;

            selectGameFolderButton = new Button();
            selectGameFolderButton.Text = "Select Elden Ring Game Folder";
            selectGameFolderButton.Left = 30;
            selectGameFolderButton.Top = 55;
            selectGameFolderButton.Width = 250;
            selectGameFolderButton.Height = 40;
            selectGameFolderButton.Click += SelectGameFolderButton_Click;

            gameFolderPathLabel = new Label();
            gameFolderPathLabel.Text = "No folder selected.";
            gameFolderPathLabel.Left = 300;
            gameFolderPathLabel.Top = 65;
            gameFolderPathLabel.Width = 450;
            gameFolderPathLabel.Height = 40;
            gameFolderPathLabel.AutoEllipsis = true;

            // ---------------------------------------------------------
            // Legacy / modded regulation
            // ---------------------------------------------------------

            targetLabel = new Label();
            targetLabel.Text = "2. Legacy / Modded regulation.bin";
            targetLabel.Left = 30;
            targetLabel.Top = 120;
            targetLabel.Width = 350;
            targetLabel.Height = 25;

            selectTargetButton = new Button();
            selectTargetButton.Text = "Select Legacy / Modded File";
            selectTargetButton.Left = 30;
            selectTargetButton.Top = 150;
            selectTargetButton.Width = 250;
            selectTargetButton.Height = 40;
            selectTargetButton.Enabled = false;
            selectTargetButton.Click += SelectTargetButton_Click;

            targetPathLabel = new Label();
            targetPathLabel.Text = "No file selected.";
            targetPathLabel.Left = 300;
            targetPathLabel.Top = 160;
            targetPathLabel.Width = 450;
            targetPathLabel.Height = 40;
            targetPathLabel.AutoEllipsis = true;

            // ---------------------------------------------------------
            // Install / Restore
            // ---------------------------------------------------------

            installButton = new Button();
            installButton.Text = "Install Torrent Fix";
            installButton.Left = 30;
            installButton.Top = 215;
            installButton.Width = 250;
            installButton.Height = 45;
            installButton.Enabled = false;
            installButton.Click += InstallButton_Click;

            restoreButton = new Button();
            restoreButton.Text = "Restore Backup";
            restoreButton.Left = 300;
            restoreButton.Top = 215;
            restoreButton.Width = 200;
            restoreButton.Height = 45;
            restoreButton.Enabled = false;
            restoreButton.Click += RestoreButton_Click;

            // ---------------------------------------------------------
            // Status box
            // ---------------------------------------------------------

            statusBox = new TextBox();
            statusBox.Multiline = true;
            statusBox.ScrollBars = ScrollBars.Vertical;
            statusBox.ReadOnly = true;
            statusBox.Left = 30;
            statusBox.Top = 285;
            statusBox.Width = 720;
            statusBox.Height = 300;

            statusBox.Text =
                "Torrent Legacy Fix\r\n\r\n" +
                "Step 1: Select your Elden Ring Game folder.\r\n" +
                "This is the folder containing eldenring.exe.\r\n\r\n" +
                "Step 2: Select the legacy/modded regulation.bin you want to repair.\r\n\r\n" +
                "Only the legacy/modded regulation will be modified.\r\n" +
                "Your Elden Ring installation is used only as a source for the current " +
                "Torrent data and required Oodle library.";

            Controls.Add(gameFolderLabel);
            Controls.Add(selectGameFolderButton);
            Controls.Add(gameFolderPathLabel);

            Controls.Add(targetLabel);
            Controls.Add(selectTargetButton);
            Controls.Add(targetPathLabel);

            Controls.Add(installButton);
            Controls.Add(restoreButton);

            Controls.Add(statusBox);
        }

        private void SelectGameFolderButton_Click(
            object? sender,
            EventArgs e)
        {
            using FolderBrowserDialog dialog =
                new FolderBrowserDialog();

            dialog.Description =
                "Select the Elden Ring Game folder containing eldenring.exe";

            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string selectedFolder = dialog.SelectedPath;

            TorrentFixer fixer = new TorrentFixer();

            string result =
                fixer.InitializeGameFolder(selectedFolder);

            if (!result.StartsWith(
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase))
            {
                gameFolderReady = false;
                selectedGameFolder = null;
                selectedDonorFile = null;

                gameFolderPathLabel.Text =
                    "Invalid Elden Ring Game folder.";

                statusBox.Text = result;

                selectTargetButton.Enabled = false;

                UpdateButtons();
                return;
            }

            selectedGameFolder = selectedFolder;

            selectedDonorFile =
                Path.Combine(
                    selectedGameFolder,
                    "regulation.bin");

            gameFolderReady = true;

            gameFolderPathLabel.Text =
                selectedGameFolder;

            selectTargetButton.Enabled = true;

            statusBox.Text =
                result +
                "\r\n\r\n" +
                "Next: Select the legacy/modded regulation.bin you want to repair.";

            // If a target was already selected before changing the
            // Game folder, re-inspect it now that Oodle is initialized.
            if (selectedTargetFile != null &&
                File.Exists(selectedTargetFile))
            {
                InspectTarget();
            }

            UpdateButtons();
        }

        private void SelectTargetButton_Click(
            object? sender,
            EventArgs e)
        {
            if (!gameFolderReady ||
                selectedDonorFile == null)
            {
                statusBox.Text =
                    "ERROR:\r\n\r\n" +
                    "Please select your Elden Ring Game folder first.";

                return;
            }

            using OpenFileDialog dialog =
                new OpenFileDialog();

            dialog.Title =
                "Select the LEGACY / MODDED regulation.bin you want to repair";

            dialog.Filter =
                "Elden Ring regulation (regulation.bin)|regulation.bin|BIN files (*.bin)|*.bin";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            selectedTargetFile = dialog.FileName;
            targetPathLabel.Text = selectedTargetFile;

            InspectTarget();
        }

        private void InspectTarget()
        {
            if (selectedTargetFile == null)
                return;

            try
            {
                if (!File.Exists(selectedTargetFile))
                {
                    targetNeedsFix = false;

                    statusBox.Text =
                        "ERROR:\r\n\r\n" +
                        "The selected legacy/modded regulation.bin no longer exists.";

                    UpdateButtons();
                    return;
                }

                FileAttributes attributes =
                    File.GetAttributes(selectedTargetFile);

                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    targetNeedsFix = false;

                    statusBox.Text =
                        "ERROR:\r\n\r\n" +
                        "The legacy/modded regulation.bin is set to read-only.\r\n\r\n" +
                        "Right-click the file, select Properties, temporarily disable Read-only, and try again.";

                    UpdateButtons();
                    return;
                }

                if (!ValidateDifferentFiles())
                {
                    targetNeedsFix = false;
                    UpdateButtons();
                    return;
                }

                statusBox.Text =
                    "Checking the legacy/modded regulation...\r\n\r\n";

                TorrentFixer fixer =
                    new TorrentFixer();

                string result =
                    fixer.Inspect(selectedTargetFile);

                statusBox.Text =
                    "LEGACY / MODDED REGULATION:\r\n" +
                    selectedTargetFile +
                    "\r\n\r\n" +
                    result;

                targetNeedsFix =
                    result.Contains(
                        "need the Torrent fix",
                        StringComparison.OrdinalIgnoreCase);

                restoreButton.Enabled =
                    File.Exists(
                        selectedTargetFile +
                        ".torrentbackup");

                UpdateButtons();
            }
            catch (Exception ex)
            {
                targetNeedsFix = false;

                statusBox.Text =
                    "ERROR:\r\n\r\n" +
                    ex.Message;

                UpdateButtons();
            }
        }

        private bool ValidateDifferentFiles()
        {
            if (selectedTargetFile == null ||
                selectedDonorFile == null)
            {
                return true;
            }

            string target =
                Path.GetFullPath(selectedTargetFile);

            string donor =
                Path.GetFullPath(selectedDonorFile);

            if (string.Equals(
                target,
                donor,
                StringComparison.OrdinalIgnoreCase))
            {
                statusBox.Text =
                    "ERROR:\r\n\r\n" +
                    "The legacy/modded regulation.bin cannot be the same file as " +
                    "the current vanilla regulation.bin in your Elden Ring Game folder.\r\n\r\n" +
                    "Select the old or modded regulation.bin you actually want to repair.\r\n\r\n" +
                    "The regulation.bin in your Elden Ring Game folder will NOT be modified.";

                return false;
            }

            return true;
        }

        private void UpdateButtons()
        {
            bool differentFiles =
                ValidateDifferentFiles();

            installButton.Enabled =
                gameFolderReady &&
                targetNeedsFix &&
                selectedTargetFile != null &&
                selectedDonorFile != null &&
                differentFiles;

            selectTargetButton.Enabled =
                gameFolderReady;

            if (selectedTargetFile != null)
            {
                restoreButton.Enabled =
                    File.Exists(
                        selectedTargetFile +
                        ".torrentbackup");
            }
            else
            {
                restoreButton.Enabled = false;
            }
        }

        private void InstallButton_Click(
            object? sender,
            EventArgs e)
        {
            if (!gameFolderReady ||
                selectedTargetFile == null ||
                selectedDonorFile == null ||
                !targetNeedsFix)
            {
                return;
            }

            if (!ValidateDifferentFiles())
            {
                UpdateButtons();
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    "Torrent Legacy Fix is ready to repair:\r\n\r\n" +
                    selectedTargetFile +
                    "\r\n\r\n" +
                    "The current vanilla regulation.bin in your Elden Ring Game folder " +
                    "will ONLY be used as a source for the missing Torrent data.\r\n\r\n" +
                    "Your Elden Ring installation will NOT be modified.\r\n\r\n" +
                    "A .torrentbackup copy of the legacy/modded regulation will be " +
                    "created before any changes are made.\r\n\r\n" +
                    "Continue?",
                    "Install Torrent Fix",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (confirmation != DialogResult.Yes)
                return;

            TorrentFixer fixer =
                new TorrentFixer();

            string result =
                fixer.Patch(
                    selectedTargetFile,
                    selectedDonorFile);

            statusBox.Text = result;

            restoreButton.Enabled =
                File.Exists(
                    selectedTargetFile +
                    ".torrentbackup");

            // If the patch succeeded, prevent the user
            // from accidentally running it again.
            if (result.Contains(
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase))
            {
                targetNeedsFix = false;
            }

            UpdateButtons();
        }

        private void RestoreButton_Click(
            object? sender,
            EventArgs e)
        {
            if (selectedTargetFile == null)
                return;

            DialogResult confirmation =
                MessageBox.Show(
                    "Restore the original legacy/modded regulation from its " +
                    ".torrentbackup file?\r\n\r\n" +
                    "This will replace the currently patched regulation.",
                    "Restore Backup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (confirmation != DialogResult.Yes)
                return;

            TorrentFixer fixer =
                new TorrentFixer();

            string result =
                fixer.RestoreBackup(
                    selectedTargetFile);

            statusBox.Text = result;

            // Re-inspect after restoring so the GUI knows
            // whether the restored regulation needs the fix.
            try
            {
                if (gameFolderReady)
                {
                    string inspection =
                        fixer.Inspect(selectedTargetFile);

                    statusBox.Text +=
                        "\r\n\r\n" +
                        inspection;

                    targetNeedsFix =
                        inspection.Contains(
                            "need the Torrent fix",
                            StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    targetNeedsFix = false;
                }
            }
            catch
            {
                targetNeedsFix = false;
            }

            UpdateButtons();
        }
    }
}