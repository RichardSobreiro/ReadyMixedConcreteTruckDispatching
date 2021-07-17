int nN = ...; // Number of nodes in map
int nA = ...; // Number of arcs per node
int nI = ...; // Set of depot nodes
int nJ = ...; // Set of customer nodes
int nK = ...; // Number of available vehicles
int nL = ...; // Maximum number of trips each vehicle can perform

range I = 1..nI;
range J = 1..nJ;
range K = 1..nK;
range L = 1..nL;

int codLoadingPlants[I] = ...; 
int codMixerTrucks[K] = ...;  
int codOrders[J] = ...;
int codDeliveries[J] = ...;

//float T = ...; // Durantion of the working day
//float q[I] = ...; // Amount of product i available on each depot
//float d[J] = ...; // Amount of product requested by customer i
float a[J] = ...; // Begin of the time window of customer j
float b[J] = ...; // End of the time window of customer j 

float c[I][J] = ...; // Cost of serving customer j from depot i
float t[I][J] = ...; // Time for serving customer j from depot i
float vold[J] = ...; // Customer j service duration
float cfr[J] = ...; 
float ld = 10;
float MTCOST = 50;
//int u[K][I] = ...; // If truck k is assigned to depot i 

dvar boolean x[I][J][K][L]; // If truck k travels from i to j in its lth trip
dvar boolean u[K][I]; // If truck k is assigned to depot i
dvar boolean mt[K]; // If truck k is used

dvar float s[K][L]; // Start time of trip l of truck k

// 1
minimize sum(i in I, j in J, k in K, l in L)(c[i][j] * x[i][j][k][l]) + sum(k in K)(mt[k] * MTCOST);

subject to{
	// 2
	// 3
	// 4
	forall(k in K, l in L: l < nL){
		s[k][l] + sum(i in I, j in J)(x[i][j][k][l] * ((2 * t[i][j]) + ld + (vold[j] * cfr[j]))) <= s[k][(l+1)];
	}
	// 5
	forall(k in K, l in L){
		s[k][l] + sum(i in I, j in J)((t[i][j] + ld) * x[i][j][k][l]) >= sum(i in I, j in J)(a[j] * x[i][j][k][l]); 	
	}
	// 6
	forall(k in K, l in L){
		s[k][l] + sum(i in I, j in J)((t[i][j] + ld) * x[i][j][k][l]) <= sum(i in I, j in J)(b[j] * x[i][j][k][l]); 	
	}	
	// 7
	forall(i in I, j in J, k in K, l in L){
		x[i][j][k][l] <= u[k][i];	
	}
	// 8
	forall(k in K){
		sum(i in I)(u[k][i]) == 1;	
	}
	// 9
	forall(k in K, l in L){
		sum(i in I, j in J)(x[i][j][k][l]) <= 1;	
	}
	// ?
	forall(j in J){
		sum(i in I, k in K, l in L)(x[i][j][k][l]) == 1;
	}
	forall(i in I, j in J, k in K, l in L){
		x[i][j][k][l] <= mt[k];
	}
	// Non negativite
	forall(k in K, l in L){
		s[k][l] >= 0;
	}
	forall(i in I, j in J, k in K, l in L){
		x[i][j][k][l] >= 0;
	}
	forall(i in I, k in K){
		u[k][i] >= 0;
	}
	forall(k in K){
		mt[k] >= 0;
	}
}

tuple Node 
{
	key int Delivery;
	key int MixerTruck;
	key float LoadingBeginTime;
	float ServiceTime;
	key float ReturnTime;
	int OrderId;
	int LoadingPlant;
	float BeginTimeWindow;
	float EndTimeWindow;
	float TravelTime;
	float TravelCost;
	float DurationOfService;
	int IfDeliveryMustBeServed;
	int CodLoadingPlant;
	int CodMixerTruck;
	int CodDelivery;
	int CodOrder;
};

sorted {Node} Nodes = {};

execute {

	for(var i in I)
	{
		for(var j in J) 
		{
			for(var k in K)
			{
				for(var l in L)
				{
					if(x[i][j][k][l] == 1)
					{
						var Delivery = i;
						var MixerTruck = k;
						var LoadingBeginTime = s[k][l];
						var ServiceTime = s[k][l] + ld + t[i][j]; 
						var ReturnTime = s[k][l] + ld + t[i][j] + (vold[j]*cfr[j]) + t[i][j];
						var OrderId = j;
						var LoadingPlant = codLoadingPlants[i];
						var BeginTimeWindow = a[j];
						var EndTimeWindow = b[j];
						var TravelTime = t[i][j];
						var TravelCost = c[i][j];
						var DurationOfService = (vold[j]*cfr[j]);
						var IfDeliveryMustBeServed = 1;
						var CodLoadingPlant = codLoadingPlants[i];
						var CodMixerTruck = codMixerTrucks[k];
						var CodDelivery = codDeliveries[j];
						var CodOrder = codOrders[j];
						Nodes.add(Delivery, MixerTruck, LoadingBeginTime, ServiceTime, ReturnTime, OrderId, LoadingPlant, Revenue, BeginTimeWindow, EndTimeWindow, TravelTime, TravelCost, DurationOfService, IfDeliveryMustBeServed, CodLoadingPlant, CodMixerTruck, CodDelivery, CodOrder);						
					}								
				}				
			}			
		}
	}
	
	for(var node in Nodes)
	{
		writeln("------------------------------------------------------");
		writeln("MixerTruck: ", node.MixerTruck);
		writeln("OrderId: ", node.OrderId);
		writeln("Delivery: ", node.Delivery);
		writeln("LoadingPlant: ", node.LoadingPlant);	
		writeln("LoadingBeginTime: ", node.LoadingBeginTime);	
		writeln("ServiceTime: ", node.ServiceTime);	
		writeln("ReturnTime: ", node.ReturnTime);
		writeln("BeginTimeWindow: ", node.BeginTimeWindow);
		writeln("EndTimeWindow: ", node.EndTimeWindow);
		writeln("TravelTime: ", node.TravelTime);
		writeln("TravelCost: ", node.TravelCost);
		writeln("DurationOfService: ", node.DurationOfService);
		writeln("IfDeliveryMustBeServed: ", node.IfDeliveryMustBeServed);
		writeln("CodDelivery: ", node.CodDelivery);
		writeln("CodOrder: ", node.CodOrder);
		writeln("------------------------------------------------------");	
	}

	var f = new IloOplOutputFile("C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\PEQUENA - GDE-TIJUCAS-15-06-2019\\CantuFunes.json");
	f.writeln("{");
	f.writeln("	\"numberOfLoadingPlaces\": ", nI, ",");
	f.writeln("	\"numberOfMixerTrucks\": ", nK, ",");
	f.writeln("	\"numberOfDeliveries\": ", nJ, ",");
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
		f.writeln(" 	\"BeginTimeWindow\": ", viagem.BeginTimeWindow, ",");
		f.writeln(" 	\"EndTimeWindow\": ", viagem.EndTimeWindow, ",");
		f.writeln(" 	\"TravelTime\": ", viagem.TravelTime, ",");
		f.writeln(" 	\"TravelCost\": ", viagem.TravelCost, ",");
		f.writeln(" 	\"DurationOfService\": ", viagem.DurationOfService, ",");
		f.writeln(" 	\"IfDeliveryMustBeServed\": ", viagem.IfDeliveryMustBeServed, ",");
		f.writeln("     \"CodDelivery\": ", viagem.CodDelivery, ",");
		f.writeln("     \"CodOrder\": ", viagem.CodOrder);
		if (i == nJ) {
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
