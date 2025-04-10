#include <iostream>
// #define MONTHS_IN_YEAR 12
using namespace std;

int main() {
    const int MONTHS_IN_YEAR=12;
    double principle=0.0;
    cout<<"Enter principle amount"<<endl;
    cin>>principle;
    double rate=0.05;
    int years=10;
    double monthlyInterest=rate/MONTHS_IN_YEAR;
    long monthsofLoan=years*MONTHS_IN_YEAR;
    cout<<principle<<" "<<rate<<" "<<years<<" "<<monthlyInterest<<" "<<monthsofLoan<<" "<<endl;
    return 0;
}

