using MapCreatorV2.Common;
using MapCreatorV2.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using System;
using System.Windows;

namespace MapCreatorV2.ViewModels
{
    public class ViewModelMainWindow : ViewModelBase
    {
        private const int MAXROWSANDCOLUMNS = 20;

        #region commands
        public RelayCommand RowDescreaseCommand { get; private set; }
        public RelayCommand ColumnDecreaseCommand { get; private set; }
        public RelayCommand RowIncreaseCommand { get; private set; }
        public RelayCommand ColumnIncreaseCommand { get; private set; }
        public RelayCommand SetColorCommand { get; private set; }
        public RelayCommand SaveFileCommand { get; private set; }
        public RelayCommand LoadFileCommand { get; private set; }
        public RelayCommand ResetMapCommand { get; set; }
        #endregion

        #region properties
        private string saveFileImageSource;
        private string loadFileImageSource;
        public string SaveFileImageSource
        {
            get { return saveFileImageSource;  }
            private set
            {
                saveFileImageSource = value;
                OnPropertyChanged();
            }
        }
        public string LoadFileImageSource 
        {
            get { return loadFileImageSource; }
            private set
            {
                loadFileImageSource = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MapObject> Titles { get; set; } = new ObservableCollection<MapObject>();
        public ObservableCollection<MapObject> ColorSources { get; set; } = new ObservableCollection<MapObject>();

        private int columnCount;
        public int ColumnCount
        {
            get { return columnCount; }
            set
            {
                columnCount = value;
                OnPropertyChanged();
                CalcTitleCount(rowCount, columnCount);
            }
        }

        private int rowCount;
        public int RowCount
        {
            get { return rowCount; }
            set
            {
                rowCount = value;
                OnPropertyChanged();
                CalcTitleCount(rowCount, columnCount);
            }
        }

        private string waitAnimatedSource;

        public string WaitAnimatedSorce
        {
            get { return waitAnimatedSource; }
            set 
            { 
                waitAnimatedSource = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ViewModelMainWindow() // Construktor
        {
            SaveFileImageSource = Path.GetFullPath(@"Assets/Buttons/saveImage.png");
            LoadFileImageSource = Path.GetFullPath(@"Assets/Buttons/loadImage.png");
            // bind Comands...
            RowDescreaseCommand = new RelayCommand(param => this.Decrease(true));
            ColumnDecreaseCommand = new RelayCommand(param => this.Decrease(false));
            RowIncreaseCommand = new RelayCommand(param => this.Increase(true));
            ColumnIncreaseCommand = new RelayCommand(param => this.Increase(false));
            SetColorCommand = new RelayCommand(param => this.SetColor(param as MapObject));
            SaveFileCommand = new RelayCommand(param => this.SaveFile());
            LoadFileCommand = new RelayCommand(param => this.LoadFile());
            ResetMapCommand = new RelayCommand(param => this.ResetMap());

            ColumnCount = 8;
            RowCount = 8;

            CalcTitleCount(RowCount, ColumnCount);
            LoadAllColorsFromFolder();
        }

        #region methods
        private void LoadAllColorsFromFolder() // Dynamic load files
        {
            string[] filePaths = Directory.GetFiles(@"Assets\", "*.png",SearchOption.TopDirectoryOnly);
            foreach (string ele in filePaths)
            {
                bool isBlocking = ele.Contains("_blocking");
                int GoTo = 0;
                if (ele.Contains("goto"))
                    GoTo = 99;
                ColorSources.Add(new MapObject(Path.GetFullPath(ele), isBlocking, GoTo));
            }
        }
        private void SetColor(MapObject mapObject)
        {
            foreach(var title in Titles.Where(x => x.IsSelected))
            {
                title.ImageSource = mapObject.ImageSource;
                title.IsSelected = false;
                title.IsPassable = mapObject.IsPassable;
                title.GoToMapID = mapObject.GoToMapID;
            }
        }

        private void ResetMap()
        {
            Titles.Clear();
            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    Titles.Add(new MapObject(Path.GetFullPath(@"white.png"), false, 0));
        }

        private void CalcTitleCount(int rows, int columns) // add new rows
        {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    Titles.Add(new MapObject(Path.GetFullPath(@"white.png"), false, 0));
        }

        private void Increase(bool rows)
        {
            if ((rows && rowCount < MAXROWSANDCOLUMNS) || (!rows && columnCount < MAXROWSANDCOLUMNS))
                if (rows)
                    RowCount = RowCount + 1;
                else
                    ColumnCount = ColumnCount + 1;
        }
        private void Decrease(bool rows)
        {
            if ((rows && rowCount > 1) || (!rows && columnCount > 1))
                if (rows)
                    RowCount = RowCount - 1;
                else
                    ColumnCount = ColumnCount - 1;
        }
        private void SaveFile()
        {
            SaveFileDialog savefileDia = new SaveFileDialog();
            savefileDia.FileName = "Mymapsave.json";
            savefileDia.Filter = "Json files(*.json)| *.json";

            var result = savefileDia.ShowDialog();
            if (result != null && result == true)
            {
                List<Export> export = new List<Export>();
                int ids = 1;
                foreach(var ele in Titles)
                {
                    export.Add(new Export(ids, ele.ImageSource, ele.IsPassable, ele.GoToMapID));
                    ids++;
                }
                JasonExportSet jasonExportSet = new JasonExportSet(RowCount, ColumnCount, export);
                var jason = System.Text.Json.JsonSerializer.Serialize(jasonExportSet).Replace(",", ",\n");
                File.WriteAllText(savefileDia.FileName, jason);
            }
        }
        private void LoadFile()
        {
            OpenFileDialog openFileDia = new OpenFileDialog();
            openFileDia.Filter = "Json files(*.json)| *.json";

            var result = openFileDia.ShowDialog();

            if (result != null && result == true)
            {
                string json = File.ReadAllText(openFileDia.FileName);
   
                StartLoadingMap(json);
            }
        }

        private void RebuildMap(JasonExportSet exportSet)
        {
            RowCount = exportSet.RowCount;
            ColumnCount = exportSet.ColumnCount;
            List<Export> import = exportSet.MetaData;
            Titles.Clear();
            foreach (var ele in import)
            {
                Titles.Add(new MapObject(ele.ImageSource, ele.IsPassable, ele.GoToMapID));
            }
            WaitAnimatedSorce = ""; // ende sprite animation              
        }

        // asyncs
        private async void StartLoadingMap(string jason)
        {     

           //await Task.Run(() => RebuildMapAsync(JsonConvert.DeserializeObject<JasonExportSet>(jason)));
           //await Task.Run(() => SpriteAnimatetion());          
           RebuildMap(JsonConvert.DeserializeObject<JasonExportSet>(jason));
        }

        private async Task RebuildMapAsync(JasonExportSet exportSet)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                RowCount = exportSet.RowCount;
                ColumnCount = exportSet.ColumnCount;
                List<Export> import = exportSet.MetaData;
                Titles.Clear();
                foreach (var ele in import)
                {
                    Titles.Add(new MapObject(ele.ImageSource, ele.IsPassable, ele.GoToMapID));
                }
                WaitAnimatedSorce = ""; // ende sprite animation              
            }));
        }

        private async Task SpriteAnimatetion()
        {
            WaitAnimatedSorce = Path.GetFullPath(@"Assets/RotateArrowSprites/RotateArrowSprite1.png");
            int counter = 1;
            while(WaitAnimatedSorce != "")
            {
                counter++;
                if (counter > 4)
                    counter = 1;

                WaitAnimatedSorce = Path.GetFullPath(@"Assets/RotateArrowSprites/RotateArrowSprite"+counter+".png");
                Thread.Sleep(1000);
            }
        }
        
        #endregion
    }
}
   
