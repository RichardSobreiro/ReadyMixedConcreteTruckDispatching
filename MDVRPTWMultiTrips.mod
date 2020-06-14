// FROM: -
// AUTORS: Richard Sobreiro

int nD = ...;
int nV = ...;
int nC = ...;
int nN = nC + (2 * nD);

range V = 1..nV;
range N = 1..nN;
range R = 1..nC;

float c[N][N] = ...;
float t[N][N] = ...;

float q = ...;
float tc[V] = ...; 

float d[N] = ...;
float a[N] = ...;
float b[N] = ...;

float p[N] = ...;

int sd[V] = ...;

float M = ...;

dvar boolean x[N][N][V][R];
dvar boolean y[V];
dvar float s[N][V][R];

// 3.1
maximize sum(k in V)(
			sum(i in N, r in R)(sum(j in N:(i <= (nN - nD)) && (j > nD))((p[j] - c[i][j]) * x[i][j][k][r])) - 
			(y[k] * tc[k]) - 
			sum(r in R)(sum(i in N)(sum(j in N: i <= nD && j > (nN - nD))(c[i][j] * x[i][j][k][r]))) - 
			sum(i in N, j in N, k in V, r in R)(t[i][j] * x[i][j][k][r])
		);

subject to {

	forall(r in R, k in V, i in N, j in N: i == j){
		x[i][j][k][r] == 0;
	}
	
	forall(r in R, k in V, i in N, j in N: (i != j) && (j <= (nN - nD))){
		y[k] >= x[i][j][k][r];
	}
	
	forall(r in R, k in V, i in N: i <= nD){
		x[i][(nN - i + 1)][k][r] == 0;	
	}
	
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nD && j > nD) {
		x[i][j][k][1] == 0;
	}
	
	forall(k in V, r in R, i in N, j in N: i != sd[k] && i <= nD && j > nD) {
		sum(pr in R, ii in N, jj in N: pr < r && ii <= nD)(x[ii][jj][k][pr]) + (1 - x[i][j][k][r]) >= 1;
	}
	
	// 3.2
	forall(i in N: i > nD && i <= (nN - nD)){
		sum(r in R, k in V)(sum(j in N: j > nD)(x[i][j][k][r])) <= 1;	
	}
	// 3.3 ?
	forall(r in R, k in V) {
		sum(i in N:i <= (nN - nD))(d[i] * (sum(j in N: (i != j) && (j > nD))(x[i][j][k][r]))) <= q;
	}
	// 3.4
	/*forall(r in R, k in V, i in N:i <= nD) {
		sum(j in N: j > nD)(x[i][j][k][r]) == 1;
	}*/
	// 3.5
	forall(r in R, k in V, h in N: h > nD && h <= (nN - nD)){
		sum(i in N: i <= (nN - nD))(x[i][h][k][r]) - sum(j in N: j > nD)(x[h][j][k][r]) == 0;
	}
	// 3.6
	/*forall(r in R, k in V, j in N: j > (nN - nD)){
		sum(i in N: i <= (nN - nD))(x[i][j][k][r]) == 1;
	}*/
	// 3.7
	forall(r in R, k in V, i in N, j in N: (i != j)) {
		s[i][k][r] + t[i][j] - M*(1 - x[i][j][k][r]) <= s[j][k][r];
	}
	// 3.8 ?
	forall(r in R, k in V, i in N){
		a[i] <= s[i][k][r] <= b[i];
	}
	// ?
	forall(r in R, sr in R, k in V, i in N, j in N: r < nC && i <= nD && j > (nN - nD)  && sr > r) {
		s[j][k][r] <= s[i][k][(sr)];	
	}
	// ?
	forall(k in V, r in R, j in N: (j > (nN - nD)) && (r < nC)) {
		sum(g in N)(x[nN - j + 1][g][k][r+1]) >= sum(h in N)(x[h][j][k][r]);
	}
	
	/*forall(i in N, k in V, r in R){
		s[i][k][r] >= 0;
	}*/
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
		}
		if(currentRoute != node.RouteId)
		{
			writeln("-----");
			currentRoute = node.RouteId;
			writeln("ROUTE ", currentRoute);
		}
		if(node.OriginId <= nD){
			writeln("- From [Base](", node.OriginId, ") To (", node.CustomerId, ")");
		}
		else if (node.CustomerId > (nN - nD)){
			writeln("- From (", node.OriginId, ") To (", node.CustomerId, ")[Base]");
		}
		else{
			writeln("- From (", node.OriginId, ") To (", node.CustomerId, ")");	
		}
		writeln("  ServiceTime: ", s[node.CustomerId][node.VehicleId][node.RouteId]);
		writeln("  Demand: ", d[node.CustomerId]);
		writeln("  Profit: ", p[node.CustomerId]);
		writeln("  Cost: ", c[node.OriginId][node.CustomerId]);
		writeln("  TravelTime: ", t[node.OriginId][node.CustomerId]);
	}
}
