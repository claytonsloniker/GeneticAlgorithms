namespace GeneticAlgorithms
{
    public class Schedule
    {
        public List<ScheduledActivity> ScheduledActivities { get; private set; } = new List<ScheduledActivity>();

        public Schedule() { }

        public void AddScheduledActivity(Activity activity, Room room, string timeSlot, string assignedFacilitator)
        {
            ScheduledActivities.Add(new ScheduledActivity(activity, room, timeSlot, assignedFacilitator));
        }

        public override string ToString()
        {
            return string.Join("\n", ScheduledActivities.Select(sa => sa.ToString()));
        }
    }

    public class ScheduledActivity
    {
        public Activity Activity { get; private set; }
        public Room Room { get; private set; }
        public string TimeSlot { get; private set; }
        public string AssignedFacilitator { get; private set; }

        public ScheduledActivity(Activity activity, Room room, string timeSlot, string assignedFacilitator)
        {
            Activity = activity;
            Room = room;
            TimeSlot = timeSlot;
            AssignedFacilitator = assignedFacilitator;
        }

        public override string ToString()
        {
            return $"Activity: {Activity.Name}, Room: {Room.Name}, TimeSlot: {TimeSlot}, Facilitator: {AssignedFacilitator}";
        }
    }
}
