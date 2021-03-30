int nc = 10;
int np = 3;
int nv = 5;

int max_time = 480;

range N = 1..nc;
range P = 1..np;
range V = 1..nv;

float t[P][N][N][N] = ...;
float c[P][N][N][N] = ...;

dvar boolean x[P][N][N][N];

minimize sum(p in P,i in N,j in N,k in N) c[p,i,j,k] * x[p,i,j,k];

subject to { 

client_assignment: 
	sum(p in P,i in N,j in N,k in N) x[p,i,j,k] 
      + sum(p in P,i in N,j in N,k in N: i != j) x[p,j,i,k]
      	+ sum(p in P,i in N,j in N,k in N: i != j && k != i) x[p,k,j,i] == 1;

vehicle_assignment: 
	sum(p in P, i in N,j in N,k in N) x[p,i,j,k] <= nv;
}