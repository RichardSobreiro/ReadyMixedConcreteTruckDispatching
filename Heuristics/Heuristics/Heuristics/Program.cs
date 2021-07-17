using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Heuristics
{
    class Program
    {
        static void Main(string[] args)
        {
            /*List<string> instancesAll = new List<string>()
            {
                "BH-02-03-2020",
                "BH-03-03-2020",
                "BH-04-03-2020",
                "BH-05-03-2020",
                "BH-06-03-2020",
                "BH-07-03-2020",
                "BH-07-08-2020",
                "BH-14-03-2020",
                "BH-14-06-2019",
                "BH-21-02-2020",
                "BH-22-02-2020",
                "BH-24-02-2020",
                "BH-25-02-2020",
                "SP-09-03-2019",
                "SP-11-03-2019",
                "SP-12-03-2019",
                "SP-13-01-2020",
                "SP-13-03-2019",
                "SP-14-01-2020",
                "SP-15-01-2020",
                "SP-16-01-2020",
                "SP-17-01-2020",
                "SP-18-01-2020",
                "SP-21-01-2020",
                "TJ-04-03-2019",
                "TJ-06-03-2019",
                "TJ-07-03-2019",
                "TJ-07-08-2020",
                "TJ-07-10-2020",
                "TJ-08-03-2019",
                "TJ-08-10-2020",
                "TJ-09-03-2019"
            };
            List<string> instances = new List<string>()
            {
                "SP-11-03-2019",
                "SP-12-03-2019",
                "SP-13-01-2020",
                "SP-13-03-2019",
                "SP-14-01-2020",
                "SP-15-01-2020",
                "SP-16-01-2020",
                "SP-17-01-2020",
                "SP-18-01-2020",
                "SP-21-01-2020",
                "TJ-04-03-2019",
                "TJ-06-03-2019",
                "TJ-07-03-2019",
                "TJ-07-08-2020",
                "TJ-07-10-2020",
                "TJ-08-03-2019",
                "TJ-08-10-2020",
                "TJ-09-03-2019"
            };*/
            List<string> instances = new List<string>()
            {
                "TJ-07-10-2020"
            };
            Program.Perform40TestsUndoRoutesAndDepotStart(args, instances);
            //Program.Perform40TestsUndoRoutes(args, instances);
            //string folderPath = args[0];
            //double PROBABILITY = double.Parse(args[1]);
            //int MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE = int.Parse(args[2]);
            //double PERCENTAGE_OF_ROUTES_TO_UNDO = double.Parse(args[3]);
            //int MAX_K_UNDO_ROUTES = int.Parse(args[4]);
            //// => StochasticRouteAcceptance
            //var result = "0 0";
            //try
            //{
            //    var stochasticDeliveryAcceptance =
            //        new Heuristics.ConstructiveHeuristics.StochasticAlgorithms.StochasticRouteAcceptance(PROBABILITY, 
            //            MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE, PERCENTAGE_OF_ROUTES_TO_UNDO, MAX_K_UNDO_ROUTES);
            //    result = stochasticDeliveryAcceptance.Execute(folderPath);
            //}
            //catch(Exception e)
            //{
            //    result = "0 0";
            //}

            //string folderPath = args[0];
            //double PROBABILITY = double.Parse(args[1]);
            //int MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE = int.Parse(args[2]);
            //double PROBABILITY_CHANGE_LP = double.Parse(args[3]);
            //int MAX_ILS_ITERATIONS = int.Parse(args[4]);
            //double CHANGE_ORDER_LP_PERCENTANGE = double.Parse(args[5]);
            //var result = "0 0";
            //try
            //{
            //    var ils = new Heuristics.ConstructiveHeuristics.IteratedLocalSearch.DepotStart.UndoRoutesAndDepotStart(PROBABILITY,
            //        MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE, PROBABILITY_CHANGE_LP, MAX_ILS_ITERATIONS, CHANGE_ORDER_LP_PERCENTANGE);
            //    result = ils.Execute(folderPath);
            //}
            //catch (Exception e)
            //{
            //    result = "0 0";
            //}
            //Console.Out.Write(result);
        }

        static void Perform40TestsUndoRoutes(string[] args, List<string> instances)
        {
            string folderPath = args[0];
            double PROBABILITY = 0.2108;
            int MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE = 535;
            double PERCENTAGE_OF_ROUTES_TO_UNDO = 0.4867;
            int MAX_K_UNDO_ROUTES = 789;
            List<string> results = new List<string>();

            foreach(string instance in instances)
            {
                results.Clear();
                string basePath = "C:\\Heuristics\\" + instance + "\\";
                folderPath = basePath + "BianchessiReal.dat";
                try
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var stochasticDeliveryAcceptance =
                            new Heuristics.ConstructiveHeuristics.StochasticAlgorithms.UndoRoutes(PROBABILITY,
                                MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE, PERCENTAGE_OF_ROUTES_TO_UNDO, MAX_K_UNDO_ROUTES);
                        string result = stochasticDeliveryAcceptance.Execute(folderPath);
                        results.Add(result);
                    }
                    using (TextWriter tw = new StreamWriter(basePath + "UndoRoutesResult.txt"))
                    {
                        foreach (string s in results)
                            tw.WriteLine(s);
                    }
                }
                catch (Exception e) 
                {
                    using (TextWriter tw = new StreamWriter(basePath + "Error.txt"))
                    {
                        tw.WriteLine("Exception Message: " + e.Message);
                        tw.WriteLine("Exception Stack Trace: " + e.StackTrace);
                    }
                }
            }
        }

        static void Perform40TestsUndoRoutesAndDepotStart(string[] args, List<string> instances)
        {
            string folderPath = args[0];
            double PROBABILITY = 0.4054;
            int MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE = 470;
            double PROBABILITY_CHANGE_LP = 0.5375;
            int MAX_ILS_ITERATIONS = 77;
            double CHANGE_ORDER_LP_PERCENTANGE = 0.2840;
            List<string> results = new List<string>();

            foreach(string instance in instances)
            {
                results.Clear();
                //string basePath = "C:\\Heuristics\\" + instance + "\\";
                string basePath = "C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\" + instance + "\\";
                folderPath = basePath + "BianchessiReal.dat";
                try
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var ils = new Heuristics.ConstructiveHeuristics.IteratedLocalSearch.DepotStart.UndoRoutesAndDepotStart(PROBABILITY,
                            MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE, PROBABILITY_CHANGE_LP, MAX_ILS_ITERATIONS, CHANGE_ORDER_LP_PERCENTANGE);
                        string result = ils.Execute(folderPath);
                        results.Add(result);
                    }
                    using (TextWriter tw = new StreamWriter(basePath + "UndoRoutesAndDepotStartResult.txt"))
                    {
                        foreach (string s in results)
                            tw.WriteLine(s);
                    }
                }
                catch (Exception e) 
                {
                    using (TextWriter tw = new StreamWriter(basePath + "Error.txt"))
                    {
                        tw.WriteLine("Exception Message: " + e.Message);
                        tw.WriteLine("Exception Stack Trace: " + e.StackTrace);
                    }
                }
            }
        }
    }
}
