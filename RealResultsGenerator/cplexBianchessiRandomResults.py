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

class Trip:  
    def __init__(self, MixerTruck, LoadigPlaceId, DeliveryId, ServiceHour,
        CustomerFlowRate, Cost, TravelTime, DatetimeNow):  
        self.MixerTruck = int(MixerTruck)
        self.LoadigPlaceId = int(LoadigPlaceId)
        self.DeliveryId = int(DeliveryId) 
        self.ServiceHour = int(ServiceHour) 
        self.CustomerFlowRate = int(CustomerFlowRate) 
        self.DurationOfService = self.CustomerFlowRate * 8
        self.Cost = int(Cost) 
        self.TravelTime = int(TravelTime)
        self.DatetimeNow = DatetimeNow
        self.ServiceTime =  DatetimeNow + timedelta(minutes=self.ServiceHour)
        self.LoadBeginTime = self.ServiceTime - timedelta(minutes=self.TravelTime) - timedelta(minutes=10)
        self.ReturnTime = self.ServiceTime + timedelta(minutes=(self.CustomerFlowRate*8)) + timedelta(minutes=self.TravelTime)

def cplexBianchessiRandomResults(basePath, dataFolder, googleMapsApiKey):
    tripsJson = 0
    with open(basePath + '\\ResultBianchessi.json') as data_file:    
        tripsJson = json.load(data_file)
    
    today = datetime.utcnow().date()
    startTime = datetime(today.year, today.month, today.day, 6, 0, 0, 0) 

    np = tripsJson['numberOfLoadingPlaces']
    nc = tripsJson['numberOfDeliveries']
    nv = tripsJson['numberOfMixerTrucks']

    df = pd.DataFrame(tripsJson['routes'])

    trips = []
    for index, row in df.iterrows():
        if row['Delivery1'] != row['Delivery2'] and row['Delivery1'] != row['Delivery3'] and row['Delivery2'] != row['Delivery3']: 
            trip1 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery1'], ServiceHour=row['ServiceTime1'], 
                CustomerFlowRate=row['CustomerFlowRate1'], Cost=row['Cost1'], 
                TravelTime=row['TravelTime1'], DatetimeNow=startTime)
            trip2 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery2'], ServiceHour=row['ServiceTime2'], 
                CustomerFlowRate=row['CustomerFlowRate2'], Cost=row['Cost2'], 
                TravelTime=row['TravelTime2'], DatetimeNow=startTime)
            trip3 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery3'], ServiceHour=row['ServiceTime3'], 
                CustomerFlowRate=row['CustomerFlowRate3'], Cost=row['Cost3'], 
                TravelTime=row['TravelTime3'], DatetimeNow=startTime)
            trips.append(trip1)
            trips.append(trip2)
            trips.append(trip3)
        elif row['Delivery1'] == row['Delivery2'] and row['Delivery1'] != row['Delivery3']: 
            trip1 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery1'], ServiceHour=row['ServiceTime1'], 
                CustomerFlowRate=row['CustomerFlowRate1'], Cost=row['Cost1'], 
                TravelTime=row['TravelTime1'], DatetimeNow=startTime)
            trip3 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery3'], ServiceHour=row['ServiceTime3'], 
                CustomerFlowRate=row['CustomerFlowRate3'], Cost=row['Cost3'], 
                TravelTime=row['TravelTime3'], DatetimeNow=startTime)
            trips.append(trip1)
            trips.append(trip3)
        elif row['Delivery1'] != row['Delivery2'] and row['Delivery2'] == row['Delivery3']: 
            trip1 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery1'], ServiceHour=row['ServiceTime1'], 
                CustomerFlowRate=row['CustomerFlowRate1'], Cost=row['Cost1'], 
                TravelTime=row['TravelTime1'], DatetimeNow=startTime)
            trip2 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery2'], ServiceHour=row['ServiceTime2'], 
                CustomerFlowRate=row['CustomerFlowRate2'], Cost=row['Cost2'], 
                TravelTime=row['TravelTime2'], DatetimeNow=startTime)
            trips.append(trip1)
            trips.append(trip2)
        elif row['Delivery1'] == row['Delivery2'] and row['Delivery1'] == row['Delivery3'] and row['Delivery2'] == row['Delivery3']: 
            trip1 = Trip(MixerTruck=row['MixerTruck'], LoadigPlaceId=row['LoadigPlaceId'], 
                DeliveryId=row['Delivery1'], ServiceHour=row['ServiceTime1'], 
                CustomerFlowRate=row['CustomerFlowRate1'], Cost=row['Cost1'], 
                TravelTime=row['TravelTime1'], DatetimeNow=startTime)
            trips.append(trip1)
        else:
            print('Delivery1: ' + str(row['Delivery1']) + 
                ' Delivery2: ' + str(row['Delivery2']) + 
                ' Delivery3: ' + str(row['Delivery3']))

    df = pd.DataFrame([vars(t) for t in trips])

    df['LoadBeginTime'] = pd.to_datetime(df['LoadBeginTime'])
    df['ReturnTime'] = pd.to_datetime(df['ReturnTime'])
    df['ServiceTime'] = pd.to_datetime(df['ServiceTime'])

    df['FINAL'] = ''
    df['BEGIN'] = ''
    df['Arrival'] = ''
    df['FINAL'] = df['ReturnTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    df['BEGIN'] = df['LoadBeginTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")
    df['Arrival'] = df['ServiceTime'].dt.strftime("%A, %d. %B %Y %I:%M%p")

    fig = px.timeline(df, 
        x_start=df['LoadBeginTime'], 
        x_end=df['ReturnTime'], 
        y=df['MixerTruck'], 
        color=df['DeliveryId'], 
        hover_data={ 'BEGIN': True, 'FINAL': True, 
            'LoadBeginTime': False, 'ReturnTime': False, 
            'ServiceTime': True, 
            'MixerTruck': True,
            'DurationOfService': True, 'Cost': True, 'TravelTime': True, 
            'LoadigPlaceId': True },
        title='BianchessiRandomData')
    fig.update_yaxes(autorange='reversed')
    fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    plotly.offline.plot(fig, filename=basePath + '\\BianchessiRandomDataGant.html')