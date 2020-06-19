// FROM: -
// AUTORS: Richard Sobreiro

// RMC = Ready Mixed Concrete

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

float p[N] = ...; // Profit obtained from attending customer trip i

int sd[V] = ...; // Start depot for each concrete mixer truck

float M = ...; // Big M for trip i and j

float MjobShop = ...; // Big M for job shop restrictions

dvar boolean x[N][N][V][R]; // If concrete mixer truck k in his route r attend trip j comming from node/trip i
dvar boolean y[V]; // If concrete mixer truck k is used
dvar float s[N][V][R]; // Service time for customer trip i being served by vehicle k is his trip r

dvar float ld[V][R]; // Loading time duration for the concrete truck mixer k in his route r
dvar float lft[V][R]; // Loading final time for the concrete truck mixer k in his route r
dvar boolean z[V][R][V][R]; // If concrete truck mixer k in his route r loads before concrete truck mixer l in his route s

maximize sum(k in V)(
			sum(i in N, r in R)(sum(j in N:(i <= (nN - nLP)) && (j > nLP))((p[j] - c[i][j]) * x[i][j][k][r])) - 
			(y[k] * tc[k]) - 
			sum(r in R)(sum(i in N)(sum(j in N: i <= nLP && j > (nN - nLP))(c[i][j] * x[i][j][k][r]))) - 
			sum(i in N, j in N, k in V, r in R)(t[i][j] * x[i][j][k][r])
		);

subject to {
	forall(r in R, k in V, i in N, j in N: (i != j)) {
		if(i <= nLP) {
			lft[k][r] >= s[j][k][r] - t[i][j] - M*(1 - x[i][j][k][r]);			
		}
		s[i][k][r] + t[i][j] - M*(1 - x[i][j][k][r]) <= s[j][k][r];
	}

	forall(r in R, s in R, k in V, l in V, i in N: i <= nLP && k != l){
		lft[l][s] >= lft[k][r] + ld[l][s] - MjobShop * (1 - z[k][r][l][s]);
		lft[k][r] >= lft[l][s] + ld[k][r] - MjobShop * z[k][r][l][s];  
	}
	
	forall(r in R, s in R, k in V, l in V: k == l){
		z[k][r][l][s] == 0;
	}
	
	forall(k in V, r in R, i in N: i <= nLP){
		lft[k][r] >= ld[k][r];	
	}
	
	forall(k in V, r in R){
		ld[k][r] == sum(i in N, j in N)(x[i][j][k][r] * d[j]);
	}
//-----
	forall(r in R, k in V, i in N, j in N: i == j){
		x[i][j][k][r] == 0;
	}
	
	forall(r in R, k in V, i in N, j in N: (i != j) && (j <= (nN - nLP))){
		y[k] >= x[i][j][k][r];
	}
	
	forall(r in R, k in V, i in N: i <= nLP){
		x[i][(nN - i + 1)][k][r] == 0;	
	}
	
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nLP && j > nLP) {
		x[i][j][k][1] == 0;
	}
	
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nLP && j > nLP) {
		sum(pr in R, ii in N, jj in N: pr < r && ii <= nLP)(x[ii][jj][k][pr]) + (1 - x[i][j][k][r]) >= 1;
	}
	
	forall(i in N: i > nLP && i <= (nN - nLP)){
		sum(r in R, k in V)(sum(j in N: j > nLP)(x[i][j][k][r])) <= 1;	
	}
	
	forall(r in R, k in V) {
		sum(i in N:i <= (nN - nLP))(d[i] * (sum(j in N: (i != j) && (j > nLP))(x[i][j][k][r]))) <= q;
	}
	
	forall(r in R, k in V, h in N: h > nLP && h <= (nN - nLP)){
		sum(i in N: i <= (nN - nLP))(x[i][h][k][r]) - sum(j in N: j > nLP)(x[h][j][k][r]) == 0;
	}
	
	forall(r in R, k in V, i in N){
		a[i] <= s[i][k][r] <= b[i];
	}
	
	forall(r in R, sr in R, k in V, i in N, j in N: r < nCT && i <= nLP && j > (nN - nLP)  && sr > r) {
		s[j][k][r] <= s[i][k][(sr)];	
	}
	
	forall(k in V, r in R, j in N: (j > (nN - nLP)) && (r < nCT)) {
		sum(g in N)(x[nN - j + 1][g][k][r+1]) >= sum(h in N)(x[h][j][k][r]);
	}
	
}

tuple Node {
	int VehicleId;
	int RouteId;
	float ServiceTime;
	int CustomerId;
	int OriginId;
	int IsUsed;
};

sorted {Node} Nodes = {};

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
					if((x[i][j][k][r] == 1) && (i <= nLP)) 
					{	
						writeln("lft[",k,"][",r,"] = ", lft[k][r]);
						writeln("ld[",k,"][",r,"] = ", ld[k][r]);
					}			
				}			
			}		
		}
	}

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
		writeln("lft[",k,"][",r,"] = ", lft[k][r]);
		writeln("ld[",k,"][",r,"] = ", ld[k][r]);
		writeln("  DemanLP: ", d[node.CustomerId]);
		writeln("  Profit: ", p[node.CustomerId]);
		writeln("  Cost: ", c[node.OriginId][node.CustomerId]);
		writeln("  TravelTime: ", t[node.OriginId][node.CustomerId]);
	}
}
