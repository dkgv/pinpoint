using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsApi
    {
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";
        private const string TopStoriesUrl = BaseUrl + "topstories.json";
        private const string SubmissionUrl = BaseUrl + "/item/{0}.json";

        public async Task<List<HackerNewsSubmission>> TopSubmissions(int n = 30)
        {
            // Fetch submission ids
            var entries = await HttpRequestHandler.SendGet(TopStoriesUrl, JArray.Parse);

            // Start tasks to fetch details about each submission
            var submissions = new List<HackerNewsSubmission>();
            var tasks = entries.Take(n).Select(id => Task.Run(async () =>
                {
                    var submission = await HttpRequestHandler.SendGet(string.Format(SubmissionUrl, id),
                        JsonConvert.DeserializeObject<HackerNewsSubmission>);
                    lock (submissions)
                    {
                        submissions.Add(submission);
                    }
                }))
                .ToArray();

            // Wait for all tasks to complete
            Task.WaitAll(tasks);

            return submissions;
        }
    }
}