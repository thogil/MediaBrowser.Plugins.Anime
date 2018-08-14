﻿//using MediaBrowser.Common.Extensions;
//using MediaBrowser.Controller.Configuration;
//using MediaBrowser.Controller.Entities.TV;
//using MediaBrowser.Controller.Library;
//using MediaBrowser.Controller.Localization;
//using MediaBrowser.Controller.Providers;
//using MediaBrowser.Model.Entities;
//using MediaBrowser.Model.Logging;
//using System.Globalization;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//
//namespace MediaBrowser.Plugins.Anime.Providers
//{
//    public class DummySeasonProvider
//    {
//        private readonly IServerConfigurationManager _config;
//        private readonly ILogger _logger;
//        private readonly ILocalizationManager _localization;
//        private readonly ILibraryManager _libraryManager;
//
//        private readonly CultureInfo _usCulture = new CultureInfo("en-US");
//
//        public DummySeasonProvider(IServerConfigurationManager config, ILogger logger, ILocalizationManager localization, ILibraryManager libraryManager)
//        {
//            _config = config;
//            _logger = logger;
//            _localization = localization;
//            _libraryManager = libraryManager;
//        }
//
//        public async Task Run(Series series, CancellationToken cancellationToken)
//        {
//            await RemoveObsoleteSeasons(series).ConfigureAwait(false);
//
//            var hasNewSeasons = await AddDummySeasonFolders(series, cancellationToken).ConfigureAwait(false);
//
//            if (hasNewSeasons)
//            {
//                var directoryService = new DirectoryService(_logger);
//
//                //await series.RefreshMetadata(new MetadataRefreshOptions(directoryService), cancellationToken).ConfigureAwait(false);
//
//                //await series.ValidateChildren(new Progress<double>(), cancellationToken, new MetadataRefreshOptions(directoryService))
//                //    .ConfigureAwait(false);
//            }
//        }
//
//        private async Task<bool> AddDummySeasonFolders(Series series, CancellationToken cancellationToken)
//        {
//            var episodesInSeriesFolder = series.GetRecursiveChildren()
//                .OfType<Episode>()
//                .Where(i => !i.IsInSeasonFolder)
//                .ToList();
//
//            var hasChanges = false;
//
//            // Loop through the unique season numbers
//            foreach (var seasonNumber in episodesInSeriesFolder.Select(i => i.ParentIndexNumber ?? -1)
//                .Where(i => i >= 0)
//                .Distinct()
//                .ToList())
//            {
//                var hasSeason = series.Children.OfType<Season>()
//                    .Any(i => i.IndexNumber.HasValue && i.IndexNumber.Value == seasonNumber);
//
//                if (!hasSeason)
//                {
//                    await AddSeason(series, seasonNumber, cancellationToken).ConfigureAwait(false);
//
//                    hasChanges = true;
//                }
//            }
//
//            // Unknown season - create a dummy season to put these under
//            if (episodesInSeriesFolder.Any(i => !i.ParentIndexNumber.HasValue))
//            {
//                var hasSeason = series.Children.OfType<Season>()
//                    .Any(i => !i.IndexNumber.HasValue);
//
//                if (!hasSeason)
//                {
//                    await AddSeason(series, null, cancellationToken).ConfigureAwait(false);
//
//                    hasChanges = true;
//                }
//            }
//
//            return hasChanges;
//        }
//
//        /// <summary>
//        /// Adds the season.
//        /// </summary>
//        /// <param name="series">The series.</param>
//        /// <param name="seasonNumber">The season number.</param>
//        /// <param name="cancellationToken">The cancellation token.</param>
//        /// <returns>Task{Season}.</returns>
//        public async Task<Season> AddSeason(Series series,
//            int? seasonNumber,
//            CancellationToken cancellationToken)
//        {
//            var seasonName = seasonNumber == 0 ?
//                _config.Configuration.SeasonZeroDisplayName :
//                (seasonNumber.HasValue ? string.Format(_localization.GetLocalizedString("NameSeasonNumber"), seasonNumber.Value.ToString(_usCulture)) : _localization.GetLocalizedString("NameSeasonUnknown"));
//
//            _logger.Info("Creating Season {0} entry for {1}", seasonName, series.Name);
//
//            var season = new Season
//            {
//                Name = seasonName,
//                IndexNumber = seasonNumber,
//                Id = (series.Id + (seasonNumber ?? -1).ToString(_usCulture) + seasonName).GetMBId(typeof(Season))
//            };
//
//            season.SetParent(series);
//
//            await series.AddChild(season, cancellationToken).ConfigureAwait(false);
//
//            await season.RefreshMetadata(new MetadataRefreshOptions(), cancellationToken).ConfigureAwait(false);
//
//            return season;
//        }
//
//        private async Task<bool> RemoveObsoleteSeasons(Series series)
//        {
//            var existingSeasons = series.Children.OfType<Season>().ToList();
//
//            var physicalSeasons = existingSeasons
//                .Where(i => i.LocationType != LocationType.Virtual)
//                .ToList();
//
//            var virtualSeasons = existingSeasons
//                .Where(i => i.LocationType == LocationType.Virtual)
//                .ToList();
//
//            var episodes = series.GetRecursiveChildren().OfType<Episode>().ToList();
//
//            var seasonsToRemove = virtualSeasons
//                .Where(i =>
//                {
//                    if (i.IndexNumber.HasValue)
//                    {
//                        var seasonNumber = i.IndexNumber.Value;
//
//                        // If there's a physical season with the same number, delete it
//                        if (physicalSeasons.Any(p => p.IndexNumber.HasValue && (p.IndexNumber.Value == seasonNumber)))
//                        {
//                            return true;
//                        }
//
//                        // If there are no episodes with this season number, delete it
//                        if (episodes.All(e => !e.ParentIndexNumber.HasValue || e.ParentIndexNumber.Value != seasonNumber))
//                        {
//                            return true;
//                        }
//
//                        return false;
//                    }
//
//                    // Season does not have a number
//                    // Remove if there are no episodes directly in series without a season number
//                    return episodes.All(s => s.ParentIndexNumber.HasValue || !s.IsInSeasonFolder);
//                })
//                .ToList();
//
//            var hasChanges = false;
//
//            foreach (var seasonToRemove in seasonsToRemove)
//            {
//                _logger.Info("Removing virtual season {0} {1}", seasonToRemove.Series.Name, seasonToRemove.IndexNumber);
//
//                await _libraryManager.DeleteItem(seasonToRemove).ConfigureAwait(false);
//
//                hasChanges = true;
//            }
//
//            return hasChanges;
//        }
//    }
//}