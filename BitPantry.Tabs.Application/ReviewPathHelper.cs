using BitPantry.Tabs.Common;
using Dapper;

namespace BitPantry.Tabs.Application
{
    public class ReviewPathHelper
    {
        private Dictionary<Tab, int> _path;

        public ReviewPathHelper(Dictionary<Tab, int> path)
        {
            _path = path;
        }

        public KeyValuePair<Tab, long>? GetNextStep(Tab tab, long order)
        {
            // try to return next order for current tab

            var nextOrd = order + 1;
            if (_path[tab] >= nextOrd)
                return new KeyValuePair<Tab, long>(tab, nextOrd);

            // return next tab

            var nextTabIndex = _path.Keys.AsList().IndexOf(tab) + 1;
             if (_path.Count > nextTabIndex)
                return new KeyValuePair<Tab, long>(_path.AsList()[nextTabIndex].Key, 1);

            // at end of tabs;

            return null;
        }

    }
}
