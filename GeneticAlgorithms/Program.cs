using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeneticAlgorithms
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] facilitators = { "Lock", "Glen", "Banks", "Richards", "Shaw", "Singer", "Uther", "Tyler", "Numen", "Zeldin" };
            string[] timeSlots = { "10:00", "11:00", "12:00", "13:00", "14:00", "15:00" };

            var rooms = new List<Room>
            {
                new Room("Slater 003", 45),
                new Room("Roman 216", 30),
                new Room("Loft 206", 75),
                new Room("Roman 201", 50),
                new Room("Loft 310", 108),
                new Room("Beach 201", 60),
                new Room("Beach 301", 75),
                new Room("Logos 325", 450),
                new Room("Frank 119", 60)
            };

            var activities = new List<Activity>
            {
                new Activity("SLA101A", 50, new List<string> { "Glen", "Lock", "Banks", "Zeldin" }, new List<string> { "Numen", "Richards" }),
                new Activity("SLA101B", 50, new List<string> { "Glen", "Lock", "Banks", "Zeldin" }, new List<string> { "Numen", "Richards" }),
                new Activity("SLA191A", 50, new List<string> { "Glen", "Lock", "Banks", "Zeldin" }, new List<string> { "Numen", "Richards" }),
                new Activity("SLA191B", 50, new List<string> { "Glen", "Lock", "Banks", "Zeldin" }, new List<string> { "Numen", "Richards" }),
                new Activity("SLA201", 50, new List<string> { "Glen", "Banks", "Zeldin", "Shaw" }, new List<string> { "Numen", "Richards", "Singer" }),
                new Activity("SLA291", 50, new List<string> { "Lock", "Banks", "Zeldin", "Singer" }, new List<string> { "Numen", "Richards", "Shaw", "Tyler" }),
                new Activity("SLA303", 60, new List<string> { "Glen", "Zeldin", "Banks" }, new List<string> { "Numen", "Singer", "Shaw" }),
                new Activity("SLA304", 25, new List<string> { "Glen", "Banks", "Tyler" }, new List<string> { "Numen", "Singer", "Shaw", "Richards", "Uther", "Zeldin" }),
                new Activity("SLA394", 20, new List<string> { "Tyler", "Singer" }, new List<string> { "Richards", "Zeldin" }),
                new Activity("SLA449", 60, new List<string> { "Tyler", "Singer", "Shaw" }, new List<string> { "Zeldin", "Uther" }),
                new Activity("SLA451", 100, new List<string> { "Tyler", "Singer", "Shaw" }, new List<string> { "Zeldin", "Uther", "Richards", "Banks" })
            };

            var population = new Population(activities, facilitators, rooms, timeSlots);
            var fitnessFunction = new FitnessFunction();

            var fitnessScores = new List<double>();

            // Generate schedules and calculate fitness
            for (int i = 0; i < 2; i++)
            {
                var schedule = population.GenerateRandomSchedule(seed: 42);
                var fitness = population.GetFitness(schedule);
                Console.WriteLine($"Schedule {i + 1}: Fitness = {fitness:F4}");
                fitnessScores.Add(fitness);
            }

            // Apply softmax normalization
            var probabilities = fitnessFunction.Softmax(fitnessScores);

            // Output probabilities
            for (int i = 0; i < probabilities.Count; i++)
            {
                Console.WriteLine($"Schedule {i + 1}: Probability = {probabilities[i]:F4}");
            }

        }
    }
}
