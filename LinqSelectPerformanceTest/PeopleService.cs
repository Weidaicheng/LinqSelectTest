using System.Collections.Generic;

namespace LinqSelectPerformanceTest
{
    public class PeopleService
    {
        public static IEnumerable<Person> GetPeople(int count)
        {
            var people = new List<Person>();
            for (var i = 0; i < count; i++)
            {
                people.Add(new Person()
                {
                    Age = 24,
                    Name = "Jonathan"
                });
            }

            return people;
        }

        public class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}
