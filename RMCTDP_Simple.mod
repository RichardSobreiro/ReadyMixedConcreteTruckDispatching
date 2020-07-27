// FROM: -
// AUTORS: 

// RMC = Ready Mixed Concrete

int nLP = ...; // Number of loading plants
int nMT = ...; // Number of concrete truck mixers
int nD = ...; // Number of customer's deliveries

range MT = 1..nMT; // Set of concrete truck mixers
range LP = 1..nLP; // Set of loading plants
range D = 1..nD; // Set of customer's deliveries

float c[MT][D] = ...; // Cost of the journey between loading plant of the mixer truck k and the construction site of the delivery i
float t[MT][D] = ...; // Duration of the journey between loading plant of the vehicle k and customer construction site i
int lpctm[MT] = ...; // If loading plant i is the base of the concrete truck mixer k 

float q = ...; // Concrete truck mixers capacity
float tc = ...; // Concrete truck mixers fixed maintenance cost

float d[D] = ...; // Demand for RMC at customer delivery i
float a[D] = ...; // Begin for the time window at customer delivery i 
float b[D] = ...; // End for the time window at customer delivery i
float cfr[D] = ...; // Concrete flow rate at customer i
float dmbs[D] = ...; // If delivery must be served

float r[D] = ...; // Profit obtained from attending customer delivery i

float ld = ...; // Loading time duration for each delivery

float M = ...; // Big M for delivery i and j

dvar boolean x[MT][D]; // If concrete mixer truck k attend trip i
dvar boolean y[MT]; // If concrete mixer truck k is used

dvar float s[MT][D]; // Service time of mixer truck k in delivery i
dvar float rs[MT][D]; // Return time of the service for mixer truck k in delivery i
dvar float ds[D]; // Duration of the service time at construction site for customer delivery i

dvar float lbt[D]; // Loading begin time for the delivery i

dvar boolean sbf[D][D]; // Window i begins first than window j

// 1
maximize sum(k in MT, i in D)(x[k][i] * (r[i] - c[k][i])) - sum(k in MT)(y[k] * tc);

subject to {
	// ? - If service time window of the delivery i begins first than j
	forall(i in D, j in D){
		a[i] - a[j] >= -M * (sbf[i][j]);	
	}
	// ? - Mixer truck can not serve more than one delivery at the same time
	forall(k in MT, i in D, j in D){
		rs[k][i] - lbt[j] <= M * (3 - x[k][i] - x[k][j] - sbf[i][j]);	
	}
	// ? - Duration of each step of each delivery must be respected
	forall(k in MT, i in D){
		lbt[i] + ld + t[k][i] - M * (1 - x[k][i]) <= s[k][i];
		s[k][i] + ds[i] + t[k][i] - M * (1 - x[k][i]) <= rs[k][i];
	}
	// ? - Duration of the service time at customer i must be respected
	forall(i in D) {
		ds[i] == d[i] * cfr[i];
	}
	// ? - Time window must be respected
	forall(k in MT, i in D) {
		a[i] <= s[k][i] <= b[i];
	}
	// ? - Each delivery can be served by only one mixer truck
	forall(i in D) {
		 sum(k in MT)(x[k][i]) <= 1;	
	}
	// ? - If truck mixer k is used in the period 
	forall(k in MT, i in D) {
		y[k] >= x[k][i];
	}
	// ? - If delivery must be served
	forall(i in D) {
		 sum(k in MT)(x[k][i]) >= dmbs[i];
	}
	// ? - If delivery of the same order is served all deliveries of the order must also be served
}

tuple Node 
{
	int Delivery;
	key int MixerTruck;
	key float LoadingBeginTime;
	float ServiceTime;
	key float ReturnTime;
	int LoadingPlant;
	float Profit;
	float BeginTimeWindow;
	float EndTimeWindow;
	float TravelTime;
	float TravelCost;
	float DurationOfService;
	int IfDeliveryMustBeServed;
};

sorted {Node} Nodes = {};

execute 
{
	for(var k in MT)
	{
		for(var i in D) 
		{
			if(x[k][i] == 1) 
			{	
				for(var j in LP)
				{
					if(lpctm[k] == j)
					{
						Nodes.add(i, k, lbt[i], s[k][i], rs[k][i], j, r[i], a[i], b[i], t[k][i], c[k][i], ds[i], dmbs[i]);						
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
		writeln("Profit: ", node.Profit);
		writeln("BeginTimeWindow: ", node.BeginTimeWindow);
		writeln("EndTimeWindow: ", node.EndTimeWindow);
		writeln("TravelTime: ", node.TravelTime);
		writeln("TravelCost: ", node.TravelCost);
		writeln("DurationOfService: ", node.DurationOfService);
		writeln("IfDeliveryMustBeServed: ", node.IfDeliveryMustBeServed);
		writeln("------------------------------------------------------");	
	}
}
