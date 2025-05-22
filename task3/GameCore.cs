using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace task3;
public class GameCore(List<Dice> dice)
{
    private const int MaxFaces = 6;
    private readonly FairRandom _random = new();

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
            var available = Enumerable.Range(0, dice.Count).ToList();

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
        Console.WriteLine($"Computer selected: {dice[choice]}");
        available.Remove(choice);
        return choice;
    }

    private int UserSelect(List<int> available)
    {
        while (true)
        {
            Console.WriteLine("Available dice:");
            for (int i = 0; i < available.Count; i++)
                Console.WriteLine($"{i} - {dice[available[i]]}");

            Console.Write("Choose dice (X-exit, ?-help): ");
            var input = Console.ReadLine();

            if (input?.ToUpper() == "X") Environment.Exit(0);
            if (input == "?") Console.WriteLine(ProbabilityTable.Generate(dice));

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
        var hmac = _random.Prepare(MaxFaces);
        Console.WriteLine($"\n{owner} roll HMAC: {hmac}");
        Console.Write("Enter your modifier (0-5): ");

        if (!GetUserChoice(MaxFaces, out int userMod)) Environment.Exit(0);

        int result = _random.Calculate(userMod, MaxFaces);
        Console.WriteLine($"Computer's modifier: {_random.ComputerValue}");
        Console.WriteLine($"Final index: {result} (Key: {_random.Key})");

        return dice[diceIndex].Faces[result];
    }

    private bool GetUserChoice(int max, out int choice)
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (input?.ToUpper() == "X") { choice = -1; return false; }
            if (input == "?") Console.WriteLine(ProbabilityTable.Generate(dice));

            if (int.TryParse(input, out choice) && choice >= 0 && choice < max)
                return true;

            Console.WriteLine($"Invalid input! Enter 0-{max - 1}");
        }
    }
}