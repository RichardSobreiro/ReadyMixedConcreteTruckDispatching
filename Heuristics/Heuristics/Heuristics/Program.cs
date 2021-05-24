using System;
using System.Diagnostics;

namespace Heuristics
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = args[0];
            double PROBABILITY = double.Parse(args[1]);
            int MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE = int.Parse(args[2]);
            double PERCENTAGE_OF_ROUTES_TO_UNDO = double.Parse(args[3]);
            int MAX_K_UNDO_ROUTES = int.Parse(args[4]);
            // => StochasticRouteAcceptance
            var stochasticDeliveryAcceptance =
                new Heuristics.ConstructiveHeuristics.StochasticAlgorithms.StochasticRouteAcceptance(PROBABILITY, 
                    MAX_K_STOCHASTIC_ROUTE_ACCEPTANCE, PERCENTAGE_OF_ROUTES_TO_UNDO, MAX_K_UNDO_ROUTES);
            var result = stochasticDeliveryAcceptance.Execute(folderPath);

            //int MAX_ILS_ITERATIONS = int.Parse(args[3]);
            //double PROBABILITY_CHANGE_LP = double.Parse(args[4]);
            //double CHANGE_ORDER_LP_PERCENTANGE = double.Parse(args[5]);
            // => UndoRoutes
            //var stochasticDeliveryAcceptance = 
            //    new ConstructiveHeuristics.StochasticAlgorithms.UndoRoutes(probability, maxK);
            //var result = stochasticDeliveryAcceptance.Execute(folderPath);
            // => UndoRoutesAndDepotStart
            //var ils = new Heuristics.ConstructiveHeuristics.IteratedLocalSearch.DepotStart.UndoRoutesAndDepotStart(probability, 
            //    MAX_K, MAX_ILS_ITERATIONS, PROBABILITY_CHANGE_LP, CHANGE_ORDER_LP_PERCENTANGE);
            //var result = ils.Execute(folderPath);
            Console.Out.Write(result);
        }
    }
}
