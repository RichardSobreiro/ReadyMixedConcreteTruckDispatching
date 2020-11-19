import sys
import json
import plotly_express as px
import plotly
import pandas as pd
from datetime import datetime, timedelta
from gmplot import *

def main(argv):
    dataFolder = 'BH-10-01-2020'
    basePath = 'C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\' + dataFolder 
    tripsJson = 0
    with open(basePath + '\\ResultHaversine.json') as data_file:    
        tripsJson = json.load(data_file)
    
    today = datetime.utcnow().date()
    startTime = datetime(today.year, today.month, today.day, 0, 0, 0, 0) 

    df = pd.DataFrame(tripsJson['trips'])

    df['LoadingBeginTime'] = df['LoadingBeginTime'].astype(int)
    df['ReturnTime'] = df['ReturnTime'].astype(int)

    df.loc[df['LoadingBeginTime'] < 0, 'LoadingBeginTime'] = (df['LoadingBeginTime'] - 20)
    
    mixerTrucks = []
    for index, row in df.iterrows():
        mixerTruck = next((cod for cod in mixerTrucks if cod == row['MixerTruck']), None)
        if mixerTruck == None:
            mixerTrucks.append(row['MixerTruck'])
        row['LoadingBeginTime'] = startTime + timedelta(minutes=row['LoadingBeginTime'])
        df.at[index, 'LoadingBeginTime'] = row['LoadingBeginTime']
        row['ReturnTime'] = startTime + timedelta(minutes=row['ReturnTime'])
        df.at[index, 'ReturnTime'] = row['ReturnTime']

    df['ReturnTime'] = pd.to_datetime(df['ReturnTime'])
    df['LoadingBeginTime'] = pd.to_datetime(df['LoadingBeginTime'])

    df['FINAL'] = ''
    df['BEGIN'] = ''
    df['FINAL'] = df['ReturnTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    df['BEGIN'] = df['LoadingBeginTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")

    fig = px.timeline(df, 
        x_start=df['LoadingBeginTime'], 
        x_end=df['ReturnTime'], 
        y=df['MixerTruck'], 
        color=df['OrderId'], 
        hover_data={ 'BEGIN': True, 'FINAL': True, 
            'LoadingBeginTime': False, 'ReturnTime': False, 
            'CodOrder': True, 'MixerTruck': True, 'CodDelivery': True },
        title='Profit/Loss = 79438 and Total MT = ' + str(len(mixerTrucks)))
    fig.update_yaxes(autorange='reversed')
    fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    plotly.offline.plot(fig, filename=basePath + '\\HaversineGant_' + dataFolder + '.html')

    googleApiKeyPathFile = 'C:\GoogleApiKey\key.txt'
    fileGmapsKey = open(googleApiKeyPathFile, 'r') 
    lines = fileGmapsKey.readlines()
    googleMapsApiKey = lines[0]

    gmap = gmplot.GoogleMapPlotter(loadingPlaces[0].LATITUDE_FILIAL, loadingPlaces[0].LONGITUDE_FILIAL, 11)

    for delivery in deliveries:
        loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == delivery.CODCENTCUSVIAGEM), None)
        gmap.marker(loadingPlace.LATITUDE_FILIAL, loadingPlace.LONGITUDE_FILIAL, color='yellow', title='', 
            label='Loading Place')
        gmap.marker(delivery.LATITUDE_OBRA, delivery.LONGITUDE_OBRA, color='cornflowerblue', 
            label=str(delivery.CODPROGRAMACAO))
        gmap.plot([loadingPlace.LATITUDE_FILIAL, delivery.LATITUDE_OBRA], 
            [loadingPlace.LONGITUDE_FILIAL, delivery.LONGITUDE_OBRA],  
           'cornflowerblue', edge_width = 2.5)

    gmap.apikey = googleMapsApiKey
    gmap.draw(basePath + '\\RealMapHaversine_' + dataFolder + '.html')


if __name__ == '__main__':
    main(sys.argv[1:])