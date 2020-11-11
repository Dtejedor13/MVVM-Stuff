using MapCreatorV2.Common;
using System;
using MapCreatorV2;

namespace MapCreatorV2.Models
{
    public class MapObject : ViewModelBase
    {
        private  string source = "";
        public string ImageSource
        {
            get { return source; }
            set 
            { 
                source = value;
                OnPropertyChanged();
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set 
            { 
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isPassable;

        public bool IsPassable
        {
            get { return isPassable; }
            set 
            { 
                isPassable = value;
                OnPropertyChanged();
            }
        }

        private int goToMapID;

        public int GoToMapID
        {
            get { return goToMapID; }
            set 
            { 
                goToMapID = value;
                OnPropertyChanged();
            }
        }

        public MapObject(string sorces, bool ispassableproperty, int gotoMapID)// Constructor
        {
            this.source = sorces;
            this.isPassable = ispassableproperty;
            this.goToMapID = gotoMapID;
        }
    }
}
