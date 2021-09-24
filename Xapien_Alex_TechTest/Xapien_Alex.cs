using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Xapien_Alex_TechTest
{
    // Note - not great organisation and OO-ness. Just made a single class with the logic and tests together for efficiency :P
    public class Tests
    {
        /////////////////////////////////////////////// ALGORITHM LOGIC /////////////////////////////////////////////////////////////////
        
        // Helper Function - Calculate Levenshtein distances between strings using the Wagner-Fischer algorithm, and then calculate similarity with custom logic.
        // Based on pseudocode from https://en.wikipedia.org/wiki/Wagner-Fischer_algorithm.
        private static float SimilarityLevenshtein(string s, string p)
        {
            // 1. The actual algorithm to calculate the Levenshtein Distance.
            
            int sl = s.Length;
            int pl = p.Length;
            
            // If a string is empty, return the other string's length as the difference.
            if (sl == 0) return pl;
            if (pl == 0) return sl;
            
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
                    
                    // Need the minimum distance of either deletions, insertions or substitutions needed to equalize the strings
                    ld[i, j] = Math.Min(ld[i - 1, j] + 1,       // deletions
                        Math.Min(ld[i, j - 1] + 1,              // insertions
                            ld[i - 1, j - 1] + substitutionCost // substitutions
                            )
                        ); 
                }
            }
            
            // 2. Calculate similarity based on distance-stringlength ratio. Subtle mistakes maybe here that could be improved?
            int distance = ld[sl, pl];
            float similarity = 1 - ((float) distance / pl);
            return similarity;
        }
        
        // The actual function that does all the work. Removes duplicates first, then removes "similar" entries and returns the final List.
        private static List<string> Cleanup(List<string> arr)
        {
            // 1. Remove exact duplicates (-- well actually it's "only insert non-duplicates in new list")
            List<string> noDuplicates = new List<string>();
            foreach (string s in arr)
            {
                if (!noDuplicates.Exists(s1 => s1 == s)) noDuplicates.Add(s);
            }

            // 2. Use Levenshtein distance and custom logic to remove similar entries for each string
            // Reversing because need reverse traversal to bez able to remove elements while stepping through the Lists. Not great for efficiency, I know...
            // [ASSUMPTION 1]: I'm assuming that similarity of >50% is significant => entries considered to belong to same 'company' => one is eliminated. This should be fine-tuned in real world application.
            // [ASSUMPTION 2]: The selection of which word to eliminate isn't really specified so I'm just keeping the first one and eliminating the one(s) that comes later in the original list. Also should be refined.
            List<string> tempReverse = new List<string>(noDuplicates.Reverse<string>());
            for (int i = tempReverse.Count - 1; i >= 1; i--)
            {
                string s = tempReverse[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    string p = tempReverse[j];
                    if (p == s) continue; // No self-comparison here, this isn't a therapy session :P
                    
                    float similarity = SimilarityLevenshtein(s, p);
                    if (similarity >=0.5 && similarity < 1)
                    {
                        var remove = tempReverse.Single(r => r == p);
                        tempReverse.Remove(remove);
                        i--;
                    }
                }
            }
            return new List<string>(tempReverse.Reverse<string>());
        }

        /////////////////////////////////////////////// UNIT TESTING /////////////////////////////////////////////////////////////////////////
        
        // This one isn't really a Unit Test, just runs the algorithm on the input file and produces an output file.
        [Test]
        public void a_FileInput()
        {
            // Deserialize the input JSON string into a List<string> object for processing.
            string path = Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../org_names.json"));
            var file = File.ReadAllText(path);
            List<string> filelist = JsonConvert.DeserializeObject<List<string>>(file);
            
            // Run the algorithm on the object
            List<string> test = Cleanup(filelist);
            
            // Check the result.json file created for the output. I'm console-printing the reducation in length here.
            Console.WriteLine("Start Length: " + filelist.Count + " | Final Length: " + test.Count);
            
            // Write the JSON string to a file.
            string resultJSON = JsonConvert.SerializeObject(test,Formatting.Indented);
            File.WriteAllText(Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,@"../../../result.json")), resultJSON);
            
            Assert.Pass(); // Just pass the test, and check output file manually - I can't realistically find and expect/compare the "correct" output in the time given.
        }

        [Test]
        public void b_EmptyInput()
        {
            List<string> test = Cleanup(new List<string>()
            {
            });

            List<string> expected = new List<string>()
            {
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void c_SingleStringInput()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple"
            });

            List<string> expected = new List<string>()
            {
                "Apple"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void d_TwoDuplicates()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple", "Apple"
            });

            List<string> expected = new List<string>()
            {
                "Apple"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void e_MixDuplicates()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple", "Microsoft", "Amazon", "Apple", "Amazon", "Google"
            });

            List<string> expected = new List<string>()
            {
                "Apple", "Microsoft", "Amazon", "Google"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void f_TwoSimilarNames()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple", "Apple, Inc"
            });

            List<string> expected = new List<string>()
            {
                "Apple"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void g_MultipleSimilarNames()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple", "Aplpe", "Alppe", "Aeppl"
            });

            List<string> expected = new List<string>()
            {
                "Apple"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void h_ReversedStrings()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "Apple", "elppA"
            });

            List<string> expected = new List<string>()
            {
                "Apple", "elppA"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void i_Mix()
        {
            List<string> test = Cleanup(new List<string>()
            {
                "john", "mary", "ashley", "john", "john", "mary", "sheibel", "jnoh", "nhoj", "bananape", "banpanae",
                "john", "bapeanana", "nonana"
            });

            List<string> expected = new List<string>()
            {
                "john", "mary", "ashley", "sheibel", "nhoj", "bananape", "nonana"
            };

            Assert.AreEqual(expected, test);
        }
    }
}