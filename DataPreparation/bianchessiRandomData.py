import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import googlemaps
import json
import random

from classes import LoadingPlace, MixerTruck, Order, Delivery, DirectionResult

def bianchessiRandomData(basePath, loadingPlaces, deliveries, numberOfTrucks, instanceCount):
    N = deliveries
    P = loadingPlaces
    V = numberOfTrucks

    cc = np.zeros((P, N))
    tt = np.zeros((P, N))

    s = np.zeros((N))
    cfr = np.zeros((N))
    vold = np.zeros((N))

    codLoadingPlants = np.zeros((P))
    codOrders = np.zeros((N))
    codDeliveries = np.zeros((N))

    p = 0
    i = 0
    while p < P:
        codLoadingPlants[p] = p
        while i < N:
            cc[p][i] = random.randint(200, 400)
            tt[p][i] = random.randint(15, 80)
            codOrders[i] = i
            codDeliveries[i] = i
            i += 1
        i = 0
        p += 1
    
    i = 0
    while i < N:
        s[i] = random.randint(0, 720)
        cfr[i] = random.uniform(4, 8)
        vold[i] = random.uniform(7, 8)
        if((i % 4) == 0) or ((i % 5) == 0):
            vold[i] = random.uniform(4, 6)
        i += 1

    filename = str(N) + '_'  + str(P) + '_'  + str(V) + '_'  + str(instanceCount) + '.dat'
    instancesFile = open(basePath + '\\Instances.txt', 'a')
    instancesFile.write(filename + '\n')
    instancesFile.close()

    datfile = open(basePath + '\\' + filename, 'w+')

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
