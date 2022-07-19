using MySql.Data.MySqlClient;
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
			Pronoun,
			Adverb,
			Preposition,
			Conjuction,
			Particle,
			Interjection,
			Numerable,
			Gerund
		}

		List<Word> words = new List<Word>();

		public Word Search(string word)
		{
			return words.Find(i => string.Equals(i.Text, word, StringComparison.CurrentCultureIgnoreCase));
		}

		public Word SearchDB(string word)
        {
			Word result = null;

			if (Database.DB.IsConnect())
			{
				//suppose col0 and col1 are defined as VARCHAR in the DB
				string query = "SELECT * FROM WordsDictionary WHERE `Text` = '" + word + "'";
				using (MySqlCommand cmd = new MySqlCommand(query, Database.DB.Connection))
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result = new Word();
							//for (int i = 0; i < 8; i++)
							//{
							//	object col = reader.GetValue(i);
							//	System.Diagnostics.Debug.WriteLine(col);
							//}

							if (!reader.HasRows)
								return null;

							if (!reader.IsDBNull(1))
								result.Text = reader.GetString(1);
							if (!reader.IsDBNull(2))
								result.Lemm = reader.GetString(2);
							if (!reader.IsDBNull(3))
								result.Frequency = reader.GetFloat(3);
							if (!reader.IsDBNull(4))
								result.speechPart = (SpeechPart)reader.GetInt32(4);
							if (!reader.IsDBNull(6))
								result.StressPosition = reader.GetInt32(6);
						}
						return result;
					}
				}
			}


			return null;
        }

		private Word SearchFile(string w, int a)
        {
			string dist;
			switch (a)
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
				using (BufferedStream bs = new(stream))
				{
					using (StreamReader sr = new(bs))
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

							if (word.Text != w)
                            {
								continue;
                            }

                            word.speechPart = parts[1].Trim().Split(' ')[0] switch
                            {
                                "гл" => SpeechPart.Verb,
                                "сущ" => SpeechPart.Noun,
                                "прч" => SpeechPart.Participle,
                                "прл" => SpeechPart.Adjective,
                                "дееп" => SpeechPart.Gerund,
                                "нар" => SpeechPart.Adverb,
                                "числ" => SpeechPart.Numerable,
                                "предл" => SpeechPart.Preposition,
                                "част" => SpeechPart.Particle,
                                "союз" => SpeechPart.Conjuction,
                                "межд" => SpeechPart.Interjection,
                                _ => SpeechPart.Unknown,
                            };
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

							//words.Add(word);
							return word;
						}

					}
				}
			}
			return null;
		}

		private Word SearchAllFiles(string w)
        {
			Word word;
			
			for (int i = 0; i < 4; i++)
            {
				if (i == 1)
				{
					word = Search(w);
				}
				else
				{
					word = SearchFile(w, i);
				}
				if (word != null)
					return word;
            }
			return null;
        }

		private void LoadDictionaryFull()
        {
			LoadDictionary(0);
			LoadDictionary(1);
			LoadDictionary(2);
			LoadDictionary(3);
        }

		private void LoadDictionaryFast()
		{
			LoadDictionary(0);
		}

		private void LoadDictionary(int a)
        {
            string dist = a switch
            {
                0 => "PoetryAPI.dictionaryFast.txt",
                1 => "PoetryAPI.dictionaryMedium.txt",
                2 => "PoetryAPI.dictionarySlow.txt",
                3 => "PoetryAPI.dictionaryRare.txt",
                _ => "PoetryAPI.dictionaryFast.txt",
            };
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
                            word.speechPart = parts[1].Trim().Split(' ')[0] switch
                            {
								"гл" => SpeechPart.Verb,
								"сущ" => SpeechPart.Noun,
								"прч" => SpeechPart.Participle,
								"прл" => SpeechPart.Adjective,
								"дееп" => SpeechPart.Gerund,
								"нар" => SpeechPart.Adverb,
								"числ" => SpeechPart.Numerable,
								"предл" => SpeechPart.Preposition,
								"част" => SpeechPart.Particle,
								"союз" => SpeechPart.Conjuction,
								"межд" => SpeechPart.Interjection,
								_ => SpeechPart.Unknown,
							};
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

			Console.WriteLine(words.Count);
			Console.WriteLine(words[0]);

			return;
		}
	}
}
