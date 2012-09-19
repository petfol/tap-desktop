﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{ 
    //the class for the configuration for a route classes
    public class RouteClassesConfiguration : Configuration
    {
        private List<RouteClassConfiguration> Classes;
        public RouteClassesConfiguration(string name)
            : base(ConfigurationType.Routeclasses, name)
        {
            this.Classes = new List<RouteClassConfiguration>();
        }
        //adds a class to the configuration
        public void addClass(RouteClassConfiguration routeclass)
        {
            if (this.Classes.Exists(c => c.Type == routeclass.Type))
                this.Classes.RemoveAll(c => c.Type == routeclass.Type);
            this.Classes.Add(routeclass);
        }
        //returns all route classes
        public List<RouteClassConfiguration> getClasses()
        {
            return this.Classes;
        }
       
        
    }
    //the class for the configuration for a route class
    public class RouteClassConfiguration
    {
        public AirlinerClass.ClassType Type { get; set; }
        private List<RouteFacility> Facilities;
        public RouteClassConfiguration(AirlinerClass.ClassType type)
        {
            this.Type = type;
            this.Facilities = new List<RouteFacility>();
        }
        //adds a facility to the configuration
        public void addFacility(RouteFacility facility)
        {
            if (this.Facilities.Exists(f => f.Type == facility.Type))
                this.Facilities.RemoveAll(f => f.Type == facility.Type);

            this.Facilities.Add(facility);
        }
        //returns the facility for a specific type
        public RouteFacility getFacility(RouteFacility.FacilityType type)
        {
            return this.Facilities.Find(f => f.Type == type);
        }
        //returns all facilities
        public List<RouteFacility> getFacilities()
        {
            return this.Facilities;
        }
    }

}