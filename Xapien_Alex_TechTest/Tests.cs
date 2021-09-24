using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Xapien_Alex_TechTest
{
    public class Tests
    {
        // This one isn't really a Unit Test, just runs the algorithm on the input file and produces an output file.
        [Test]
        public void a_FileInput()
        {
            // Deserialize the input JSON string into a List<string> object for processing.
            string path = Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../org_names.json"));
            var file = File.ReadAllText(path);
            List<string> filelist = JsonConvert.DeserializeObject<List<string>>(file);

            // Run the algorithm on the object
            List<string> test = Logic.Cleanup(filelist);

            // Check the result.json file created for the output. I'm console-printing the reducation in length here.
            Console.WriteLine("Start Length: " + filelist.Count + " | Final Length: " + test.Count);

            // Write the JSON string to a file.
            string resultJSON = JsonConvert.SerializeObject(test, Formatting.Indented);
            File.WriteAllText(Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../result.json")), resultJSON);

            Assert.Pass(); // Just pass the test, and check output file manually - I can't realistically find and expect/compare the "correct" output in the time given.
        }

        [Test]
        public void b_EmptyInput()
        {
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
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
            List<string> test = Logic.Cleanup(new List<string>()
            {
                "john", "mary", "ashley", "john", "john", "mary", "sheibel", "jnoh", "nhoj", "antoine", "antonie",
                "john", "natoine"
            });

            List<string> expected = new List<string>()
            {
                "john", "mary", "ashley", "sheibel", "nhoj", "antoine"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void j_IntertwinedEmptyString()
        {
            List<string> test = Logic.Cleanup(new List<string>()
            {
                "john", "mary", "ashley", "john", "john", "", "mary", "sheibel", "jnoh", "nhoj", "antoine", "antonie",
                "john", "natoine"
            });

            List<string> expected = new List<string>()
            {
                "john", "mary", "ashley", "", "sheibel", "nhoj", "antoine"
            };

            Assert.AreEqual(expected, test);
        }

        [Test]
        public void k_MultipleIntertwinedEmptyStrings()
        {
            List<string> test = Logic.Cleanup(new List<string>()
            {
                "john", "mary", "ashley", "john", "john", "", "mary", "sheibel", "jnoh", "", "nhoj", "antoine", "antonie",
                "john", "natoine", ""
            });

            List<string> expected = new List<string>()
            {
                "john", "mary", "ashley", "", "sheibel", "nhoj", "antoine"
            };

            Assert.AreEqual(expected, test);
        }
        
        [Test] 
        public void l_MultipleContinuousEmptyStrings()
        {
            List<string> test = Logic.Cleanup(new List<string>()
            {
                "john", "mary", "ashley", "john", "john", "", "", "", "", "mary", "sheibel", "jnoh", "", "nhoj", "antoine", "antonie",
                "john", "natoine", ""
            });

            List<string> expected = new List<string>()
            {
                "john", "mary", "ashley", "", "sheibel", "nhoj", "antoine"
            };

            Assert.AreEqual(expected, test);
        }
    }
}
