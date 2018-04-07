using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace TexConvGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TEXCONV_PATH = "texconv.exe";
        private bool _processing = false;

        private StringBuilder _tmpBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Helpers

        private void AddFiles(string[] filenames)
        {
            if (filenames == null || filenames.Length == 0)
                return;

            foreach (var item in filenames)
            {
                if (!ListSelection.Items.Contains(item))
                    ListSelection.Items.Add(item);
            }
        }

        private static bool HasWriteAccess(string directory)
        {
            var permission = new FileIOPermission(FileIOPermissionAccess.Write, directory);
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(permission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        private string GetConvertToDdsArgs(string path)
        {
            _tmpBuilder.Clear();

            _tmpBuilder.Append(" -nologo");

            if (CheckPow2.IsChecked.HasValue && CheckPow2.IsChecked.Value)
                _tmpBuilder.Append(" -pow2");

            _tmpBuilder.Append($" -ft dds -f {ComboDdsFormat.SelectedItem} -m {ComboMipmaps.SelectedItem}");

            if (!string.IsNullOrEmpty(InputDestination.Text))
                _tmpBuilder.Append($" -o {InputDestination.Text}");

            _tmpBuilder.Append(path);

            return _tmpBuilder.ToString();
        }

        private string GetConvertToPngArgs(string path)
        {
            return $" -nologo -ft png -o {InputDestination.Text} {path}";
        }

        private void StartProcessing()
        {
            _processing = true;

            ListSelection.IsEnabled = false;
            BtnClear.IsEnabled = false;
            BtnSource.IsEnabled = false;
            BtnSourceClear.IsEnabled = false;
            BtnDestination.IsEnabled = false;
            BtnConvert.IsEnabled = false;
            ComboDdsFormat.IsEnabled = false;
            ComboMipmaps.IsEnabled = false;
            CheckPow2.IsEnabled = false;
        }

        private void FinishProcessing()
        {
            _processing = false;

            ListSelection.IsEnabled = true;
            BtnClear.IsEnabled = true;
            BtnSource.IsEnabled = true;
            BtnSourceClear.IsEnabled = true;
            BtnDestination.IsEnabled = true;
            BtnConvert.IsEnabled = true;
            ComboDdsFormat.IsEnabled = true;
            ComboMipmaps.IsEnabled = true;
            CheckPow2.IsEnabled = true;
        }

        #endregion

        #region Bindings

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            InputDestination.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var dxgi_values = Enum.GetNames(typeof(DxgiFormat));
            foreach (var value in dxgi_values)
                ComboDdsFormat.Items.Add(value);

            ComboDdsFormat.SelectedIndex = ComboDdsFormat.Items.Count - 1;

            for (int i = 1; i <= 6; i++)
                ComboMipmaps.Items.Add(i);

            ComboMipmaps.SelectedIndex = 0;
        }

        private void BtnSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Files",
                Filter = "Image (*.png)|*.png|Texture (*.dds)|*.dds|All files (*.*)|*.*",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
                AddFiles(dialog.FileNames);
        }

        private void BtnSourceClear_Click(object sender, RoutedEventArgs e)
        {
            ListSelection.Items.Clear();
        }

        private void BtnDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                Description = "Select Destination Folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!Directory.Exists(dialog.SelectedPath))
                {
                    MessageBox.Show("Selected directory does not exist!");
                    return;
                }

                if (!HasWriteAccess(dialog.SelectedPath))
                {
                    MessageBox.Show(
                        "You don't have permission to write to this folder. Either start this application as an administrator, or select a different folder.");
                    return;
                }

                InputDestination.Text = dialog.SelectedPath;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            InputDestination.Text = string.Empty;
        }

        private void ListSelection_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete || ListSelection.SelectedIndex == -1)
                return;

            for (var i = ListSelection.SelectedItems.Count - 1; i >= 0; i--)
                ListSelection.Items.Remove(ListSelection.SelectedItems[i]);
        }

        private void ListSelection_OnDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            AddFiles(files);
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {

            if (_processing || ListSelection.Items.Count == 0)
                return;

            if (!File.Exists(TEXCONV_PATH))
            {
                MessageBox.Show("texconv.exe not found! Place texconv.exe into the same directory as this program.", "Error");
                return;
            }
            
            StartProcessing();
            var totalFiles = ListSelection.Items.Count;
            var convertedCount = 0;

            for (var i = 0; i < ListSelection.Items.Count; i++)
            {

                var item = ListSelection.Items[i] as string;
                if (item == null)
                    continue;

                var proc = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = TEXCONV_PATH
                    }
                };
                
                var ext = Path.GetExtension(item).ToLowerInvariant();
                proc.StartInfo.Arguments = ext == ".dds" ? GetConvertToPngArgs(item) : GetConvertToDdsArgs(item);

                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode == 0)
                {
                    convertedCount++;
                    continue;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Failed to convert {item}!");
                sb.AppendLine();
                sb.AppendLine(output);

                MessageBox.Show(sb.ToString(), "Error");
            }

            FinishProcessing();
            MessageBox.Show($"{convertedCount} of {totalFiles} files were converted successfully.", "Finished");
        }

        #endregion
    }
}
