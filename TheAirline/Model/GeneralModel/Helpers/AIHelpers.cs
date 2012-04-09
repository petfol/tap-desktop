﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for the AI
    public class AIHelpers
    {
        private static Random rnd = new Random();
        //updates a cpu airline
        public static void UpdateCPUAirline(Airline airline)
        {
            CheckForNewHub(airline);
            CheckForNewRoute(airline);
            CheckForUpdateRoute(airline);


        }
        //checks for etablishing a new hub
        private static void CheckForNewHub(Airline airline)
        {

            int hubs = Airports.GetAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));

            int newHubInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newHubInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newHubInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newHubInterval = 10000000;
                    break;
            }

            Boolean newHub = rnd.Next(newHubInterval * hubs) == 0;// 100000 == 0;

            if (newHub)
            {
                //creates a new hub for the airline
                CreateNewHub(airline);

            }
        }
        //creates a new hub for an airline
        private static void CreateNewHub(Airline airline)
        {
            List<Airport> airports = airline.Airports.FindAll(a => CanCreateHub(airline,a));

            if (airports.Count > 0)
            {
                Airport airport = (from a in airports orderby a.Profile.Size descending select a).First();
                
                airport.Hubs.Add(new Hub(airline));

                airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airport.getHubPrice()));
          
            }

        }
        //checks if it is possible to create a hub at an airport
        private static Boolean CanCreateHub(Airline airline, Airport airport)
        {
            int airlineValue = (int)airline.getAirlineValue() + 1;

            int totalAirlineHubs = Airports.GetAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));
            double airlineGatesPercent = Convert.ToDouble(airport.Terminals.getNumberOfGates(airline)) / Convert.ToDouble(airport.Terminals.getNumberOfGates()) * 100;
            Boolean airlineHub = airport.Hubs.Count(h => h.Airline == airline) > 0;

            return (airline.Money > airport.getHubPrice()) && (!airlineHub) && (airlineGatesPercent > 20) && (totalAirlineHubs < airlineValue) && (airport.Hubs.Count < (int)airport.Profile.Size) && (airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service) == Hub.MinimumServiceFacilities);


        }
        //checks for updating of an existing route for an airline
        private static void CheckForUpdateRoute(Airline airline)
        {
            int totalHours = rnd.Next(24 * 7, 24 * 13);
            foreach (Route route in airline.Routes.FindAll(r => GameObject.GetInstance().GameTime.Subtract(r.LastUpdated).TotalHours > totalHours))
            {
                if (route.Airliner != null)
                {
                    double balance = route.getBalance(route.LastUpdated, GameObject.GetInstance().GameTime);
                    if (balance < -1000)
                    {
                        if (route.IncomePerPassenger < 0 && route.FillingDegree > 0.50)
                        {
                            foreach (RouteAirlinerClass rac in route.Classes)
                            {
                                rac.FarePrice += 10;
                            }
                            route.LastUpdated = GameObject.GetInstance().GameTime;
                        }
                        if (route.FillingDegree < 0.25)
                        {
                            airline.removeRoute(route);

                            if (route.Airliner != null)
                                route.Airliner.Route = null;

                            route.Destination1.Terminals.getUsedGate(airline).Route = null;
                            route.Destination2.Terminals.getUsedGate(airline).Route = null;

                            if (airline.Routes.Count == 0)
                                CreateNewRoute(airline);
                        }
                    }
                }


            }
        }
        //checks for a new route for an airline
        private static void CheckForNewRoute(Airline airline)
        {
            int newRouteInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newRouteInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newRouteInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newRouteInterval = 10000000;
                    break;
            }

            Boolean newRoute = rnd.Next(newRouteInterval) / 110 == 0;

            if (newRoute)
            {
                //creates a new route for the airline
                CreateNewRoute(airline);

            }
        }
        //creates a new route for an airline
        private static void CreateNewRoute(Airline airline)
        {
            List<Airport> homeAirports = airline.Airports.FindAll(a => a.getAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);
            homeAirports.AddRange(airline.Airports.FindAll(a => a.Hubs.Count(h => h.Airline == airline) > 0)); //hubs

            Airport airport = homeAirports.Find(a => a.Terminals.getFreeGates(airline) > 0);

            if (airport == null)
            {
                airport = homeAirports.Find(a => a.Terminals.getFreeGates() > 0);
                if (airport != null)
                    airport.Terminals.rentGate(airline);
                else
                {
                    airport = GetServiceAirport(airline);
                    if (airport != null)
                        airport.Terminals.rentGate(airline);
                }

            }

            if (airport != null)
            {
                Airport destination = GetDestinationAirport(airline, airport);


                if (destination != null)
                {
                    FleetAirliner fAirliner;

                    KeyValuePair<Airliner, Boolean>? airliner = GetAirlinerForRoute(airline, airport, destination);
                    fAirliner = GetFleetAirliner(airline, airport, destination);

                    if (airliner.HasValue || fAirliner != null)
                    {
                        if (destination.Terminals.getFreeGates(airline) == 0) destination.Terminals.rentGate(airline);

                        if (!airline.Airports.Contains(destination)) airline.addAirport(destination);

                        double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                        Guid id = Guid.NewGuid();

                        Route route = new Route(id.ToString(), airport, destination, price, airline.getNextFlightCode(), airline.getNextFlightCode());

                        foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                        {
                            route.getRouteAirlinerClass(type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(type);
                            route.getRouteAirlinerClass(type).CabinCrew = 2;
                            route.getRouteAirlinerClass(type).DrinksFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Count)];// RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
                            route.getRouteAirlinerClass(type).FoodFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Count)];//RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Food);
                        }

                        airline.addRoute(route);


                        airport.Terminals.getEmptyGate(airline).Route = route;
                        destination.Terminals.getEmptyGate(airline).Route = route;


                        if (fAirliner == null)
                        {

                            if (Countries.GetCountryFromTailNumber(airliner.Value.Key.TailNumber).Name != airline.Profile.Country.Name)
                                airliner.Value.Key.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();


                            if (airliner.Value.Value) //loan
                            {
                                double amount = airliner.Value.Key.getPrice() - airline.Money + 20000000;

                                Loan loan = new Loan(GameObject.GetInstance().GameTime, amount, 120, GeneralHelpers.GetAirlineLoanRate(airline));

                                double payment = loan.getMonthlyPayment();

                                airline.addLoan(loan);
                                airline.addInvoice(new Invoice(loan.Date, Invoice.InvoiceType.Loans, loan.Amount));
                            }
                            else
                                airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Value.Key.getPrice()));

                            fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, airline, airliner.Value.Key, airliner.Value.Key.TailNumber, airport);
                            airline.Fleet.Add(fAirliner);

                        }

                        fAirliner.Route = route;
                        route.Airliner = fAirliner;

                        fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                        route.LastUpdated = GameObject.GetInstance().GameTime;
                    }
                }
            }
        }
        //returns an airliner from the fleet which fits a route
        private static FleetAirliner GetFleetAirliner(Airline airline, Airport destination1, Airport destination2)
        {
            //Order new airliner
            var fleet = airline.Fleet.FindAll(f => f.Route == null && f.Airliner.Type.Range > MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates));

            if (fleet.Count > 0)
                return (from f in fleet orderby f.Airliner.Type.Range select f).First();
            else
                return null;
        }
        //returns the destination for an airline with a start airport
        public static Airport GetDestinationAirport(Airline airline, Airport airport)
        {
            double maxDistance = (from a in Airliners.GetAirlinersForSale()
                                  select a.Type.Range).Max();

            double minDistance = (from a in Airports.GetAirports().FindAll(a => a != airport) select MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates)).Min();


            List<Airport> airports = new List<Airport>();
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            switch (airline.MarketFocus)
            {
                case Airline.AirlineMarket.Global:
                    airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
                case Airline.AirlineMarket.Local:
                    airports = Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < Math.Max(minDistance, 1000) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
                    break;
                case Airline.AirlineMarket.Regional:
                    airports = Airports.GetAirports(airport.Profile.Country.Region).FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
            }

            Airport destination = null;
            int counter = 0;

            if (airports.Count == 0)
            {
                airports = (from a in Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 5000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50) orderby a.Profile.Size descending select a).ToList();

            }

            airports = (from a in airports orderby a.Profile.Size descending select a).ToList();

            while (destination == null && counter < airports.Count)
            {
                destination = airports[counter];

                if ((routes.Find(r => r.Destination1 == destination || r.Destination2 == destination) != null) || (destination.Terminals.getFreeGates() == 0 && destination.Terminals.getFreeGates(airline) == 0) || (destination == airport)) destination = null;
                counter++;

            }
            return destination;
        }
        //returns the best fit for an airliner for sale for a route true for loan
        public static KeyValuePair<Airliner, Boolean>? GetAirlinerForRoute(Airline airline, Airport destination1, Airport destination2)
        {

            double maxLoanTotal = 100000000;
            double distance = MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates);

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(distance);

            List<Airliner> airliners = Airliners.GetAirlinersForSale().FindAll(a => a.getPrice() < airline.Money - 1000000 && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

            if (airliners.Count > 0)
                return new KeyValuePair<Airliner, Boolean>((from a in airliners orderby a.Type.Range select a).First(), false);
            else
            {
                if (airline.Mentality == Airline.AirlineMentality.Aggressive)
                {
                    double airlineLoanTotal = airline.Loans.Sum(l => l.PaymentLeft);

                    if (airlineLoanTotal < maxLoanTotal)
                    {
                        List<Airliner> loanAirliners = Airliners.GetAirlinersForSale().FindAll(a => a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

                        if (loanAirliners.Count > 0)
                            return new KeyValuePair<Airliner, Boolean>((from a in loanAirliners orderby a.Price select a).First(), true);
                        else
                            return null;
                    }
                    else
                        return null;

                }
                else
                    return null;
            }


        }
        //finds an airport and creates a basic service facility for an airline
        private static Airport GetServiceAirport(Airline airline)
        {

            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);

            var airports = from a in airline.Airports.FindAll(aa => aa.Terminals.getFreeGates() > 0) orderby a.Profile.Size descending select a;

            if (airports.Count() > 0)
            {
                Airport airport = airports.First();

                airport.setAirportFacility(airline, facility);

                return airport;
            }

            return null;

        }
        //returns a random item based on a weighted value
        public static T GetRandomItem<T>(Dictionary<T, int> list)
        {

            List<T> tList = new List<T>();

            foreach (T item in list.Keys)
            {
                for (int i = 0; i < list[item]; i++)
                    tList.Add(item);
            }

            return tList[rnd.Next(tList.Count)];
        }
    }
}