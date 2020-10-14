using System;
using System.Collections.Generic;
using System.Threading;

namespace UsingDAL
{
    class Program
    {
        int countOfAttemp = 3; // количество попыток выполнить операцию Upsert
        int pauseBetweenAttemp = 0;//время ожидания между попытками

        //код для подгонки под заданную сигнатуру метода Update
        Mapper mapper = new Mapper();

        //объявление класса-делегата для хранения ссылки на IncrementCount
        delegate void IncrementCountDelegate(string id, int increment);

        //объект синхронизации доступа. Используется в конструкции lock. Им может быть любой экземпляр ссылочного типа.
        string objectForSync = "Объект синхронизации доступа";

        //используется для сохранения невыполненных операций типа IncrementCountDelegate и аргументов, поэтому закрыт параметром-типом object
        Queue<object> restOfJobs = new Queue<object>();

        static void Main(string[] args)
        {


            Program instanceOfProgram = new Program();

            //Экземпляр класса рандом исползуется для генерации случайного инкремента в диапазоне от -9 до +9.
            Random rand = new Random();


            //Моделируется обращение к записи БД несколькими пользователями. Один пользователь - один поток исполнения.
            for (int i = 0; i < 12; i++)
            {
                new IncrementCountDelegate(instanceOfProgram.IncrementCount).BeginInvoke("5", rand.Next(-9, 9), null, null);
                new IncrementCountDelegate(instanceOfProgram.IncrementCount).BeginInvoke("5", rand.Next(-9, 9), null, null);
                //Пауза в потоке исполнения для стохастичности
                Thread.Sleep(rand.Next(0, 100));
            }



            //здесь можно вызвать метод выполнения операций, который накопились во время автономной работы.
            //instanceOfProgram.DoRestJobs();
            Console.ReadKey();
        }


        public void IncrementCount(string id, int increment)
        {
            //глоабальная переменная countOfAttemp задает количество попыток выполнения операции Upsert .
            for (int i = 0; i < this.countOfAttemp; i++)
            {
                //начало критической секции
                lock (objectForSync)
                {

                    try
                    {
                        this.mapper.Update(id: id, update: record => record.Counter + increment, upsert: true);
                        break; //выход из цикла в случаи успешного вполнения операции
                    }
                    catch (DuplicateKeyException ex)
                    {
                        //обработка исключения DuplicateKeyException
                        Console.WriteLine("Запись "+id+" уже существует." + ex.Message);
                        if (i == this.countOfAttemp - 1)
                        {
                            //запоминание невыполненной операции в стек или файл (нереализовано)
                            //После возобнолвения работоспособности сервера
                            //можно считать стек/файл и выполнить все невыполненные операции, сохраненные во время автономной работы

                            restOfJobs.Enqueue(new IncrementCountDelegate(IncrementCount));
                            restOfJobs.Enqueue(id);
                            restOfJobs.Enqueue(countOfAttemp);
                        }
                    }
                    catch (Exception ex)
                    {

                        //обработка исключения 
                        Console.WriteLine("Сервер упал или уборщица выдернула шнур." + ex.Message);
                        if (i == this.countOfAttemp - 1)
                        {
                            //запоминание невыполненной операции в стек или файл (нереализовано)
                            //После возобнолвения работоспособности сервера
                            //можно считать стек/файл и выполнить все невыполненные операции, сохраненные во время автономной работы

                            restOfJobs.Enqueue(new IncrementCountDelegate(IncrementCount));
                            restOfJobs.Enqueue(id);
                            restOfJobs.Enqueue(countOfAttemp);
                        }
                    }
                }

                Thread.Sleep(pauseBetweenAttemp); //пауза между попытка выполнения операции Upsert 
            }
        }

        public void DoRestJobs()
        {//метод для доделывания оставшихся работ
            Console.WriteLine("Остаток:");
            IncrementCountDelegate operation;
            for (int i = 0; i < restOfJobs.Count; i++)
            {

                operation = restOfJobs.Dequeue() as IncrementCountDelegate;
                if (operation != null)
                {


                    //вызов метода
                    operation.BeginInvoke((string)restOfJobs.Dequeue(), (int)restOfJobs.Dequeue(),null,null); ;
                }
                else { Console.WriteLine("Извлеченный элмент из стека не является операцией."); return; }
            }
        }
    }
}
