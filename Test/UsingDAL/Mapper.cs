using System;
using System.Collections.Generic;
using System.Threading;

namespace UsingDAL
{
    public class Mapper
    {
        public delegate int Operation(Record rec);
        private Dictionary<string, Record> db { get; set; }
        private Random rand;
        public Mapper()
        {
            db = new Dictionary<string, Record>();
            db.Add("5", new Record(12));
            db.Add("6", new Record(34));
            db.Add("7", new Record(45));
            rand = new Random();
        }
        public void Update(string id,Operation update,bool upsert, string firstParam = "SomeValue")
        {
            Thread.Sleep(rand.Next(1000, 2500));//1000,2500
            Record value = this.db[id];
            int oldValue = 0;
          
            int getMeanOfIncrement = update(new Record(0));
            if (value == null)
            {
                db.Add(id, new Record(getMeanOfIncrement));
                Console.WriteLine("Поток №{2}. Инкремент = {3}. Добавлена запись {0}, counter = {1}.",id,db[id],Thread.CurrentThread.ManagedThreadId, getMeanOfIncrement);
            }
            else
            {
                oldValue = db[id].Counter;
            
                db[id].Counter = update(db[id]);
                Console.WriteLine("Поток №{2}. Исходное значение: {4}. Инкремент = {3}. Запись {0} изменена: {1}", id,db[id],Thread.CurrentThread.ManagedThreadId, getMeanOfIncrement,oldValue);
            }
        }
    }
}