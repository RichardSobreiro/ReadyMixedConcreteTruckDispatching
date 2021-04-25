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
float vold[N] = ...;
float t[P][N][N][N][N];
float c[P][N][N][N][N];

float codLoadingPlants[P] = ...;
float codOrders[N] = ...;
float codDeliveries[N] = ...; 

dvar boolean x[P][N][N][N][N];

dvar float tus;
dvar float cs[N];

execute INITIALIZE {
	for (var pp in P){
		for(var ii in N){
			for(var jj in N){
				for(var kk in N){
					for(var ll in N){
						if(ii == jj && jj == kk && kk == ll){
							t[pp][ii][jj][kk][ll] = (2 * tt[pp][ii]) + (cfr[ii] * vold[ii]) + 10;
							c[pp][ii][jj][kk][ll] = cc[pp][ii];			
						}
						else{
							if(ii == jj && jj != kk && kk != ll){
								t[pp][ii][jj][kk][ll] = 2 * (tt[pp][ii] + tt[pp][kk] + tt[pp][ll]) + (cfr[ii] * vold[ii]) + (cfr[kk] * vold[kk]) + (cfr[ll] * vold[ll]) + 30;
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][kk] + cc[pp][ll];			
							}
							else if(ii != jj && jj == kk && kk != ll){
								t[pp][ii][jj][kk][ll] = 2 * (tt[pp][ii] + tt[pp][kk] + tt[pp][ll]) + (cfr[ii] * vold[ii]) + (cfr[kk] * vold[kk]) + (cfr[ll] * vold[ll]) + 30;
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][kk] + cc[pp][ll];														
							}
							else if(ii != jj && jj != kk && kk == ll){
								t[pp][ii][jj][kk][ll] = 2 * (tt[pp][ii] + tt[pp][jj] + tt[pp][ll]) + (cfr[ii] * vold[ii]) + (cfr[jj] * vold[jj]) + (cfr[ll] * vold[ll]) + 30;
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][jj] + cc[pp][ll];						
							}
							else if((ii == jj && jj == kk && kk != ll) ||(ii != jj && jj == kk && kk == ll)){
								t[pp][ii][jj][kk][ll] = 2 * (tt[pp][ii] + tt[pp][ll]) + (cfr[ii] * vold[ii]) + (cfr[ll] * vold[ll]) + 20;			
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][ll];				
							}
							else if(ii == jj && jj != kk && kk == ll){
								t[pp][ii][jj][kk][ll] = 2 * (tt[pp][ii] + tt[pp][ll]) + (cfr[ii] * vold[ii]) + (cfr[ll] * vold[ll]) + 20;			
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][ll];				
							}
							else{
								t[pp][ii][jj][kk][ll] = (2 * tt[pp][ii]) + (2 * tt[pp][jj]) + (2 * tt[pp][kk]) + (2 * tt[pp][ll]) + 
									(cfr[ii] * vold[ii]) + (cfr[jj] * vold[jj]) + (cfr[kk] * vold[kk]) + (cfr[ll] * vold[ll]) + 40;	
								c[pp][ii][jj][kk][ll] = cc[pp][ii] + cc[pp][jj] + cc[pp][kk] + cc[pp][ll];		
							}
						}	
         			}			        
				}
			}
 		}
	}
}

minimize sum(p in P, i in N, j in N, k in N, l in N)(c[p,i,j,k,l] * x[p,i,j,k,l]) + (tus * 50);

