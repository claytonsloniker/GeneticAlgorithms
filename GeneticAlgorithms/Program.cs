using System.IO;

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
            var reproduction = new Reproduction(rooms, timeSlots, facilitators);

            int generationCount = 0;
            int populationSize = 500;
            double improvementThreshold = 0.01; // 1% improvement
            List<Schedule> currentGeneration = population.GenerateInitialPopulation(populationSize);
            List<double> averageFitnessHistory = new List<double>();

            while (true)
            {
                generationCount++;

                // Calculate fitness for the current generation
                var fitnessScores = currentGeneration
                    .Select(schedule => fitnessFunction.CalculateFitness(schedule))
                    .ToList();

                double averageFitness = fitnessScores.Average();
                averageFitnessHistory.Add(averageFitness);

                Console.WriteLine($"Generation {generationCount}: Average Fitness = {averageFitness:F4}");

                // Stop condition: After 100 generations, check improvement
                if (generationCount > 100)
                {
                    double previousAverageFitness = averageFitnessHistory[generationCount - 2];
                    double improvement = (averageFitness - previousAverageFitness) / previousAverageFitness;

                    if (improvement < improvementThreshold)
                    {
                        Console.WriteLine("Stopping criteria met: Improvement < 1%");
                        break;
                    }
                }

                // Generate the next generation
                currentGeneration = reproduction.GenerateNextGeneration(
                    currentGeneration,
                    fitnessFunction,
                    populationSize
                );
            }

            // Find the best schedule in the final generation
            var bestSchedule = currentGeneration
                .OrderByDescending(schedule => fitnessFunction.CalculateFitness(schedule))
                .First();

            double bestFitness = fitnessFunction.CalculateFitness(bestSchedule);
            Console.WriteLine($"Best Fitness in Final Generation: {bestFitness:F4}");

            // Write the best schedule to an output file
            WriteScheduleToFile(bestSchedule, "BestSchedule.txt");
        }

        static void WriteScheduleToFile(Schedule schedule, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine("Best Schedule:");
                foreach (var activity in schedule.ScheduledActivities)
                {
                    writer.WriteLine($"Activity: {activity.Activity.Name}, Room: {activity.Room.Name}, Time Slot: {activity.TimeSlot}, Facilitator: {activity.AssignedFacilitator}");
                }
            }

            Console.WriteLine($"Best schedule written to {fileName}");
        }
    }
}
