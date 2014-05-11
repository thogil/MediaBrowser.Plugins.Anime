using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.Providers.AniDB;

namespace MediaBrowser.Plugins.Anime.Providers
{
    public class SeriesIndexProvider
        : ICustomMetadataProvider<Series>, IPreRefreshProvider
    {
        private readonly IAniDbTitleMatcher _titleMatcher;

        public SeriesIndexProvider()
        {
            _titleMatcher = AniDbTitleMatcher.DefaultInstance;
        }

        public async Task<ItemUpdateType> FetchAsync(Series item, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var aniDbId = await FindAniDbId(item, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(aniDbId)) {
                return ItemUpdateType.None;
            }
            item.ProviderIds.Add(ProviderNames.AniDb, aniDbId);
            item.AnimeSeriesIndex = 1;

            return ItemUpdateType.MetadataImport;
        }

        public string Name
        {
            get { return "Anime"; }
        }

        private async Task<string> FindAniDbId(Series series, CancellationToken cancellationToken)
        {
            string aid = series.ProviderIds.GetOrDefault(ProviderNames.AniDb);
            if (string.IsNullOrEmpty(aid))
                aid = await _titleMatcher.FindSeries(series.Name, cancellationToken);
            return aid;
        }
    }
}