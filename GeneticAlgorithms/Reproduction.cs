namespace GeneticAlgorithms
{
    internal class Reproduction
    {
        private readonly Random _random = new Random();
        private double _mutationRate = 0.01; // Initial mutation rate
        private readonly List<Room> _rooms;
        private readonly string[] _timeSlots;
        private readonly string[] _facilitators;

        public Reproduction(List<Room> rooms, string[] timeSlots, string[] facilitators)
        {
            _rooms = rooms;
            _timeSlots = timeSlots;
            _facilitators = facilitators;
        }

        public Schedule Crossover(Schedule parent1, Schedule parent2)
        {
            var offspring = new Schedule();

            for (int i = 0; i < parent1.ScheduledActivities.Count; i++)
            {
                var activity = _random.Next(2) == 0
                    ? parent1.ScheduledActivities[i]
                    : parent2.ScheduledActivities[i];

                offspring.AddScheduledActivity(
                    activity.Activity,
                    activity.Room,
                    activity.TimeSlot,
                    activity.AssignedFacilitator
                );
            }

            return offspring;
        }

        public List<Schedule> GenerateNextGeneration(List<Schedule> currentGeneration, FitnessFunction fitnessFunction, int nextGenSize)
        {
            var nextGeneration = new List<Schedule>();
            var fitnessScores = currentGeneration.Select(fitnessFunction.CalculateFitness).ToList();
            var probabilities = fitnessFunction.Softmax(fitnessScores);

            // Track the best fitness score
            double bestFitness = fitnessScores.Max();
            double previousBestFitness = double.MinValue;
            int stableGenerations = 0; // Count generations with no significant improvement

            while (nextGeneration.Count < nextGenSize)
            {
                var parent1 = SelectParent(currentGeneration, probabilities);
                var parent2 = SelectParent(currentGeneration, probabilities);

                var offspring = Crossover(parent1, parent2);

                // Apply mutation to the offspring
                Mutate(offspring);

                nextGeneration.Add(offspring);
            }

            // Check if fitness is improving
            if (bestFitness > previousBestFitness + 0.01) // Threshold for improvement
            {
                AdjustMutationRate(0.5); // Cut mutation rate in half
                stableGenerations = 0; //reset stability counter
            }
            else
            {
                stableGenerations++;
            }

            // Stop adjusting mutation rate if fitness is stable for 5 generations
            if (stableGenerations >= 5)
            {
                Console.WriteLine("Fitness has stabilized. Mutation rate adjustment stopped.");
            }

            previousBestFitness = bestFitness;

            return nextGeneration;
        }

        private Schedule SelectParent(List<Schedule> population, List<double> probabilities)
        {
            double randomValue = _random.NextDouble();
            double cumulativeProbability = 0.0;

            for (int i = 0; i < population.Count; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomValue <= cumulativeProbability)
                {
                    return population[i];
                }
            }

            return population.Last();
        }

        private void Mutate(Schedule schedule)
        {
            foreach (var scheduledActivity in schedule.ScheduledActivities)
            {
                if (_random.NextDouble() < _mutationRate)
                {
                    // Randomly mutate one of the attributes (room, time slot, or facilitator)
                    int mutationType = _random.Next(3);
                    switch (mutationType)
                    {
                        case 0: // Mutate room
                            scheduledActivity.Room = _rooms[_random.Next(_rooms.Count)];
                            break;
                        case 1: // Mutate time slot
                            scheduledActivity.TimeSlot = _timeSlots[_random.Next(_timeSlots.Length)];
                            break;
                        case 2: // Mutate facilitator
                            scheduledActivity.AssignedFacilitator = _facilitators[_random.Next(_facilitators.Length)];
                            break;
                    }
                }
            }
        }

        public void AdjustMutationRate(double factor)
        {
            _mutationRate *= factor;
        }
    }
}
