using System;
using System.IO;

namespace MarkovChain_ArcaniteSauce
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Yet TODO:
             * +    Handle "Mr." and "Mrs."
             * +    Deal with "..."
             */
            
            Chain chain = new Chain();

            //Console.OutputEncoding = Encoding.Unicode;
            
            foreach (string file in Directory.EnumerateFiles("alexandria\\", "*.txt"))
            {
                chain.LearnWords(file);
            }
            string userChoice;
            
            while ((userChoice = Console.ReadLine()) != "no")
            {
                Console.Clear();
                chain.PrintSentence();
            }

            return;
        }
    }
}
