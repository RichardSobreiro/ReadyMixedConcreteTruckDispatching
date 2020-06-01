// Parameters

int p = ...; // Number of plants
int c = ...; // Number of customers
range P = 1..p;
range C = 1..c;
int t[P] = ...; // Number of trucks per plant
int pt[P] = ...; // Number of pump trucks per plant 
float css[C] = ...; // Customer service start
float csi[C] = ...; // Customer service interval per trip
int cms[C] = ...; // Customer must be served
float tcp[C] = ...; // Total customer payment
float tccpp[C][P] = ...; // Total customer cost per plant
int ndpc[C] = ...; // Number of deliveries per customer
float wd[P] = ...; // Weighing truck duration at the plant j


float M = ...;
// Variables

dvar boolean x[C][P]; // If client c will be served by plant p
dvar int ntpp[P]; // Number of trips per plant
// Model

maximize sum(i in C, j in P)(tcp[i] - tccpp[i][j])*x[i][j];

subject to {
	ClientServerByOnePlant:
	forall(i in C){
		sum(j in P)(x[i][j]) <= 1;	
	}
	CustomerMustBeServer:
	forall(i in C){
		sum(j in P)(x[i][j]) >= cms[c];	
	}
	
}
