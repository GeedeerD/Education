#include <iostream>
using namespace std;

bool testIf();
bool testSwitch();
bool testLoop();
void testLoopBreak();

int main()
{

	//testIf();
	testSwitch();
	//testLoop();
	//testLoopBreak();

	cout << "String1" << endl;



}

void testLoopBreak()
{
	int number1 = 10;

	// return
	for (int i = 0; i < 15; i++) {
		if (i == number1) {
			//return;
			//goto loop1;
		}
		cout << i << endl;
	}

	// break
	for (int i = 0; i < 15; i++) {
		if (i == number1) {
			break;
		}
		cout << i << endl;
	}


	// continue
	for (int i = 0; i < 15; i++) {
		if (i == number1) {
			continue;
		}
		cout << i << endl;
	}

}

bool testLoop() {
	// while
	int number1 = 6;
	while (number1 > 0) {
		cout << number1 << endl;
		number1 = number1 - 1;
	}

	//	do while
	number1 = 6;
	do
	{
		cout << number1-- << endl;
	} while (number1 > 0);

	// I
	for (int i = 0; i < 6; i++) {
		cout << i << endl;
	}


	cout << "for II" << endl;
	// II
	number1 = 6;
	for(; number1-- > 0; )
		cout << number1 << endl;

	return false;
}

bool testSwitch()
{
	int number1 = -3;
	switch (number1) {
	case 2:
		cout << "2" << endl;
		break;

	case 3:
		cout << "3" << endl;
		break;

	case 4:
		cout << "4" << endl;
		break;

	case 5:
		cout << "5" << endl;
		break;
	default:
		cout << "UNKNOWN" << endl;
		break;
	}

	return false;
}


bool testIf()
{
	int number1 = 4;

	if (number1 == 5) {
		cout << "= 5" << endl;
	}
	else if (number1 == 6) {
		cout << "= 6" << endl;
	}
	else if (number1 == 7) {
		cout << "= 7" << endl;
	}
	else if (number1 == 8) {
		cout << "= 8" << endl;
	}
	else{
		cout << "UNKNOWN" << endl;
	}

	return true;
	// if
	// else
	// esle if
	if (true) {
		cout << "TRUE" << endl;
	}
	else if (true || false) {
		cout << "TRUE || FALSE" << endl;
	}
	else {
		cout << "ELSE" << endl;
	}

	if (true)
	{
		if (false)
			if (false)
				cout << "false 1" << endl;

			else if (false || true)
				if (true) {

				}
	}
	else {

	}
}