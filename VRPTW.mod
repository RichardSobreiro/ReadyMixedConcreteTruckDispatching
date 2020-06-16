// FROM: An exact algorithm for a vehicle routing problem with time windows and multiple use of vehicles
// AUTORS: Nabila Azi, Michel Gendreau, Jean-Yves Potvin 

int K = ...; // Number of vehicles
float Q = ...; // Vehicles capacity

int nV = ...; // Number of customers
int nVp = nV+2; // Number of customers plus base at 0 and n+1

range V = 1..nV;
range A = 1..nV;

range Vp = 1..nVp;
range Ap = 1..nVp;

float d[Ap][Ap] = ...; // Distance between each node including the base
float t[Ap][Ap] = ...; // Travelling time between each node including the base
float g[Vp] = ...; // Revenue per customer
float q[Vp] = ...; // Demand per customer
float s[Vp] = ...; // Service or dwell time
float a[Vp] = ...; // Earliest time to begin service at customer
float b[Vp] = ...; // Latest time to begin service at customer

int mR = ...; // Maximal number of routes that the fleet can perform in a day
range R = 1..mR;

int mK = ...; // Number of workdays part of the solution 

float alpha = ...; // Wheight for total revenue

float tmax = ...; // Maximal duration of a route
float beta = ...; // Multiplier setup time for each route
float M = ...;

dvar boolean x[R][Ap][Ap]; // Binary variable indicating whether or not arc (i,j) appear in route r
dvar boolean y[R][Vp]; // Binary variable indicating whether or not customer i is server by route r
dvar float ts[R][Vp]; // When the service starts at customer i if it is served by route r
// For each route r 2 R ts[r][0] (resp. tr[r][V+1] is the time at which the route starts (resp. ends) 
// at the depot.
dvar boolean z[R][R]; // Binary variable z[r][s] indicates whether or not route s immediately follows 
// route r in the workday of one of the vehicles
dvar float sigma[R]; // Setup time for each route

minimize sum(r in R, i in Ap, j in Ap)(d[i][j] * x[r][i][j]) - 
	alpha * sum(r in R, i in Vp)(g[i] * y[r][i]);

//maximize sum(r in R, i in Vp)(g[i] * y[r][i]);
	
subject to {
	
	EveryClientMustBelongOneRoute:
	forall(i in Vp, r in R: i != 1 && i != nVp) {
		sum(j in Vp)(x[r][i][j]) == y[r][i];	
	}
	
	EveryCustomerCanBeServedByMaximumOneRoute:
	forall(i in Vp: i != 1 && i != nVp) {
		sum(r in R)(y[r][i]) <= 1;
	}
	
	RoutesMustGetOutCustomerIfArrivedAtCustomer:
	forall(r in R, h in Vp: h != 1 && h != nVp) {
		sum(i in Vp)(x[r][i][h]) - sum(j in Vp)(x[r][h][j]) == 0;	
	}
	
	EveryRouteMustGetOutBaseEvenIfGoingToSameBase:
	forall(r in R) {
		sum(i in Vp)(x[r][1][i]) == 1;
	}
	
	EveryRouteMustReturnToBaseEvenIfComingToSameBase:
	forall(r in R) {
		sum(i in Vp)(x[r][i][nVp]) == 1;	
	}	
	
	CapacityVehicleMustBeRespectedEveryRoute:
	forall(r in R) {
		sum(i in Vp)(q[i] * y[r][i]) <= Q;	
	}
	
	TimingInEveryRouteMustBeRespected:
	forall(r in R, i in Vp, j in Vp) {
		ts[r][i] + s[i] + t[i][j] - M * (1 - x[r][i][j]) <= ts[r][j];
	}
	
	TimeWindowsMustBeRespected:
	forall(r in R, i in Vp: i != 1 && i != nVp) {
		a[i] * y[r][i] <= ts[r][i];
		ts[r][i] <= b[i] * y[r][i];	
	}
	
	SetupTimeForEachRouteMustBeRespected:
	forall(r in R) {
		ts[r][1] >= sigma[r]; 	
	}
	
	MaximalTimeOfEachRouteMustBeRespected:
	forall(r in R, i in Vp: i != 1 && i != nVp) {
		ts[r][i] <= ts[r][1] + tmax;	
	}
	
	SetupTimeEachRouteMustBeRespected:
	forall(r in R) {
		sigma[r] == beta * sum(i in Vp: i != 1 && i != nVp)(s[i] * y[r][i]);		
	}
	
	StartTimeRouteAfterRoute:
	forall(r in R, s in R: r < s) {
		ts[s][1] + M * (1 - z[r][s]) >= ts[r][nVp] + sigma[s];
	}
	
	// At most |K| workdays can be part of a solution through this constraint, because the number of
	// z[r][s] variables set to one is always equal to the number of routes minus the number of workdays 
	MaximalNumberOfZrsVariablesSetToOne:
	forall(r in R, s in R){
		sum(r in R, s in R: r < s)(z[r][s]) == mR - K;	
	}
	
	ServiceTimeMustBeGreatherThanZero:
	forall(i in Vp, r in R){
		ts[r][i] >= 0;	
	}
} 

execute 
{
	/*for(var r in R) 
	{
		for(var i in Vp) 
		{
			if((y[r][i] == 1) && (i != 1) && (i != nVp)) 
			{
				writeln("CUSTOMER (", i, ") -> ROUTE (", r, ")");
				writeln("y[", r, "][", i, "] = 1;");
				writeln("x[",r,"][", i, "][", i, "] = ", x[r][i][i], ";");
			}			
		}
	}*/

	for(var r in R) 
	{
		writeln("--------------------------------------------------------------------------------------");
		writeln("ROUTE ", r);
		for(var i in Vp) 
		{
			for(var j in Vp)
			{
				if(x[r][i][j] == 1) 
				{	
					writeln("From (", i, ") To (", j, ")");
					/*if((i == 1) && (j == nVp))
					{
						//writeln("NO CUSTOMER SERVED");
					}
					else
					{
						writeln("From (", i, ") To (", j, ")");
						writeln("CustomerId: ", j);
						//writeln("StartTime: ", ts[r][1]);
						writeln("ServiceTime: ", ts[r][j]);
						//writeln("EndTime: ", ts[r][nVp]);								
					}*/						
				}			
			}			
		}
		writeln("--------------------------------------------------------------------------------------");
		/*writeln("--------------------------------------------------------------------------------------");
		//if(r == 2)
		//{
			writeln("ROUTE ", r);
			for(var i in Vp) 
			{
				if( i == 2)
				{
					writeln("From (", 2, ") To (", i, ")");
					writeln("x[",r,"][2][",i,"]: ", x[r][2][i]);		
					
					writeln("From (", i, ") To (", 2, ")");
					writeln("x[",r,"][",i,"][2]: ", x[r][i][2]);			
				}	
			}		
		//}
		writeln("--------------------------------------------------------------------------------------");*/
	}
}

