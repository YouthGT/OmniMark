using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace OmniMark;

/// <summary>
/// Save Preset Dialog
/// </summary>
public partial class SavePresetDialog : Window
{
    public string PresetName => TxtPresetName.Text;

    public SavePresetDialog(string defaultName = "")
    {
        InitializeComponent();
        TxtPresetName.Text = defaultName;
        TxtPresetName.SelectAll();
        TxtPresetName.Focus();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtPresetName.Text))
        {
            MessageBox.Show("Please enter a preset name.", "OmniMark", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
