using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace Pinpoint.Plugin.Bookmarks
{
    public class FirefoxBookmarkExtractor : IBookmarkExtractor
    {
        private readonly IBookmarkFileLocator _locator;
        private const string SqlQuery = "SELECT moz_bookmarks.title, moz_places.url FROM moz_bookmarks LEFT JOIN moz_places WHERE moz_bookmarks.fk = moz_places.id AND moz_bookmarks.title != 'null' AND moz_places.url LIKE '%http%';";

        public FirefoxBookmarkExtractor(IBookmarkFileLocator locator)
        {
            _locator = locator;
        }

        public IEnumerable<AbstractBookmarkModel> Extract()
        {
            var databasePath = _locator.Locate();
            if (string.IsNullOrEmpty(databasePath) || !File.Exists(databasePath))
            {
                yield break;
            }

            using var connection = new SqliteConnection("Data Source=" + databasePath);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = SqlQuery;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var url = reader.GetString(1);

                yield return new DefaultBookmarkModel
                {
                    Name = name,
                    Url = url
                };
            }
        }

        public class WindowsDatabaseLocator : IBookmarkFileLocator
        {
            public string Locate()
            {
                var profileDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Mozilla",
                    "Firefox",
                    "Profiles"
                );

                var directories = Directory.GetDirectories(profileDirectory);
                if (!directories.Any())
                {
                    return string.Empty;
                }

                return Path.Combine(
                    profileDirectory, 
                    directories[0],
                    "places.sqlite"
                );
            }
        }
    }
}