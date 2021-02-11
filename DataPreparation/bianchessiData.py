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

def bianchessiData(mixerTrucks, loadingPlaces, deliveries, orders, 
    NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath):
    googleApiKeyPathFile = 'C:\GoogleApiKey\key.txt'
    fileGmapsKey = open(googleApiKeyPathFile, 'r') 
    lines = fileGmapsKey.readlines()
    googleMapsApiKey = lines[0]
    gmaps = googlemaps.Client(key=googleMapsApiKey)

    REF_CODCENTCUS = 166020

    #loadingPlaces = next((cod for cod in mixerTrucks if cod == REF_CODCENTCUS), None)
    loadingPlaces = [d for d in loadingPlaces if d.CODCENTCUS == REF_CODCENTCUS]

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

    nN = (2*len(loadingPlaces)) + len(deliveries)
    m = len(mixerTrucks)
    Tmax = 1500

    p = np.zeros(nN)
    t = np.zeros((nN, nN))
    t0 = np.zeros((nN, nN))

    i = 0
    while i < 39:
        if i == 0 or i == (nN - 1):
            p[i] = 0
            i += 1
        else:
            for order in orders:
                loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lpo.CODCENTCUS == REF_CODCENTCUS), None)
                if order.MEDIA_M3_DESCARGA == None or order.MEDIA_M3_DESCARGA <= 2:
                    order.MEDIA_M3_DESCARGA = 4
                for dl in order.TRIPS:
                    distance = loadingPlaceInfo.DISTANCE
                    if dl.CUSVAR == 0 or dl.CUSVAR == None:
                        dl.CUSVAR = DEFAULT_RMC_COST
                    cost = (dl.CUSVAR * dl.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * DEFAULT_DIESEL_COST * 2)
                    p[i] = int(dl.VLRVENDA) * int(dl.VALVOLUMEPROG) - cost
                    i += 1

    i = 0
    while i < nN:
        j = 0
        while j < nN:
            if (i == 0 and j == 0) or (i == 0 and j != (nN)) or (j == 0 and (i != (nN))):
                t[i][j] = 0
                j += 1
            elif (i == 0 and j != 0 and j != (nN)) or (j == (nN) and i != 0 and i != (nN)):
                for order in orders:
                    loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lpo.CODCENTCUS == REF_CODCENTCUS), None)
                    if order.MEDIA_M3_DESCARGA == None or order.MEDIA_M3_DESCARGA <= 2:
                        order.MEDIA_M3_DESCARGA = 4
                    for dl in order.TRIPS:
                        t[i][j] = loadingPlaceInfo.TRAVELTIME
                        j += 1
            else:
                t[i][j] = 1000
                j += 1
        i += 1
    
    i = 0
    j = 0
    for i in range(nN):
        for j in range(nN):
            t0[i][j] = t[0][j] + t[i][j]

    datfile = open(basePath + '\\RMCTDP_Bianchessi.dat', 'w+')

    datfile.write('nN = ' + str(nN) + ';\n')
    datfile.write('m = ' + str(m) + ';\n')
    datfile.write('Tmax = ' + str(Tmax) + ';\n')

    i = 1
    strp = 'p = [' + str(int(p[0]))
    while i < (nJ):
        strp += ', ' + str(int(p[i]))
        i += 1
    strp += '];\n'
    datfile.write(strp)

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
    
    datfile.write('t0 = [\n')
    i = 0
    while i < nI:
        strTLine = ''
        strTLine = '[' + str(int(t0[i][0]))
        j = 1
        while j < nJ:
            strTLine += (', ' + str(int(t0[i][j])))
            j += 1
        if i == (nI - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')