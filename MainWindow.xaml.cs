using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LAB_5
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow() {
            InitializeComponent();
        }

        private void ProcessData() {
            // Measure execution time
            System.Diagnostics.Stopwatch watch = new();
            watch.Start();

            // Get individual words. First, remove the trailing dot. Then split the rest.
            List<string> parts = new(InputTextBox.Text[0..^1].Split(';'));

            List<string> results = new();
            List<string> word_set = new();
            for (int i = 0; i < parts.Count; i++) {
                string word = parts[i];
                if (!word_set.Contains(word)) {
                    word_set.Add(word);
                    List<string> substrings = new();
                    foreach (string checked_word in parts) {
                        if (checked_word.Contains(word) && checked_word != word) {
                            substrings.Add(checked_word);
                        }
                    }
                    switch (substrings.Count) {
                        case 0:
                            // results.Add($"Other words in the sentence do not contain word \"{word}\".");
                            break;
                        case 1:
                            results.Add($"Word \"{String.Join(", ", substrings.ToArray())}\" contains word \"{word}\".");
                            break;
                        default:
                            results.Add($"Words \"{String.Join(", ", substrings.ToArray())}\" contain word \"{word}\".");
                            break;
                    }
                }
            }

            string result = $"Input string: {InputTextBox.Text} Length: {InputTextBox.Text.Length}.\n";
            result += $"Unique words ({word_set.Count}): {String.Join(", ", word_set.ToArray())}.\n\nProcessing result:\n";
            result += String.Join("\n", results.ToArray());

            watch.Stop();

            result += $"\n\nExecution time: {watch.ElapsedMilliseconds} ms";

            OutputTextBox.Text = result;

            StringOutputGrid.Visibility = Visibility.Visible;
            SaveToFileButton.IsEnabled = true;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (StringOutputGrid.Visibility == Visibility.Visible) StringOutputGrid.Visibility = Visibility.Collapsed;

            if (InputTextBox.Text.Length == 0) {
                //InputTextBoxBorder.BorderBrush = App.Current.Resources["TextBoxDefaultBorder"] as Brush;
                InputTextBoxBorder.BorderBrush = null;
                ProcessDataButton.IsEnabled = false;
                EnterTipTextBlock.Opacity = 0.0;
                // Don't run code below, because regex will fail and display red border
                return;
            }

            if (Utils.CheckInputRegex(InputTextBox.Text)) {
                InputTextBoxBorder.BorderBrush = new SolidColorBrush(Colors.ForestGreen);
                ProcessDataButton.IsEnabled = true;
                EnterTipTextBlock.Opacity = 1.0;
            }
            else { 
                InputTextBoxBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                ProcessDataButton.IsEnabled = false;
                EnterTipTextBlock.Opacity = 0.0;
            }

            //if (regex.Match(InputTextBox.Text).Success) { InputTextBoxBorder.BorderBrush = App.Current.Resources["TextBoxCorrectDataBorder"] as Brush; }
            //else { InputTextBoxBorder.BorderBrush = App.Current.Resources["TextBoxIncorrectDataBorder"] as Brush; }
        }

        private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && Utils.CheckInputRegex(InputTextBox.Text)) {
                ProcessData();
            }
        }

        private void SwitchInputVisibility_Click(object sender, RoutedEventArgs e) => SwitchInputVisibility();

        private void SwitchInputVisibility() {
            if (StringInputPanel.Visibility == Visibility.Visible) {
                StringInputPanel.Visibility = Visibility.Collapsed;
                OutputTextBoxRow.Height = new GridLength(341);
            }
            else {
                StringInputPanel.Visibility = Visibility.Visible;
                OutputTextBoxRow.Height = new GridLength(235);
            }
        }

        private void ProcessData_Click(object sender, RoutedEventArgs e) => ProcessData();

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            // Create the file picker
            FileOpenPicker filePicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, ViewMode = PickerViewMode.List, FileTypeFilter = { ".txt" } };

            // Associate the HWND with the file picker
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file != null && file.IsAvailable) {
                try { 
                    InputTextBox.Text = File.ReadAllText(file.Path).Replace("\n", "").Replace("\r", "");
                    // Reveal input box if it's hidden
                    if (StringInputPanel.Visibility == Visibility.Collapsed) SwitchInputVisibility();
                }
                catch (Exception ex) {
                    ErrorTip.Subtitle = $"Unable to open/read file: {ex.Message}";
                    ErrorTip.IsOpen = true;
                    return;
                }
            }
        }

        private async void SaveToFileButton_Click(object sender, RoutedEventArgs e) {
            // Create the file picker
            FileSavePicker filePicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, SuggestedFileName = "Processing result" };
            // Add file type to save as
            filePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

            // Associate the HWND with the file picker
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSaveFileAsync();

            if (file != null && file.IsAvailable) {
                try {
                    StreamWriter sw = new(file.Path);
                    sw.WriteLine(OutputTextBox.Text);
                    sw.Close();
                }
                catch (Exception ex) {
                    ErrorTip.Subtitle = $"Unable to open/write file: {ex.Message}";
                    ErrorTip.IsOpen = true;
                    return;
                }
            }
        }

        private void ExitApplication(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        // Show/hide tip
        private void InputTextBox_FocusEngaged(object sender, RoutedEventArgs e) => EnterTipTextBlock.Opacity = Utils.CheckInputRegex(InputTextBox.Text) ? 1.0 : 0.0;
        private void InputTextBox_FocusDisengaged(object sender, RoutedEventArgs e) => EnterTipTextBlock.Opacity = 0.0;

        private bool five = true;
        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (five) {
                sender.Inlines.Clear();
                sender.Inlines.Add(new Run() { Text = "№6" });
            }
            else {
                sender.Inlines.Clear();
                sender.Inlines.Add(new Run() { Text = "№5" });
            }
            five = !five;
        }
    }
}
