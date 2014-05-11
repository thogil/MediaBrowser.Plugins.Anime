using System;
using System.IO;
using System.Text;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Plugins.Anime.Providers.AniList;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AniListSeriesProviderTests
    {
        [Test]
        public void TestScrapePage()
        {
            var data = File.ReadAllText("TestData/anilist/9756.html", Encoding.UTF8);

            var series = new Series();

            AniListSeriesProvider.ParseTitle(series, data, "en");
            AniListSeriesProvider.ParseSummary(series, data);
            AniListSeriesProvider.ParseStudio(series, data);
            AniListSeriesProvider.ParseRating(series, data);
            AniListSeriesProvider.ParseGenres(series, data);
            AniListSeriesProvider.ParseDuration(series, data);
            AniListSeriesProvider.ParseAirDates(series, data);

            Assert.That(series.Name, Is.EqualTo("Mahou Shoujo Madoka★Magica"));
            Assert.That(series.Genres, Contains.Item("Drama"));
            Assert.That(series.Genres, Contains.Item("Fantasy"));
            Assert.That(series.Genres, Contains.Item("Psychological Thriller"));
            Assert.That(series.Genres, Contains.Item("Thriller"));
            Assert.That(series.PremiereDate, Is.EqualTo(new DateTime(2011, 1, 7)));
            Assert.That(series.EndDate, Is.EqualTo(new DateTime(2011, 4, 22)));

        }
    }
}