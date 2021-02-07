int nA = ...;
int nN = ...; // Number of nodes in map
int nI = ...; // Set of depot nodes
int nJ = ...; // Set of customer nodes
int nK = ...; // Number of available vehicles
int nL = ...; // Maximum number of trips each vehicle can perform

range I = 1..nI;
range J = 1..nJ;
range K = 1..nK;
range L = 1..nL;

int revenues[J] = ...;
int codLoadingPlants[I] = ...; 
int codMixerTrucks[K] = ...;  
int codOrders[J] = ...;
int codDeliveries[J] = ...;

float a[J] = ...; // Begin of the time window of customer j
float b[J] = ...; // End of the time window of customer j 

float c[I][J] = ...; // Cost of serving customer j from depot i
float t[I][J] = ...; // Time for serving customer j from depot i
float csd[J] = ...; // Customer j service duration
float ld = 4;
float MTCOST = 50;

dvar boolean x[K][I][J][J][J][J]; // If truck k is assigned to depot i and serves trip j1, j2, j3 and j4
dvar boolean u[K][I]; // If truck k is assigned to depot i
dvar boolean z[J][I]; // If trip j is assigned to depot i
dvar boolean mt[K]; // If truck k is used
dvar float s[J][K]; // Start time of trip j served by truck k
//dvar float w[J][J]; // Waiting time between trips

// 1
minimize sum(i in I, j in J)(c[i][j] * z[j][i]) + sum(k in K)(mt[k] * MTCOST);// + 1000 * sum(j1 in J, j2 in J)(w[j1][j2]);

subject to{
	// X - If truck k is assigned to depot i
	forall(k in K, i in I, j1 in J, j2 in J, j3 in J, j4 in J){
		x[k][i][j1][j2][j4][j3] <= u[k][i];	
	}
	// X - A truck k must be assigned to exactly one depot i
	forall(k in K){
		sum(i in I)(u[k][i]) == 1;	
	}
	// X - If trip j is assigned to depot i
	forall(k in K, i in I, j1 in J, j2 in J, j3 in J, j4 in J){
		x[k][i][j1][j2][j3][j4] <= z[j1][i];
		x[k][i][j1][j2][j3][j4] <= z[j2][i];	
		x[k][i][j1][j2][j3][j4] <= z[j3][i];
		x[k][i][j1][j2][j3][j4] <= z[j4][i];
	} 
	// X - A trip can only be served by one truck k and one depot i at any position on the journey
	forall(j in J: j != 1){
		sum(k in K, i in I, j2 in J, j3 in J, j4 in J: j != j2 && j != j3 && j != j4)(x[k][i][j][j2][j3][j4]) <= 1;
		sum(k in K, i in I, j1 in J, j3 in J, j4 in J: j != j1 && j != j3 && j != j4)(x[k][i][j1][j][j3][j4]) <= 1;
		sum(k in K, i in I, j1 in J, j2 in J, j4 in J: j != j1 && j != j2 && j != j4)(x[k][i][j1][j2][j][j4]) <= 1;
		sum(k in K, i in I, j1 in J, j2 in J, j3 in J: j != j1 && j != j2 && j != j3)(x[k][i][j1][j2][j3][j]) <= 1;
	} 
	// X - A trip must be at one position on the journey of any truck k once
	/*forall(j in J: j != 1){
		sum(k in K, i in I, j2 in J, j3 in J, j4 in J: j != j2 && j != j3 && j != j4)(x[k][i][j][j2][j3][j4]) +
			sum(k in K, i in I, j1 in J, j3 in J, j4 in J: j != j1 && j != j3 && j != j4)(x[k][i][j1][j][j3][j4]) + 
		 		sum(k in K, i in I, j1 in J, j2 in J, j4 in J: j != j1 && j != j2 && j != j4)(x[k][i][j1][j2][j][j4]) + 
		 			sum(k in K, i in I, j1 in J, j2 in J, j3 in J: j != j1 && j != j2 && j != j3)(x[k][i][j1][j2][j3][j]) == 1;
	}*/
	forall(j in J: j != 1){
		sum(i in I)(z[j][i]) == 1;
	}
	// X - If truck k is used
	forall(k in K, i in I, j1 in J, j2 in J, j3 in J, j4 in J){
		x[k][i][j1][j2][j4][j3] <= mt[k];
	}
	// X - Time windows must b respected
	forall(k in K, i in I, j1 in J, j2 in J, j3 in J, j4 in J: j1 != 1 && j2 != 1 && j3 != 1 && j4 != 1){
		s[j1][k] + (x[k][i][j1][j2][j3][j4] * ((t[i][j1]) * 2) + ld + csd[j1]) <= s[j2][k];
		s[j2][k] + (x[k][i][j1][j2][j3][j4] * ((t[i][j2]) * 2) + ld + csd[j2]) <= s[j3][k];
		s[j3][k] + (x[k][i][j1][j2][j3][j4] * ((t[i][j3]) * 2) + ld + csd[j3]) <= s[j4][k];
		
		s[j1][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j1]) + ld) >= x[k][i][j1][j2][j3][j4] * a[j1];
		s[j1][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j1]) + ld) <= x[k][i][j1][j2][j3][j4] * b[j1];
		
		s[j2][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j2]) + ld) >= x[k][i][j1][j2][j3][j4] * a[j2];
		s[j2][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j2]) + ld) <= x[k][i][j1][j2][j3][j4] * b[j2];
		
		s[j3][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j3]) + ld) >= x[k][i][j1][j2][j3][j4] * a[j3];
		s[j3][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j3]) + ld) <= x[k][i][j1][j2][j3][j4] * b[j3];
		
		s[j4][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j4]) + ld) >= x[k][i][j1][j2][j3][j4] * a[j4];
		s[j4][k] + (x[k][i][j1][j2][j3][j4] * (t[i][j4]) + ld) <= x[k][i][j1][j2][j3][j4] * b[j4];
	}
	// X - 
	forall(k in K, j in J){
		s[j][k] >= 0;
	}
	
	/*forall(k in K, i in I, j1 in J, j2 in J, j3 in J, j4 in J){
		x[k][i][j1][j2][j3][j4] >= 0;
	}
	forall(k in K, i in I){
		u[k][i] >= 0;	
	}
	forall(j in J, i in I){
		z[j][i] >= 0;
	}	
	forall(k in K){
		mt[k] >= 0;
	}*/
}

