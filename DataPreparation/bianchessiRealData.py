import sys
import pandas as pd
import numpy as np
from numpy import *
import haversine as hs
from datetime import datetime, timedelta
import googlemaps
import json
import random
import os

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
    a = np.zeros((N))
    b = np.zeros((N))
    cfr = np.zeros((N))

    codLoadingPlants = np.zeros((P))
    codOrders = np.zeros((N))
    codDeliveries = np.zeros((N))
    codMixerTrucks = np.zeros((len(mixerTrucks)))

    vold = np.zeros((N))

    p = 0
    for lp in loadingPlaces:
        codLoadingPlants[p] = lp.CODCENTCUS
        i = 0
        for order in orders:
            loadingPlaceInfo = next((lpo for lpo in order.LOADINGPLACES_INFO if lp.CODCENTCUS == lpo.CODCENTCUS), None)
            for dl in order.TRIPS:
                s[i] = dl.HORCHEGADAOBRA
                a[i] = dl.HORCHEGADAOBRA
                b[i] = dl.HORCHEGADAOBRA + 15
                if isnan(loadingPlaceInfo.TRAVELTIME):
                    loadingPlaceInfo.TRAVELTIME = 10000
                tt[p][i] = loadingPlaceInfo.TRAVELTIME
                distance = loadingPlaceInfo.DISTANCE
                cost = (dl.CUSVAR * dl.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * DEFAULT_DIESEL_COST * 2)
                if isnan(cost):
                    cost = 350
                cc[p][i] = cost
                codOrders[i] = dl.CODPROGRAMACAO
                codDeliveries[i] = dl.CODPROGVIAGEM
                cfr[i] = int(order.MEDIA_M3_DESCARGA)
                vold[i] = dl.VALVOLUMEPROG
                i += 1
        p += 1

    for k in range(len(mixerTrucks)):
        codMixerTrucks[k] = mixerTrucks[k].CODVEICULO

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

    #region CANTU-FUNES DATA
    nN = len(loadingPlaces) + len(deliveries)
    nA = len(loadingPlaces) + len(deliveries)
    nI = len(loadingPlaces)
    nJ = len(deliveries)
    nK = len(mixerTrucks)
    nL = 8

    datfile = open(basePath + '\\CantuFunes.dat', 'w+')

    datfile.write('nN = ' + str(nN) + ';\n')
    datfile.write('nA = ' + str(nA) + ';\n')
    datfile.write('nI = ' + str(nI) + ';\n')
    datfile.write('nJ = ' + str(nJ) + ';\n')
    datfile.write('nK = ' + str(nK) + ';\n')
    datfile.write('nL = ' + str(nL) + ';\n')

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
        strCLine = '[' + str(int(cc[i][0]))
        j = 1
        while j < nJ:
            strCLine += (', ' + str(int(cc[i][j])))
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
        strTLine = '[' + str(int(tt[i][0]))
        j = 1
        while j < nJ:
            strTLine += (', ' + str(int(tt[i][j])))
            j += 1
        if i == (nI - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')

    i = 1
    scfr = 'cfr = [' + str(int(cfr[0]))
    while i < (nJ):
        scfr += ', ' + str(int(cfr[i]))
        i += 1
    scfr += '];\n'
    datfile.write(scfr)

    i = 1
    svold = 'vold = [' + str(int(vold[0]))
    while i < (nJ):
        svold += ', ' + str(int(vold[i]))
        i += 1
    svold += '];\n'
    datfile.write(svold)

    #endregion
