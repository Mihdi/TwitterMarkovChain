using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovChain_ArcaniteSauce
{
    public class Chain
    {
        private List<Word> wordList;
        private Random rnd { get; }
        public Word theEmptyWord;

        public static readonly char[] hardPunctuation = new char[] { '.', '!', '?'/*, '"' */};
        public static readonly char[] softPunctuation = new char[] { ',', ';', ':','(',')' };

        public Chain()
       {
            this.wordList = new List<Word>();
            this.rnd = new Random();
            this.wordList.Add(new Word("")); //serves as beginning and end of the sentence
            this.theEmptyWord = this.wordList.Find(w => w.value == "");
       }

       public void LearnWords(string source)
       {
       //get the file's content
            string temp = File.ReadAllText(source).Trim();
            temp = temp.Replace("\"", "").Replace(""+'\n',""); //dealing with the '"' char is a mess and I don't want to spend time on that, hence this line
            string[] sentences = temp.Split('\n');
               
            
            //Determine the hard punctuation's place
            for(int i = 0; i < Chain.hardPunctuation.Length; i++)
            {
                //Console.WriteLine("Parsing hard punctuation #" + i);
                sentences = Parse2Sentences(sentences, Chain.hardPunctuation[i]);
            }

            //Determine the soft punctuation's place
            int debug_i = 0;
            foreach(char softPunct in softPunctuation)
            {
               // Console.WriteLine("Parsing soft punctuation #" + debug_i);
                sentences = SoftPunctuationParser(sentences, softPunct);
            }

            //Determine places of words in sentence
            string[][] sentencesAsChainOfWords = new string[sentences.Length][];
            for(int i = 0; i < sentences.Length; i++)
            {
                //Console.WriteLine("sentence to chain of words #" + i);
                sentencesAsChainOfWords[i] = Regex.Split(sentences[i], "[^\\S\\r\\n]{1,}");
            }

            //Wordify
            List<Word> wordsInSentences = new List<Word>();
            for(int i = 0; i < sentencesAsChainOfWords.Length; i++)//for each sentence
            {
                for(int j = 0; j < sentencesAsChainOfWords[i].Length; j++) //for each word in the sentence
                {
                    //Console.WriteLine("words in sentence #" + i +j);
                    Wordify(ref wordsInSentences, sentencesAsChainOfWords[i][j].Trim());
                }
            }

            //addNeighbours
            int wordifyCounter = 0;
            foreach(string[] s in sentencesAsChainOfWords)
            {
                this.theEmptyWord.AddNeighbour(wordsInSentences[wordifyCounter]);
       
                for(int i = 0; i < s.Length-1; i++)
                {
                  //  Console.WriteLine("Wordify #" + wordifyCounter);
                    wordsInSentences[wordifyCounter++].AddNeighbour(wordsInSentences[wordifyCounter]);
                }

                wordsInSentences[wordifyCounter++].AddNeighbour(theEmptyWord);
            }

            //add the words to this.wordList
            foreach(Word neoWord in wordsInSentences)
            {
                this.AddWord(neoWord);
            }
        }
        private void Wordify(ref List<Word> ListOfWords, string wordValue)
        {
            Word sameExistingWord= ListOfWords.Find(w => w.value == wordValue);
            
            if(sameExistingWord == null)
            {
                sameExistingWord = this.wordList.Find(w => w.value == wordValue);

                if(sameExistingWord == null)
                {
                    ListOfWords.Add(new Word(wordValue));
                }
                else
                {
                    ListOfWords.Add(sameExistingWord);
                }
            }
            else
            {
                ListOfWords.Add(sameExistingWord);
            }
        }
        // private string determinePunctuationPlace(string og, char c)
        //  {
        /*string[] valOfWords = og.Split(c);
        string output = "";

        for (int i = 0; i < valOfWords.Length - 1; i++)
        {
            output += valOfWords[i] + c;
        }

        if (og[og.Length - 1] == c)
        {
            valOfWords[valOfWords.Length - 1] += c;
        }
            output += valOfWords[valOfWords.L]

        return output;*/
        // }
        private string[] Parse2Sentences(string str2Parse, char endOfSentence) {

            string[] sentences = str2Parse.Split(endOfSentence);

            for(int i = 0; i < sentences.Length-1; i++)
            {
                sentences[i] += endOfSentence;
            }
            if(str2Parse[str2Parse.Length -1] == endOfSentence)
            {
                sentences[sentences.Length - 1] += endOfSentence;
            }
            return sentences;
        }
        
        private string[] Parse2Sentences(string[] oldSentences, char endOfSentence)
        {
            List<string> neoSentences = new List<string>();

            foreach(String s in oldSentences)
            {
                string[] sentencesInS = s.Split(endOfSentence);

                for(int i = 0; i < sentencesInS.Length -1; i++)
                {
                    neoSentences.Add(sentencesInS[i] +" "+ endOfSentence);
                }

                string lastSentenceInS = sentencesInS[sentencesInS.Length - 1]; //this line isn't really needed, but I believe it is better for the readability
                if(s[s.Length-1] == endOfSentence)
                {
                    lastSentenceInS += endOfSentence;
                }
                neoSentences.Add(lastSentenceInS);
            }

            return StrList2StrArray(neoSentences);
        }
        private string[] SoftPunctuationParser(string[] sentences, char softPunct)
        {
            for(int i = 0; i < sentences.Length; i++)
            {
                sentences[i] = sentences[i].Replace(""+softPunct, " " +softPunct+" ");
            }
            return sentences;
        }
        private String[] StrList2StrArray(List<String> strList)
        {
            string[] output = new string[strList.Count];
            for (int i = 0; i < strList.Count; i++)
            {
                output[i] = strList[i];
            }
            return output;
        }

       private void AddWord(Word word)
       {
            //update this to take into account having 2 different words with the same name
            if (!this.wordList.Contains(word))
            {
                this.wordList.Add(word);
            }
       }

       private string GenerateSentence()
       {
            //init
            Word NextWord = this.theEmptyWord.GetRandomNeighbour(this.rnd);
            string output = "";

            //gen sentence
            while(NextWord != this.theEmptyWord)
            {
                output += NextWord.value+" ";
                NextWord = NextWord.GetRandomNeighbour(this.rnd);
            }
            foreach(char softPunct in Chain.softPunctuation)
            {
                output = output.Replace(" "+softPunct, softPunct + "");
            }
            foreach (char hardPunct in Chain.hardPunctuation)
            {
                output = output.Replace(" " + hardPunct, hardPunct + "");
            }
            output = First2Upper(output);
            
            return output;
       }
        private string First2Upper(string str)
        {
            string output ="";

            if(str.Length > 0)
            {
                output = (""+str[0]).ToUpper();
            }

            for(int i = 1; i < str.Length; i++)
            {
                output += str[i]+"";
            }

            return output;
        }

       public void PrintSentence()
       {
            Console.WriteLine(this.GenerateSentence());
       }
     /*  private string PreStringSplitter(string s)
       {
            string output ="";
            string temp = "";
            char[] softPunctuation = new char[] {',',';',':', '\n'};
            char[] hardPunctuation = new char[] { '.', '!', '?', '"' };
            foreach(char c in s)
            {
                bool isHardPunctuation = hardPunctuation.Contains(c);
                if (softPunctuation.Contains(c) || isHardPunctuation)
                {
                    output += " "+temp + " "+c;
                    temp = "";

                    if (isHardPunctuation)
                    {
                        output += ' '+'\n'+' ';
                    }
                }
                else
                {
                    temp += c;
                }
            }
            return output;
       }*/
    }
}
