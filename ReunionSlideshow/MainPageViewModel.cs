using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Text.Core;
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
        private List<StorageFile> _files;
        private DispatcherTimer _timer;
        private LocalStorage _localStorage;

        public MainPageViewModel()
        {
            InitializeCommands();
            InitializeTimer();
            ControlPanelVisibility = Visibility.Visible;
            _localStorage = new LocalStorage();
            ResumeSessionVisibility = _localStorage.ReadImageCount() != 0 ? Visibility.Visible : Visibility.Collapsed;
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
            StartSlideshowCommand = InititializeStartSlideshowCommand();
            NextImageCommand = InitializeNextImageCommand();
            PauseSlideshowCommand = InitializePauseSlideshowCommand();
            PreviousImageCommand = InitializePreviousImageCommand();
            ResumeLastSessionCommand = InitializeResumeLastSessionCommand();
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
                if (_imageFolder != null)
                {
                    _localStorage.WriteStorageFolderPath(_imageFolder.Path);
                    var fileQuery = new QueryOptions() { FolderDepth = FolderDepth.Deep };
                    var result = await _imageFolder.CreateFileQueryWithOptions(fileQuery).GetFilesAsync();
                    _files = result.ToList();
                    _localStorage.WriteStorageFiles(_files);
                    ImageCount = _files.Count() - 1;
                    OnPropertyChanged(nameof(StorageFolderControlsVisibility));
                }
            });
        }

        private DelegateCommand InitializeResumeLastSessionCommand()
        {
            return new DelegateCommand(async() =>
            {
                FolderPicker fp = new FolderPicker();
                fp.FileTypeFilter.Add(".jpg");
                fp.FileTypeFilter.Add(".bmp");
                fp.FileTypeFilter.Add(".png");
                fp.FileTypeFilter.Add(".gif");
                _imageFolder = await fp.PickSingleFolderAsync();
                _files = new List<StorageFile>();
                if (_imageFolder != null && _imageFolder.Path == _localStorage.ReadStorageFolderPath())
                {
                    ImageCount = _localStorage.ReadImageCount();
                    CurrentImage = _localStorage.ReadLastImageNumber();

                    var result = await _imageFolder.CreateFileQueryWithOptions(new QueryOptions() { FolderDepth = FolderDepth.Deep }).GetFilesAsync();
                    var files = result.ToList();
                    for (int i = 0; i < ImageCount; i++)
                    {
                        var id = _localStorage.ReadFileId(i);
                        if (id != null)
                            _files.Add(files.FirstOrDefault(f => f.FolderRelativeId == id));
                    }
                    ViewImage = await StorageFileToImage(_files[_currentImage]);
                }
                ResumeSessionVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(StorageFolderControlsVisibility)); 
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

        private DelegateCommand InitializePauseSlideshowCommand()
        {
            return new DelegateCommand(() =>
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    ControlPanelVisibility = Visibility.Visible;
                }
                else
                {
                    _timer.Start();
                    ControlPanelVisibility = Visibility.Collapsed;
                }
            });
        }
        private DelegateCommand InitializeNextImageCommand()
        {
            return new DelegateCommand(async () =>
            {
                if (_currentImage < _imageCount - 1)
                    CurrentImage = CurrentImage + 1;
                else
                    CurrentImage = 0;
                ViewImage = await StorageFileToImage(_files[_currentImage]);
            });
        }

        private DelegateCommand InitializePreviousImageCommand()
        {
            return new DelegateCommand(async () =>
            {
                if (_currentImage > 0)
                    CurrentImage = CurrentImage - 1;
                else
                    CurrentImage = _imageCount;
                ViewImage = await StorageFileToImage(_files[_currentImage]);
                OnPropertyChanged(nameof(ViewImage));
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

        public Visibility StorageFolderControlsVisibility => _files != null && _files.Any() ? Visibility.Visible : Visibility.Collapsed;

        private Visibility _resumeSessionVisibility;
        public Visibility ResumeSessionVisibility { get { return _resumeSessionVisibility; } set { SetProperty(ref _resumeSessionVisibility, value); } }


        private BitmapImage _viewImage;
        public BitmapImage ViewImage { get { return _viewImage ?? new BitmapImage(); } set { SetProperty(ref _viewImage, value); } }

        private int _imageCount;
        public int ImageCount { get { return _imageCount; } set { SetProperty(ref _imageCount, value); } }

        private int _currentImage;

        public int CurrentImage
        {
            get
            {
                return _currentImage;
            }
            set
            {
                SetProperty(ref _currentImage, value);
                _startImage = value;
                _localStorage.WriteLastImageNumber(value);
                OnPropertyChanged(nameof(StartImage));
            }
        }

        private int _startImage;
        public int StartImage { get {return _startImage;} set { SetProperty(ref _startImage, value); } }
        public DelegateCommand PauseSlideshowCommand { get; set; }
        public DelegateCommand SetImageFolderCommand { get; set; }
        public DelegateCommand StartSlideshowCommand { get; set; }
        public DelegateCommand NextImageCommand { get; set; }
        public DelegateCommand PreviousImageCommand { get; set; }
        public DelegateCommand ResumeLastSessionCommand { get; set; }
        #endregion

        private async void TimedImage(object sender, object e)
        {
            if (CurrentImage < ImageCount - 1)
                CurrentImage = CurrentImage + 1;
            else
                CurrentImage = 0;
            ViewImage = await StorageFileToImage(_files[CurrentImage]);
        }

        private static async Task<BitmapImage> StorageFileToImage(StorageFile savedStorageFile)
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