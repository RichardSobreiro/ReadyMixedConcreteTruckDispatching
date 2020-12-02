import sys
import json
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import plotly_express as px
import plotly

from gmplot import *

from classes import LoadingPlace, MixerTruck, Order, Delivery

def simpleHeuristicResults(fileName, basePath, dataFolder, googleMapsApiKey, deliveries, loadingPlaces):
    tripsJson = 0
    with open(basePath + '\\Result' + fileName + 'Heuristic.json') as data_file:    
        tripsJson = json.load(data_file)
    
    today = datetime.utcnow().date()
    startTime = datetime(today.year, today.month, today.day, 0, 0, 0, 0) 

    df = pd.DataFrame(tripsJson['trips'])

    df['LoadingBeginTime'] = df['LoadingBeginTime'].astype(int)
    df['ReturnTime'] = df['ReturnTime'].astype(int)
    df['ServiceTime'] = df['ServiceTime'].astype(int)

    mixerTrucksCount = []
    mixerTruckIndex = 1
    for index, row in df.iterrows():
        mixerTruck = next((cod for cod in mixerTrucksCount if cod == row['MixerTruck']), None)
        if mixerTruck == None:
            mixerTrucksCount.append(row['MixerTruck'])
            df.at[index, 'MIXERTRUCKINDEX'] = mixerTruckIndex
            mixerTruckIndex += 1
        row['LoadingBeginTime'] = startTime + timedelta(minutes=float(row['LoadingBeginTime']))
        df.at[index, 'LoadingBeginTime'] = row['LoadingBeginTime']
        row['ReturnTime'] = startTime + timedelta(minutes=float(row['ReturnTime']))
        df.at[index, 'ReturnTime'] = row['ReturnTime']
        row['ServiceTime'] = startTime + timedelta(minutes=float(row['ServiceTime']))
        df.at[index, 'ServiceTime'] = row['ServiceTime']

    df['ReturnTime'] = pd.to_datetime(df['ReturnTime'])
    df['LoadingBeginTime'] = pd.to_datetime(df['LoadingBeginTime'])
    df['ServiceTime'] = pd.to_datetime(df['ServiceTime'])

    df['FINAL'] = ''
    df['BEGIN'] = ''
    df['Arrival'] = ''
    df['FINAL'] = df['ReturnTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    df['BEGIN'] = df['LoadingBeginTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    df['Arrival'] = df['ServiceTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")

    fig = px.timeline(df, 
        x_start=df['LoadingBeginTime'], 
        x_end=df['ReturnTime'], 
        y=df['MixerTruck'], 
        color=df['OrderId'], 
        hover_data={ 'BEGIN': True, 'FINAL': True, 
            'LoadingBeginTime': False, 'ReturnTime': False, 
            'CodOrder': True, 'MixerTruck': True, 'CodDelivery': True, 'Lateness': True, 'Arrival': True,
            'DurationOfService': True, 'TravelTime': True, 'LoadingPlant': True },
        title='Haversine: Profit/Loss = '+ str(tripsJson['objective']) + ' and Total MT = ' + str(len(mixerTrucksCount)))
    fig.update_yaxes(autorange='reversed')
    fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    plotly.offline.plot(fig, filename=basePath + '\\SimpleHeuristic' + fileName + 'Gant_' + dataFolder + '.html')

    gmap = gmplot.GoogleMapPlotter(loadingPlaces[0].LATITUDE_FILIAL, loadingPlaces[0].LONGITUDE_FILIAL, 11)

    for index, row in df.iterrows():
        loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == row['LoadingPlant']), None)
        gmap.marker(loadingPlace.LATITUDE_FILIAL, loadingPlace.LONGITUDE_FILIAL, color='yellow', title=str(loadingPlace.CODCENTCUS), 
            label=str(loadingPlace.CODCENTCUS))
        delivery = next((d for d in deliveries if d.CODPROGVIAGEM == row['CodDelivery']), None)
        gmap.marker(delivery.LATITUDE_OBRA, delivery.LONGITUDE_OBRA, color='cornflowerblue', 
            label=str(delivery.CODPROGRAMACAO), title='')
        gmap.plot([loadingPlace.LATITUDE_FILIAL, delivery.LATITUDE_OBRA], 
            [loadingPlace.LONGITUDE_FILIAL, delivery.LONGITUDE_OBRA],  
           'cornflowerblue', edge_width = 2.5)

    gmap.apikey = googleMapsApiKey
    gmap.draw(basePath + '\\SimpleHeuristic' + fileName + 'Map_' + dataFolder + '.html')