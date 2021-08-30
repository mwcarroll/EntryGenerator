using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using EntryGenerator.Enums;

namespace EntryGenerator.ShellClasses
{
    public class FileSystemObjectInfo : BaseObject
    {
        public FileSystemObjectInfo(FileSystemInfo info)
        {
            if (this is DummyFileSystemObjectInfo)
            {
                return;
            }

            Children = new ObservableCollection<FileSystemObjectInfo>();
            FileSystemInfo = info;

            switch (info)
            {
                case DirectoryInfo _:
                    ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
                    AddDummy();
                    break;
                case FileInfo _:
                    ImageSource = FileManager.GetImageSource(info.FullName);
                    break;
            }

            PropertyChanged += FileSystemObjectInfo_PropertyChanged;
            CheckedChanged += FileSystemObjectInfo_CheckedChanged;
        }
        
        public FileSystemObjectInfo(DriveInfo drive)
            : this(drive.RootDirectory)
        {
        }

        #region Events

        public event EventHandler BeforeExpand;

        public event EventHandler AfterExpand;

        public event EventHandler BeforeExplore;

        public event EventHandler AfterExplore;

        public event EventHandler CheckedChanged;

        private void RaiseBeforeExpand()
        {
            BeforeExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExpand()
        {
            AfterExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseBeforeExplore()
        {
            BeforeExplore?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExplore()
        {
            AfterExplore?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseCheckedChanged()
        {
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region EventHandlers

        private void FileSystemObjectInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(FileSystemInfo is DirectoryInfo)) return;

            if (string.Equals(e.PropertyName, "IsExpanded", StringComparison.CurrentCultureIgnoreCase))
            {
                RaiseBeforeExpand();
                if (IsExpanded)
                {
                    ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Open);
                    if (HasDummy())
                    {
                        RaiseBeforeExplore();
                        RemoveDummy();
                        ExploreDirectories();
                        // ExploreFiles();
                        RaiseAfterExplore();
                    }
                }
                else
                {
                    ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Close);
                }
                RaiseAfterExpand();
            }
            else if (string.Equals(e.PropertyName, "IsChecked", StringComparison.CurrentCultureIgnoreCase))
            {
                RaiseCheckedChanged();
            }
        }

        private static void FileSystemObjectInfo_CheckedChanged(object sender, EventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.FileSystemObjectInfo_CheckedChanged(sender, e);
        }

        #endregion

        #region Properties

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get => GetValue<ObservableCollection<FileSystemObjectInfo>>("Children");
            private set => SetValue("Children", value);
        }

        public ImageSource ImageSource
        {
            get => GetValue<ImageSource>("ImageSource");
            private set => SetValue("ImageSource", value);
        }

        public bool IsChecked
        {
            get => GetValue<bool>("IsChecked");
            set => SetValue("IsChecked", value);
        }

        public bool IsExpanded
        {
            get => GetValue<bool>("IsExpanded");
            set => SetValue("IsExpanded", value);
        }

        public FileSystemInfo FileSystemInfo
        {
            get => GetValue<FileSystemInfo>("FileSystemInfo");
            private set => SetValue("FileSystemInfo", value);
        }

        private DriveInfo Drive
        {
            get => GetValue<DriveInfo>("Drive");
            set => SetValue("Drive", value);
        }

        #endregion

        #region Methods

        private void AddDummy()
        {
            Children.Add(new DummyFileSystemObjectInfo());
        }

        private bool HasDummy()
        {
            return GetDummy() != null;
        }

        private DummyFileSystemObjectInfo GetDummy()
        {
            return Children.OfType<DummyFileSystemObjectInfo>().FirstOrDefault();
        }

        private void RemoveDummy()
        {
            Children.Remove(GetDummy());
        }

        private void ExploreDirectories()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }

            if (!(FileSystemInfo is DirectoryInfo info)) return;

            DirectoryInfo[] directories = info.GetDirectories();

            foreach (DirectoryInfo directory in directories.OrderBy(d => d.Name))
            {
                if ((directory.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;

                FileSystemObjectInfo fileSystemObject = new FileSystemObjectInfo(directory);
                fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                Children.Add(fileSystemObject);
            }
        }

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            RaiseAfterExplore();
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            RaiseBeforeExplore();
        }

        private void ExploreFiles()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }

            if (!(FileSystemInfo is DirectoryInfo info)) return;

            FileInfo[] files = info.GetFiles();

            foreach (FileInfo file in files.OrderBy(d => d.Name))
            {
                if ((file.Attributes & FileAttributes.System) != FileAttributes.System &&
                    (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    Children.Add(new FileSystemObjectInfo(file));
                }
            }
        }

        #endregion
    }
}
