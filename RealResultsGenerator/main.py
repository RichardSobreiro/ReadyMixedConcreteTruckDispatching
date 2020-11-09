import sys
import pandas as pd
import numpy as np
import haversine as hs
from datetime import datetime, timedelta
import plotly_express as px
import plotly

class LoadingPlace:  
    def __init__(self, index, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 

class MixerTruck:  
    def __init__(self, index, CODVEICULO, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODVEICULO = CODVEICULO 
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 

class Order:  
    def __init__(self, CODPROGRAMACAO, CODCENTCUS, MEDIA_M3_DESCARGA, VALTOTALPROGRAMACAO, 
        LATITUDE_OBRA, LONGITUDE_OBRA, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.CODPROGRAMACAO = CODPROGRAMACAO 
        self.CODCENTCUS = CODCENTCUS 
        self.MEDIA_M3_DESCARGA = MEDIA_M3_DESCARGA 
        self.VALTOTALPROGRAMACAO = VALTOTALPROGRAMACAO 
        self.LATITUDE_OBRA = LATITUDE_OBRA
        self.LONGITUDE_OBRA = LONGITUDE_OBRA
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 

class Delivery:  
    def __init__(self, VLRVENDA, HORCHEGADAOBRA, CODPROGRAMACAO, CODPROGVIAGEM, CODCENTCUSVIAGEM, 
        VLRTOTALNF, VALVOLUMEPROG, CUSVAR, CODTRACO, LATITUDE_OBRA, LONGITUDE_OBRA):  
        self.VLRVENDA = VLRVENDA
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

def main(argv):
    # G = ox.graph_from_place('wien flughafen austria')
    # ox.save_graph_xml(G, filepath='./osm/test.osm')

    dataFolder = 'RJ-13-06-2019'
    basePath = 'C:\\Users\\Richard Sobreiro\\Desktop\\' + dataFolder
    DEFAULT_DIESEL_COST = 3.5
    FIXED_L_PER_KM = 27.5/100

    # dfLoadingPlaces = pd.read_csv(basePath + '\\LoadingPlaces.csv')
    # print(dfLoadingPlaces)

    dfTrips = pd.read_csv(basePath + '\\Trips.csv', encoding = "ISO-8859-1")
    dfTrips = dfTrips.sort_values('CODPROGRAMACAO')
    #print(dfTrips)

    dfTrips['HORSAIDACENTRAL'] = pd.to_datetime(dfTrips['HORSAIDACENTRAL'])
    dfTrips['HORCHEGADACENTRAL'] = pd.to_datetime(dfTrips['HORCHEGADACENTRAL'])

    dfTrips['MIXERTRUCKINDEX'] = 0

    loadingPlaces = []
    loadingPlacesIndex = 0
    mixerTrucks = []
    mixerTrucksIndex = 0
    orders = []
    deliveries = []
    today = datetime.utcnow().date()
    startTime = datetime(today.year, today.month, today.day, 0, 0, 0, 0) 
    for index, row in dfTrips.iterrows():
        loadingPlace = next((lp for lp in loadingPlaces if lp.CODCENTCUS == row['CODCENTCUSNOTAFISCAL']), None)
        if loadingPlace == None:
            loadingPlacesIndex += 1
            loadingPlace = LoadingPlace(loadingPlacesIndex, row['CODCENTCUSNOTAFISCAL'], row['LATITUDE_FILIAL'], row['LONGITUDE_FILIAL'])
            loadingPlaces.append(loadingPlace)

        mixerTruck = next((mt for mt in mixerTrucks if mt.CODVEICULO == row['CODVEICULO']), None)
        if mixerTruck == None:
            mixerTrucksIndex += 1
            mixerTruck = MixerTruck(mixerTrucksIndex, row['CODVEICULO'], row['CODCENTCUSNOTAFISCAL'], row['LATITUDE_FILIAL'], row['LONGITUDE_FILIAL'])
            mixerTrucks.append(mixerTruck)
        dfTrips.at[index, 'MIXERTRUCKINDEX'] = mixerTruck.index
        
        order = next((o for o in orders if o.CODPROGRAMACAO == row['CODPROGRAMACAO']), None)
        if order == None:
            order = Order(row['CODPROGRAMACAO'], row['CODCENTCUSNOTAFISCAL'], row['MEDIA_M3_DESCARGA'], row['VALTOTALPROGRAMACAO'], 
                LATITUDE_OBRA=row['LATITUDE_OBRA'], LONGITUDE_OBRA=row['LONGITUDE_OBRA'], 
                LATITUDE_FILIAL=row['LATITUDE_FILIAL'], LONGITUDE_FILIAL=row['LONGITUDE_FILIAL'])
            orders.append(order)

        delivery = next((v for v in deliveries if v.CODPROGVIAGEM == row['CODPROGVIAGEM']), None)
        if delivery == None:
            constructionTime = datetime.strptime(row['HORCHEGADAOBRA'], '%m/%d/%Y %H:%M')
            minutes = (constructionTime.hour * 60) + constructionTime.minute
            delivery = Delivery(VLRVENDA=row['VLRVENDA'], HORCHEGADAOBRA = minutes, CODPROGRAMACAO=row['CODPROGRAMACAO'], 
                CODPROGVIAGEM=row['CODPROGVIAGEM'], 
                CODCENTCUSVIAGEM=row['CODCENTCUSVIAGEM'], VLRTOTALNF=row['VLRTOTALNF'], VALVOLUMEPROG=row['VALVOLUMEPROG'], 
                CUSVAR=row['CUSVAR'], CODTRACO=row['CODTRACO'], LATITUDE_OBRA=row['LATITUDE_OBRA'], 
                LONGITUDE_OBRA=row['LONGITUDE_OBRA'])
            if delivery.CUSVAR <= 0:
                delivery.CUSVAR = 150
            deliveries.append(delivery)

    totalProfit = 0
    i = 0
    for mt in mixerTrucks:
        j = 0
        for dl in deliveries:
            loadingPlaceLatLong = (float(mt.LATITUDE_FILIAL), float(mt.LONGITUDE_FILIAL))
            constructionSiteLatLong = (float(dl.LATITUDE_OBRA), float(dl.LONGITUDE_OBRA))
            distance = hs.haversine(loadingPlaceLatLong, constructionSiteLatLong)
            if distance > 200:
                print('ARCHTUNG! PANZER!')
                print('Location 1: ' + str((float(mt.LATITUDE_FILIAL), float(mt.LONGITUDE_FILIAL))))
                print('Location 2: '+ str((float(dl.LATITUDE_OBRA), float(dl.LONGITUDE_OBRA))))
                print('Distance: '+ str(distance))
            cost = dl.CUSVAR + (distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST)
            totalProfit += (dl.VLRVENDA - cost) * dl.VALVOLUMEPROG
            j += 1
        i += 1

    dfTrips['FINAL'] = ''
    dfTrips['BEGIN'] = ''
    dfTrips['FINAL'] = dfTrips['HORCHEGADACENTRAL'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    dfTrips['BEGIN'] = dfTrips['HORSAIDACENTRAL'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    
    fig = px.timeline(dfTrips, 
        x_start=dfTrips['HORSAIDACENTRAL'], 
        x_end=dfTrips['HORCHEGADACENTRAL'], 
        y=dfTrips['MIXERTRUCKINDEX'], 
        color=dfTrips['CODPROGRAMACAO'], 
        #hover_name='CODPROGVIAGEM',
        #hover_data=['CODVEICULO', 'CODPROGRAMACAO', 'HORSAIDACENTRAL','HORCHEGADACENTRAL'],
        #hover_data={ 'HORSAIDACENTRAL': '|%A, %d. %B %Y %I:%M%p', 'FINAL': True, 'HORCHEGADACENTRAL': False, 'CODPROGRAMACAO': True, 'CODVEICULO': True },
        hover_data={ 'BEGIN': True, 'FINAL': True, 
            'HORSAIDACENTRAL': False, 'HORCHEGADACENTRAL': False, 
            'CODPROGRAMACAO': True, 'CODVEICULO': True, 'CODPROGVIAGEM': True },
        title="Profit/Loss = " + str(totalProfit) + ' and Total MT = ' + str(len(mixerTrucks)))
    fig.update_yaxes(autorange='reversed')
    fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    plotly.offline.plot(fig, filename='TripsOverviewGantReal.html')

if __name__ == '__main__':
    main(sys.argv[1:])

