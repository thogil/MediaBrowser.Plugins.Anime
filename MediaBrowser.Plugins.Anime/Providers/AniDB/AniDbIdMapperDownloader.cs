using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB {
    /// <summary>
    /// The AniDbTitleDownloader class downloads the anime titles file from AniDB and stores it.
    /// </summary>
    public class AniDbIdMapperDownloader : IAniDbIdMapperDownloader
    {
        /// <summary>
        /// The URL for retrieving a list of all anime titles and their AniDB IDs.
        /// </summary>
        private const string AniDbIdMapperUrl = "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        private readonly IApplicationPaths _paths;
        private readonly ILogger _logger;

        public AniDbIdMapperDownloader(ILogger logger, IApplicationPaths paths)
        {
            _logger = logger;
            _paths = paths;
        }

        /// <summary>
        /// Gets the path to the anidb data folder.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <returns>The path to the anidb data folder.</returns>
        public static string GetDataPath(IApplicationPaths applicationPaths)
        {
            return Path.Combine(applicationPaths.CachePath, "anidb");
        }

        public async Task Load(CancellationToken cancellationToken)
        {
            var dbIdMapperFile = AniDbIdMapperFilePath;
            var fileInfo = new FileInfo(dbIdMapperFile);

            // download titles if we do not already have them, or have not updated for a week
            if (!fileInfo.Exists || (DateTime.UtcNow - fileInfo.LastWriteTimeUtc).TotalDays > 7)
            {
                await DownloadAniDbIdMapper(dbIdMapperFile).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Downloads an xml file from AniDB which contains all of the titles for every anime, and their IDs,
        /// and saves it to disk.
        /// </summary>
        /// <param name="titlesFile">The destination file name.</param>
        private async Task DownloadAniDbIdMapper(string titlesFile)
        {
            _logger.Debug("Downloading new AniDB mapper file.");

            var client = new WebClient();

            await AniDbSeriesProvider.RequestLimiter.Tick();

            using (var stream = await client.OpenReadTaskAsync(AniDbIdMapperUrl))
            using (var writer = File.Open(titlesFile, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(writer).ConfigureAwait(false);
            }
        }

        public string AniDbIdMapperFilePath
        {
            get
            {
                var data = GetDataPath(_paths);
                Directory.CreateDirectory(data);

                return Path.Combine(data, "anime-list.xml");
            }
        }
    }
}