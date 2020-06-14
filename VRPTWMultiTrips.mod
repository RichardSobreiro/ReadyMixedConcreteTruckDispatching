// FROM: -
// AUTORS: Richard Sobreiro

int nV = ...;
int nC = ...;
int nN = nC + 2;

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

float M = ...;

dvar boolean x[N][N][V][R];
dvar boolean y[V];
dvar float s[N][V][R];

// 3.1
maximize sum(k in V)(
			sum(i in N, r in R)(sum(j in N:(i != nN) && (j != 1))((p[j] - c[i][j]) * x[i][j][k][r])) - (y[k] * tc[k])
		);

subject to{
	forall(r in R, k in V, i in N, j in N: i == j){
		x[i][j][k][r] == 0;	
	}
	
	forall(r in R, k in V, i in N, j in N: (i != j) && (j != nN)){
		y[k] >= x[i][j][k][r];
	}
	
	// 3.2
	forall(i in N: i != 1 && i != nN){
		sum(r in R, k in V)(sum(j in N: j != 1)(x[i][j][k][r])) <= 1;	
	}
	// 3.3
	forall(r in R, k in V) {
		sum(i in N:i != nN)(d[i] * (sum(j in N: (i != j) && (j != 1))(x[i][j][k][r]))) <= q;
	}
	// 3.4
	forall(r in R, k in V) {
		sum(j in N: j != 1)(x[1][j][k][r]) == 1;
	}
	// 3.5
	forall(r in R, k in V, h in N: h != 1 && h != nN){
		sum(i in N: i != nN)(x[i][h][k][r]) - sum(j in N: j != 1)(x[h][j][k][r]) == 0;
	}
	// 3.6
	forall(r in R, k in V){
		sum(i in N: i != nN)(x[i][nN][k][r]) == 1;
	}
	// 3.7
	forall(r in R, k in V, i in N, j in N: (i != nN) && (j != 1) && (i != j)) {
		s[i][k][r] + t[i][j] - M*(1 - x[i][j][k][r]) <= s[j][k][r];
	}
	// 3.8
	forall(r in R, k in V, i in N){
		a[i] <= s[i][k][r] <= b[i];
	}
	
	forall(r in R, k in V: r != nC){
		s[nN][k][r] <= s[1][k][(r + 1)];	
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
		writeln("- From (", node.OriginId, ") To (", node.CustomerId, ")");
		writeln("  ServiceTime: ", s[node.CustomerId][node.VehicleId][node.RouteId]);
		writeln("  Demand: ", d[node.CustomerId]);
		writeln("  TravelTime: ", t[node.OriginId][node.CustomerId]);
	}
}
