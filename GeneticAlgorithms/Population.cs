

namespace GeneticAlgorithms
{
    internal class Population
    {
        public List<Schedule> Schedules { get; private set; } = new List<Schedule>();
        private List<Activity> activities;
        private string[] facilitators;
        private List<Room> rooms;
        private string[] timeSlots;

        public Population(List<Activity> activities, string[] facilitators, List<Room> rooms, string[] timeSlots)
        {
            this.activities = activities;
            this.facilitators = facilitators;
            this.rooms = rooms;
            this.timeSlots = timeSlots;
        }

        /*
         * <summary> Generates a random schedule by assigning activities to rooms, time slots, and facilitators. </summary>
         * <returns> A randomly generated schedule. </returns>
         */
        public Schedule GenerateRandomSchedule(int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random(); // Use a random seed if provided
            var schedule = new Schedule();

            // Randomly assign activities to rooms, time slots, and facilitators
            foreach (var activity in activities)
            {
                var room = rooms[new Random().Next(rooms.Count)];
                var timeSlot = timeSlots[new Random().Next(timeSlots.Length)];
                var assignedFacilitator = facilitators[new Random().Next(facilitators.Length)];

                schedule.AddScheduledActivity(activity, room, timeSlot, assignedFacilitator);
            }

            Schedules.Add(schedule);
            return schedule;
        }


        /*
         * <summary> Calculates the fitness of a schedule based on predefined criteria. </summary>
         * <param name="schedule"> The schedule to evaluate. </param>
         * <returns> A fitness score representing the quality of the schedule. </returns>
         */
        public double GetFitness(Schedule schedule)
        {
            var fitnessFunction = new FitnessFunction();
            return fitnessFunction.CalculateFitness(schedule);
        }
    }
}