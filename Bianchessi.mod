int nc = ...;
int np = ...;
int nv = ...;

int max_time = 840;

range N = 1..nc;
range P = 1..np;

float tt[P][N] = ...;
float cc[P][N] = ...;
float cfr[N] = ...;
float s[N] = ...;
float t[P][N][N][N] = ...;
float c[P][N][N][N] = ...;

float codLoadingPlants[P] = ...;
float codOrders[N] = ...;
float codDeliveries[N] = ...; 

dvar boolean x[P][N][N][N];
dvar float tus;

minimize sum(p in P,i in N,j in N,k in N)(c[p,i,j,k] * x[p,i,j,k]) + tus * 50;

subject to { 

maximum_time:
	forall(p in P,i in N,j in N,k in N){
		t[p,i,j,k] * x[p,i,j,k] <= max_time;	
	}

client_assignment: 
	forall(i in N){
		sum(p in P,j in N,k in N) x[p,i,j,k] 
			+ sum(p in P,j in N,k in N: i != j) x[p,j,i,k]
  				+ sum(p in P,j in N,k in N: i != j && k != i) x[p,k,j,i] == 1;	
	}

vehicle_assignment: 
	tus == sum(p in P, i in N,j in N,k in N) x[p,i,j,k];
	tus <= nv;
	//sum(p in P, i in N,j in N,k in N) x[p,i,j,k] <= nv;

time_windows:
	forall(p in P, i in N, j in N, k in N){
		if(i != j && i != k && j != k && 
		((s[i] + ((cfr[i] * 8) + tt[p][i])) <= (s[j] - tt[p][j] - 10))  &&
		((s[j] + ((cfr[j] * 8) + tt[p][j])) <= (s[k] - tt[p][k] - 10))){
			x[p][i][j][k] <= 1; 	
		}	
	 	else if (i != j && j == k &&
	 	((s[i] + (cfr[i] * 8) + tt[p][i]) <= (s[j] - tt[p][j] - 10))){
			x[p][i][j][k] <= 1;
		}		
		else if(i == j && j != k && 
		((s[j] + (cfr[j] * 8) + tt[p][j]) <= (s[k] - tt[p][k] - 10))){
			x[p][i][j][k] <= 1;
		}
		else if(i == j && j == k){
			x[p][i][j][k] <= 1;	
		}	
		else{
			x[p][i][j][k] == 0;	
		}
	
		/*if(i != j && i != k && j != k){
			(s[i] + ((cfr[i] * 8) + tt[p][i])) * x[p][i][j][k] <= (s[j] - tt[p][j] - 10) * x[p][i][j][k]; 
			(s[j] + ((cfr[j] * 8) + tt[p][j])) * x[p][i][j][k] <= (s[k] - tt[p][k] - 10) * x[p][i][j][k];		
		}	
	 	else if (i != j && j == k){
			(s[i] + (cfr[i] * 8) + tt[p][i]) * x[p][i][j][k] <= (s[j] - tt[p][j] - 10) * x[p][i][j][k];
		}		
		else if(i == j && j != k){
			(s[j] + (cfr[j] * 8) + tt[p][j]) * x[p][i][j][k] <= (s[k] - tt[p][k] - 10) * x[p][i][j][k];
		}
		
		if((s[i] < s[j] < s[k]) ){
			x[p][i][j][k] <= 1; 	
		}
		else{
			x[p][i][j][k] == 0; 	
		}*/	
	}

nonzero:
	tus >= 0;
	forall(p in P,i in N,j in N,k in N){
		x[p,i,j,k] >= 0;	
	}
	
}

tuple Node 
{
	int MixerTruck;
	int LoadingPlaceId;
	int Delivery1;
	int Delivery2;
	int Delivery3;
	float RouteCost;
	float RouteTotalTime;
	float cfr1;
	float cfr2;
	float cfr3;
	float s1;
	float s2;
	float s3;
	float TravelTime1;
	float TravelTime2;
	float TravelTime3;
	float Cost1;
	float Cost2;
	float Cost3;
	float CodLoadingPlace;
	float CodOrder1;
	float CodOrder2;
	float CodOrder3;
	float CodDelivery1;
	float CodDelivery2;
	float CodDelivery3;
};

sorted {Node} Nodes = {};

