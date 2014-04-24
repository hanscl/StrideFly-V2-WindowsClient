using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Telerik.Windows.Controls.Map;

namespace StrideFlyWPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XBee xbeeClient;

        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += OnWindowSizeChanged;

            // Instantiate Xbee class
            xbeeClient = new XBee(textBoxLog);

            // initialize com ports
            comboPortName.ItemsSource = xbeeClient.initCOMPorts();

           

            LoadDefaultTrails();
            
        }

        private void OpenCloseComPort_Click(object sender, RoutedEventArgs e)
        {
          

            // Initialize the port; use default parameters for now
            if (comboPortName.SelectedItem == null)
            {
                MessageBox.Show("Please select a serial port!", "Serial Port Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            int comPortStatus = xbeeClient.OpenCloseComPort(comboPortName.SelectedItem.ToString());

            if (comPortStatus == -1) {
                MessageBox.Show("Error accessing Com Port. Please try a different port!", "Serial Port Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else if (comPortStatus == 0) {
                MessageBox.Show("Com Port has been closed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (comPortStatus == 1)
            {
                MessageBox.Show("Com Port has been opened.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        private void LoadDefaultTrails()
        {
            string path = @"C:\StrideFly\{0}.gpx";

            LoadGpxPointsFromFile(TrailRed, String.Format(path, "red"));
            LoadGpxPointsFromFile(TrailYellow, String.Format(path, "yellow"));
            LoadGpxPointsFromFile(TrailGreen, String.Format(path, "green"));
        }

        private void LoadCourse_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog ofd = new OpenFileDialog();
            MapPolyline selTrail = null;

            ofd.Filter = "GPX Files (*.gpx)|*.gpx";

            bool? ret = ofd.ShowDialog();



            if(ret == true) {


                string item = TrailColorSelect.SelectionBoxItem.ToString();

                if (item.Equals("Red"))
                    selTrail = TrailRed;
                else if (item.Equals("Yellow"))
                    selTrail = TrailYellow;
                else if (item.Equals("Green"))
                    selTrail = TrailGreen;
                else
                    return;

                LoadGpxPointsFromFile(selTrail, ofd.FileName);
            }


        }

        private void LoadGpxPointsFromFile(MapPolyline selTrail, string gpxFile) {
            XDocument myGPX = XDocument.Load(gpxFile);
            

            List<Location> trkptList =
                (from trkpt in myGPX.Descendants("{http://www.topografix.com/GPX/1/1}trkpt")
                 select new Location()
                {
                   Latitude = Convert.ToDouble(trkpt.Attribute("lat").Value),
                   Longitude = Convert.ToDouble(trkpt.Attribute("lon").Value)
                }).ToList();

            selTrail.Points.Clear();
            foreach (Location loc in trkptList)
            {
                selTrail.Points.Add(loc);
            }           
        }

        private void StrideFly_GoodBye(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth > 1000)
                RadMap1.ZoomLevel = 16;
            else
                RadMap1.ZoomLevel = 15;
        }
    }
}
