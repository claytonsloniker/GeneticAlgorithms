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
        var activitiesGroupedByTimeSlot = activitiesByFacilitator
            .GroupBy(sa => sa.TimeSlot);

        foreach (var group in activitiesGroupedByTimeSlot)
        {
            if (group.Count() == 1)
            {
                score += 0.2; // Reward for only one activity in this time slot
            }
            else if (group.Count() > 1)
            {
                score -= 0.2; // Penalty for multiple activities in the same time slot
            }
        }

        // Check for consecutive time slots
        for (int i = 0; i < activitiesByFacilitator.Count - 1; i++)
        {
            var currentActivity = activitiesByFacilitator[i];
            var nextActivity = activitiesByFacilitator[i + 1];

            if (GetTimeInHours(nextActivity.TimeSlot) - GetTimeInHours(currentActivity.TimeSlot) == 1)
            {
                // Check if one activity is in Roman or Beach and the other isn’t
                if ((currentActivity.Room.Name == "Roman" || currentActivity.Room.Name == "Beach") !=
                    (nextActivity.Room.Name == "Roman" || nextActivity.Room.Name == "Beach"))
                {
                    score -= 0.4;
                }
                else
                {
                    score += 0.5; // Reward for consecutive activities
                }
            }
        }

        return score;
    }



    private double EvaluateActivitySpecificAdjustments(ScheduledActivity activity, Schedule schedule)
    {
        double score = 0;

        // Intra-Activity Adjustments for matching sections (SLA101A/B and SLA191A/B)
        if (activity.Activity.Name.Contains("SLA101") || activity.Activity.Name.Contains("SLA101B"))
        {
            // Determine the matching section
            string matchingSection = activity.Activity.Name switch
            {
                "SLA101A" => "SLA101B",
                "SLA101B" => "SLA101A",
                "SLA191A" => "SLA191B",
                "SLA191B" => "SLA191A",
                _ => null
            };

            if (matchingSection != null)
            {
                var matchingActivity = schedule.ScheduledActivities
                    .FirstOrDefault(sa => sa.Activity.Name == matchingSection);

                if (matchingActivity != null)
                {
                    var timeDifference = Math.Abs(
                        GetTimeInHours(activity.TimeSlot) - GetTimeInHours(matchingActivity.TimeSlot));

                    if (timeDifference == 0)
                    {
                        score -= 0.5; // Penalty for being in the same time slot
                    }
                    else if (timeDifference > 4)
                    {
                        score += 0.5; // Reward for being more than 4 hours apart
                    }
                }
            }
        }

        // Inter-Activity Adjustments for SLA101 and SLA191
        if (activity.Activity.Name.Contains("SLA101") || activity.Activity.Name.Contains("SLA191"))
        {
            var relatedActivities = schedule.ScheduledActivities
                .Where(sa => (sa.Activity.Name.Contains("SLA101") || sa.Activity.Name.Contains("SLA191")) && sa != activity)
                .OrderBy(sa => GetTimeInHours(sa.TimeSlot))
                .ToList();

            foreach (var relatedActivity in relatedActivities)
            {
                // Skip over matching pairs (e.g., SLA101A skips SLA101B and vice versa)
                if ((activity.Activity.Name == "SLA101A" && relatedActivity.Activity.Name == "SLA101B") ||
                    (activity.Activity.Name == "SLA101B" && relatedActivity.Activity.Name == "SLA101A") ||
                    (activity.Activity.Name == "SLA191A" && relatedActivity.Activity.Name == "SLA191B") ||
                    (activity.Activity.Name == "SLA191B" && relatedActivity.Activity.Name == "SLA191A"))
                {
                    continue;
                }

                var timeDifference = Math.Abs(
                    GetTimeInHours(activity.TimeSlot) - GetTimeInHours(relatedActivity.TimeSlot));

                if (timeDifference == 1)
                {
                    // Roman/Beach logic
                    if ((activity.Room.Name.Contains("Roman") || activity.Room.Name.Contains("Beach")) !=
                        (relatedActivity.Room.Name.Contains("Roman") || relatedActivity.Room.Name.Contains("Beach")))
                    {
                        score -= 0.4; // Penalty for mismatched room categories
                    }
                    else
                    {
                        score += 0.5; // Reward for consecutive time slots
                    }
                }
                else if (timeDifference == 0)
                {
                    score -= 0.25; // Penalty for being in the same time slot
                }
                else if (timeDifference == 2)
                {
                    score += 0.25; // Reward for 2-hour separation
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
