using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithms
{
    public class Activity
    {
        public string? Name { get; set; }
        public int ExpectedEnrollment { get; set; }
        public List<string> PreferredFacilitators { get; set; }
        public List<string> OtherFacilitators { get; set; }

        public Activity(string name, int expectedEnrollment, List<string> preferredFacilitators, List<string> otherFacilitators)
        {
            Name = name;
            ExpectedEnrollment = expectedEnrollment;
            PreferredFacilitators = preferredFacilitators;
            OtherFacilitators = otherFacilitators;
        }
    }
}
