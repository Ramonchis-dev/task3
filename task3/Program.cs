namespace task3;
class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length < 3)
                throw new ArgumentException("Requires at least 3 dice arguments\nExample: 2,2,4,4,9,9 1,1,6,6,8,8 3,3,5,5,7,7");

            var dice = args.Select(a =>
                new Dice(a.Split(',').Select(int.Parse).ToArray())).ToList();

            new GameCore(dice).Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid configuration: {ex.Message}");
        }
    }
}
