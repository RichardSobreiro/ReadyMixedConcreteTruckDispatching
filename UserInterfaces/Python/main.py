import sys
import json
import plotly_express as px
import plotly
import pandas as pd
from datetime import datetime, timedelta

def main(argv):
    tripsJson = 0
    with open('C:\\RMCDP\\Result.json') as data_file:    
        tripsJson = json.load(data_file)
    
    today = datetime.utcnow().date()
    startTime = datetime(today.year, today.month, today.day, 0, 0, 0, 0) 

    df = pd.DataFrame(tripsJson['trips'])

    df['LoadingBeginTime'] = df['LoadingBeginTime'].astype(int)
    df['ReturnTime'] = df['ReturnTime'].astype(int)

    df.loc[df['LoadingBeginTime'] < 0, 'LoadingBeginTime'] = (df['ServiceTime'] - 20)
    
    for index, row in df.iterrows():
        row['LoadingBeginTime'] = startTime + timedelta(minutes=row['LoadingBeginTime'])
        df.at[index, 'LoadingBeginTime'] = row['LoadingBeginTime']
        row['ReturnTime'] = startTime + timedelta(minutes=row['ReturnTime'])
        df.at[index, 'ReturnTime'] = row['ReturnTime']

    df['ReturnTime'] = pd.to_datetime(df['ReturnTime'])
    df['LoadingBeginTime'] = pd.to_datetime(df['LoadingBeginTime'])

    df.info()

    fig = px.timeline(df, 
        x_start=df['LoadingBeginTime'], 
        x_end=df['ReturnTime'], 
        y=df['MixerTruck'], 
        color=df['OrderId'], 
        #hover_data=['LoadingBeginTime','ReturnTime','MixerTruck'],
        #hover_data=df.columns,
        title="Trips Overview")
    fig.update_yaxes(autorange='reversed')
    fig.update_layout(title_font_size=42, font_size=18, title_font_family='Arial')
    plotly.offline.plot(fig, filename='TripsOverviewGant.html')


if __name__ == '__main__':
    main(sys.argv[1:])