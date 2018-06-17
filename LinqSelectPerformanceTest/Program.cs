using System;
using System.Collections.Generic;
using System.Linq;
using static LinqSelectPerformanceTest.PeopleService;

namespace LinqSelectPerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var count = 1000000;
            var people = PeopleService.GetPeople(count);

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    Console.WriteLine("----------热身----------");
                }
                else
                {
                    Console.WriteLine($"----------第{i}轮----------");
                }
                people = PeopleService.GetPeople(count);
                SelectWithLinq(people);

                people = PeopleService.GetPeople(count);
                SelectWithForeach(people);

                //people = PeopleService.GetPeople(count);
                //SelectWithFor(people); 
            }

            Console.ReadKey();
        }

        public static List<Person> SelectWithLinq(IEnumerable<Person> people)
        {
            Console.WriteLine("----------Select With Linq Start----------");
            var timeStart = DateTime.Now;

            var peopleList = people.Select(p =>
            {
                p.Name = "Mr. " + p.Name;
                return p;
            }).ToList();

            var timeEnd = DateTime.Now;
            Console.WriteLine("----------Select With Linq End----------");
            Console.WriteLine($"\tUse time: {(timeEnd - timeStart).TotalMilliseconds}ms");
            Console.WriteLine();
            return peopleList;
        }

        public static List<Person> SelectWithForeach(IEnumerable<Person> people)
        {
            Console.WriteLine("----------Select With Foreach Start----------");
            var timeStart = DateTime.Now;

            var peopleList = new List<Person>();
            foreach(var p in people)
            {
                p.Name = "Mr. " + p.Name;
                peopleList.Add(p);
            }

            var timeEnd = DateTime.Now;
            Console.WriteLine("----------Select With Foreach End----------");
            Console.WriteLine($"\tUse time: {(timeEnd - timeStart).TotalMilliseconds}ms");
            Console.WriteLine();
            return peopleList;
        }

        public static List<Person> SelectWithFor(IEnumerable<Person> people)
        {
            Console.WriteLine("----------Select With For Start----------");
            var timeStart = DateTime.Now;

            var peopleList = new List<Person>();
            for(var i = 0; i < people.Count(); i++)
            {
                people.ToList()[i].Name = "Mr. " + people.ToList()[i].Name;
                peopleList.Add(people.ToList()[i]);
            }

            var timeEnd = DateTime.Now;
            Console.WriteLine("----------Select With For End----------");
            Console.WriteLine($"\tUse time: {(timeEnd - timeStart).TotalMilliseconds}ms");
            Console.WriteLine();
            return peopleList;
        }
    }
}
