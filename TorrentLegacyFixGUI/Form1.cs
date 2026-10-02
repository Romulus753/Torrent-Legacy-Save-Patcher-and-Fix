using System;
using System.IO;
using System.Windows.Forms;

namespace TorrentLegacyFixGUI
{
    public partial class Form1 : Form
    {
        private Button selectButton;
        private Button installButton;
        private Button restoreButton;
        private TextBox statusBox;

        private string? selectedFile;
        private bool canInstall = false;

        public Form1()
        {
            InitializeComponent();

            Text = "Torrent Legacy Fix";
            Width = 750;
            Height = 550;

            selectButton = new Button();
            selectButton.Text = "Select regulation.bin";
            selectButton.Width = 220;
            selectButton.Height = 40;
            selectButton.Left = 30;
            selectButton.Top = 30;
            selectButton.Click += SelectButton_Click;


            installButton = new Button();
            installButton.Text = "Install Torrent Fix";
            installButton.Width = 220;
            installButton.Height = 40;
            installButton.Left = 280;
            installButton.Top = 30;
            installButton.Enabled = false;
            installButton.Click += InstallButton_Click;


            restoreButton = new Button();
            restoreButton.Text = "Restore Backup";
            restoreButton.Width = 180;
            restoreButton.Height = 40;
            restoreButton.Left = 530;
            restoreButton.Top = 30;
            restoreButton.Enabled = false;
            restoreButton.Click += RestoreButton_Click;


            statusBox = new TextBox();
            statusBox.Multiline = true;
            statusBox.ScrollBars = ScrollBars.Vertical;
            statusBox.ReadOnly = true;
            statusBox.Left = 30;
            statusBox.Top = 100;
            statusBox.Width = 650;
            statusBox.Height = 350;
            statusBox.Text =
                "Waiting for regulation.bin...";


            Controls.Add(selectButton);
            Controls.Add(installButton);
            Controls.Add(restoreButton);
            Controls.Add(statusBox);
        }


        private void SelectButton_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Select Elden Ring regulation.bin";
            dialog.Filter = "Elden Ring regulation (*.bin)|*.bin";


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedFile = dialog.FileName;


                FileAttributes attributes = File.GetAttributes(selectedFile);

                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    statusBox.Text =
                        $"Selected:\r\n{selectedFile}\r\n\r\n" +
                        "ERROR:\r\n\r\n" +
                        "Your regulation.bin is set to read only.\r\n\r\n" +
                        "Right-click the file, select Properties, temporarily disable Read-only, and try again.";

                    canInstall = false;
                    installButton.Enabled = false;
                    restoreButton.Enabled = false;

    return;
}


                statusBox.Text =
                    "Checking regulation...\r\n\r\n";


                TorrentFixer fixer = new TorrentFixer();

                string result = fixer.Inspect(selectedFile);


                statusBox.Text =
                    $"Selected:\r\n{selectedFile}\r\n\r\n{result}";


                if (result.Contains("need the Torrent fix"))
                {
                    canInstall = true;
                    installButton.Enabled = true;
                }
                else
                {
                    canInstall = false;
                    installButton.Enabled = false;
                }


                restoreButton.Enabled =
                    File.Exists(selectedFile + ".torrentbackup");
            }
        }


        private void InstallButton_Click(object? sender, EventArgs e)
        {
            if (!canInstall || selectedFile == null)
                return;


            TorrentFixer fixer = new TorrentFixer();

            string result = fixer.Patch(selectedFile);

            statusBox.Text = result;


            restoreButton.Enabled =
                File.Exists(selectedFile + ".torrentbackup");
        }


        private void RestoreButton_Click(object? sender, EventArgs e)
        {
            if (selectedFile == null)
                return;


            TorrentFixer fixer = new TorrentFixer();

            string result = fixer.RestoreBackup(selectedFile);

            statusBox.Text = result;
        }
    }
}