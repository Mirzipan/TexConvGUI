using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

namespace TexConvGUI
{
    public partial class MainWindow
    {
        private static bool HasWriteAccess(string directory)
        {
            var permission = new FileIOPermission(FileIOPermissionAccess.Write, directory);
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(permission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        #region GUI

        private void SetInitialValues()
        {
            InputDestination.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var imageValues = Enum.GetValues(typeof(ImageFormat));
            foreach (var value in imageValues)
                ComboImageFormat.Items.Add(value);

            ComboImageFormat.SelectedIndex = 0;

            var dxgiValues = Enum.GetNames(typeof(DxgiFormat));
            foreach (var value in dxgiValues)
                ComboDdsFormat.Items.Add(value);

            ComboDdsFormat.SelectedIndex = ComboDdsFormat.Items.Count - 1;

            var alphaValues = Enum.GetNames(typeof(AlphaOptions));
            for (var i = 0; i < alphaValues.Length; i++)
                ComboAlpha.Items.Add(GetText((AlphaOptions)i));

            ComboAlpha.SelectedIndex = 0;

            for (var i = 1; i <= 6; i++)
                ComboMipmaps.Items.Add(i);

            ComboMipmaps.SelectedIndex = 0;
        }

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

        private string GetText(AlphaOptions alpha)
        {
            switch (alpha)
            {
                case AlphaOptions.None:
                    return "Don't modify";
                case AlphaOptions.Straight:
                    return "To non-premultiplied";
                case AlphaOptions.Premultiplied:
                    return "To premultiplied";
                default:
                    return string.Empty;
            }
        }

        private string GetAlphaArgs(AlphaOptions alpha)
        {
            switch (alpha)
            {
                case AlphaOptions.None:
                    return string.Empty;
                case AlphaOptions.Straight:
                    return " -alpha";
                case AlphaOptions.Premultiplied:
                    return " -pmalpha";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Processing

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

        private string GetConvertToDdsArgs(string path)
        {
            _tmpBuilder.Clear();

            _tmpBuilder.Append(" -nologo");

            if (CheckPow2.IsChecked.HasValue && CheckPow2.IsChecked.Value)
                _tmpBuilder.Append(" -pow2");

            _tmpBuilder.Append(GetAlphaArgs((AlphaOptions) ComboAlpha.SelectedIndex));
            _tmpBuilder.Append($" -ft dds -f {ComboDdsFormat.SelectedItem} -m {ComboMipmaps.SelectedItem}");

            if (!string.IsNullOrEmpty(InputDestination.Text))
                _tmpBuilder.Append($" -o {InputDestination.Text}");

            if (CheckOverwrite.IsChecked.HasValue && CheckOverwrite.IsChecked.Value)
                _tmpBuilder.Append(" -y");

            _tmpBuilder.Append(" ");
            _tmpBuilder.Append(path);

            return _tmpBuilder.ToString();
        }

        private string GetConvertToPngArgs(string path)
        {
            _tmpBuilder.Clear();

            _tmpBuilder.Append(" -nologo");

            _tmpBuilder.Append($" -ft {ComboImageFormat.SelectedItem}");

            if (!string.IsNullOrEmpty(InputDestination.Text))
                _tmpBuilder.Append($" -o {InputDestination.Text}");

            if (CheckOverwrite.IsChecked.HasValue && CheckOverwrite.IsChecked.Value)
                _tmpBuilder.Append(" -y");


            _tmpBuilder.Append(" ");
            _tmpBuilder.Append(path);

            return _tmpBuilder.ToString();
        }

        #endregion
    }
}