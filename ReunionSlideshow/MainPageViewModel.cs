using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
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
        private int _imageCount;
        private int _currentImage;

        public MainPageViewModel()
        {
            InitializeCommands();
            ControlPanelVisibility = Visibility.Visible;
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
                _imageFolder = await fp.PickSingleFolderAsync();
                _files = await _imageFolder.CreateFileQuery().GetFilesAsync();
                _imageCount = _files.Count();
            });
        }

        private DelegateCommand InititializeStartSlideshowCommand()
        {
            return new DelegateCommand(async () =>
            {
                ControlPanelVisibility = Visibility.Collapsed;
                ViewImage = await StorageFileToImage(_files.FirstOrDefault());
            });
        }

        private DelegateCommand InitializeNextImageCommand()
        {
            return new DelegateCommand(async() =>
            {
                if (_currentImage < _imageCount)
                    _currentImage++;
                ViewImage = await StorageFileToImage(_files[_currentImage]);
            });
        }

        #region Bindable 

        private Visibility _controlPanelVisibility;
        public Visibility ControlPanelVisibility
        {
            get { return _controlPanelVisibility;}
            set { SetProperty(ref _controlPanelVisibility, value);
        } }
        private Image _viewImage;
        public Image ViewImage { get {return _viewImage;} set { SetProperty(ref _viewImage, value); } }
        public DelegateCommand SetImageFolderCommand { get; set; }
        public DelegateCommand StartSlideShowCommand { get; set; }
        public DelegateCommand NextImageCommand { get; set; }
        #endregion

        public static async Task<Image> StorageFileToImage(StorageFile savedStorageFile)
        {
            using (var fileStream = await savedStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                return new Image {Source = bitmapImage};
            }
        }
    }
}