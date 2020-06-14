// FROM: -
// AUTORS: Richard Sobreiro

int nV = ...;
int nC = ...;
int nN = nC + 2;

range V = 1..nV;
range N = 1..nN;
range A = 1..nN;

float c[A][A] = ...;
float t[A][A] = ...;

float q = ...;

float d[N] = ...;
float a[N] = ...;
float b[N] = ...;

float p[N] = ...;

float M = ...;

dvar boolean x[N][N][V];
dvar float s[N][V];

// 3.1
maximize sum(k in V)(
			sum(i in N)(
				sum(j in N:(i != nN) && (j != 1))((p[j] - c[i][j]) * x[i][j][k])
			)
		);

subject to{
	forall(k in V, i in N, j in N: i == j){
		x[i][j][k] == 0;	
	}
	
	// 3.2
	forall(i in N: i != 1 && i != nN){
		sum(k in V)(sum(j in N: j != 1)(x[i][j][k])) <= 1;	
	}
	// 3.3
	forall(k in V){
		sum(i in N:i != nN)(d[i] * (sum(j in N: (i != j) && (j != 1))(x[i][j][k]))) <= q;
	}
	// 3.4
	forall(k in V){
		sum(j in N: j != 1)(x[1][j][k]) == 1;
	}
	// 3.5
	forall(k in V, h in N: h != 1 && h != nN){
		sum(i in N: i != nN)(x[i][h][k]) - sum(j in N: j != 1)(x[h][j][k]) == 0;
	}
	// 3.6
	forall(k in V){
		sum(i in N: i != nN)(x[i][nN][k]) == 1;
	}
	// 3.7
	forall(k in V, i in N, j in N: (i != nN) && (j != 1) && (i != j)){
		s[i][k] + t[i][j] - M*(1 - x[i][j][k]) <= s[j][k];
	}
	// 3.8
	forall(k in V, i in N){
		a[i] <= s[i][k] <= b[i];
	}
	
}

execute 
{
	for(var k in V)
	{
		writeln("--------------------------------------------------------------------------------------");
		writeln("VEHICLE ", k);
		for(var i in N) 
		{
			for(var j in N)
			{
				if(x[i][j][k] == 1) 
				{	
					writeln("From (", i, ") To (", j, ")");
					writeln("ServiceTime: ", s[j][k]);
					if(a[j] <= s[j][k] <= b[j])
					{
						writeln("In");
					}
					else 
					{
						writeln("Out");					
					}
				}			
			}			
		}
		writeln("--------------------------------------------------------------------------------------");
	}
}
		
execute 
{
	for(var k in V) 
	{
		writeln("--------------------------------------------------------------------------------------");
		writeln("VEHICLE ", k);
		for(var i in N) 
		{
			for(var j in N)
			{
				if(x[i][j][k] == 1) 
				{	
					writeln("From (", i, ") To (", j, ")");
					writeln("ServiceTime: ", s[j][k]);
					if(a[j] <= s[j][k] <= b[j])
					{
						writeln("In");
					}
					else 
					{
						writeln("Out");					
					}
				}			
			}			
		}
		writeln("--------------------------------------------------------------------------------------");
	}
}
