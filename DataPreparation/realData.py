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

def realData(mixerTrucks, loadingPlaces, deliveries, orders, 
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
                if directionResult.OriginLatitude == float(loadingPlace.LATITUDE_FILIAL) and 
                    directionResult.OriginLongitude == float(loadingPlace.LONGITUDE_FILIAL) and 
                    directionResult.DestinyLatitude == float(order.LATITUDE_OBRA) and
                    directionResult.DestinyLongitude == float(order.LONGITUDE_OBRA) and 
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
                    directionResult.Distance = loadingPlace.DISTANCE
                    directionResult.TravelTime = loadingPlace.TRAVELTIME
                    directionResult.OriginLatitude = float(loadingPlace.LATITUDE_FILIAL)
                    directionResult.OriginLongitude = float(loadingPlace.LONGITUDE_FILIAL)
                    directionResult.DestinyLatitude = float(order.LATITUDE_OBRA)
                    directionResult.DestinyLongitude = float(order.LONGITUDE_OBRA)
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
                    directionResult.OriginLatitude = float(loadingPlace.LATITUDE_FILIAL)
                    directionResult.OriginLongitude = float(loadingPlace.LONGITUDE_FILIAL)
                    directionResult.DestinyLatitude = float(order.LATITUDE_OBRA)
                    directionResult.DestinyLongitude = float(order.LONGITUDE_OBRA)
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
            order.LOADINGPLACES_INFO.append(loadingPlace)

    nLP = len(loadingPlaces)
    nMT = len(mixerTrucks)
    nD = len(deliveries)
    dcod = np.zeros((len(deliveries)))
    odcod = np.zeros((len(deliveries)))
    lpmt = np.zeros((len(mixerTrucks)))
    c = np.zeros((len(mixerTrucks), len(deliveries)))
    t = np.zeros((len(mixerTrucks), len(deliveries)))
    q = FIXED_MIXED_TRUCK_CAPACIT_M3
    tc = FIXED_MIXED_TRUCK_COST
    d = np.zeros((len(deliveries)))
    a = np.zeros((len(deliveries)))
    b = np.zeros((len(deliveries)))
    cfr = np.zeros((len(deliveries)))
    od = np.zeros((len(deliveries)))
    dmbs = np.zeros((len(deliveries)))
    dmt = np.zeros((len(mixerTrucks), len(deliveries)))

    r = np.zeros((len(deliveries)))

    ld = 8

    fdno = 0

    M = 720

    i = 0
    for mt in mixerTrucks:
        j = 0
        for order in orders:
            loadingPlace = next((lp for lp in order.LOADINGPLACES_INFO if lp.CODCENTCUS == mt.CODCENTCUS), None)
            for dl in order.TRIPS:
                distance = loadingPlace.DISTANCE
                if dl.CUSVAR == 0 or dl.CUSVAR == None:
                    dl.CUSVAR = DEFAULT_RMC_COST
                cost = dl.CUSVAR + (distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST)
                dcod[j] = dl.CODPROGVIAGEM
                odcod[j] = dl.CODPROGRAMACAO
                c[i][j] = round(cost * int(dl.VALVOLUMEPROG))
                t[i][j] = loadingPlace.TRAVELTIME
                lpmt[i] = int(loadingPlace.index)
                d[j] = dl.VALVOLUMEPROG
                a[j] = dl.HORCHEGADAOBRA
                b[j] = dl.HORCHEGADAOBRA + 15
                cfr[j] = 3
                od[j] = dl.CODPROGRAMACAO
                if dl.CODPROGRAMACAO == NEW_ORDER_ID:
                    dmbs[j] = 0
                else:
                    dmbs[j] = 1
                dmt[i][j] = 1
                r[j] = int(dl.VLRVENDA) * int(dl.VALVOLUMEPROG)
                ld = 8
                if fdno == 0 and dl.CODPROGRAMACAO == NEW_ORDER_ID:
                    fdno = j

                j += 1
        i += 1
    
    lpmt = lpmt.astype(np.int32)

    datfile = open(basePath + '\\RMCTDP_Simple_Ref_Real.dat', 'w+')

    datfile.write('nLP = ' + str(nLP) + ';\n')
    datfile.write('nMT = ' + str(nMT) + ';\n')        
    datfile.write('nD = ' + str(nD) + ';\n')

    i = 1
    strLpmt = 'lpmt = [' + str(lpmt[0])
    while i < (nMT):
        strLpmt += ', ' + str(lpmt[i])
        i += 1
    strLpmt += '];\n'
    datfile.write(strLpmt)

    i = 1
    strDcod = 'dcod = [' + str(int(dcod[0]))
    while i < (nD):
        strDcod += ', ' + str(int(dcod[i]))
        i += 1
    strDcod += '];\n'
    datfile.write(strDcod)

    i = 1
    strOdcod = 'odcod = [' + str(int(odcod[0]))
    while i < (nD):
        strOdcod += ', ' + str(int(odcod[i]))
        i += 1
    strOdcod += '];\n'
    datfile.write(strOdcod)

    datfile.write('c = [\n')
    i = 0
    while i < nMT:
        strCLine = ''
        strCLine = '[' + str(c[i][0])
        j = 1
        while j < nD:
            strCLine += (', ' + str(c[i][j]))
            j += 1
        if i == (nMT - 1):
            strCLine += ']\n'
        else:
            strCLine += '],\n'
        datfile.write(strCLine)
        i += 1
    datfile.write('];\n')

    datfile.write('t = [\n')
    i = 0
    while i < nMT:
        strTLine = ''
        strTLine = '[' + str(t[i][0])
        j = 1
        while j < nD:
            strTLine += (', ' + str(t[i][j]))
            j += 1
        if i == (nMT - 1):
            strTLine += ']\n'
        else:
            strTLine += '],\n'
        datfile.write(strTLine)
        i += 1
    datfile.write('];\n')

    datfile.write('dmt = [\n')
    i = 0
    while i < nMT:
        strDmtLine = ''
        strDmtLine = '[' + str(dmt[i][0])
        j = 1
        while j < nD:
            strDmtLine += (', ' + str(dmt[i][j]))
            j += 1
        if i == (nMT - 1):
            strDmtLine += ']\n'
        else:
            strDmtLine += '],\n'
        datfile.write(strDmtLine)
        i += 1
    datfile.write('];\n')

    datfile.write('q = ' + str(q) + ';\n')
    datfile.write('tc = ' + str(tc) + ';\n')
    datfile.write('fdno = ' + str(fdno) + ';\n')

    i = 1
    strD = 'd = [' + str(d[0])
    while i < nD:
        strD += ', ' + str(d[i])
        i += 1
    strD += '];\n'
    datfile.write(strD)

    i = 1
    strA = 'a = [' + str(a[0])
    while i < nD:
        strA += ', ' + str(a[i])
        i += 1
    strA += '];\n'
    datfile.write(strA)

    i = 1
    strB = 'b = [' + str(b[0])
    while i < nD:
        strB += ', ' + str(b[i])
        i += 1
    strB += '];\n'
    datfile.write(strB)

    i = 1
    strCfr = 'cfr = [' + str(cfr[0])
    while i < nD:
        strCfr += ', ' + str(cfr[i])
        i += 1
    strCfr += '];\n'
    datfile.write(strCfr)

    i = 1
    strOd = 'od = [' + str(od[0])
    while i < nD:
        strOd += ', ' + str(od[i])
        i += 1
    strOd += '];\n'
    datfile.write(strOd)

    i = 1
    strR = 'r = [' + str(r[0])
    while i < nD:
        strR += ', ' + str(r[i])
        i += 1
    strR += '];\n'
    datfile.write(strR)

    i = 1
    strDmbs = 'dmbs = [' + str(dmbs[0])
    while i < nD:
        strDmbs += ', ' + str(dmbs[i])
        i += 1
    strDmbs += '];\n'
    datfile.write(strDmbs)

    datfile.write('ld = ' + str(ld) + ';\n')

    datfile.write('M = ' + str(M) + ';\n')