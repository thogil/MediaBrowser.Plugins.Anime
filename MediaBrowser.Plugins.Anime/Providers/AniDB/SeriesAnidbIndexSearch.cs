using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using System.Linq;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB
{
    public class SeriesAnidbIndexSearch {
        readonly IServerConfigurationManager _configurationManager;
        readonly IHttpClient _httpClient;
        readonly IAniDbIdMapperDownloader _downloader;

        public SeriesAnidbIndexSearch(IServerConfigurationManager configurationManager, IHttpClient httpClient, IAniDbIdMapperDownloader downloader) {
            _configurationManager = configurationManager;
            _httpClient = httpClient;
            _downloader = downloader;
        }
        
        public async Task<string> FindSeriesByRelativeIndex(string anidbId, int seasonNumber, CancellationToken cancellationToken) {
            await _downloader.Load(cancellationToken);

            var mapper = GetMapper();
            if (!mapper.Any(x => x.Value.ContainsValue(anidbId)))
                return null;
            var tvdbId = mapper.First(x => x.Value.ContainsValue(anidbId)).Key;
            if (!mapper[tvdbId].ContainsKey(seasonNumber.ToString(CultureInfo.InvariantCulture)))
                return null;
            var id = mapper[tvdbId][seasonNumber.ToString(CultureInfo.InvariantCulture)];
            await AniDbSeriesProvider.GetSeriesData(_configurationManager.ApplicationPaths, _httpClient, id, cancellationToken);
            return id;
        }

        Dictionary<string, Dictionary<string, string>> GetMapper() {

            var mapperFilePath = _downloader.AniDbIdMapperFilePath;
            var xdoc = XDocument.Load(mapperFilePath);

            int test;
            var elements =
                xdoc.Descendants("anime")
                    .Select(
                        x =>
                            new {
                                    tvdbid = x.Attribute("tvdbid") != null ? x.Attribute("tvdbid").Value : "",
                                    anidbid = x.Attribute("anidbid") != null ? x.Attribute("anidbid").Value : "",
                                    season =
                                        x.Attribute("defaulttvdbseason") != null
                                            ? (x.Attribute("defaulttvdbseason").Value != "a" ? x.Attribute("defaulttvdbseason").Value : "1")
                                            : ""
                                }).Where(x => x.season != "0" && x.tvdbid != "" && int.TryParse(x.tvdbid, out test))
                    .GroupBy(x => x.tvdbid)
                    .ToDictionary(x => x.Key,
                        x => x.GroupBy(y => y.season).Select(y => new {k = y.Key, v = y.First()}).ToDictionary(y => y.k, y => y.v.anidbid));
            return elements;
        }
    }
}