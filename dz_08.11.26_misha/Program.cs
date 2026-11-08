//pervaya

//static unsafe void Swap(int* a, int* b)
//{
//int temp = *a;
//*a = *b;
//*b = temp;
//}


//Vtoraya

//int[] numbers = { 3, 7, 1, 9, 4 };

//fixed (int* p = numbers)
//{
// a) Вывод всех элементов
//Console.Write("Элементы массива: ");
//for (int i = 0; i < numbers.Length; i++)
//{
//Console.Write(*(p + i) + " ");
//}
//Console.WriteLine();

// b) Поиск суммы
//int sum = 0;
//for (int i = 0; i < numbers.Length; i++)
//{
//sum += *(p + i);
//}
//Console.WriteLine($"Сумма: {sum}");

// c) Поиск максимума
//int max = *p; 
//for (int i = 1; i < numbers.Length; i++)
//{
//if (*(p + i) > max)
//{
//max = *(p + i);
//}
//}
//Console.WriteLine($"Максимум: {max}");
//}


//tratiya

//string text = "программирование";
//char target = 'р';
//int count = 0;

//fixed (char* c = text)
//{
//for (int i = 0; i < text.Length; i++)
//{
//if (*(c + i) == target)
//{
//count++;
//}
//}
//

//Console.WriteLine($"Буква '{target}' встречается {count} раз(а).");


//otveti na voprosi
//1. p -- указатель, p* -- разыменование указателя

//2. unsafe разрешает использовать арифметику указателей и прямой доступ к памяти в размеченном блоке или методе

//3. Без ключевого слова fixed писать int* p = &arr[0]; нельзя, потому что массивы хранятся в управляемой куче, и сборщик мусора (Garbage Collector) в любой момент может переместить массив на другой адрес памяти для оптимизации, что сделает указатель недействительным.

//4. Оператор sizeof(double) вернет 8 байт, а sizeof(char) — 2 байта. Тип char в C# занимает 2 байта, так как использует кодировку UTF-16 для поддержки широкого спектра символов и алфавитов мира, в отличие от однобайтового

//5. Если int* p указывает на адрес 1000, то p + 1 будет указывать на адрес 1004, так как тип int занимает 4 байта. Если бы p имел тип double*, то p + 1 указывал бы на адрес 1008, потому что double занимает 8 байт.