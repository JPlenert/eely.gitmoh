// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace eely.gitmoh
{
    record ArgumentItem(string Key, string Value, int LeadingDashes);

    internal class ArgumentList : ReadOnlyCollection<ArgumentItem>
    {
        public ArgumentList(IList<ArgumentItem> list) : base(list) { }

        public static ArgumentList Create(string[] args)
        {
            var itemList = new List<ArgumentItem>();
            foreach (string arg in args)
            {
                int leadingDashes = arg.TakeWhile(x => x == '-').Count();
                string key = null;
                string value = null;

                int eqIdx = arg.IndexOf("=");
                if (eqIdx == -1)
                {
                    key = arg.Substring(leadingDashes);
                }
                else
                {
                    key = arg.Substring(leadingDashes, eqIdx - leadingDashes);
                    value = arg.Substring(eqIdx+1);
                    if ((value.StartsWith('\"') && value.EndsWith('\"')) || value.StartsWith('\'') && value.EndsWith('\''))
                        value = value.Substring(1, value.Length - 2);
                }
                itemList.Add(new ArgumentItem(key, value, leadingDashes));
            }
            return new ArgumentList(itemList);
        }

        public string GetDoubleDashArg(string key) => 
            this.FirstOrDefault((x) => x.Key == key && x.LeadingDashes == 2)?.Value;


    }
}
