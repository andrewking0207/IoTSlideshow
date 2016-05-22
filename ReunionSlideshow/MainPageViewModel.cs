using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ReunionSlideshow;

namespace ReunionSlideshow
{
    public class MainPageViewModel : BindableBase
    {
        private StorageFolder _imageFolder;
        private IReadOnlyList<StorageFile> _files;
        private DispatcherTimer _timer;

        public MainPageViewModel()
        {
            InitializeCommands();
            InitializeTimer();
            ControlPanelVisibility = Visibility.Visible;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += TimedImage;
            _timer.Interval = TimeSpan.FromSeconds(15);
        }

        private void SetStartingImage()
        {
            if (StartImage >= 0 && StartImage <= _imageCount)
            {
                CurrentImage = StartImage;
            }
            else
            {
                CurrentImage = 0;
            }
        }

        private void InitializeCommands()
        {
            SetImageFolderCommand = InitializeSetImageFolderCommand();
            StartSlideShowCommand = InititializeStartSlideshowCommand();
            NextImageCommand = InitializeNextImageCommand();
        }

        private DelegateCommand InitializeSetImageFolderCommand()
        {
            return new DelegateCommand(async () =>
            {
                var fp = new FolderPicker();
                fp.FileTypeFilter.Add(".jpg");
                fp.FileTypeFilter.Add(".bmp");
                fp.FileTypeFilter.Add(".png");
                fp.FileTypeFilter.Add(".gif");
                _imageFolder = await fp.PickSingleFolderAsync();
                var fileQuery = new QueryOptions() {FolderDepth = FolderDepth.Deep};
                _files = await _imageFolder.CreateFileQueryWithOptions(fileQuery).GetFilesAsync();
                ImageCount = _files.Count();
            });
        }

        private DelegateCommand InititializeStartSlideshowCommand()
        {
            return new DelegateCommand(async () =>
            {
                _timer.Start();
                SetStartingImage();
                ControlPanelVisibility = Visibility.Collapsed;
                ViewImage = await StorageFileToImage(_files[_currentImage]);
            });
        }

        private DelegateCommand InitializeNextImageCommand()
        {
            return new DelegateCommand(async () =>
            {
                if (_currentImage < _imageCount)
                    CurrentImage = _currentImage + 1;
                ViewImage = await StorageFileToImage(_files[_currentImage]);
            });
        }

        #region Bindable 

        private Visibility _controlPanelVisibility;
        public Visibility ControlPanelVisibility
        {
            get { return _controlPanelVisibility; }
            set
            {
                SetProperty(ref _controlPanelVisibility, value);
            }
        }
        private BitmapImage _viewImage;
        public BitmapImage ViewImage { get { return _viewImage ?? new BitmapImage(); } set { SetProperty(ref _viewImage, value); } }

        private string _imagePath;
        public string ImagePath { get { return _imagePath; } set { SetProperty(ref _imagePath, value); } }

        private int _imageCount;
        public int ImageCount { get { return _imageCount; } set { SetProperty(ref _imageCount, value); } }

        private int _currentImage;
        public int CurrentImage { get { return _currentImage;} set { SetProperty(ref _currentImage, value); } }

        public int StartImage { get; set; }

        public DelegateCommand SetImageFolderCommand { get; set; }
        public DelegateCommand StartSlideShowCommand { get; set; }
        public DelegateCommand NextImageCommand { get; set; }
        #endregion

        private async void TimedImage(object sender, object e)
        {
            if (_currentImage < _imageCount)
                CurrentImage = _currentImage + 1;
            ViewImage = await StorageFileToImage(_files[_currentImage]);
        }

        public static async Task<BitmapImage> StorageFileToImage(StorageFile savedStorageFile)
        {
            using (var fileStream = await savedStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelHeight = bitmapImage.PixelHeight;
                bitmapImage.DecodePixelWidth = bitmapImage.PixelWidth;
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
            }
        }
    }
}