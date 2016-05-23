using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ReunionSlideshow
{
    public class LocalStorage
    {
        private readonly Windows.Storage.ApplicationDataContainer _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        

        public void WriteLastImageNumber(int lastImageNumber)
        {
            _localSettings.Values["LastImage"] = lastImageNumber.ToString();
        }

        public int ReadLastImageNumber()
        {
            if (_localSettings.Values.ContainsKey("LastImage")) 
                return int.Parse(_localSettings.Values["LastImage"].ToString());
            return 0;
        }

        public int ReadImageCount()
        {
            return int.Parse(_localSettings.Values["ImageCount"].ToString());
        }

        public void WriteStorageFolderPath(string folderPath)
        {
            _localSettings.Values["FolderPath"] = folderPath;
        }

        public string ReadStorageFolderPath()
        {
            return _localSettings.Values["FolderPath"].ToString();
        }

        public void WriteStorageFiles(IReadOnlyList<StorageFile> storageFiles)
        {
            _localSettings.Values["ImageCount"] = storageFiles.Count().ToString();
            for (int i = 0; i < storageFiles.Count; i++)
            {
                _localSettings.Values[i.ToString()] = storageFiles[i].FolderRelativeId; 
            }
        }

        public string ReadFileId(int imageIndex)
        {
            return _localSettings.Values[imageIndex.ToString()].ToString();
        }
    }
}