execute {

	var m = 1;
	for(var p in P)
	{
		for(var i in N) 
		{
			for(var j in N)
			{
				for(var k in N)
				{
					if(x[p][i][j][k] == 1) 
					{
						Nodes.add(m, p, i, j, k, c[p][i][j][k], t[p][i][j][k], cfr[i], cfr[j], cfr[k], s[i], s[j], s[k], 
							tt[p][i], tt[p][j], tt[p][k], cc[p][i], cc[p][j], cc[p][k], 
							codLoadingPlants[p], 
							codOrders[i], codOrders[j], codOrders[k], 
							codDeliveries[i], codDeliveries[j], codDeliveries[k]);
						m = m + 1;
					}					
				}						
			}			
		}
	}
	var nodesLenght = 0;
	for(var node in Nodes)
	{
		writeln("------------------------------------------------------");
		writeln("MixerTruck: ", node.MixerTruck);
		writeln("LoadigPlaceId: ", node.LoadingPlaceId);
		writeln("Delivery1: ", node.Delivery1);
		writeln("Delivery2: ", node.Delivery2);
		writeln("Delivery3: ", node.Delivery3);
		writeln("RouteCost: ", node.RouteCost);
		writeln("RouteTotalTime: ", node.RouteTotalTime);
		writeln("CustomerFlowRate1: ", node.cfr1);
		writeln("CustomerFlowRate2: ", node.cfr2);
		writeln("CustomerFlowRate3: ", node.cfr3);
		writeln("ServiceTime1: ", node.s1);
		writeln("ServiceTime2: ", node.s2);
		writeln("ServiceTime3: ", node.s3);
		writeln("TravelTime1: ", node.TravelTime1);
		writeln("TravelTime2: ", node.TravelTime2);
		writeln("TravelTime3: ", node.TravelTime3);
		writeln("Cost1: ", node.Cost1);
		writeln("Cost2: ", node.Cost2);
		writeln("Cost3: ", node.Cost3);
		writeln("CodLoadingPlace: ", node.CodLoadingPlace);
		writeln("CodOrder1: ", node.CodOrder1);
		writeln("CodOrder2: ", node.CodOrder2);
		writeln("CodOrder3: ", node.CodOrder3);
		writeln("CodDelivery1: ", node.CodDelivery1);
		writeln("CodDelivery2: ", node.CodDelivery2);
		writeln("CodDelivery3: ", node.CodDelivery3);
		writeln("------------------------------------------------------");
		nodesLenght = nodesLenght + 1;
	}

	var f = new IloOplOutputFile("C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\BH-10-01-2020\\BianchessiResult.json");
	f.writeln("{");
	f.writeln("	\"numberOfLoadingPlaces\": ", np, ",");
	f.writeln("	\"numberOfMixerTrucks\": ", nv, ",");
	f.writeln("	\"numberOfDeliveries\": ", nc, ",");
	f.writeln("	\"routes\": [");
	var i = 1;
	for(var node in Nodes){
		f.writeln("	{");
		f.writeln("		\"MixerTruck\": ", node.MixerTruck, ",");
		f.writeln("		\"LoadigPlaceId\": ", node.LoadingPlaceId, ",");
		f.writeln("		\"Delivery1\": ", node.Delivery1, ",");
		f.writeln("		\"Delivery2\": ", node.Delivery2, ",");
		f.writeln("		\"Delivery3\": ", node.Delivery3, ",");
		f.writeln("		\"RouteCost\": ", node.RouteCost, ",");
		f.writeln("		\"RouteTotalTime\": ", node.RouteTotalTime, ",");
		f.writeln("		\"CustomerFlowRate1\": ", node.cfr1, ",");
		f.writeln("		\"CustomerFlowRate2\": ", node.cfr2, ",");
		f.writeln("		\"CustomerFlowRate3\": ", node.cfr3, ",");
		f.writeln("		\"ServiceTime1\": ", node.s1, ",");
		f.writeln("		\"ServiceTime2\": ", node.s2, ",");
		f.writeln("		\"ServiceTime3\": ", node.s3, ",");
		f.writeln("		\"TravelTime1\": ", node.TravelTime1, ",");
		f.writeln("		\"TravelTime2\": ", node.TravelTime2, ",");
		f.writeln("		\"TravelTime3\": ", node.TravelTime3, ",");
		f.writeln("		\"Cost1\": ", node.Cost1, ",");
		f.writeln("		\"Cost2\": ", node.Cost2, ",");
		f.writeln("		\"Cost3\": ", node.Cost3, ",");
		f.writeln("		\"CodLoadingPlace\": ", node.CodLoadingPlace, ",");
		f.writeln("		\"CodOrder1\": ", node.CodOrder1, ",");
		f.writeln("		\"CodOrder2\": ", node.CodOrder2, ",");
		f.writeln("		\"CodOrder3\": ", node.CodOrder3, ",");
		f.writeln("		\"CodDelivery1\": ", node.CodDelivery1, ",");
		f.writeln("		\"CodDelivery2\": ", node.CodDelivery2, ",");
		f.writeln("		\"CodDelivery3\": ", node.CodDelivery3);
		
		if(i == nodesLenght) {
			f.writeln("	}");				
		}
		else{
			f.writeln("	},");	
		}
		i = i + 1;
	}
	f.writeln("]");
	f.writeln("}");	
	f.close();
}

