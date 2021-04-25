import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime
import googlemaps
import json

from googleMapsFirstModelData import googleMapsFirstModelData
from googleMapsSecondModelData import googleMapsSecondModelData
from googleMapsCombinations import googleMapsCombinations
from haversineData import haversineData
from bianchessiData import bianchessiData
from bianchessiRandomData import bianchessiRandomData
from bianchessiRealData import bianchessiRealData
from classes import LoadingPlace, MixerTruck, Order, Delivery, DirectionResult

def main(argv):
    # basePathRandom = 'C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\'
    # deliveries = 70
    # loadingPlaces = 2 
    # numberOfTrucks = 20
    # bianchessiRandomData(basePathRandom, loadingPlaces, deliveries, numberOfTrucks)

    dataFolder = 'SP-20-01-2020'
    basePath = 'C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\' + dataFolder

    DEFAULT_DIESEL_COST = 3.5
    FIXED_L_PER_KM = 27.5/100
    FIXED_MIXED_TRUCK_COST = 50

    DEFAULT_RMC_COST = 150
    FIXED_MIXED_TRUCK_CAPACIT_M3 = 10
    FIXED_KM_PER_L = 2

    dfTrips = pd.read_csv(basePath + '\\Trips.csv', encoding = "ISO-8859-1")
    dfTrips = dfTrips.sort_values('CODPROGRAMACAO')

    NEW_ORDER_ID = dfTrips.tail(1).iloc[0]['CODPROGRAMACAO']

    loadingPlaces = []
    loadingPlacesIndex = 1
    mixerTrucks = []
    mixerTrucksIndex = 0
    orders = []
    deliveries = []
    for index, row in dfTrips.iterrows():
        loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == row['CODCENTCUSNOTAFISCAL']), None)
        if loadingPlace == None:
            loadingPlace = LoadingPlace(loadingPlacesIndex, row['CODCENTCUSNOTAFISCAL'], row['LATITUDE_FILIAL'], row['LONGITUDE_FILIAL'])
            if loadingPlace.CODCENTCUS == 17050 or loadingPlace.CODCENTCUS == 17250:
                loadingPlace.LATITUDE_FILIAL = -23.689984434115285
                loadingPlace.LONGITUDE_FILIAL = -46.60277603494763
            if loadingPlace.CODCENTCUS == 14050:
                loadingPlace.LATITUDE_FILIAL = -23.152246049659357
                loadingPlace.LONGITUDE_FILIAL = -45.80131066379455
            if loadingPlace.LATITUDE_FILIAL == 0 or loadingPlace.LONGITUDE_FILIAL == 0 or loadingPlace.LATITUDE_FILIAL == 1e-08 or loadingPlace.LONGITUDE_FILIAL == 1e-08:
                print('Loading Place ' + str(loadingPlace.CODCENTCUS) + ' withou coordinates')
            loadingPlaces.append(loadingPlace)
            loadingPlacesIndex += 1

        mixerTruck = next((mt for mt in mixerTrucks if mt.CODVEICULO == row['CODVEICULO']), None)
        if mixerTruck == None:
            mixerTruck = MixerTruck(mixerTrucksIndex, row['CODVEICULO'], row['CODCENTCUSNOTAFISCAL'], row['LATITUDE_FILIAL'], row['LONGITUDE_FILIAL'])
            mixerTrucks.append(mixerTruck)
            mixerTrucksIndex += 1
        
        order = next((o for o in orders if o.CODPROGRAMACAO == row['CODPROGRAMACAO']), None)
        if order == None:
            order = Order(row['CODPROGRAMACAO'], row['CODCENTCUSNOTAFISCAL'], row['MEDIA_M3_DESCARGA'], 
                row['VALTOTALPROGRAMACAO'], row['HORSAIDACENTRAL'], 
                row['LATITUDE_OBRA'], row['LONGITUDE_OBRA'], row['VLRVENDA'])
            orders.append(order)

        delivery = next((v for v in deliveries if v.CODPROGVIAGEM == row['CODPROGVIAGEM']), None)
        if delivery == None:
            constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%y %H:%M %p')
            #constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%Y-%m-%d %H:%M:%S')
            #constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%Y %H:%M')
            minutes = (constructionTime.hour * 60) + constructionTime.minute
            delivery = Delivery(HORCHEGADAOBRA = minutes, CODPROGRAMACAO=row['CODPROGRAMACAO'], CODPROGVIAGEM=row['CODPROGVIAGEM'], 
                CODCENTCUSVIAGEM=row['CODCENTCUSVIAGEM'], VLRTOTALNF=row['VLRTOTALNF'], VALVOLUMEPROG=row['VALVOLUMEPROG'], 
                CUSVAR=row['CUSVAR'], CODTRACO=row['CODTRACO'], LATITUDE_OBRA=row['LATITUDE_OBRA'], 
                LONGITUDE_OBRA=row['LONGITUDE_OBRA'], VLRVENDA=row['VLRVENDA'])
            order.TRIPS.append(delivery)
            deliveries.append(delivery)

    # bianchessiRealData(basePath, mixerTrucks, loadingPlaces, deliveries, orders, 
    #     NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    #     FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST)
    
    # bianchessiData(mixerTrucks, loadingPlaces, deliveries, orders, 
    #     NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    #     FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath)

    # googleMapsFirstModelData(mixerTrucks, loadingPlaces, deliveries, orders, 
    #     NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    #     FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath)
    
    # haversineData(mixerTrucks, loadingPlaces, deliveries, orders, 
    #     NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    #     FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath)

    googleMapsSecondModelData(mixerTrucks, loadingPlaces, deliveries, orders, 
        NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
        FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath)
    
    # googleMapsCombinations(mixerTrucks, loadingPlaces, deliveries, orders, 
    #     NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    #     FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath)

if __name__ == '__main__':
    main(sys.argv[1:])

