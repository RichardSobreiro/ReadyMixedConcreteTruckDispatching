int nc = ...;
int np = ...;
int nv = ...;

int max_time = 780;

range N = 1..nc;
range P = 1..np;
range V = 1..nv;

//float cfr[N] = ...;
float t[P][N][N][N] = ...;
float c[P][N][N][N] = ...;

dvar boolean x[P][N][N][N];

minimize sum(p in P,i in N,j in N,k in N) c[p,i,j,k] * x[p,i,j,k];

subject to { 

maximum_time:
	forall(p in P,i in N,j in N,k in N){
		t[p,k,j,i] <= max_time;	
	}

client_assignment: 
	forall(i in N){
		sum(p in P,j in N,k in N) x[p,i,j,k] 
			+ sum(p in P,j in N,k in N: i != j) x[p,j,i,k]
  				+ sum(p in P,j in N,k in N: i != j && k != i) x[p,k,j,i] == 1;	
	}

vehicle_assignment: 
	sum(p in P, i in N,j in N,k in N) x[p,i,j,k] <= nv;
}

tuple Node 
{
	int MixerTruck;
	int LoadigPlaceId;
	int Delivery1;
	int Delivery2;
	int Delivery3;
	float RouteCost;
	float RouteTotalTime;
	/*float LoadingBeginTime1;
	float ServiceTime1;
	float ReturnTime1;
	float LoadingBeginTime2;
	float ServiceTime2;
	float ReturnTime2;
	float LoadingBeginTime3;
	float ServiceTime3;
	float ReturnTime3;*/
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
						Nodes.add(m, p, i, j, k, c[p][i][j][k], t[p][i][j][k]);
						m = m + 1;
					}					
				}						
			}			
		}
	}
	
	for(var node in Nodes)
	{
		writeln("------------------------------------------------------");
		writeln("MixerTruck: ", node.MixerTruck);
		writeln("LoadigPlaceId: ", node.LoadigPlaceId);
		writeln("Delivery1: ", node.Delivery1);
		writeln("Delivery2: ", node.Delivery2);	
		writeln("Delivery3: ", node.Delivery3);	
		writeln("RouteCost: ", node.RouteCost);	
		writeln("RouteTotalTime: ", node.RouteTotalTime);
		writeln("------------------------------------------------------");	
	}

	/*var f = new IloOplOutputFile("C:\\RMCDP\\Result.json");
	f.writeln("{");
	f.writeln("	\"numberOfLoadingPlaces\": ", nLP, ",");
	f.writeln("	\"numberOfMixerTrucks\": ", nMT, ",");
	f.writeln("	\"numberOfDeliveries\": ", nD, ",");
	f.writeln("	\"trips\": [");
	var i = 1;
	for(var viagem in Nodes){
		f.writeln("	{");
		f.writeln("		\"OrderId\": ", viagem.OrderId, ",");
		f.writeln("		\"Delivery\": ", viagem.Delivery, ",");			
		f.writeln("		\"MixerTruck\": ", viagem.MixerTruck, ",");
		f.writeln("		\"LoadingBeginTime\": ", viagem.LoadingBeginTime, ",");
		f.writeln("		\"ServiceTime\": ", viagem.ServiceTime, ",");
		f.writeln(" 	\"ReturnTime\": ", viagem.ReturnTime, ",");
		f.writeln(" 	\"LoadingPlant\": ", viagem.LoadingPlant, ",");
		f.writeln(" 	\"Revenue\": ", viagem.Revenue, ",");	
		f.writeln(" 	\"BeginTimeWindow\": ", viagem.BeginTimeWindow, ",");
		f.writeln(" 	\"EndTimeWindow\": ", viagem.EndTimeWindow, ",");
		f.writeln(" 	\"TravelTime\": ", viagem.TravelTime, ",");
		f.writeln(" 	\"TravelCost\": ", viagem.TravelCost, ",");
		f.writeln(" 	\"DurationOfService\": ", viagem.DurationOfService, ",");
		f.writeln(" 	\"IfDeliveryMustBeServed\": ", viagem.IfDeliveryMustBeServed, ",");
		f.writeln("     \"CodDelivery\": ", viagem.CodDelivery, ",");
		f.writeln("     \"CodOrder\": ", viagem.CodOrder);
		if (i == nD) {
			f.writeln("	}");				
		}
		else{
			f.writeln("	},");	
		}
		i = i + 1;
	}
	f.writeln("]");
	f.writeln("}");	
	f.close();*/
}

