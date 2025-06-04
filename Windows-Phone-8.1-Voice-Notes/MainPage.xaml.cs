using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Windows_Phone_8._1_Voice_Notes
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;
        private StorageFile _currentFile;
        private readonly List<string> _recordings = new List<string>();

        public MainPage()
        {
            this.InitializeComponent();
            LoadRecordingsAsync();
        }

        private async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };
                await _mediaCapture.InitializeAsync(settings);
                _currentFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav", CreationCollisionOption.GenerateUniqueName);

                await _mediaCapture.StartRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium), _currentFile);

                RecordButton.IsEnabled = false;
                StopButton.IsEnabled = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Microphone access denied: {ex.Message}").ShowAsync();
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error: {ex.Message}").ShowAsync();
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _mediaCapture.StopRecordAsync();
                _mediaCapture.Dispose();
                _recordings.Add(_currentFile.Name);
                RecordingsList.ItemsSource = null;
                RecordingsList.ItemsSource = _recordings;
                RecordButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = RecordingsList.SelectedItem as string;
                if (fileName != null)
                {
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    MediaElement mediaElement = new MediaElement();
                    mediaElement.SetSource(stream, file.ContentType);
                    mediaElement.Play();
                }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void RecordingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayButton.IsEnabled = RecordingsList.SelectedItem != null;
        }

        private async Task LoadRecordingsAsync()
        {
            try
            {
                var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
                foreach (var file in files)
                {
                    if (file.FileType == ".wav")
                        _recordings.Add(file.Name);
                }
                RecordingsList.ItemsSource = _recordings;
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog(ex.Message).ShowAsync();
            }
        }
    }
}