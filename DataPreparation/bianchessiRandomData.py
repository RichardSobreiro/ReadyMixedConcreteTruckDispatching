import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import googlemaps
import json
import random

from classes import LoadingPlace, MixerTruck, Order, Delivery, DirectionResult

def bianchessiRandomData(basePath, loadingPlaces, deliveries, numberOfTrucks):
    N = deliveries
    P = mixerTrucks
    V = numberOfTrucks

    cc = np.zeros((len(P), len(N)))
    tt = np.zeros((len(P), len(N)))

    c = np.zeros((len(P), len(N), len(N), len(N)))
    t = np.zeros((len(P), len(N), len(N), len(N)))
    s = np.zeros((len(N)))
    cfr = np.zeros((len(N)))

    p = 0
    i = 0
    while p < P:
        while i < N:
            cc[p][i] = random.randint(200, 400)
            tt[p][i] = random.randint(15, 80)
            i += 1
        p += 1
    
    i = 0
    while i < N:
        s[i] = random.randint(0, 480)
        cfr[i] = random.uniform(4, 8)
        i += 1

    p = 0
    i = 0
    j = 0
    k = 0
    while p < P:
        while i < N:
            while j < N:
                while k < N:
                        if i == j and j == k:
                            t[p][i][j][k] = (2 * t[p][i]) + (cfr[i] * 8) + 10
                        else:
                            else:
                                if (i == j and j != k) or (i != j and j == k):
                                    t[p][i][j][k] = 2 * (tt[p,i] + tt[p,k]) + (cfr[i] * 8) + (cfr[k] * 8) + 20
                                else:
                                    t[p][i][j][k] = (2 * tt[p,i]) + (2 * tt[p,j]) + (2 * tt[p,k])
                                         + (cfr[i] * 8) + (cfr[j] * 8) + (cfr[k] * 8) + 30
                    k += 1
                j += 1
            i += 1
        p += 1

    p = 0
    i = 0
    j = 0
    k = 0
    while p < P:
        while i < N:
            while j < N:
                while k < N:
                        if i == j and j == k:
                            c[p][i][j][k] = 10 * t[p][i][j][k]
                    k += 1
                j += 1
            i += 1
        p += 1

    datfile = open(basePath + '\\Bianchessi.dat', 'w+')

    datfile.write('nc = ' + str(N) + ';\n')
    datfile.write('np = ' + str(P) + ';\n')        
    datfile.write('nv = ' + str(V) + ';\n')

    datfile.write('c = [\n')
    p = 0
    i = 0
    j = 0
    k = 0
    while p < P:
        strCLine = ''
        strCLine = '[[[' + str(c[p][0][0][0])
        i = 1
        while i < N:
            j = 1
            while j < N:
                k = 1
                while k < N:
                    strCLine += (', ' + str(c[p][i][j][k]))
                    k += 1
                if j == (N - 1):
                    strCLine += ']\n'
                else:
                    strCLine += '],\n'
                j += 1
            if i == (N - 1):
                strCLine += ']\n'
            else:
                strCLine += '],\n'
            i += 1
        if p == (P - 1):
            strCLine += ']\n'
        else:
            strCLine += '],\n'
            datfile.write(strCLine)
        p += 1
    datfile.write('];\n')