subject to {

maximum_time:
	forall(p in P,i in N,j in N,k in N, l in N){
		t[p][i][j][k][l] * x[p][i][j][k][l] <= max_time;	
	}

client_assignment: 
	forall(i in N){
		sum(p in P, j in N, k in N, l in N)(x[p][i][j][k][l]) + 
			sum(p in P, j in N, k in N, l in N: i != j)(x[p][j][i][k][l]) +
				sum(p in P, j in N, k in N, l in N: i != j && i != k)(x[p][j][k][i][l]) +
					sum(p in P, j in N, k in N, l in N: i != j && i != k && i != l)(x[p][j][k][l][i])  == 1;	
	}

vehicle_assignment: 
	tus == sum(p in P, i in N, j in N, k in N, l in N) x[p][i][j][k][l];
	tus <= nv;
	//sum(p in P, i in N,j in N,k in N) x[p,i,j,k] <= nv;

time_windows:
	forall(p in P, i in N, j in N, k in N, l in N){
		if(i != j && i != k && i != l && j != k && j != l && k != l && (s[i] < s[j]) && (s[j] < s[k]) && (s[k] < s[l]))// x x x x 
		{
			x[p][i][j][k][l] <= 1;
			cs[i] + ((cfr[i] * 8) + tt[p][i]) * x[p][i][j][k][l] <= cs[j] - (tt[p][j] + 10) * x[p][i][j][k][l];
			cs[j] + ((cfr[j] * 8) + tt[p][j]) * x[p][i][j][k][l] <= cs[k] - (tt[p][k] + 10) * x[p][i][j][k][l];
			cs[k] + ((cfr[k] * 8) + tt[p][k]) * x[p][i][j][k][l] <= cs[l] - (tt[p][l] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(i == j && k != i && l != i && k != l && (s[j] < s[k] < s[l]))// 1 1 x x
		{
			x[p][i][j][k][l] <= 1;
			cs[j] + ((cfr[j] * 8) + tt[p][j]) * x[p][i][j][k][l] <= cs[k] - (tt[p][k] + 10) * x[p][i][j][k][l];
			cs[k] + ((cfr[k] * 8) + tt[p][k]) * x[p][i][j][k][l] <= cs[l] - (tt[p][l] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(j == k && i != j && l != j && l != i && (s[i] < s[j] < s[l])) // x 1 1 x
		{
			x[p][i][j][k][l] <= 1;
			cs[i] + ((cfr[i] * 8) + tt[p][i]) * x[p][i][j][k][l] <= cs[j] - (tt[p][j] + 10) * x[p][i][j][k][l];
			cs[j] + ((cfr[j] * 8) + tt[p][j]) * x[p][i][j][k][l] <= cs[l] - (tt[p][l] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(k == l && i != k && j != k && i != j && (s[i] < s[j] < s[l])) // x x 1 1
		{
			x[p][i][j][k][l] <= 1;
			cs[i] + (((cfr[i] * 8) + tt[p][i])) * x[p][i][j][k][l] <= cs[j] - (tt[p][j] + 10) * x[p][i][j][k][l];
			cs[j] + (((cfr[j] * 8) + tt[p][j])) * x[p][i][j][k][l] <= cs[k] - (tt[p][k] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(i == j && j == k && k != l && (s[k] < s[l])) // 1 1 1 x
		{
			x[p][i][j][k][l] <= 1;
			cs[k] + ((cfr[k] * 8) + tt[p][k]) * x[p][i][j][k][l] <= cs[l] - (tt[p][l] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(j == k && k == l && i != j && (s[i] < s[j])) // x 1 1 1
		{
			x[p][i][j][k][l] <= 1;
			cs[i] + ((cfr[i] * 8) + tt[p][i]) * x[p][i][j][k][l] <= cs[j] - (tt[p][j] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(i == j && k == l && j != k && (s[j] < s[k])) // 1 1 0 0
		{
			x[p][i][j][k][l] <= 1;
			cs[j] + ((cfr[j] * 8) + tt[p][j]) * x[p][i][j][k][l] <= cs[k] - (tt[p][k] + 10) * x[p][i][j][k][l];
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}
		else if(i == j && j == k && k == l){ // 1 1 1 1 
			x[p][i][j][k][l] <= 1;
			s[i] <= cs[i] <= s[i] + 15;
			s[j] <= cs[j] <= s[j] + 15;
			s[k] <= cs[k] <= s[k] + 15;
			s[l] <= cs[l] <= s[l] + 15;
		}	
		else{
			x[p][i][j][k][l] == 0;	
		}
	}

nonzero:
	tus >= 0;
	forall(p in P,i in N,j in N,k in N, l in N){
		x[p][i][j][k][l] >= 0;	
	}
	forall(i in N){
		cs[i] >= 0;		
	}
}

tuple Node 
{
	int MixerTruck;
	int LoadingPlaceId;
	int Delivery1;
	int Delivery2;
	int Delivery3;
	int Delivery4;
	float RouteCost;
	float RouteTotalTime;
	float cfr1;
	float cfr2;
	float cfr3;
	float cfr4;
	float s1;
	float s2;
	float s3;
	float s4;
	float cs1;
	float cs2;
	float cs3;
	float cs4;
	float TravelTime1;
	float TravelTime2;
	float TravelTime3;
	float TravelTime4;
	float Cost1;
	float Cost2;
	float Cost3;
	float Cost4;
	float CodLoadingPlace;
	float CodOrder1;
	float CodOrder2;
	float CodOrder3;
	float CodOrder4;
	float CodDelivery1;
	float CodDelivery2;
	float CodDelivery3;
	float CodDelivery4;
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
					for(var l in N){
						if(x[p][i][j][k][l] == 1) 
						{
							Nodes.add(m, p, i, j, k, l, 
								c[p][i][j][k][l], t[p][i][j][k][l], 
								cfr[i], cfr[j], cfr[k], cfr[l], 
								s[i], s[j], s[k], s[l],
								cs[i], cs[j], cs[k], cs[l],  
								tt[p][i], tt[p][j], tt[p][k], tt[p][l], 
								cc[p][i], cc[p][j], cc[p][k], cc[p][l], 
								codLoadingPlants[p], 
								codOrders[i], codOrders[j], codOrders[k], codOrders[l], 
								codDeliveries[i], codDeliveries[j], codDeliveries[k], codDeliveries[l]);
							m = m + 1;
						}										
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
		writeln("Delivery4: ", node.Delivery4);
		writeln("RouteCost: ", node.RouteCost);
		writeln("RouteTotalTime: ", node.RouteTotalTime);
		writeln("CustomerFlowRate1: ", node.cfr1);
		writeln("CustomerFlowRate2: ", node.cfr2);
		writeln("CustomerFlowRate3: ", node.cfr3);
		writeln("CustomerFlowRate4: ", node.cfr3);
		writeln("ServiceTime1: ", node.s1);
		writeln("ServiceTime2: ", node.s2);
		writeln("ServiceTime3: ", node.s3);
		writeln("ServiceTime4: ", node.s4);
		writeln("CustomerServiceTime1: ", node.cs1);
		writeln("CustomerServiceTime2: ", node.cs2);
		writeln("CustomerServiceTime3: ", node.cs3);
		writeln("CustomerServiceTime4: ", node.cs4);
		writeln("TravelTime1: ", node.TravelTime1);
		writeln("TravelTime2: ", node.TravelTime2);
		writeln("TravelTime3: ", node.TravelTime3);
		writeln("TravelTime4: ", node.TravelTime4);
		writeln("Cost1: ", node.Cost1);
		writeln("Cost2: ", node.Cost2);
		writeln("Cost3: ", node.Cost3);
		writeln("Cost4: ", node.Cost4);
		writeln("CodLoadingPlace: ", node.CodLoadingPlace);
		writeln("CodOrder1: ", node.CodOrder1);
		writeln("CodOrder2: ", node.CodOrder2);
		writeln("CodOrder3: ", node.CodOrder3);
		writeln("CodOrder4: ", node.CodOrder4);
		writeln("CodDelivery1: ", node.CodDelivery1);
		writeln("CodDelivery2: ", node.CodDelivery2);
		writeln("CodDelivery3: ", node.CodDelivery3);
		writeln("CodDelivery4: ", node.CodDelivery4);
		writeln("------------------------------------------------------");
		nodesLenght = nodesLenght + 1;
	}

	var f = new IloOplOutputFile("C:\\Result.json");
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
		f.writeln("		\"Delivery4\": ", node.Delivery4, ",");
		f.writeln("		\"RouteCost\": ", node.RouteCost, ",");
		f.writeln("		\"RouteTotalTime\": ", node.RouteTotalTime, ",");
		f.writeln("		\"CustomerFlowRate1\": ", node.cfr1, ",");
		f.writeln("		\"CustomerFlowRate2\": ", node.cfr2, ",");
		f.writeln("		\"CustomerFlowRate3\": ", node.cfr3, ",");
		f.writeln("		\"CustomerFlowRate4\": ", node.cfr4, ",");
		f.writeln("		\"ServiceTime1\": ", node.s1, ",");
		f.writeln("		\"ServiceTime2\": ", node.s2, ",");
		f.writeln("		\"ServiceTime3\": ", node.s3, ",");
		f.writeln("		\"ServiceTime4\": ", node.s4, ",");
		f.writeln("		\"CustomerServiceTime1\": ", node.cs1, ",");
		f.writeln("		\"CustomerServiceTime2\": ", node.cs2, ",");
		f.writeln("		\"CustomerServiceTime3\": ", node.cs3, ",");
		f.writeln("		\"CustomerServiceTime4\": ", node.cs4, ",");
		f.writeln("		\"TravelTime1\": ", node.TravelTime1, ",");
		f.writeln("		\"TravelTime2\": ", node.TravelTime2, ",");
		f.writeln("		\"TravelTime3\": ", node.TravelTime3, ",");
		f.writeln("		\"TravelTime4\": ", node.TravelTime4, ",");
		f.writeln("		\"Cost1\": ", node.Cost1, ",");
		f.writeln("		\"Cost2\": ", node.Cost2, ",");
		f.writeln("		\"Cost3\": ", node.Cost3, ",");
		f.writeln("		\"Cost4\": ", node.Cost4, ",");
		f.writeln("		\"CodLoadingPlace\": ", node.CodLoadingPlace, ",");
		f.writeln("		\"CodOrder1\": ", node.CodOrder1, ",");
		f.writeln("		\"CodOrder2\": ", node.CodOrder2, ",");
		f.writeln("		\"CodOrder3\": ", node.CodOrder3, ",");
		f.writeln("		\"CodOrder4\": ", node.CodOrder4, ",");
		f.writeln("		\"CodDelivery1\": ", node.CodDelivery1, ",");
		f.writeln("		\"CodDelivery2\": ", node.CodDelivery2, ",");
		f.writeln("		\"CodDelivery3\": ", node.CodDelivery3, ",");
		f.writeln("		\"CodDelivery4\": ", node.CodDelivery4);
		
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

