using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PoetryAPI
{
	public class Word
	{
		public string Lemm;
		public string Text;
		public RussianDict.SpeechPart speechPart;
		public int StressPosition;
		public double Frequency;

		public override string ToString()
        {
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
	}

	public class RussianDict
	{
		public enum SpeechPart
		{
			Unknown,
			Adjective,
			Verb,
			Noun,
			Participle,
		}

		List<Word> words = new List<Word>(5650013);

		public Word Search(string word)
		{
			return words.Find(i => string.Equals(i.Text, word, StringComparison.CurrentCultureIgnoreCase));
		}

		public void LoadDictionaryFull()
        {
			LoadDictionary(0);
			LoadDictionary(1);
			LoadDictionary(2);
			LoadDictionary(3);
        }

		public void LoadDictionaryFast()
		{
			LoadDictionary(0);
		}

		void LoadDictionary(int a)
        {
			string dist;
			switch(a)
            {
				case 0:
					dist = "PoetryAPI.dictionaryFast.txt";
					break;
				case 1:
					dist = "PoetryAPI.dictionaryMedium.txt";
					break;
				case 2:
					dist = "PoetryAPI.dictionarySlow.txt";
					break;
				case 3:
					dist = "PoetryAPI.dictionaryRare.txt";
					break;
				default:
					dist = "PoetryAPI.dictionaryFast.txt";
					break;
			}

			var assembly = IntrospectionExtensions.GetTypeInfo(typeof(RussianDict)).Assembly;
			using (Stream stream = assembly.GetManifestResourceStream(dist))
			{
				using (BufferedStream bs = new BufferedStream(stream))
				{
					using (StreamReader sr = new StreamReader(bs))
					{
						string line;
						string lemm = "";
						double freq = -1;
						while ((line = sr.ReadLine()) != null)
						{
							if (line == "" || line == " ")
							{
								lemm = "";
								freq = -1;
								continue;
							}

							Word word = new Word();
							var parts = line.Split('|');

							word.Text = parts[0].Trim();
							Console.WriteLine("here");
							if (lemm == "")
							{
								word.Lemm = word.Text;
								lemm = word.Lemm;
								try
								{
									freq = double.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
								}
								catch
                                {
									freq = 0;
                                }
								word.Frequency = freq;
							}
							else
							{
								word.Lemm = lemm;
								word.Frequency = freq;
							}

							switch (parts[1].Trim().Split(' ')[0])
							{
								case "гл":
									word.speechPart = SpeechPart.Verb;
									break;
								case "сущ":
									word.speechPart = SpeechPart.Noun;
									break;
								case "прч":
									word.speechPart = SpeechPart.Participle;
									break;
								case "прл":
									word.speechPart = SpeechPart.Adjective;
									break;

								default:
									word.speechPart = SpeechPart.Unknown;
									break;
							}

							string stressed_text = "";
							string stressed_orig = parts[2].Trim().ToLower();
							StringBuilder sb = new StringBuilder(stressed_text);
							for (int i = 0; i < stressed_orig.Length; i++)
							{
								if (stressed_orig[i] == '\'')
								{
									sb[sb.Length - 1] = char.ToUpper(sb[sb.Length - 1]);
								}
								else
								{
									sb.Append(stressed_orig[i]);
								}
							}
							stressed_text = sb.ToString();
							int sp = 0;
							for (int i = 0; i < stressed_text.Length; i++)
							{
								if (char.IsUpper(stressed_text[i]))
									sp = i;
							}

							word.StressPosition = sp;

							words.Add(word);
						}

					}
				}
			}

			return;
		}
	}

}
