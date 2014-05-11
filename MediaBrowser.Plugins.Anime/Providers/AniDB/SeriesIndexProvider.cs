using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB
{
    public class SeriesIndexProvider : ICustomMetadataProvider<Series>, IPreRefreshProvider
    {

        public async Task<ItemUpdateType> FetchAsync(Series item, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            string aid = item.ProviderIds.GetOrDefault(ProviderNames.AniDb);
            if (string.IsNullOrEmpty(aid))
                aid = await AniDbTitleMatcher.DefaultInstance.FindSeries(Path.GetDirectoryName(item.Path), cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(aid))
            {
                item.AnimeSeriesIndex = 1;
                return ItemUpdateType.MetadataDownload;
            }

            return ItemUpdateType.None;
        }

        public string Name
        {
            get { return "Anime Series Index"; }
        }
    }
}