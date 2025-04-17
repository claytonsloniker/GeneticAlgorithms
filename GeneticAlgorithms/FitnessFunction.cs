using GeneticAlgorithms;
using System.Diagnostics;

internal class FitnessFunction
{
    public double CalculateFitness(Schedule schedule)
    {
        double fitness = 0;

        fitness += EvaluateSameTimeSlotAndRoom(schedule); // TODO, check if this is correct

        // Evaluate each scheduled activity
        foreach (var activity in schedule.ScheduledActivities)
        {
            fitness += EvaluateRoomSize(activity);
            fitness += EvaluateFacilitatorAssignment(activity, schedule);
            fitness += EvaluateActivitySpecificAdjustments(activity, schedule); // TODO, fix logic, should be for the 2 activity sections A and B
        }

        // Evaluate facilitator load across the entire schedule
        var facilitators = schedule.ScheduledActivities
            .Select(sa => sa.AssignedFacilitator)
            .Distinct();
        foreach (var facilitator in facilitators)
        {
            fitness += EvaluateFacilitatorLoad(facilitator, schedule); // TODO, check if correct
        }

        return fitness;
    }

    private double EvaluateSameTimeSlotAndRoom(Schedule schedule)
    {
        double penalty = 0;
        var sameTimeSlotAndRoom = schedule.ScheduledActivities
            .GroupBy(sa => new { sa.TimeSlot, sa.Room })
            .Where(g => g.Count() > 1);

        foreach (var group in sameTimeSlotAndRoom)
        {
            penalty -= 0.5 * group.Count();
        }

        return penalty;
    }

    private double EvaluateRoomSize(ScheduledActivity activity)
    {
        double fitness = 0;
        // Room too small
        if (activity.Room.Capacity < activity.Activity.ExpectedEnrollment)
            fitness -= 0.5;
        // Room too big
        if (activity.Room.Capacity > 6 * activity.Activity.ExpectedEnrollment)
            fitness -= 0.2;
        else if (activity.Room.Capacity > 3 * activity.Activity.ExpectedEnrollment)
            fitness -= 0.4;
        else
            fitness += 0.3;


        return fitness;
    }

    private double EvaluateFacilitatorAssignment(ScheduledActivity activity, Schedule schedule)
    {
        double fitness = 0;

        var facilitator = activity.AssignedFacilitator;

        if (activity.Activity.PreferredFacilitators.Contains(facilitator))
            fitness += 0.5;
        else if (activity.Activity.OtherFacilitators.Contains(facilitator))
            fitness += 0.2;
        else
            fitness -= 0.1;

        return fitness;
    }

    private double EvaluateFacilitatorLoad(string facilitator, Schedule schedule)
    {
        double score = 0;

        // Get all activities assigned to the facilitator
        var activitiesByFacilitator = schedule.ScheduledActivities
            .Where(sa => sa.AssignedFacilitator == facilitator)
            .OrderBy(sa => GetTimeInHours(sa.TimeSlot))
            .ToList();

        // Check total number of activities
        if (activitiesByFacilitator.Count > 4)
            score -= 0.5;
        else if (activitiesByFacilitator.Count <= 2 && facilitator != "Dr. Tyler")
            score -= 0.4;

        // Check for activities in the same time slot
        var activitiesInSameTimeSlot = activitiesByFacilitator
            .GroupBy(sa => sa.TimeSlot)
            .Where(g => g.Count() > 1);

        foreach (var group in activitiesInSameTimeSlot)
        {
            score -= 0.2; // Penalty for multiple activities in the same time slot
        }

        // Check for consecutive time slots
        for (int i = 0; i < activitiesByFacilitator.Count - 1; i++)
        {
            // only assigned 1 activity
            if (activitiesByFacilitator.Count == 1) 
            {
                score += 0.2;
                break;
            }
            
            var currentActivity = activitiesByFacilitator[i];
            var nextActivity = activitiesByFacilitator[i + 1];

            if (GetTimeInHours(nextActivity.TimeSlot) - GetTimeInHours(currentActivity.TimeSlot) == 1)
            {
                // Check if one activity is in Roman or Beach and the other isn’t
                if ((currentActivity.Room.Name == "Roman" || currentActivity.Room.Name == "Beach") !=
                    (nextActivity.Room.Name == "Roman" || nextActivity.Room.Name == "Beach"))
                {
                    score -= 0.4;
                } else
                {
                    score += 0.5; // Consecutive activities
                }
            }
        }

        return score;
    }


    private double EvaluateActivitySpecificAdjustments(ScheduledActivity activity, Schedule schedule)
    {
        double score = 0;

        // Intra-Activity Adjustments
        if (activity.Activity.Name.Contains("SLA101") || activity.Activity.Name.Contains("SLA191"))
        {
            var relatedActivities = schedule.ScheduledActivities
                .Where(sa => sa.Activity.Name == activity.Activity.Name);

            foreach (var relatedActivity in relatedActivities)
            {
                if (relatedActivity == activity) continue;

                var timeDifference = Math.Abs(
                    GetTimeInHours(activity.TimeSlot) - GetTimeInHours(relatedActivity.TimeSlot));

                if (timeDifference > 4)
                    score += 0.5; // More than 4 hours apart
                else if (timeDifference == 0)
                    score -= 0.5; // Same time slot
            }
        }

        // Inter-Activity Adjustments
        if (activity.Activity.Name.Contains("SLA101") || activity.Activity.Name.Contains("SLA191"))
        {
            var relatedActivities = schedule.ScheduledActivities
                .Where(sa => (sa.Activity.Name.Contains("SLA101") || sa.Activity.Name.Contains("SLA191")) && sa != activity)
                .OrderBy(sa => GetTimeInHours(sa.TimeSlot))
                .ToList();

            foreach (var relatedActivity in relatedActivities)
            {
                // Ensure the comparison is between SLA 101 and SLA 191
                if (activity.Activity.Name == relatedActivity.Activity.Name) continue;

                var timeDifference = Math.Abs(
                    GetTimeInHours(activity.TimeSlot) - GetTimeInHours(relatedActivity.TimeSlot));

                if (timeDifference == 1)
                {

                    // Roman/Beach logic
                    if ((activity.Room.Name == "Roman" || activity.Room.Name == "Beach") !=
                        (relatedActivity.Room.Name == "Roman" || relatedActivity.Room.Name == "Beach"))
                    {
                        score -= 0.4;
                    }
                    else
                    {
                        score += 0.5; // Consecutive time slots
                    }  
                }
                else if (timeDifference == 0)
                {
                    score -= 0.25; // Same time slot
                }
                else if (timeDifference == 2)
                {
                    score += 0.25; // 2-hour separation
                }
            }
        }

        return score;
    }


    private int GetTimeInHours(string timeSlot)
    {
        // Convert 13:00 to 13
       return int.Parse(timeSlot.Split(':')[0]);
    }

    public List<double> Softmax(List<double> fitnessScores)
    {
        var expScores = fitnessScores.Select(Math.Exp).ToList();
        var sumExpScores = expScores.Sum();
        return expScores.Select(score => score / sumExpScores).ToList();
    }

}
