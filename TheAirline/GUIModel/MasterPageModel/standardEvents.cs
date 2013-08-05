﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GUIModel.PagesModel.AirportsPageModel;
using TheAirline.GUIModel.PagesModel.AlliancesPageModel;

namespace TheAirline.GUIModel.MasterPageModel
{
    public partial class standardEvents
    {
        private void RoutesManager_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Airports_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirports());
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
   
        }
        private void Alliances_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAlliances());
        }
    }
}
