using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Contollo.Rdel.ZipRunner.AI.Engine;
using Contollo.Rdel.ZipRunner.AI.Settings;

namespace Contollo.Rdel.ZipRunner
{
    internal sealed class AiSessionSettingsDialog : Form
    {
        private readonly ComboBox initializeLevel;
        private readonly ComboBox rehydrateLevel;
        private readonly ComboBox continueLevel;
        private readonly TextBox outputDirectory;
        private readonly Button browseButton;
        private readonly Button saveButton;
        private readonly Button cancelButton;
        private readonly Button resetButton;

        public AiSessionSettingsDialog()
        {
            Text = "AI Session Settings";
            StartPosition = FormStartPosition.CenterParent;
            Width = 640;
            Height = 330;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var labelFont = new Font(Font, FontStyle.Bold);

            var title = new Label
            {
                Text = "RDEL Session Protocol / AI Context Settings",
                Left = 16,
                Top = 16,
                Width = 580,
                Height = 24,
                Font = labelFont
            };

            var help = new Label
            {
                Text = "Choose how much documentation is included when copying AI session context.",
                Left = 16,
                Top = 42,
                Width = 580,
                Height = 24
            };

            initializeLevel = CreateLevelCombo(210, 78);
            rehydrateLevel = CreateLevelCombo(210, 112);
            continueLevel = CreateLevelCombo(210, 146);

            Controls.Add(title);
            Controls.Add(help);
            Controls.Add(CreateLabel("Initialize context level", 16, 82));
            Controls.Add(initializeLevel);
            Controls.Add(CreateLabel("Rehydrate context level", 16, 116));
            Controls.Add(rehydrateLevel);
            Controls.Add(CreateLabel("Continue context level", 16, 150));
            Controls.Add(continueLevel);

            Controls.Add(CreateLabel("Context package output folder", 16, 190));

            outputDirectory = new TextBox
            {
                Left = 210,
                Top = 186,
                Width = 320
            };

            browseButton = new Button
            {
                Text = "Browse...",
                Left = 540,
                Top = 184,
                Width = 80
            };
            browseButton.Click += BrowseButton_Click;

            resetButton = new Button
            {
                Text = "Reset Defaults",
                Left = 16,
                Top = 240,
                Width = 120
            };
            resetButton.Click += ResetButton_Click;

            saveButton = new Button
            {
                Text = "Save",
                Left = 430,
                Top = 240,
                Width = 90,
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Left = 530,
                Top = 240,
                Width = 90,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(outputDirectory);
            Controls.Add(browseButton);
            Controls.Add(resetButton);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;

            LoadSettings();
        }

        private static Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 180,
                Height = 24
            };
        }

        private static ComboBox CreateLevelCombo(int left, int top)
        {
            var combo = new ComboBox
            {
                Left = left,
                Top = top,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            combo.Items.Add(ContextLevel.Reference);
            combo.Items.Add(ContextLevel.Summary);
            combo.Items.Add(ContextLevel.Full);

            return combo;
        }

        private void LoadSettings()
        {
            var settings = AiSessionSettingsService.Load();

            initializeLevel.SelectedItem = settings.InitializeDocumentLevel;
            rehydrateLevel.SelectedItem = settings.RehydrateDocumentLevel;
            continueLevel.SelectedItem = settings.ContinueDocumentLevel;
            outputDirectory.Text = settings.OutputDirectory;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var settings = new AiSessionSettings
            {
                InitializeDocumentLevel = (ContextLevel)initializeLevel.SelectedItem,
                RehydrateDocumentLevel = (ContextLevel)rehydrateLevel.SelectedItem,
                ContinueDocumentLevel = (ContextLevel)continueLevel.SelectedItem,
                OutputDirectory = outputDirectory.Text
            };

            AiSessionSettingsService.Save(settings);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            var defaults = new AiSessionSettings();

            initializeLevel.SelectedItem = defaults.InitializeDocumentLevel;
            rehydrateLevel.SelectedItem = defaults.RehydrateDocumentLevel;
            continueLevel.SelectedItem = defaults.ContinueDocumentLevel;
            outputDirectory.Text = defaults.OutputDirectory;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select AI context package output folder.";

                if (!string.IsNullOrWhiteSpace(outputDirectory.Text) && Directory.Exists(outputDirectory.Text))
                {
                    dialog.SelectedPath = outputDirectory.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    outputDirectory.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
