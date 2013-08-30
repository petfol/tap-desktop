﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirportPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowAirports.xaml
    /// </summary>
    public partial class PageShowAirports : Page
    {
        public List<AirportMVVM> AllAirports { get; set; }
        public List<Airline> AllAirlines { get; set; }

        public PageShowAirports(List<Airport> airports)
        {
            object o = this.Tag;
            createPage(airports);
        }
        public PageShowAirports()
        {
            createPage(Airports.GetAllActiveAirports());
        }
        //creates the page
        private void createPage(List<Airport> airports)
        {
            this.AllAirlines = new List<Airline>();

            Airline dummyAirline = new Airline(new AirlineProfile("All Airlines", "99", "Blue", "", false, 1900, 1900), Airline.AirlineMentality.Safe, Airline.AirlineFocus.Domestic, Airline.AirlineLicense.Domestic, Route.RouteType.Passenger);
            dummyAirline.Profile.addLogo(new AirlineLogo(AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png"));

            this.AllAirlines.Add(dummyAirline);

            foreach (Airline airline in Airlines.GetAllAirlines().Where(a => a != GameObject.GetInstance().HumanAirline).OrderBy(a=>a.Profile.Name))
                this.AllAirlines.Add(airline);

            this.AllAirports = new List<AirportMVVM>();

            foreach (Airport airport in airports.OrderBy(a=>a.Profile.Name))
                this.AllAirports.Add(new AirportMVVM(airport));

            InitializeComponent();

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airports")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                tab_main.SelectedItem = matchingItem;
            }

           
        }

       
        private void clName_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }

        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            AirportMVVM airport = (AirportMVVM)((Button)sender).Tag;
            
            Boolean hasCheckin = airport.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            object o = PopUpAirportContract.ShowPopUp(airport.Airport);

           //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), airport.Profile.Name), WPFMessageBoxButtons.YesNo);
            
           if (o!=null)
           {
               if (!hasCheckin)
               {
                   AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                   airport.Airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                   AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

               }

               airport.addAirlineContract((AirportContract)o);
          
            }

        }

        private void cbHuman_Checked(object sender, RoutedEventArgs e)
        {
            var source = this.AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                AirportMVVM a = o as AirportMVVM;
                return  a!=null && a.IsHuman;       
            };

        }

        private void cbHuman_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = this.AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                AirportMVVM a = o as AirportMVVM;
                return !a.IsHuman || a.IsHuman;
            };

            cbAirlines.SelectedIndex = 0;
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airline airline = (Airline)((ComboBox)sender).SelectedItem;

            if (this.AirportsList != null)
            {
                var source = this.AirportsList.Items as ICollectionView;
                source.Filter = o =>
                {
                    AirportMVVM a = o as AirportMVVM;
                    return a != null && a.Airport.AirlineContracts.Exists(c => c.Airline == airline) || airline.Profile.IATACode == "99";
                };
            }
        }
        
    }
}
