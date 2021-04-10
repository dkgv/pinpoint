using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<AbstractBookmarkModel>> Extract()
        {
            var result = new List<AbstractBookmarkModel>();

            var databasePath = _locator.Locate();
            if (string.IsNullOrEmpty(databasePath) || !File.Exists(databasePath))
            {
                return result;
            }

            await using var connection = new SqliteConnection("Data Source=" + databasePath);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = SqlQuery;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var url = reader.GetString(1);

                var bookmarkModel = new DefaultBookmarkModel
                {
                    Name = name,
                    Url = url
                };

                result.Add(bookmarkModel);
            }

            return result;
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

                if (!Directory.Exists(profileDirectory))
                {
                    return string.Empty;
                }

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