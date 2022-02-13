using UnityEngine;

public static class QuoteGenerator
{
    // _ = total earned, - = drinks type
    private static string[] celebrationQuotes = {"Congratulations","Amazing","Wow","Cheers","Goodness"};
    private static string[] lineQuotes = { "You Earned _ -.", "Take _ - for yourself" };
    private static string[] drinksTypes = { "CocaCola", "Sprite", "Kas", "Fanta", "Pepsi", "Red Bull" };


    public static string GetCelebrationQuote()
    {
        var celebrationQuote = celebrationQuotes[Random.Range(0, celebrationQuotes.Length)];
        var lineQuote = lineQuotes[Random.Range(0, lineQuotes.Length)];
        var drinksType = drinksTypes[Random.Range(0, drinksTypes.Length)];

        lineQuote = lineQuote.Replace("_", Random.Range(1, 6).ToString());
        lineQuote = lineQuote.Replace("-", drinksType);

        return $"{celebrationQuote}! {lineQuote}.";
    }


}
