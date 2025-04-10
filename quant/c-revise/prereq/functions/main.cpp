#include <iostream>
#include <cmath>
#include <iomanip>
using namespace std;
#include "divisor.h"
// #define MONTHS_IN_YEAR 12

// double multiplier(double a, double b) {

//     return a*b;
// }
int main() {
    const int MONTHS_IN_YEAR=12;
    const int percentDenominator=100;
    double principle=0.0;
    cout<<"Enter principle amount"<<endl;
    cin>>principle;
    double humanInterest=0.0;
    cout<<"Enter rate of interest"<<endl;
    cin>>humanInterest;
    double interest=divisor(humanInterest, percentDenominator);
    int years=0;
    cout<<"Enter Years of loan"<<endl;
    cin>>years;
    double monthlyInterest=divisor(interest, MONTHS_IN_YEAR);
    long monthsofLoan=years*MONTHS_IN_YEAR;
    
    double monthlyPayment=0.0;
    monthlyPayment= principle*
            (monthlyInterest / 
            (1-pow((double)1+monthlyInterest,(double)-monthsofLoan)));
    cout<<"Monthly payment is: "<<setiosflags(ios::fixed)<<setprecision(2)<<monthlyPayment<<endl;

    return 0;
}