tuple Node 
{
	key int Delivery;
	key int MixerTruck;
	key float LoadingBeginTime;
	float ServiceTime;
	key float ReturnTime;
	int LoadingPlant;
	float Revenue;
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

	for(var k in K)
	{
		for(var i in I) 
		{
			for(var j1 in J)
			{
				for(var j2 in J)
				{
					for(var j3 in J)
					{
						for(var j4 in J)
						{
							if(x[k][i][j1][j2][j3][j4] == 1)
							{
								var MixerTruck = k;
								var LoadingPlant = i;
								var Delivery = j1;
								var LoadingBeginTime = s[j1][k];
								var ServiceTime = s[j1][k] + ld + t[i][j1]; 
								var ReturnTime = s[j1][k] + ld + t[i][j1] + csd[j1] + t[i][j1];
								var Revenue = revenues[j1];
								var BeginTimeWindow = a[j1];
								var EndTimeWindow = b[j1];
								var TravelTime = t[i][j1];
								var TravelCost = c[i][j1];
								var DurationOfService = csd[j1];
								var IfDeliveryMustBeServed = 1;
								var CodLoadingPlant = codLoadingPlants[i];
								var CodMixerTruck = codMixerTrucks[k];
								var CodDelivery = codDeliveries[j1];
								var CodOrder = codOrders[j1];
								Nodes.add(Delivery, MixerTruck, LoadingBeginTime, ServiceTime, ReturnTime, LoadingPlant, Revenue, BeginTimeWindow, EndTimeWindow, TravelTime, TravelCost, DurationOfService, IfDeliveryMustBeServed, CodLoadingPlant, CodMixerTruck, CodDelivery, CodOrder);
								
								var MixerTruck = k;
								var LoadingPlant = i;
								var Delivery = j2;
								var LoadingBeginTime = s[j2][k];
								var ServiceTime = s[j2][k] + ld + t[i][j2]; 
								var ReturnTime = s[j2][k] + ld + t[i][j2] + csd[j2] + t[i][j2];
								var Revenue = revenues[j2];
								var BeginTimeWindow = a[j2];
								var EndTimeWindow = b[j2];
								var TravelTime = t[i][j2];
								var TravelCost = c[i][j2];
								var DurationOfService = csd[j2];
								var IfDeliveryMustBeServed = 1;
								var CodLoadingPlant = codLoadingPlants[i];
								var CodMixerTruck = codMixerTrucks[k];
								var CodDelivery = codDeliveries[j2];
								var CodOrder = codOrders[j2];
								Nodes.add(Delivery, MixerTruck, LoadingBeginTime, ServiceTime, ReturnTime, LoadingPlant, Revenue, BeginTimeWindow, EndTimeWindow, TravelTime, TravelCost, DurationOfService, IfDeliveryMustBeServed, CodLoadingPlant, CodMixerTruck, CodDelivery, CodOrder);
								
								var MixerTruck = k;
								var LoadingPlant = i;
								var Delivery = j3;
								var LoadingBeginTime = s[j3][k];
								var ServiceTime = s[j3][k] + ld + t[i][j3]; 
								var ReturnTime = s[j3][k] + ld + t[i][j3] + csd[j3] + t[i][j3];
								var Revenue = revenues[j3];
								var BeginTimeWindow = a[j3];
								var EndTimeWindow = b[j3];
								var TravelTime = t[i][j3];
								var TravelCost = c[i][j3];
								var DurationOfService = csd[j3];
								var IfDeliveryMustBeServed = 1;
								var CodLoadingPlant = codLoadingPlants[i];
								var CodMixerTruck = codMixerTrucks[k];
								var CodDelivery = codDeliveries[j3];
								var CodOrder = codOrders[j3];
								Nodes.add(Delivery, MixerTruck, LoadingBeginTime, ServiceTime, ReturnTime, LoadingPlant, Revenue, BeginTimeWindow, EndTimeWindow, TravelTime, TravelCost, DurationOfService, IfDeliveryMustBeServed, CodLoadingPlant, CodMixerTruck, CodDelivery, CodOrder);
								
								var MixerTruck = k;
								var LoadingPlant = i;
								var Delivery = j4;
								var LoadingBeginTime = s[j4][k];
								var ServiceTime = s[j4][k] + ld + t[i][j4]; 
								var ReturnTime = s[j4][k] + ld + t[i][j4] + csd[j4] + t[i][j4];
								var Revenue = revenues[j4];
								var BeginTimeWindow = a[j4];
								var EndTimeWindow = b[j4];
								var TravelTime = t[i][j4];
								var TravelCost = c[i][j4];
								var DurationOfService = csd[j4];
								var IfDeliveryMustBeServed = 1;
								var CodLoadingPlant = codLoadingPlants[i];
								var CodMixerTruck = codMixerTrucks[k];
								var CodDelivery = codDeliveries[j4];
								var CodOrder = codOrders[j4];
								Nodes.add(Delivery, MixerTruck, LoadingBeginTime, ServiceTime, ReturnTime, LoadingPlant, Revenue, BeginTimeWindow, EndTimeWindow, TravelTime, TravelCost, DurationOfService, IfDeliveryMustBeServed, CodLoadingPlant, CodMixerTruck, CodDelivery, CodOrder);						
							}								
						}
  					}
    			}  												
			}			
		}
	}
	
	for(var node in Nodes)
	{
		writeln("------------------------------------------------------");
		writeln("MixerTruck: ", node.MixerTruck);
		writeln("Delivery: ", node.Delivery);
		writeln("LoadingPlant: ", node.LoadingPlant);	
		writeln("LoadingBeginTime: ", node.LoadingBeginTime);	
		writeln("ServiceTime: ", node.ServiceTime);	
		writeln("ReturnTime: ", node.ReturnTime);
		writeln("Revenue: ", node.Revenue);
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

	var f = new IloOplOutputFile("C:\\RMCDP\\ResultCombinations.json");
	f.writeln("{");
	f.writeln("	\"numberOfLoadingPlaces\": ", nI, ",");
	f.writeln("	\"numberOfMixerTrucks\": ", nK, ",");
	f.writeln("	\"numberOfDeliveries\": ", nJ, ",");
	f.writeln("	\"trips\": [");
	var i = 1;
	for(var viagem in Nodes){
		f.writeln("	{");
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