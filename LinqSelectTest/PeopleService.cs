using System.Collections.Generic;

namespace LinqSelectTest
{
    public class PeopleService
    {
        public static IEnumerable<Person> GetPeople()
        {
            var people = new List<Person>()
            {
                new Person()
                {
                    Age = 24,
                    Name = "Jonathan"
                }
            };

            return people;
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
