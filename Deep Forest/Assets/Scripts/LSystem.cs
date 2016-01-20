using UnityEngine;
using System.Text;

public class LSystem{

    Rule[] rules;
    string alphabet;
    int generation;

    // Constructor
    public LSystem(string _axiom, Rule[] ruleset)
    {
        rules = ruleset;
        generation = 0;

        alphabet = _axiom;
    }
    // Class that generates the alphabet for the Lsystem
    public void Generate()
    {
        StringBuilder next = new StringBuilder();

        for (int i = 0; i < alphabet.Length; i++)
        {
            // String to append the alphabet
            string toReplace = "";
            // Get the next character from alphabet
            char current = alphabet[i];
            // Add the character in case no rules are matched
            toReplace += current;
            // Iterate through the rules and append the toReplace string
            // at the end append the next string buffer
            //
            // Future Work: Apply Stochastic rules to randomize generation
            for (int j = 0; j< rules.Length; j++)
            {
                char a = rules[j].a;
                if (a == current)
                {
                    toReplace = rules[j].b;
                    break;
                }
            }
            next.Append(toReplace);
        }
        // replace the old alphabet with a new one and increase generation number
        alphabet = next.ToString();
        generation++;
    }
    // Getters & Setters
    public string GetAlphabet()
    {
        return alphabet;
    }
    public int GetGeneration()
    {
        return generation;
    }
}
