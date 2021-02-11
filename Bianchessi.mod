int nN = ...;
int m = ...;
int Tmax = ...;

range N = 1..nN;

float p[N] = ...;
float t[N][N] = ...;
float t0[N][N] = ...;

dvar float x[N][N];
dvar boolean y[N];
dvar float z[N][N];
dvar float Tmaxj[N];


// 1
maximize sum(i in N)(p[i] * y[i]);

subject to{
	// 2
	sum(j in N: j != nN)(x[1][j]) == sum(i in N: i != 1)(x[i][nN]) == m; 
	// 3
	forall(i in N){
		sum(j in N: i != j)(x[j][i]) == sum(j in N: i != j)(x[i][j]) == y[i];	
	}
	// 4
	forall(j in N){
		z[0][j] == t[0][j] * x[0][j];	
	}
	// 5
	forall(i in N){
		sum(j in N: i != j)(x[i][j]) - sum(j in N: i != j)(z[j][i]) == sum(j in N: i != j)(t[i][j] * x[i][j]);
	}
	// 6
	forall(i in N, j in N: i != j && i != 1 && j != nN){
		z[i][j] <= Tmaxj[j] * x[i][j];
	}
	forall(j in N: j != 1 && j != nN){
		Tmaxj[j] == Tmax - t[j][nN];	
	}
	t[nN][nN] == 0;
	// 7
	forall(i in N, j in N: i != j && i != 1 && j != nN){
		z[i][j] >= t0[i][j] * x[i][j];	
	}
	t[1][1] == 0;
	// 9
	forall(i in N, j in N){
		if(i == 1 && j == nN){
			0 <= x[1][nN] <= m;
		}
		else{
			x[i][j] + (1 - x[i][j]) == 1;
			//(x[i][j] - 1) - x[i][j] == -1;		
		}
	}
}
