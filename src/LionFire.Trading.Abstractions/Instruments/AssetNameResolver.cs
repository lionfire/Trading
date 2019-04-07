using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.Trading.Instruments
{
    public static class AssetNameResolver
    {
        public static HashSet<string> ValidAssets;
        static AssetNameResolver()
        {
            ValidAssets = new HashSet<string>();
            foreach (var s in Currencies.Symbols
                .Concat(Commodities.Symbols)
                .Concat(Cryptos.Symbols)
                .Concat(Indices.Symbols.Keys))
            {
                ValidAssets.Add(s);
            }

            // FUTURE: Per-broker Standardizations.  E.g. Oil might be WTI normally, or something else
            Standardizations = new Dictionary<string, string>
            {
                ["XBT"] = "BTC",
                ["GOLD"] = "XAU",
                ["SILVER"] = "XAG",
            };
        }

        public static Dictionary<string, string> Standardizations = new Dictionary<string, string>();

        public static (string longAsset, string shortAsset) ResolvePair(string symbol)
        {
            if (symbol.Length == 6) // Common case for a lot of forex pairs
            {
                var l = symbol.Substring(0, 3);
                var s = symbol.Substring(3, 3);

                l = StandardizeAssetName(l);
                if (l != null)
                {
                    s = StandardizeAssetName(s);
                    if (s != null)
                    {
                        return (l, s);
                    }
                }
            }

            {
                if (symbol.Contains("-"))
                {
                    var chunks = symbol.Split(new char[] { '-' }, 2);
                    StandardizeAssetNames(chunks[0], chunks[1], out string l, out string s);
                    return (l, s);
                }
                if (symbol.Contains("/"))
                {
                    var chunks = symbol.Split(new char[] { '/' }, 2);
                    StandardizeAssetNames(chunks[0], chunks[1], out string l, out string s);
                    return (l, s);
                }

            }

            string standardizedSymbol;
            if(Indices.Symbols.ContainsKey(symbol) )
            {
                return (symbol, Indices.Symbols[symbol]);
            }
            else if (Indices.Symbols.ContainsKey((standardizedSymbol = StandardizeAssetName(symbol))))
            {
                return (standardizedSymbol, Indices.Symbols[standardizedSymbol]);
            }

            return (null, null);
        }

        public static bool StandardizeAssetNames(string l, string s, out string longSymbolResult, out string shortSymbolResult)
        {
            l = StandardizeAssetName(l);
            if (l != null)
            {
                s = StandardizeAssetName(s);
                if (s != null)
                {
                    longSymbolResult = l;
                    shortSymbolResult = s;
                    return true;
                }
            }
            longSymbolResult = null;
            shortSymbolResult = null;
            return false;
        }
        public static string StandardizeAssetName(string assetSymbol)
        {
            if (ValidAssets.Contains(assetSymbol)) return assetSymbol;

            if(Standardizations.ContainsKey(assetSymbol))
            {
                return Standardizations[assetSymbol];
            }

            return null;
        }
    }
}
