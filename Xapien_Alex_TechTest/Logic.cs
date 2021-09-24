using System;
using System.Collections.Generic;
using System.Linq;

namespace Xapien_Alex_TechTest
{

    // NOTE: Not great solution in terms of OO-ness, using static functions to get things done quickly. Would be fixed in real world.

    public class Logic
    {
        // Helper Function - Calculate Levenshtein distances between strings using the Wagner-Fischer algorithm, and then calculate similarity with custom logic.
        // Based on pseudocode from https://en.wikipedia.org/wiki/Wagner-Fischer_algorithm.
        private static float SimilarityLevenshtein(string s, string p)
        {
            // 1. The actual algorithm to calculate the Levenshtein Distance.
            int sl = s.Length;
            int pl = p.Length;

            // Create 2-dimensional array, all elements set to 0 - C# initiates it to that by default yay!
            int[,] ld = new int[sl + 1, pl + 1];

            // Initial fill (horizontal & vertical) of arrays according to length of each string
            for (int i = 0; i <= sl; i++) { ld[i, 0] = i; }
            for (int j = 0; j <= pl; j++) { ld[0, j] = j; }

            // Build the matrix with the algorithm
            for (int i = 1; i <= sl; i++)
            {
                for (int j = 1; j <= pl; j++)
                {
                    // Calculate if there's a diff here, then add +1 substitution cost
                    int substitutionCost;
                    if (p[j - 1] == s[i - 1]) substitutionCost = 0;
                    else substitutionCost = 1;

                    // Point = the minimum of either deletions, insertions or substitutions
                    ld[i, j] = Math.Min(ld[i - 1, j] + 1,       // deletions
                        Math.Min(ld[i, j - 1] + 1,              // insertions
                            ld[i - 1, j - 1] + substitutionCost // substitutions
                            )
                        );
                }
            }
            int distance = ld[sl, pl];

            // 2. Calculate similarity based on distance-stringlength ratio. Subtle mistakes maybe here that could be improved?
            float similarity = 1 - ((float)distance / pl);
            return similarity;
        }

        // The actual function that does all the work. Removes duplicates first, then removes "similar" entries and returns the final List.
        public static List<string> Cleanup(List<string> arr)
        {
            // 1. Remove exact duplicates (-- well actually it's "only insert non-duplicates in new list")
            List<string> noDuplicates = new List<string>();
            foreach (string s in arr)
            {
                if (!noDuplicates.Exists(s1 => s1 == s)) noDuplicates.Add(s);
            }

            // 2. Use Levenshtein distance and custom logic to remove similar entries for each string
            // Reversing because need reverse traversal to be able to remove elements while stepping through the Lists. Not great for efficiency, but will have to suffice for now

            List<string> tempReverse = new List<string>(noDuplicates.Reverse<string>());

            // [ASSUMPTION 1]: I'm assuming that >=50% similarity is significant => entries considered to belong to same 'company' => one is eliminated. This should be fine-tuned in real world application.
            // [ASSUMPTION 2]: The selection process of which string to eliminate isn't really specified so I'm just keeping the first one and eliminating the one(s) that come later in the original list. Also should be refined.

            for (int i = tempReverse.Count - 1; i >= 1; i--)
            {
                string s = tempReverse[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    string p = tempReverse[j];
                    if (p == s) continue; // No self-comparison here, this isn't a therapy session :P

                    float similarity = SimilarityLevenshtein(s, p);
                    if (similarity >= 0.5 && similarity < 1)
                    {
                        tempReverse.Remove(tempReverse.Single(t => t == p));
                        i--;
                    }
                }
            }
            return new List<string>(tempReverse.Reverse<string>());
        }
    }
    
}