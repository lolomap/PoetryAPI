using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PoetryAPI
{
    public class SpellChecker
    {
        int initialCapacity = 6000000;
        int maxEditDistanceDictionary = 2;
        SymSpell spell;

        public SpellChecker()
        {
            spell = new SymSpell(initialCapacity, maxEditDistanceDictionary);
        }

        public void LoadSpellChecker()
        {
            Assembly assembly = IntrospectionExtensions.GetTypeInfo(typeof(SpellChecker)).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("PoetryAPI.ru-spell.txt"))
            {
                if (!spell.LoadDictionary(stream, 0, 1))
                    throw new Exception("Load Spell Checker failed");
            }
        }

        public string SpellFix(string text)
        {
            List<SymSpell.SuggestItem> suggetions = spell.LookupCompound(text);
            return suggetions[0].term;
        }

    }
}
