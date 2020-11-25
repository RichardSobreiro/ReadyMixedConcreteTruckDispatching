import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime
import googlemaps
import json

from classes import LoadingPlace, MixerTruck, Order, Delivery, DirectionResult

def haversineData(mixerTrucks, loadingPlaces, deliveries, orders, 
    NEW_ORDER_ID, DEFAULT_RMC_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_CAPACIT_M3,
    FIXED_MIXED_TRUCK_COST, DEFAULT_DIESEL_COST, basePath):
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
        for dl in deliveries:
            loadingPlaceLatLong = (float(mt.LATITUDE_FILIAL), float(mt.LONGITUDE_FILIAL))
            constructionSiteLatLong = (float(dl.LATITUDE_OBRA), float(dl.LONGITUDE_OBRA))
            distance = hs.haversine(loadingPlaceLatLong, constructionSiteLatLong)
            if dl.CUSVAR == 0 or dl.CUSVAR == None:
                dl.CUSVAR = DEFAULT_RMC_COST
            cost = dl.CUSVAR * int(dl.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST)
            c[i][j] = round(cost)
            r[j] = int(dl.VLRVENDA) * int(dl.VALVOLUMEPROG)
            dcod[j] = dl.CODPROGVIAGEM
            odcod[j] = dl.CODPROGRAMACAO
            t[i][j] = round(distance * 2)
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
            ld = 8
            if fdno == 0 and dl.CODPROGRAMACAO == NEW_ORDER_ID:
                fdno = j

            j += 1
        i += 1
    
    lpmt = lpmt.astype(np.int32)

    datfile = open(basePath + '\\RMCTDP_Simple_Ref_Haversine.dat', 'w+')

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