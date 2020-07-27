// FROM: -
// AUTORS: Richard Sobreiro

// RMC = Ready Mixed Concrete - One customer by delivery

int nLP = ...; // Number of loading plants
int nCTM = ...; // Number of concrete truck mixers
int nCT = ...; // Number of customer's trips
int nN = nCT + (2 * nLP); // Number of loading plants plus customer's trips to be realized

range V = 1..nCTM; // Set of concrete truck mixers
range N = 1..nN; // Set of loading plants plus customer's trips to be realized
range R = 1..nCT; // Set of possible routes to be realized by each vehicle

float c[N][N] = ...; // Cost to travel from node i to node j
float t[N][N] = ...; // Duration of the journey between node i and node j

float q = ...; // Concrete truck mixers capacity
float tc[V] = ...;  // Concrete truck mixers fixed cost

float d[N] = ...; // Demand for RMC at customer trip i
float a[N] = ...; // Begin for the time window at customer trip i 
float b[N] = ...; // End for the time window at customer trip i
float cfr[N] = ...; // Concrete flow rate at customer i -- NEW

float p[N] = ...; // Profit obtained from attending customer trip i

int sd[V] = ...; // Start depot for each concrete mixer truck

float M = ...; // Big M for trip i and j

float MjobShop = ...; // Big M for job shop restrictions

dvar boolean x[N][N][V][R]; // If concrete mixer truck k in his route r attend trip j comming from node/trip i
dvar boolean y[V]; // If concrete mixer truck k is used
dvar float s[N][V][R]; // Service time for customer trip i being served by vehicle k is his trip r
dvar float ds[N]; // Duration of the service time at construction site for customer trip i -- NEW

dvar float ld[V][R]; // Loading time duration for the concrete truck mixer k in his route r
dvar float lft[V][R]; // Loading final time for the concrete truck mixer k in his route r
dvar boolean z[V][R][V][R]; // If concrete truck mixer k in his route r loads before concrete truck mixer l in his route s

// 1
maximize sum(k in V)(
	sum(i in N, r in R)(sum(j in N:(i <= (nN - nLP)) && (j > nLP))((p[j]) * x[i][j][k][r])) - 
	(y[k] * tc[k]) - 
	sum(r in R)(sum(i in N)(sum(j in N)(c[i][j] * x[i][j][k][r]))) - 
	sum(i in N, j in N, r in R)(t[i][j] * x[i][j][k][r])
			/*sum(i in N, r in R)(sum(j in N:(i <= (nN - nLP)) && (j > nLP))((p[j] - c[i][j]) * x[i][j][k][r])) - 
			(y[k] * tc[k]) - 
			sum(r in R)(sum(i in N)(sum(j in N: i <= nLP && j > (nN - nLP))(c[i][j] * x[i][j][k][r]))) - 
			sum(i in N, j in N, k in V, r in R)(t[i][j] * x[i][j][k][r])*/
);

