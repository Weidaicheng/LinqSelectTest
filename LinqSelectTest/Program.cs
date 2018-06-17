using Newtonsoft.Json;
using System;

namespace LinqSelectTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var people = PeopleService.GetPeople();

            //without tolist
            people = people.Select(p =>
            {
                p.Name = "Mr. " + p.Name;
                return p;
            });

            //with tolist
            //people = people.Select(p =>
            //{
            //    p.Name = "Mr. " + p.Name;
            //    return p;
            //}).ToList();

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(JsonConvert.SerializeObject(people)); 
            }
            Console.ReadKey();
        }
    }
}

