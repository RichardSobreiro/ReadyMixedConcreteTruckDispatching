import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import plotly_express as px
import plotly
from gmplot import *

from classes import LoadingPlace, MixerTruck, Order, Delivery

def realResults(dataFolder, basePath, DEFAULT_DIESEL_COST, FIXED_L_PER_KM, FIXED_MIXED_TRUCK_COST):
    googleApiKeyPathFile = 'C:\GoogleApiKey\key.txt'
    fileGmapsKey = open(googleApiKeyPathFile, 'r') 
    lines = fileGmapsKey.readlines()
    googleMapsApiKey = lines[0]

    dfTrips = pd.read_csv(basePath + '\\Trips.csv', encoding = "ISO-8859-1")
    dfTrips = dfTrips.sort_values('CODPROGRAMACAO')

    dfTrips['HORSAIDACENTRAL'] = pd.to_datetime(dfTrips['HORSAIDACENTRAL'])
    dfTrips['HORCHEGADACENTRAL'] = pd.to_datetime(dfTrips['HORCHEGADACENTRAL'])

    dfTrips['MIXERTRUCKINDEX'] = 0

    loadingPlaces = []
    loadingPlacesIndex = 0
    mixerTrucks = []
    mixerTrucksIndex = 0
    orders = []
    deliveries = []
    totalProfit = 0
    for index, row in dfTrips.iterrows():
        loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == row['CODCENTCUSNOTAFISCAL']), None)
        if loadingPlace == None:
            loadingPlacesIndex += 1
            loadingPlace = LoadingPlace(loadingPlacesIndex, row['CODCENTCUSNOTAFISCAL'], row['LATITUDE_FILIAL'], 
                row['LONGITUDE_FILIAL'])
            if loadingPlace.CODCENTCUS == 17050 or loadingPlace.CODCENTCUS == 17250:
                loadingPlace.LATITUDE_FILIAL = -23.689984434115285
                loadingPlace.LONGITUDE_FILIAL = -46.60277603494763
                row['LATITUDE_FILIAL'] = -23.689984434115285
                row['LONGITUDE_FILIAL'] = -46.60277603494763
                dfTrips.at[index, 'LATITUDE_FILIAL'] = -23.689984434115285
                dfTrips.at[index, 'LONGITUDE_FILIAL'] = -46.60277603494763
            if loadingPlace.CODCENTCUS == 14050:
                loadingPlace.LATITUDE_FILIAL = -23.152246049659357
                loadingPlace.LONGITUDE_FILIAL = -45.80131066379455
                row['LATITUDE_FILIAL'] = -23.152246049659357
                row['LONGITUDE_FILIAL'] = -45.80131066379455
                dfTrips.at[index, 'LATITUDE_FILIAL'] = -23.152246049659357
                dfTrips.at[index, 'LONGITUDE_FILIAL'] = -45.80131066379455
            if loadingPlace.LATITUDE_FILIAL == 0 or loadingPlace.LONGITUDE_FILIAL == 0 or loadingPlace.LATITUDE_FILIAL == 1e-08 or loadingPlace.LONGITUDE_FILIAL == 1e-08:
                print('Loading Place ' + str(loadingPlace.CODCENTCUS) + ' withou coordinates')
            loadingPlaces.append(loadingPlace)

        mixerTruck = next((mt for mt in mixerTrucks if mt.CODVEICULO == row['CODVEICULO']), None)
        if mixerTruck == None:
            mixerTrucksIndex += 1
            mixerTruck = MixerTruck(mixerTrucksIndex, row['CODVEICULO'], row['CODCENTCUSNOTAFISCAL'], 
                loadingPlace.LATITUDE_FILIAL, loadingPlace.LONGITUDE_FILIAL)
            mixerTrucks.append(mixerTruck)
        dfTrips.at[index, 'MIXERTRUCKINDEX'] = mixerTruck.index
        
        order = next((o for o in orders if o.CODPROGRAMACAO == row['CODPROGRAMACAO']), None)
        if order == None:
            order = Order(row['CODPROGRAMACAO'], row['CODCENTCUSNOTAFISCAL'], row['MEDIA_M3_DESCARGA'], 
                row['VALTOTALPROGRAMACAO'], 
                LATITUDE_OBRA=row['LATITUDE_OBRA'], LONGITUDE_OBRA=row['LONGITUDE_OBRA'], 
                LATITUDE_FILIAL=row['LATITUDE_FILIAL'], LONGITUDE_FILIAL=row['LONGITUDE_FILIAL'])
            orders.append(order)

        delivery = next((v for v in deliveries if v.CODPROGVIAGEM == row['CODPROGVIAGEM']), None)
        if delivery == None:
            constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%y %H:%M %p')
            #constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%Y %H:%M')
            minutes = (constructionTime.hour * 60) + constructionTime.minute
            delivery = Delivery(VLRVENDA=row['VLRVENDA'], HORCHEGADAOBRA = minutes, CODPROGRAMACAO=row['CODPROGRAMACAO'], 
                CODPROGVIAGEM=row['CODPROGVIAGEM'], 
                CODCENTCUSVIAGEM=row['CODCENTCUSVIAGEM'], VLRTOTALNF=row['VLRTOTALNF'], VALVOLUMEPROG=row['VALVOLUMEPROG'], 
                CUSVAR=row['CUSVAR'], CODTRACO=row['CODTRACO'], LATITUDE_OBRA=row['LATITUDE_OBRA'], 
                LONGITUDE_OBRA=row['LONGITUDE_OBRA'])
            if delivery.CUSVAR <= 0:
                delivery.CUSVAR = 150
            
            loadingPlaceLatLong = (float(mixerTruck.LATITUDE_FILIAL), float(mixerTruck.LONGITUDE_FILIAL))
            constructionSiteLatLong = (float(delivery.LATITUDE_OBRA), float(delivery.LONGITUDE_OBRA))
            distance = hs.haversine(loadingPlaceLatLong, constructionSiteLatLong)
            if distance > 200:
                print('ARCHTUNG! PANZER!')
                print('Location 1: ' + str((float(mixerTruck.LATITUDE_FILIAL), float(mixerTruck.LONGITUDE_FILIAL))))
                print('Location 2: '+ str((float(delivery.LATITUDE_OBRA), float(delivery.LONGITUDE_OBRA))))
                print('Distance: '+ str(distance))
            cost = (delivery.CUSVAR * delivery.VALVOLUMEPROG) + (distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST)
            totalProfit += (delivery.VLRVENDA * delivery.VALVOLUMEPROG) - cost

            deliveries.append(delivery)

    totalProfit -= len(mixerTrucks) * FIXED_MIXED_TRUCK_COST

    dfTrips['FINAL'] = ''
    dfTrips['BEGIN'] = ''
    dfTrips['FINAL'] = dfTrips['HORCHEGADACENTRAL'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    dfTrips['BEGIN'] = dfTrips['HORSAIDACENTRAL'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    
    # fig = px.timeline(dfTrips, 
    #     x_start=dfTrips['HORSAIDACENTRAL'], 
    #     x_end=dfTrips['HORCHEGADACENTRAL'], 
    #     y=dfTrips['MIXERTRUCKINDEX'], 
    #     color=dfTrips['CODPROGRAMACAO'], 
    #     hover_data={ 'BEGIN': True, 'FINAL': True, 
    #         'HORSAIDACENTRAL': False, 'HORCHEGADACENTRAL': False, 
    #         'CODPROGRAMACAO': True, 'CODVEICULO': True, 'CODPROGVIAGEM': True },
    #     title='Real: Profit/Loss = ' + str(round(totalProfit, 0)) + ' and Total MT = ' + str(len(mixerTrucks)))
    # fig.update_yaxes(autorange='reversed')
    # fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    # plotly.offline.plot(fig, filename=basePath + '\\RealGant_' + dataFolder + '.html')

    # gmap = gmplot.GoogleMapPlotter(loadingPlaces[0].LATITUDE_FILIAL, loadingPlaces[0].LONGITUDE_FILIAL, 11)

    # for delivery in deliveries:
    #     loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == delivery.CODCENTCUSVIAGEM), None)
    #     gmap.marker(loadingPlace.LATITUDE_FILIAL, loadingPlace.LONGITUDE_FILIAL, color='yellow', title='', label='Loading Place')
    #     gmap.marker(delivery.LATITUDE_OBRA, delivery.LONGITUDE_OBRA, color='cornflowerblue', 
    #         label=str(delivery.CODPROGRAMACAO))
    #     gmap.plot([loadingPlace.LATITUDE_FILIAL, delivery.LATITUDE_OBRA], 
    #         [loadingPlace.LONGITUDE_FILIAL, delivery.LONGITUDE_OBRA],  
    #        'cornflowerblue', edge_width = 2.5)

    # gmap.apikey = googleMapsApiKey
    # gmap.draw(basePath + '\\RealMap_' + dataFolder + '.html')

    return googleMapsApiKey, deliveries, loadingPlaces, mixerTrucks