subject to {

	// 2 - Each customer's trip can be server only by one vehicle and his respective route
	forall(i in N: i > nLP && i <= (nN - nLP)) {
		sum(r in R, k in V)(sum(j in N: j > nLP)(x[i][j][k][r])) <= 1;	
	}
	// 3 - A vehicle cannot travel from a loading place to the same loading place
	forall(r in R, k in V, i in N: i <= nLP){
		x[i][(nN - i + 1)][k][r] == 0;	
	}
	// 4 - Time must be respected
	forall(r in R, k in V, i in N){
		a[i] <= s[i][k][r] <= b[i];
	}
	// 5 - Loading time for each trip must be respected
	forall(k in V, r in R){
		ld[k][r] == sum(i in N, j in N)(x[i][j][k][r] * d[j]);
	}
	// 6 - Loading final time for each route of each vehicle
	forall(k in V, r in R){
		lft[k][r] >= ld[k][r];	
	}
	// 7/8 - No one concrete truck mixer on his respective route can load at the same time in the same loading place
	forall(r in R, s in R, k in V, l in V: k != l){
		lft[l][s] >= lft[k][r] + ld[l][s] - MjobShop * (1 - z[k][r][l][s]);
		lft[k][r] >= lft[l][s] + ld[k][r] - MjobShop * z[k][r][l][s];  
	}
	// 9 - The capacity for each vehicle on his route must be respected 
	forall(r in R, k in V) {
		sum(i in N:i <= (nN - nLP))(d[i] * (sum(j in N: (i != j) && (j > nLP))(x[i][j][k][r]))) <= q;
	}
	// 10 - If the concrete truck mixer is been used at the period
	forall(r in R, k in V, i in N, j in N: (i != j) && (j <= (nN - nLP))){
		y[k] >= x[i][j][k][r];
	}
	// 11 - The start loading place for each vehicle must be respected 
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nLP && j > nLP) {
		sum(pr in R, ii in N, jj in N: pr < r && ii <= nLP)(x[ii][jj][k][pr]) + (1 - x[i][j][k][r]) >= 1;
	}
	// 12 - If a concrete truck mixer arrives at some customer's trip destiny it must leave from it
	forall(r in R, k in V, h in N: h > nLP && h <= (nN - nLP)){
		sum(i in N: i <= (nN - nLP))(x[i][h][k][r]) - sum(j in N: j > nLP)(x[h][j][k][r]) == 0;
	}
	// 13 - Service time of a preceeding route of each vehicle must be less than or equal the service time of the 
	// routes with a greather index
	forall(r in R, sr in R, k in V, i in N, j in N: r < nCT && i <= nLP && j > (nN - nLP)  && sr > r) {
		s[j][k][r] <= s[i][k][sr];	
	}
	// 14 - The number of times a vehicle arrives at some base in a route must be equal to the number of times 
	// the same vehicle leaves that same base in the next route
	forall(k in V, r in R, j in N: (j > (nN - nLP)) && (r < nCT)) {
		sum(g in N)(x[nN - j + 1][g][k][r+1]) >= sum(h in N)(x[h][j][k][r]);
	}
	// ? - ?
	forall(k in V, r in R, i in N: i <= nLP && (r < nCT)) {
		s[i][k][r+1] >= s[nN - i + 1][k][r] + ld[k][r];
	}
	// 15/16 - Time windows between subsequent customer's trips must be respected
	forall(r in R, k in V, i in N, j in N: (i != j)) {
		if((i <= nLP) && (nLP < j < (nN - nLP))) {
			lft[k][r] >= s[j][k][r] - t[i][j] - M*(1 - x[i][j][k][r]);			
		}
		/*else if((j > (nN - nLP)) && (nLP < i <= (nN - nLP))){
			s[i][k][r] + t[i][j] - M*(1 - x[i][j][k][r]) <= s[j][k][r];
		}
		else if((i <= nLP) && (j > (nN - nLP))) {
			s[i][k][r] + 1 - M*(1 - x[i][j][k][r]) <= s[j][k][r];
		}
		else {*/
			s[i][k][r] + ds[i] + t[i][j] - M*(1 - x[i][j][k][r]) <= s[j][k][r];				
		//}
	}
	// ?? - Duration of the service time at customer i
	forall(i in N) {
		ds[i] == d[i] * cfr[i];
	}
	// 17 - A vehicle must not load at any loading place after himself 
	forall(r in R, s in R, k in V, l in V: k == l){
		z[k][r][l][s] == 0;
	}
	// 18 - A vehicle on his respective route cannot leave some customer's trip destiny an go to the same
	// customer's trip destiny
	forall(r in R, k in V, i in N, j in N: i == j){
		x[i][j][k][r] == 0;
	}
	// 19 - In the first route a vehicle can only left the start loading plant
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nLP && j > nLP) {
		x[i][j][k][1] == 0;
	}
}

tuple Node {
	key int VehicleId;
	key int RouteId;
	key float ServiceTime;
	int CustomerId;
	int OriginId;
	int IsUsed;
};

sorted {Node} Nodes = {};

/*execute 
{
	for(var kk in V)
	{
		for(var rr in R)
		{
			for(var ii in N) 
			{
				for(var jj in N)
				{
					if((x[ii][jj][kk][rr] == 1) && (ii <= nLP)) 
					{	
						writeln("lft[",kk,"][",rr,"] = ", lft[kk][rr]);
						writeln("ld[",kk,"][",rr,"] = ", ld[kk][rr]);
					}			
				}			
			}		
		}
	}
}*/	

execute 
{
	for(var k in V)
	{
		for(var r in R)
		{
			for(var i in N) 
			{
				for(var j in N)
				{
					if(x[i][j][k][r] == 1) 
					{	
						Nodes.add(k, r, s[j][k][r], j, i, y[k]);
					}			
				}			
			}		
		}
	}
	var currentVehicle = -1;
	var currentRoute = -1;
	for(var node in Nodes)
	{
		if(currentVehicle != node.VehicleId)
		{
			currentVehicle = node.VehicleId;
			writeln("--------------------------------------------------------------------------------------");
			if(node.IsUsed > 0){
				writeln("VEHICLE ", currentVehicle);
			}
			else{
				writeln("VEHICLE ", currentVehicle, " IS NOT USED");
			}
			currentRoute = -1;
		}
		if(currentRoute != node.RouteId)
		{
			writeln("-----");
			currentRoute = node.RouteId;
			writeln("ROUTE ", currentRoute);
		}
		if(node.OriginId <= nLP){
			writeln("- From [Base](", node.OriginId, ") To (", node.CustomerId, ")");
		}
		else if (node.CustomerId > (nN - nLP)){
			writeln("- From (", node.OriginId, ") To (", node.CustomerId, ")[Base]");
		}
		else{
			writeln("- From (", node.OriginId, ") To (", node.CustomerId, ")");	
		}
		writeln("  ServiceTime: ", s[node.CustomerId][node.VehicleId][node.RouteId]);
		writeln("  lft[",node.VehicleId,"][",node.RouteId,"] = ", lft[node.VehicleId][node.RouteId]);
		writeln("  ld[",node.VehicleId,"][",node.RouteId,"] = ", ld[node.VehicleId][node.RouteId]);
		writeln("  DemanLP: ", d[node.CustomerId]);
		writeln("  Profit: ", p[node.CustomerId]);
		writeln("  Cost: ", c[node.OriginId][node.CustomerId]);
		writeln("  TravelTime: ", t[node.OriginId][node.CustomerId]);
	}
}