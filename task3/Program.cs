using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace task3
{
    public class Dice
    {
        public int[] Faces { get; }
        public Dice(int[] faces) => Faces = faces;
        public override string ToString() => $"[{string.Join(",", Faces)}]";
    }

    public class FairRandom
    {
        private byte[] _key;
        private int _computerValue;
        public int ComputerValue => _computerValue;
        public string Key => BitConverter.ToString(_key).Replace("-", "").ToLower();

        public string Prepare(int max)
        {
            using var rng = RandomNumberGenerator.Create();
            _key = new byte[32];
            rng.GetBytes(_key);

            byte[] buffer = new byte[4];
            uint maxAcceptable = uint.MaxValue - (uint)(uint.MaxValue % (uint)max);

            do rng.GetBytes(buffer);
            while (BitConverter.ToUInt32(buffer) > maxAcceptable);

            _computerValue = (int)(BitConverter.ToUInt32(buffer) % (uint)max);
            return BitConverter.ToString(
    new HMACSHA256(_key).ComputeHash(BitConverter.GetBytes(_computerValue))
).Replace("-", "").ToLower();

        }

        public int Calculate(int user, int max) => (_computerValue + user) % max;
    }

    public class ProbabilityTable
    {
        public static string Generate(List<Dice> dice)
        {
            var sb = new StringBuilder().Append("        |");
            foreach (var d in dice) sb.Append($" D{dice.IndexOf(d) + 1,-5} |");
            sb.Append("\n--------|").Append(string.Join("", dice.Select(_ => "-------|")));

            for (int i = 0; i < dice.Count; i++)
            {
                sb.Append($"\nDice {i + 1,-2} |");
                for (int j = 0; j < dice.Count; j++)
                    sb.Append(i == j ? "   -    |" :
                        $" {CalculateProbability(dice[i], dice[j]):F2}  |");
            }
            return sb.ToString();
        }

        private static double CalculateProbability(Dice a, Dice b) =>
            a.Faces.Sum(f => b.Faces.Count(bf => f > bf)) / 36.0;
    }

    public class GameCore
    {
        private readonly List<Dice> _dice;
        private readonly FairRandom _random = new();

        public GameCore(List<Dice> dice) => _dice = dice;

        public void Run()
        {
            try
            {
                Console.WriteLine("Determining first player...");
                var hmac = _random.Prepare(2);
                Console.WriteLine($"HMAC: {hmac}\nChoose 0 or 1 (X-exit, ?-help):");

                if (!GetUserChoice(2, out int userGuess)) return;

                int result = _random.Calculate(userGuess, 2);
                Console.WriteLine($"Computer's choice: {result} (Key: {_random.Key})");

                bool userFirst = result == 1;
                var available = Enumerable.Range(0, _dice.Count).ToList();

                var selections = userFirst
                    ? new[] { UserSelect(available), ComputerSelect(available) }
                    : new[] { ComputerSelect(available), UserSelect(available) };

                var rolls = new[]
                {
                    PerformRoll(selections[0], "Your"),
                    PerformRoll(selections[1], "Computer's")
                };

                Console.WriteLine($"\nYour roll: {rolls[userFirst ? 0 : 1]}");
                Console.WriteLine($"Computer's roll: {rolls[userFirst ? 1 : 0]}");
                Console.WriteLine(rolls[0] > rolls[1] ? "You win!" : "Computer wins!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private int ComputerSelect(List<int> available)
        {
            int choice = available[RandomNumberGenerator.GetInt32(available.Count)];
            Console.WriteLine($"Computer selected: {_dice[choice]}");
            available.Remove(choice);
            return choice;
        }

        private int UserSelect(List<int> available)
        {
            while (true)
            {
                Console.WriteLine("Available dice:");
                for (int i = 0; i < available.Count; i++)
                    Console.WriteLine($"{i} - {_dice[available[i]]}");

                Console.Write("Choose dice (X-exit, ?-help): ");
                var input = Console.ReadLine();

                if (input?.ToUpper() == "X") Environment.Exit(0);
                if (input == "?") Console.WriteLine(ProbabilityTable.Generate(_dice));

                if (int.TryParse(input, out int index) && index >= 0 && index < available.Count)
                {
                    int selected = available[index];
                    available.RemoveAt(index);
                    return selected;
                }
                Console.WriteLine("Invalid selection!");
            }
        }

        private int PerformRoll(int diceIndex, string owner)
        {
            var hmac = _random.Prepare(6);
            Console.WriteLine($"\n{owner} roll HMAC: {hmac}");
            Console.Write("Enter your modifier (0-5): ");

            if (!GetUserChoice(6, out int userMod)) Environment.Exit(0);

            int result = _random.Calculate(userMod, 6);
            Console.WriteLine($"Computer's modifier: {_random.ComputerValue}");
            Console.WriteLine($"Final index: {result} (Key: {_random.Key})");

            return _dice[diceIndex].Faces[result];
        }

        private bool GetUserChoice(int max, out int choice)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input?.ToUpper() == "X") { choice = -1; return false; }
                if (input == "?") Console.WriteLine(ProbabilityTable.Generate(_dice));

                if (int.TryParse(input, out choice) && choice >= 0 && choice < max)
                    return true;

                Console.WriteLine($"Invalid input! Enter 0-{max - 1}");
            }
        }
    }

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
}