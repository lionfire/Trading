using LionFire.Trading.Portfolios;
using System.Collections.Generic;

namespace LionFire.Trading.Analysis
{
    public static class CorrelationUtils
    {
        public static string CorrelationId(string id1, string id2) {
            if (id1.CompareTo(id2) < 0) {
                return $"{id1}|{id2}";
            } else {
                return $"{id2}|{id1}";
            }
        }
        public static IEnumerable<string> AllCorrellationIds(IEnumerable<PortfolioComponent> components) {
            var list = new HashSet<string>();
            foreach (var c1 in components) {
                foreach (var c2 in components) {
                    if (c1.ComponentId == c2.ComponentId) continue;
                    var id = CorrelationId(c1.ComponentId, c2.ComponentId);
                    if (!list.Contains(id)) {
                        list.Add(id);
                    }
                }
            }
            return list;
        }
    }
}
