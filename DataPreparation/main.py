import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime
import googlemaps
import json

class LoadingPlace:  
    def __init__(self, index, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 
        self.DISTANCE = 0
        self.TRAVELTIME = 0

class MixerTruck:  
    def __init__(self, index, CODVEICULO, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODVEICULO = CODVEICULO 
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 

class Order:  
    def __init__(self, CODPROGRAMACAO, CODCENTCUS, MEDIA_M3_DESCARGA, VALTOTALPROGRAMACAO, 
        HORSAIDACENTRAL, LATITUDE_OBRA, LONGITUDE_OBRA):  
        self.CODPROGRAMACAO = CODPROGRAMACAO 
        self.CODCENTCUS = CODCENTCUS 
        self.MEDIA_M3_DESCARGA = MEDIA_M3_DESCARGA 
        self.VALTOTALPROGRAMACAO = VALTOTALPROGRAMACAO 
        self.HORSAIDACENTRAL = HORSAIDACENTRAL
        self.LATITUDE_OBRA = LATITUDE_OBRA
        self.LONGITUDE_OBRA = LONGITUDE_OBRA
        self.TRIPS = []
        self.LOADINGPLACES_INFO = []

class Delivery:  
    def __init__(self, HORCHEGADAOBRA, CODPROGRAMACAO, CODPROGVIAGEM, CODCENTCUSVIAGEM, VLRTOTALNF, 
        VALVOLUMEPROG, CUSVAR, CODTRACO, LATITUDE_OBRA, LONGITUDE_OBRA):  
        self.HORCHEGADAOBRA = HORCHEGADAOBRA
        self.CODPROGRAMACAO = CODPROGRAMACAO 
        self.CODPROGVIAGEM = CODPROGVIAGEM 
        self.CODCENTCUSVIAGEM = CODCENTCUSVIAGEM 
        self.VLRTOTALNF = VLRTOTALNF 
        self.VALVOLUMEPROG = VALVOLUMEPROG
        self.CODTRACO = CODTRACO
        self.CUSVAR = CUSVAR
        self.LATITUDE_OBRA = LATITUDE_OBRA
        self.LONGITUDE_OBRA = LONGITUDE_OBRA
        self.LOADINGPLACESINFO = []

def main(argv):
    googleApiKeyPathFile = 'C:\GoogleApiKey\key.txt'
    fileGmapsKey = open(googleApiKeyPathFile, 'r') 
    lines = fileGmapsKey.readlines()
    googleMapsApiKey = lines[0]
    gmaps = googlemaps.Client(key=googleMapsApiKey)

    # mountGeoData = True
    # try:
    #     with open('OrdersData.json') as json_file:
    #         geodata = json.load(json_file)
    #     mountGeoData = False
    # except FileNotFoundError:
    #     print("Geo data file does not exists")
    #     mountGeoData = True

    dataFolder = 'RJ-13-06-2019'
    basePath = 'C:\\Users\\Richard Sobreiro\\Desktop\\' + dataFolder
    DEFAULT_DIESEL_COST = 3.5
    DEFAULT_RMC_COST = 150
    FIXED_MIXED_TRUCK_COST = 50
    FIXED_MIXED_TRUCK_CAPACIT_M3 = 10
    FIXED_KM_PER_L = 2
    FIXED_L_PER_KM = 27.5/100

    # dfLoadingPlaces = pd.read_csv(basePath + '\\LoadingPlaces.csv')
    # print(dfLoadingPlaces)

    dfTrips = pd.read_csv(basePath + '\\Trips.csv', encoding = "ISO-8859-1")
    dfTrips = dfTrips.sort_values('CODPROGRAMACAO')
    #print(dfTrips)

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
                row['LATITUDE_OBRA'], row['LONGITUDE_OBRA'])
            orders.append(order)

        delivery = next((v for v in deliveries if v.CODPROGVIAGEM == row['CODPROGVIAGEM']), None)
        h = row['HORCHEGADAOBRA']

        if delivery == None:
            constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%Y %H:%M')
            minutes = (constructionTime.hour * 60) + constructionTime.minute
            delivery = Delivery(HORCHEGADAOBRA = minutes, CODPROGRAMACAO=row['CODPROGRAMACAO'], CODPROGVIAGEM=row['CODPROGVIAGEM'], 
                CODCENTCUSVIAGEM=row['CODCENTCUSVIAGEM'], VLRTOTALNF=row['VLRTOTALNF'], VALVOLUMEPROG=row['VALVOLUMEPROG'], 
                CUSVAR=row['CUSVAR'], CODTRACO=row['CODTRACO'], LATITUDE_OBRA=row['LATITUDE_OBRA'], 
                LONGITUDE_OBRA=row['LONGITUDE_OBRA'])
            order.TRIPS.append(delivery)
            deliveries.append(delivery)

    with open(basePath + '\\DirectionsResultsStored.json') as json_file:
        directionsResultsStored = json.load(json_file)

    for order in orders:
        for loadingPlace in loadingPlaces:
            directionResult = next((directionResult for directionResult in odirectionsResultsStored 
                if directionResult.OriginLatitude == float(loadingPlace.LATITUDE_FILIAL) and 
                    directionResult.OriginLongitude = float(loadingPlace.LONGITUDE_FILIAL) and 
                    directionResult.DestinyLatitude = float(order.LATITUDE_OBRA) and
                    directionResult.DestinyLongitude = float(order.LONGITUDE_OBRA) and 
                    directionResult.Hour = pd.to_datetime(order.HORSAIDACENTRAL).hour), None)
            if directionResult == None:
                loadingPlaceLatLong = (float(loadingPlace.LATITUDE_FILIAL), float(loadingPlace.LONGITUDE_FILIAL))
                constructionSiteLatLong = (float(order.LATITUDE_OBRA), float(order.LONGITUDE_OBRA))
                now = datetime.now()   
                #directions_result = gmaps.directions("Sydney Town Hall", "Parramatta, NSW", mode="transit")
                directions_result = gmaps.directions(loadingPlaceLatLong, constructionSiteLatLong, mode="transit", 
                    departure_time=pd.to_datetime(order.HORSAIDACENTRAL))
                loadingPlace.DISTANCE = directions_result[0]['legs'][0]['distance']['value']
                loadingPlace.TRAVELTIME = int(directions_result[0]['legs'][0]['duration']['value']/60)
                directionResult = {}
                directionResult.OriginLatitude = float(loadingPlace.LATITUDE_FILIAL)
                directionResult.OriginLongitude = float(loadingPlace.LONGITUDE_FILIAL)
                directionResult.DestinyLatitude = float(order.LATITUDE_OBRA)
                directionResult.DestinyLongitude = float(order.LONGITUDE_OBRA)
                directionResult.Hour = pd.to_datetime(order.HORSAIDACENTRAL).hour
                directionResult.TimeString = order.HORSAIDACENTRAL
                directionResult.Result = directions_result
                directionsResultsStored.append(directionResult)
            else:
                loadingPlace.DISTANCE = directions_result[0]['legs'][0]['distance']['value']
                loadingPlace.TRAVELTIME = int(directions_result[0]['legs'][0]['duration']['value']/60)
            order.LOADINGPLACES_INFO.append(loadingPlace)

    with open(basePath + '\\DirectionsResultsStored.json', 'w') as outfile:
        json.dump(directionsResultsStored, outfile, indent=4)
    
    nLP = len(loadingPlaces)
    nMT = len(mixerTrucks)
    nD = len(deliveries)
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
        for dl in deliveries:
            loadingPlaceLatLong = (float(mt.LATITUDE_FILIAL), float(mt.LONGITUDE_FILIAL))
            constructionSiteLatLong = (float(dl.LATITUDE_OBRA), float(dl.LONGITUDE_OBRA))
            distance = hs.haversine(loadingPlaceLatLong, constructionSiteLatLong)
            if dl.CUSVAR == 0 or dl.CUSVAR == None:
                dl.CUSVAR = DEFAULT_RMC_COST
            cost = dl.CUSVAR + (distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST)
            c[i][j] = round(cost)
            t[i][j] = round(distance * 2 * 2)
            loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == mt.CODCENTCUS), None)
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
            r[j] = dl.VLRTOTALNF
            ld = 8
            if fdno == 0 and dl.CODPROGRAMACAO == NEW_ORDER_ID:
                fdno = j

            j += 1
        i += 1
    
    lpmt = lpmt.astype(np.int32)

    datfile = open(basePath + '\\RMCTDP_Simple_Ref.dat', 'w+')

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

if __name__ == '__main__':
    main(sys.argv[1:])

