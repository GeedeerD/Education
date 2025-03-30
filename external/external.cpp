// external.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include "chrono"
using namespace std;

int SumNumbers(int a);

int main()
{

    namespace cr = std::chrono;

    // you can replace this with steady_clock or system_clock
    typedef cr::high_resolution_clock my_clock;

    // get the clock time before operation.
    // note that this is a static function, and
    // we don't actually create a clock object
    auto start_time = my_clock::now();

    int number1 = 14;
    int iterations = 15000000;
    int i = 0;
    int minSumNumbers = 177;
    int minIndex = 0;

    while (i++ < iterations) 
    {
        int sumNubers = SumNumbers(number1 * i);
        if (minSumNumbers >= sumNubers) {
            minSumNumbers = sumNubers;
            minIndex = i;

            cout << "Min index: " << minIndex << "\t Number:" << number1 * i << endl;
        }
    }

    // get the clock time after the operation
    auto end_time = my_clock::now();

    // get the elapsed time
    auto diff = end_time - start_time;

    // convert from the clock rate to a millisecond clock
    auto milliseconds = cr::duration_cast<cr::milliseconds>(diff);

    // get the clock count (i.e. the number of milliseconds)
    auto millisecond_count = milliseconds.count();

    std::cout << "!!!!!!!!!!!!!!" << millisecond_count << '\n';
    cin >> number1;
}


int SumNumbers(int a)
{
        string str = to_string(a);
        int length = str.length();
        int sum = 0;
        for (int i = 0; i < length; i++) 
        {
            int number = str[i] - '0';
            sum += number;
        }
        return sum;
}

// 1 5 5


//нужно найти такое натуральное число, которое делится без остатка на 14 и у которого сумма цифр в числе минимальное