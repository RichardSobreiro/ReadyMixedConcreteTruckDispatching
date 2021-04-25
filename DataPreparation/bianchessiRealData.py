import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import googlemaps
import json
import random

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


def bianchessiRealData(basePath, mixerTrucks, loadingPlaces, deliveries, orders, 
    NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST):
    N = len(deliveries)
    P = len(loadingPlaces)
    V = len(mixerTrucks)

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

    for lp in loadingPlaces:
        for order in orders:
            loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lp.CODCENTCUS == lpo.CODCENTCUS), None)
            if order.MEDIA_M3_DESCARGA == None or order.MEDIA_M3_DESCARGA <= 2:
                order.MEDIA_M3_DESCARGA = 4
            for dl in order.TRIPS:
                distance = loadingPlaceInfo.DISTANCE
                if dl.CUSVAR == 0 or dl.CUSVAR == None:
                    dl.CUSVAR = DEFAULT_RMC_COST
                cost = (dl.CUSVAR * dl.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * DEFAULT_DIESEL_COST * 2)
                #dl.TotalTripCost = cost

    cc = np.zeros((P, N))
    tt = np.zeros((P, N))

    c = np.zeros((P, N, N, N))
    t = np.zeros((P, N, N, N))
    s = np.zeros((N))
    cfr = np.zeros((N))

    codLoadingPlants = np.zeros((P))
    codOrders = np.zeros((N))
    codDeliveries = np.zeros((N))

    vold = np.zeros((N))

    p = 0
    for lp in loadingPlaces:
        codLoadingPlants[p] = lp.CODCENTCUS
        i = 0
        for order in orders:
            loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lp.CODCENTCUS == lpo.CODCENTCUS), None)
            for dl in order.TRIPS:
                s[i] = dl.HORCHEGADAOBRA
                # a[j] = dl.HORCHEGADAOBRA
                # b[j] = dl.HORCHEGADAOBRA + 15
                tt[p][i] = loadingPlaceInfo.TRAVELTIME
                distance = loadingPlaceInfo.DISTANCE
                cost = (dl.CUSVAR * dl.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * DEFAULT_DIESEL_COST * 2)
                cc[p][i] = cost
                codOrders[i] = dl.CODPROGRAMACAO
                codDeliveries[i] = dl.CODPROGVIAGEM
                cfr[i] = int(order.MEDIA_M3_DESCARGA)
                vold[i] = dl.VALVOLUMEPROG
                i += 1
        p += 1

    # p = 0
    # while p < P:
    #     i = 0
    #     while i < N:
    #         j = 0
    #         while j < N:
    #             k = 0
    #             while k < N:
    #                 if i == j and j == k:
    #                     t[p][i][j][k] = int((2 * tt[p][i]) + (cfr[i] * vold[i]) + 10)
    #                 else:
    #                     if (i == j and j != k) or (i != j and j == k):
    #                         t[p][i][j][k] = int(2 * (tt[p,i] + tt[p,k]) + (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + 20)
    #                     else:
    #                         t[p][i][j][k] = int((2 * tt[p,i]) + (2 * tt[p,j]) + (2 * tt[p,k]) + (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 30)
    #                 k += 1
    #             j += 1
    #         i += 1
    #     p += 1

    # p = 0
    # while p < P:
    #     i = 0
    #     while i < N:
    #         j = 0
    #         while j < N:
    #             k = 0
    #             while k < N:
    #                 if i == j and j == k:
    #                     c[p][i][j][k] = cc[p][i]
    #                 else:
    #                     if(i == j and j != k):
    #                         c[p][i][j][k] = cc[p][i] + cc[p][k]
    #                         # if(s[i] + (cfr[i] * 8) + tt[p][i]) <= (s[k] - tt[p][k] - 10):
    #                         #     c[p][i][j][k] = cc[p][i] + cc[p][k]
    #                         # else:
    #                         #     c[p][i][j][k] = 100000
    #                         #     #t[p][i][j][k] = 100000
    #                     elif (i != j and j == k):
    #                         c[p][i][j][k] = cc[p][i] + cc[p][j]
    #                         # if(s[i] + (cfr[i] * 8) + tt[p][i]) <= (s[j] - tt[p][j] - 10):
    #                         #     c[p][i][j][k] = cc[p][i] + cc[p][j]
    #                         # else:
    #                         #     c[p][i][j][k] = 100000
    #                         #     #t[p][i][j][k] = 100000
    #                     else:
    #                         c[p][i][j][k] = cc[p][i] + cc[p][j] + cc[p][k]
    #                         # if ((s[i] + (cfr[i] * 8) + tt[p][i]) <= (s[j] - 10 - tt[p][j])) and ((s[j] + (cfr[j] * 8) + tt[p][j]) <= (s[k] - 10 - tt[p][k])):
    #                         #     c[p][i][j][k] = cc[p][i] + cc[p][j] + cc[p][k]
    #                         # else:
    #                         #     c[p][i][j][k] = 100000
    #                         #     #t[p][i][j][k] =  100000
    #                 k += 1
    #             j += 1
    #         i += 1
    #     p += 1

    datfile = open(basePath + '\\BianchessiReal.dat', 'w+')

    datfile.write('nc = ' + str(N) + ';\n')
    datfile.write('np = ' + str(P) + ';\n')        
    datfile.write('nv = ' + str(V) + ';\n')

    datfile.write('tt = [\n')
    i = 0
    while i < P:
        strTLine = ''
        strTLine = '[' + str(int(tt[i][0]))
        j = 1
        while j < N:
            strTLine += (', ' + str(int(tt[i][j])))
            j += 1
        if i == (P - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')

    datfile.write('cc = [\n')
    i = 0
    while i < P:
        strTLine = ''
        strTLine = '[' + str(int(cc[i][0]))
        j = 1
        while j < N:
            strTLine += (', ' + str(int(cc[i][j])))
            j += 1
        if i == (P - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')

    i = 1
    strp = 's = [' + str(int(s[0]))
    while i < (N):
        strp += ', ' + str(int(s[i]))
        i += 1
    strp += '];\n'
    datfile.write(strp)

    i = 1
    scfr = 'cfr = [' + str(int(cfr[0]))
    while i < (N):
        scfr += ', ' + str(int(cfr[i]))
        i += 1
    scfr += '];\n'
    datfile.write(scfr)

    i = 1
    svold = 'vold = [' + str(int(vold[0]))
    while i < (N):
        svold += ', ' + str(int(vold[i]))
        i += 1
    svold += '];\n'
    datfile.write(svold)

    i = 1
    scfr = 'codLoadingPlants = [' + str(int(codLoadingPlants[0]))
    while i < (P):
        scfr += ', ' + str(int(codLoadingPlants[i]))
        i += 1
    scfr += '];\n'
    datfile.write(scfr)

    i = 1
    scfr = 'codOrders = [' + str(int(codOrders[0]))
    while i < (N):
        scfr += ', ' + str(int(codOrders[i]))
        i += 1
    scfr += '];\n'
    datfile.write(scfr)

    i = 1
    scfr = 'codDeliveries = [' + str(int(codDeliveries[0]))
    while i < (N):
        scfr += ', ' + str(int(codDeliveries[i]))
        i += 1
    scfr += '];\n'
    datfile.write(scfr)

    # datfile.write('c = ')
    # p = 0
    # i = 0
    # j = 0
    # k = 0
    # #strCLine = '[\n'
    # while p < P:
    #     i = 0
    #     strCLine = '[\n[\n'
    #     while i < N:
    #         j = 0
    #         strCLine += '[\n'
    #         while j < N:
    #             k = 0
    #             strCLine += '['
    #             while k < N:
    #                 if k == 0:
    #                     strCLine += (str(c[p][i][j][k]))
    #                 else:
    #                     strCLine += (', ' + str(c[p][i][j][k]))
    #                 k += 1
    #             if j == (N - 1):
    #                 strCLine += ']\n'
    #             else:
    #                 strCLine += '],\n'
    #             j += 1
    #         if i == (N - 1):
    #             strCLine += ']\n'
    #         else:
    #             strCLine += '],\n'
    #         i += 1
    #     if p == (P - 1):
    #         strCLine += ']\n'
    #     else:
    #         strCLine += '],\n'
    #     datfile.write(strCLine)
    #     p += 1
    # #datfile.write(strCLine)
    # datfile.write('];\n')

    # datfile.write('t = ')
    # p = 0
    # i = 0
    # j = 0
    # k = 0
    # strCLine = '[\n'
    # while p < P:
    #     i = 0
    #     strCLine += '[\n'
    #     while i < N:
    #         j = 0
    #         strCLine += '[\n'
    #         while j < N:
    #             k = 0
    #             strCLine += '['
    #             while k < N:
    #                 if k == 0:
    #                     strCLine += (str(t[p][i][j][k]))
    #                 else:
    #                     strCLine += (', ' + str(t[p][i][j][k]))
    #                 k += 1
    #             if j == (N - 1):
    #                 strCLine += ']\n'
    #             else:
    #                 strCLine += '],\n'
    #             j += 1
    #         if i == (N - 1):
    #             strCLine += ']\n'
    #         else:
    #             strCLine += '],\n'
    #         i += 1
    #     if p == (P - 1):
    #         strCLine += ']\n'
    #     else:
    #         strCLine += '],\n'
    #     p += 1
    # datfile.write(strCLine)
    # datfile.write('];\n')