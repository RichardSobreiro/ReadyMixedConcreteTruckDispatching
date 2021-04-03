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
    P = loadingPlaces
    V = numberOfTrucks

    cc = np.zeros((P, N))
    tt = np.zeros((P, N))

    c = np.zeros((P, N, N, N))
    t = np.zeros((P, N, N, N))
    s = np.zeros((N))
    cfr = np.zeros((N))

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
        i += 1

    p = 0
    while p < P:
        i = 0
        while i < N:
            j = 0
            while j < N:
                k = 0
                while k < N:
                    if i == j and j == k:
                        t[p][i][j][k] = int((2 * tt[p][i]) + (cfr[i] * 8) + 10)
                    else:
                        if (i == j and j != k) or (i != j and j == k):
                            t[p][i][j][k] = int(2 * (tt[p,i] + tt[p,k]) + (cfr[i] * 8) + (cfr[k] * 8) + 20)
                        else:
                            t[p][i][j][k] = int((2 * tt[p,i]) + (2 * tt[p,j]) + (2 * tt[p,k]) + (cfr[i] * 8) + (cfr[j] * 8) + (cfr[k] * 8) + 30)
                    k += 1
                j += 1
            i += 1
        p += 1

    p = 0
    while p < P:
        i = 0
        while i < N:
            j = 0
            while j < N:
                k = 0
                while k < N:
                    if i == j and j == k:
                        c[p][i][j][k] = 10 * t[p][i][j][k]
                    else:
                        if(i == j and j != k):
                            if(s[i] + tt[p][i] + (cfr[i] * 8)) > (s[k] - tt[p][k] - 10):
                                c[p][i][j][k] = 1000 * t[p][i][j][k]
                                #t[p][i][j][k] = c[p][i][j][k]
                            else:
                                c[p][i][j][k] = 10 * t[p][i][j][k]
                        elif (i != j and j == k):
                            if(s[i] + (cfr[i] * 8) + tt[p][i]) > (s[j] - tt[p][j] - 10):
                                c[p][i][j][k] = 1000 * t[p][i][j][k]
                                #t[p][i][j][k] = c[p][i][j][k]
                            else:
                                c[p][i][j][k] = 10 * t[p][i][j][k]
                        else:
                            if ((s[i] + (cfr[i] * 8) + tt[p][i]) > (s[j] - tt[p][j] - 10)) or ((s[j] + (cfr[j] * 8) + tt[p][j]) > (s[k] - tt[p][k] - 10)):
                                c[p][i][j][k] = 1000 * t[p][i][j][k]
                                #t[p][i][j][k] = c[p][i][j][k]
                            else:
                                c[p][i][j][k] = 10 * t[p][i][j][k]
                    k += 1
                j += 1
            i += 1
        p += 1

    datfile = open(basePath + '\\Bianchessi.dat', 'w+')

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

    datfile.write('c = ')
    p = 0
    i = 0
    j = 0
    k = 0
    strCLine = '[\n'
    while p < P:
        i = 0
        strCLine += '[\n'
        while i < N:
            j = 0
            strCLine += '[\n'
            while j < N:
                k = 0
                strCLine += '['
                while k < N:
                    if k == 0:
                        strCLine += (str(c[p][i][j][k]))
                    else:
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
            #datfile.write(strCLine)
        p += 1
    datfile.write(strCLine)
    datfile.write('];\n')

    datfile.write('t = ')
    p = 0
    i = 0
    j = 0
    k = 0
    strCLine = '[\n'
    while p < P:
        i = 0
        strCLine += '[\n'
        while i < N:
            j = 0
            strCLine += '[\n'
            while j < N:
                k = 0
                strCLine += '['
                while k < N:
                    if k == 0:
                        strCLine += (str(t[p][i][j][k]))
                    else:
                        strCLine += (', ' + str(t[p][i][j][k]))
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
        p += 1
    datfile.write(strCLine)
    datfile.write('];\n')