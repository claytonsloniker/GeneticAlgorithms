namespace GeneticAlgorithms
{
    public class Room
    {
        public string Name { get; set; }
        public int Capacity { get; set; }

        public Room(string name, int capacity)
        {
            Name = name;
            Capacity = capacity;
        }
    }
}
