using System.Text;

namespace task3;
public static class ProbabilityTable
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