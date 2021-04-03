class LoadingPlace:  
    def __init__(self, index, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 
        self.DISTANCE = 0
        self.TRAVELTIME = 0

class MixerTruck:  
    def __init__(self, index, CODVEICULO, CODCENTCUS, LATITUDE_FILIAL, LONGITUDE_FILIAL):  
        self.index = index
        self.CODVEICULO = CODVEICULO 
        self.CODCENTCUS = CODCENTCUS 
        self.LATITUDE_FILIAL = LATITUDE_FILIAL 
        self.LONGITUDE_FILIAL = LONGITUDE_FILIAL 

class Order:  
    def __init__(self, CODPROGRAMACAO, CODCENTCUS, MEDIA_M3_DESCARGA, VALTOTALPROGRAMACAO, 
        HORSAIDACENTRAL, LATITUDE_OBRA, LONGITUDE_OBRA, VLRVENDA):  
        self.CODPROGRAMACAO = CODPROGRAMACAO 
        self.CODCENTCUS = CODCENTCUS 
        self.MEDIA_M3_DESCARGA = MEDIA_M3_DESCARGA 
        self.VALTOTALPROGRAMACAO = VALTOTALPROGRAMACAO 
        self.HORSAIDACENTRAL = HORSAIDACENTRAL
        self.LATITUDE_OBRA = LATITUDE_OBRA
        self.LONGITUDE_OBRA = LONGITUDE_OBRA
        self.VLRVENDA = VLRVENDA
        self.TRIPS = []
        self.LOADINGPLACES_INFO = []

class Delivery:  
    def __init__(self, HORCHEGADAOBRA, CODPROGRAMACAO, CODPROGVIAGEM, CODCENTCUSVIAGEM, VLRTOTALNF, 
        VALVOLUMEPROG, CUSVAR, CODTRACO, LATITUDE_OBRA, LONGITUDE_OBRA, VLRVENDA):  
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
        self.VLRVENDA = VLRVENDA
        self.LOADINGPLACESINFO = []
        self.TotalTripCost = 0

class DirectionResult:
    Distance = ''
    TravelTime = ''
    OriginLatitude = ''
    OriginLongitude = ''
    DestinyLatitude = ''
    DestinyLongitude = ''
    Hour = ''
    TimeString = ''
    Result = ''
    RealResult = True