namespace Pinpoint.Plugin.Everything.API
{
    public class SearchProvider
    {
        public void Search(string query, ISearchConfig config)
        {
            _ = EverythingDll.Everything_SetSearchW(query);
            ConfigureDll(config);
            EverythingDll.Everything_QueryW(true);
        }

        private void ConfigureDll(ISearchConfig config)
        {
            if (config == null)
            {
                return;
            }

            EverythingDll.Everything_SetSort((uint) config.SortMethod);
            EverythingDll.Everything_SetRegex(config.RegexEnabled);
            EverythingDll.Everything_SetMatchCase(config.MatchCase);
            EverythingDll.Everything_SetMax(config.MaxResults);
            EverythingDll.Everything_SetRequestFlags(config.RequestFlag);
        }
    }
}
