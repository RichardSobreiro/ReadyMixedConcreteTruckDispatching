import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import googlemaps
import json

from classes import LoadingPlace, MixerTruck, Order, Delivery, DirectionResult

def convert_dictionary_directions_results_to_array(directionsResultsStored):
    directionsResults = []
    for d in directionsResultsStored['DirectionsResults']:
        directionResult = DirectionResult()
        directionResult.Distance = d['Distance']
        directionResult.TravelTime = d['TravelTime']
        directionResult.OriginLatitude = d['OriginLatitude']
        directionResult.OriginLongitude = d['OriginLongitude']
        directionResult.DestinyLatitude = d['DestinyLatitude']
        directionResult.DestinyLongitude = d['DestinyLongitude']
        directionResult.Hour = d['Hour']
        directionResult.TimeString = d['TimeString']
        directionResult.Result = d['Result']
        if 'RealResult' in d:
            directionResult.RealResult = d['RealResult']
        else:
            directionResult.RealResult = True
        directionsResults.append(directionResult)
    return directionsResults

def googleMapsSecondModelData(mixerTrucks, loadingPlaces, deliveries, orders, 
    NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath):
    googleApiKeyPathFile = 'C:\GoogleApiKey\key.txt'
    fileGmapsKey = open(googleApiKeyPathFile, 'r') 
    lines = fileGmapsKey.readlines()
    googleMapsApiKey = lines[0]
    gmaps = googlemaps.Client(key=googleMapsApiKey)

    with open(basePath + '\\DirectionsResultsStored.json') as infile:
        directionsResultsStored = json.load(infile)

    directionsResults = convert_dictionary_directions_results_to_array(directionsResultsStored)
    for order in orders:
        for loadingPlace in loadingPlaces:
            directionResult = next((directionResult for directionResult in directionsResults 
                if directionResult.OriginLatitude == round(float(loadingPlace.LATITUDE_FILIAL), 6) and 
                    directionResult.OriginLongitude == round(float(loadingPlace.LONGITUDE_FILIAL), 6) and 
                    directionResult.DestinyLatitude == round(float(order.LATITUDE_OBRA), 8) and
                    directionResult.DestinyLongitude == round(float(order.LONGITUDE_OBRA), 8) and 
                    directionResult.Hour == pd.to_datetime(order.HORSAIDACENTRAL).hour), None)
            if directionResult == None:
                loadingPlaceLatLong = (float(loadingPlace.LATITUDE_FILIAL), float(loadingPlace.LONGITUDE_FILIAL))
                constructionSiteLatLong = (float(order.LATITUDE_OBRA), float(order.LONGITUDE_OBRA))
                now = datetime.now() + timedelta(days=1)
                directions_result = gmaps.directions(loadingPlaceLatLong, constructionSiteLatLong, mode="driving", 
                    departure_time=now.replace(hour=pd.to_datetime(order.HORSAIDACENTRAL).hour, minute=0))
                if len(directions_result) > 0:
                    loadingPlace.DISTANCE = round(float(directions_result[0]['legs'][0]['distance']['value'])/1000, 1)
                    loadingPlace.TRAVELTIME = int(directions_result[0]['legs'][0]['duration']['value']/60)
                    directionResult = DirectionResult()
                    directionResult.Distance = int(loadingPlace.DISTANCE)
                    directionResult.TravelTime = int(loadingPlace.TRAVELTIME)
                    directionResult.OriginLatitude = round(float(loadingPlace.LATITUDE_FILIAL), 6)
                    directionResult.OriginLongitude = round(float(loadingPlace.LONGITUDE_FILIAL), 6)
                    directionResult.DestinyLatitude = round(float(order.LATITUDE_OBRA), 8)
                    directionResult.DestinyLongitude = round(float(order.LONGITUDE_OBRA), 8)
                    directionResult.Hour = pd.to_datetime(order.HORSAIDACENTRAL).hour
                    directionResult.TimeString = order.HORSAIDACENTRAL
                    directionResult.Result = ''
                    directionResult.RealResult = True
                    #directionResult.Result = directions_result
                else:
                    loadingPlace.DISTANCE = round(hs.haversine(loadingPlaceLatLong, constructionSiteLatLong),1)
                    loadingPlace.TRAVELTIME = loadingPlace.DISTANCE * 2
                    directionResult = DirectionResult()
                    directionResult.Distance = loadingPlace.DISTANCE
                    directionResult.TravelTime = loadingPlace.TRAVELTIME
                    directionResult.OriginLatitude = round(float(loadingPlace.LATITUDE_FILIAL), 6)
                    directionResult.OriginLongitude = round(float(loadingPlace.LONGITUDE_FILIAL), 6)
                    directionResult.DestinyLatitude = round(float(order.LATITUDE_OBRA), 8)
                    directionResult.DestinyLongitude = round(float(order.LONGITUDE_OBRA), 8)
                    directionResult.Hour = pd.to_datetime(order.HORSAIDACENTRAL).hour
                    directionResult.TimeString = order.HORSAIDACENTRAL
                    directionResult.Result = ''
                    directionResult.RealResult = False
                directionsResultsStored['DirectionsResults'].append(directionResult.__dict__)
                with open(basePath + '\\DirectionsResultsStored.json', 'w') as outfile:
                    json.dump(directionsResultsStored, outfile, indent=4)
                directionsResults = convert_dictionary_directions_results_to_array(directionsResultsStored)
            else:
                loadingPlace.DISTANCE = directionResult.Distance
                loadingPlace.TRAVELTIME = directionResult.TravelTime
            loadingPlaceInfo = LoadingPlace(index=loadingPlace.index, 
                CODCENTCUS=loadingPlace.CODCENTCUS, LATITUDE_FILIAL=loadingPlace.LATITUDE_FILIAL, 
                LONGITUDE_FILIAL=loadingPlace.LONGITUDE_FILIAL)
            loadingPlaceInfo.DISTANCE = loadingPlace.DISTANCE
            loadingPlaceInfo.TRAVELTIME = loadingPlace.TRAVELTIME
            order.LOADINGPLACES_INFO.append(loadingPlaceInfo)

    nN = len(loadingPlaces) + len(deliveries)
    nA = len(loadingPlaces) + len(deliveries)
    nI = len(loadingPlaces)
    nJ = len(deliveries)
    nK = len(mixerTrucks)
    nL = 8
    revenues = np.zeros((len(deliveries)))
    codLoadingPlants = np.zeros((len(loadingPlaces)))
    codMixerTrucks = np.zeros((len(mixerTrucks)))
    codOrders = np.zeros((len(deliveries)))
    codDeliveries = np.zeros((len(deliveries)))
    a = np.zeros((len(deliveries)))
    b = np.zeros((len(deliveries)))
    c = np.zeros((len(loadingPlaces), len(deliveries)))
    t = np.zeros((len(loadingPlaces), len(deliveries)))
    csd = np.zeros((len(deliveries)))

    u = np.zeros((len(mixerTrucks), len(loadingPlaces)))

    i = 0
    for lp in loadingPlaces:
        codLoadingPlants[i] = lp.CODCENTCUS
        j = 0
        for order in orders:
            loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lp.CODCENTCUS == lpo.CODCENTCUS), None)
            if order.MEDIA_M3_DESCARGA == None or order.MEDIA_M3_DESCARGA <= 2:
                order.MEDIA_M3_DESCARGA = 4
            for dl in order.TRIPS:
                a[j] = dl.HORCHEGADAOBRA
                b[j] = dl.HORCHEGADAOBRA + 15
                t[i][j] = loadingPlaceInfo.TRAVELTIME
                distance = loadingPlaceInfo.DISTANCE
                if dl.CUSVAR == 0 or dl.CUSVAR == None:
                    dl.CUSVAR = DEFAULT_RMC_COST
                cost = (dl.CUSVAR * int(dl.VALVOLUMEPROG)) + (distance * FIXED_L_PER_KM * DEFAULT_DIESEL_COST * 2)
                c[i][j] = round(cost)
                revenues[j] = int(dl.VLRVENDA) * int(dl.VALVOLUMEPROG)
                codOrders[j] = order.CODPROGRAMACAO
                codDeliveries[j] = dl.CODPROGVIAGEM
                csd[j] = order.MEDIA_M3_DESCARGA * dl.VALVOLUMEPROG
                j += 1
        i += 1
    
    for k in range(len(mixerTrucks)):
        codMixerTrucks[k] = mixerTrucks[k].CODVEICULO
        loadingPlaceMixerTruck = next((lp for lp in loadingPlaces if lp.CODCENTCUS == mixerTrucks[k].CODCENTCUS), None)
        u[k][(loadingPlaceMixerTruck.index - 1)] = 1

    datfile = open(basePath + '\\RMCTDP_Simple_Ref_GoogleMaps.dat', 'w+')

    datfile.write('nN = ' + str(nN) + ';\n')
    datfile.write('nA = ' + str(nA) + ';\n')
    datfile.write('nI = ' + str(nI) + ';\n')
    datfile.write('nJ = ' + str(nJ) + ';\n')
    datfile.write('nK = ' + str(nK) + ';\n')
    datfile.write('nL = ' + str(nL) + ';\n')

    # datfile.write('u = [\n')
    # i = 0
    # while i < nK:
    #     strCLine = ''
    #     strCLine = '[' + str(int(u[i][0]))
    #     j = 1
    #     while j < nI:
    #         strCLine += (', ' + str(int(u[i][j])))
    #         j += 1
    #     if i == (nK - 1):
    #         strCLine += ']\n'
    #     else:
    #         strCLine += '],\n'
    #     datfile.write(strCLine)
    #     i += 1
    # datfile.write('];\n')

    i = 1
    strrevenues = 'revenues = [' + str(int(revenues[0]))
    while i < (nJ):
        strrevenues += ', ' + str(int(revenues[i]))
        i += 1
    strrevenues += '];\n'
    datfile.write(strrevenues)

    i = 1
    strcodLoadingPlants = 'codLoadingPlants = [' + str(int(codLoadingPlants[0]))
    while i < (nI):
        strcodLoadingPlants += ', ' + str(int(codLoadingPlants[i]))
        i += 1
    strcodLoadingPlants += '];\n'
    datfile.write(strcodLoadingPlants)

    i = 1
    strcodMixerTrucks = 'codMixerTrucks = [' + str(int(codMixerTrucks[0]))
    while i < (nK):
        strcodMixerTrucks += ', ' + str(int(codMixerTrucks[i]))
        i += 1
    strcodMixerTrucks += '];\n'
    datfile.write(strcodMixerTrucks)

    i = 1
    strcodOrders = 'codOrders = [' + str(int(codOrders[0]))
    while i < (nJ):
        strcodOrders += ', ' + str(int(codOrders[i]))
        i += 1
    strcodOrders += '];\n'
    datfile.write(strcodOrders)

    i = 1
    strcodDeliveries = 'codDeliveries = [' + str(int(codDeliveries[0]))
    while i < (nJ):
        strcodDeliveries += ', ' + str(int(codDeliveries[i]))
        i += 1
    strcodDeliveries += '];\n'
    datfile.write(strcodDeliveries)

    i = 1
    stra = 'a = [' + str(int(a[0]))
    while i < (nJ):
        stra += ', ' + str(int(a[i]))
        i += 1
    stra += '];\n'
    datfile.write(stra)
    
    i = 1
    strb = 'b = [' + str(int(b[0]))
    while i < (nJ):
        strb += ', ' + str(int(b[i]))
        i += 1
    strb += '];\n'
    datfile.write(strb)

    datfile.write('c = [\n')
    i = 0
    while i < nI:
        strCLine = ''
        strCLine = '[' + str(int(c[i][0]))
        j = 1
        while j < nJ:
            strCLine += (', ' + str(int(c[i][j])))
            j += 1
        if i == (nI - 1):
            strCLine += ']\n'
        else:
            strCLine += '],\n'
        datfile.write(strCLine)
        i += 1
    datfile.write('];\n')

    datfile.write('t = [\n')
    i = 0
    while i < nI:
        strTLine = ''
        strTLine = '[' + str(int(t[i][0]))
        j = 1
        while j < nJ:
            strTLine += (', ' + str(int(t[i][j])))
            j += 1
        if i == (nI - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')

    i = 1
    strcsd = 'csd = [' + str(int(csd[0]))
    while i < (nJ):
        strcsd += ', ' + str(int(csd[i]))
        i += 1
    strcsd += '];\n'
    datfile.write(strcsd)