using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovChain_ArcaniteSauce
{

    public class Word
    {
        public string value { get; }
        private Dictionary<Word, int> neighbours { get; set; }
        private int encounteredNeighbours { get; set; }

        public Word(string name)
        {
            this.value = name;
            this.neighbours = new Dictionary<Word, int>();
            this.encounteredNeighbours = 0;
        }
        public void AddNeighbour(Word neighbour)
        {
            if(neighbour.value == "")
            {
                if (this.value == "")
                {
                    return;
                }
                /*foreach(char softPunct in Chain.softPunctuation)
                {
                    if (this.value.Contains("" + softPunct))
                    {
                        return;
                    }
                }*/
            }
    
            if (!this.neighbours.ContainsKey(neighbour))
            {
                this.neighbours.Add(neighbour, 0);
            }
            this.neighbours[neighbour]++;
            this.encounteredNeighbours++;
        }
        public Word GetRandomNeighbour(Random rng)
        {
                /*
            foreach(KeyValuePair<Word,int> n in this.neighbours)
            {
                Console.WriteLine(n.Key.value);
            }
            Console.ReadLine();*/

            int chosen = rng.Next(this.encounteredNeighbours);
            int temp = 0;
            foreach (KeyValuePair<Word, int> score in this.neighbours)
            {
                temp += score.Value;
                if(temp >= chosen)
                {
                    return score.Key;
                }
            }
            throw new Exception("Word " + this.value + ".GetRandomNeighbour(): unreachable code reached"); //TODO: HANDLE THIS!!
        }
    }
}
