using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell;

namespace TeaTime
{
    public class TeaHouseOptionPage : DialogPage, INotifyPropertyChanged
    {
        public TeaHouseOptionPage()
        {
            this.timescale = Timescale.Java;
        }

        #region properties

        [Category("TeaHouse")]
        [DisplayName("TeaHouse Root Directory")]
        [Description("The root directory of the TeaHouse")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string TeaHouseRootDirectory
        {
            get { return this.teaHouseRootDirectory; }
            set
            {
                if (this.teaHouseRootDirectory != value)
                {
                    this.teaHouseRootDirectory = Path.GetFullPath(value);

                    if (this.OnTeaHouseRootChanged != null)
                    {
                        this.OnTeaHouseRootChanged();
                    }
                    this.Changed("TeaHouseRootDirectory");
                    this.Changed("IsConfigured");

                    Environment.SetEnvironmentVariable(TeaTimeConstants.WarehouseEnvironmentVariable, this.teaHouseRootDirectory, EnvironmentVariableTarget.User);
                }
            }
        }

        [Category("Time Representation")]        
        [Description("The number of days since 1.1.0000 at which time begins to count.")]
        public long Epoch
        {
            get { return this.timescale.Epoch; }
            set
            {
                if(this.timescale.Epoch != value)
                {
                    this.timescale = Timescale.FromEpoch(value, this.timescale.TicksPerDay);
                    this.Changed("WellKnownTimeScale");
                }
            }
        }

        [Category("Time Representation")]
        [DisplayName("Ticks per Day")]
        [Description("The ticks counted per day. For instance if seconds are counted, this would be 24 * 60 * 60")]
        public long TicksPerDay
        {
            get { return this.timescale.TicksPerDay; }
            set
            {
                if(value < 1)
                {
                    throw new Exception("Ticks per Day must be at least 1");
                }
                if (this.timescale.Epoch != value)
                {                    
                    this.timescale = Timescale.FromEpoch(this.timescale.Epoch, value);
                    this.Changed("WellKnownTimeScale");                     
                }
            }
        }

        enum WellknownTimeScale
        {
            Java,
            Net,
            Custom
        }

        [Category("Time Representation")]
        [DisplayName("Wellknown Timescale")]
        [Description("The name of a wellknown format (Java time format or .Net time format")]
        WellknownTimeScale WellKnownTimeScale
        {
            get
            {
                return (WellknownTimeScale)Enum.Parse(typeof (WellknownTimeScale), this.timescale.WellKnownName);
            }
            set
            {
                if (value == WellknownTimeScale.Net)
                {
                    this.timescale = Timescale.Net;
                    this.Changed("Epoch");
                    this.Changed("TicksPerDay");
                }
                else if (value == WellknownTimeScale.Java)
                {
                    this.timescale = Timescale.Java;
                    this.Changed("Epoch");
                    this.Changed("TicksPerDay");
                }
                // if set to custom, we do not change anything
            }
        }


        //[Category("TeaHouse")]
        //[DisplayName("TeaPlant User Name")]
        //[Description("The name of the user of TeaPlant")]        
        //public string TeaPlantUser
        //{
        //    get
        //    {
        //        return teaPlantUser;
        //    }
        //    set
        //    {
        //        if (teaPlantUser != value)
        //        {
        //            teaPlantUser = value;

        //            //if (OnTeaHouseRootChanged != null)
        //            //{
        //            //    OnTeaHouseRootChanged(this, new TeaHouseRootChangedEventArgs(teaHouseRootDirectory));
        //            //}
        //            Changed("TeaPlantUser");
        //            Changed("IsConfigured");
        //        }
        //    }
        //}

        //[Category("TeaHouse")]
        //[DisplayName("TeaPlant Server Name")]
        //[Description("The name of the user of TeaPlant")]
        //public string TeaPlantServer
        //{
        //    get
        //    {
        //        return teaPlantServer;
        //    }
        //    set
        //    {
        //        if (teaPlantServer != value)
        //        {
        //            teaPlantServer = value;

        //            //if (OnTeaHouseRootChanged != null)
        //            //{
        //            //    OnTeaHouseRootChanged(this, new TeaHouseRootChangedEventArgs(teaHouseRootDirectory));
        //            //}
        //            Changed("TeaPlantServer");
        //            Changed("IsConfigured");
        //        }
        //    }
        //}

        [Browsable(false)]
        public bool IsConfigured { get { return Directory.Exists(this.teaHouseRootDirectory); } }

        internal Timescale Timescale { get { return this.timescale; } }

        #endregion

        #region events

        public event Action OnTeaHouseRootChanged;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void Changed(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region state

        string teaHouseRootDirectory;        
        Timescale timescale;

        //string teaPlantUser;
        //string teaPlantServer;

        #endregion
    }
}
