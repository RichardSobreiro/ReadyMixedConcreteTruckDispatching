import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import plotly_express as px
import plotly
from gmplot import *

from realResults import realResults
from cplexHaversineResults import cplexHaversineResults
from simpleHeuristicResults import simpleHeuristicResults

def main(argv):
    dataFolder = 'SP-20-01-2020'
    basePath = 'C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\' + dataFolder
    DEFAULT_DIESEL_COST = 3.5
    FIXED_L_PER_KM = 27.5/100
    FIXED_MIXED_TRUCK_COST = 50

    googleMapsApiKey, deliveries, loadingPlaces, mixerTrucks = realResults(dataFolder, basePath, DEFAULT_DIESEL_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_COST)

    #cplexHaversineResults(basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces)

    #simpleHeuristicResults('HaversineSimple', basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces)

    #simpleHeuristicResults('GoogleMapsSimple', basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces)

    #simpleHeuristicResults('NoTruckLimitation', basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces)

    simpleHeuristicResults('DeliveryByDeliveryAllocation', basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces)

if __name__ == '__main__':
    main(sys.argv[1:])

