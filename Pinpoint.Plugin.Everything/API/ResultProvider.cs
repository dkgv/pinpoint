using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Everything.API
{
    public class ResultProvider
    {
        public async IAsyncEnumerable<QueryResultItem> GetCurrentResult()
        {
            var numResults = EverythingDll.Everything_GetNumResults();
            if (numResults == 0)
            {
                yield break;
            }

            var collection = new BlockingCollection<QueryResultItem>();

            _ = Task.Run(() =>
              {
                  Parallel.For(0, numResults, body: i =>
                  {
                      QueryResultItem item = null;
                      var index = (uint) i;

                      switch (GetResultType(index))
                      {
                          case ResultType.Unknown:
                              break;

                          case ResultType.File:
                              item = CreateResult(index, MakeFile);
                              break;

                          case ResultType.Directory:
                              item = CreateResult(index, MakeDir);
                              break;

                          case ResultType.Volume:
                              break;

                          default:
                              throw new ArgumentOutOfRangeException();
                      }

                      collection.Add(item);
                  });

                  collection.CompleteAdding();
              });

            foreach (var result in collection.GetConsumingEnumerable())
            {
                yield return result;
            }
        }

        private ResultType GetResultType(uint index)
        {
            if (EverythingDll.Everything_IsFileResult(index))
            {
                return ResultType.File;
            }

            if (EverythingDll.Everything_IsFolderResult(index))
            {
                return ResultType.Directory;
            }

            if (EverythingDll.Everything_IsVolumeResult(index))
            {
                return ResultType.Volume;
            }

            return ResultType.Unknown;
        }

        private QueryResultItem MakeFile(string fullPath)
        {
            return new()
            {
                ResultType = ResultType.File,
                Name = Path.GetFileNameWithoutExtension(fullPath),
                FullPath = fullPath
            };
        }

        private QueryResultItem MakeDir(string fullPath)
        {
            return new()
            {
                ResultType = ResultType.Directory,
                Name = Path.GetDirectoryName(fullPath),
                FullPath = fullPath
            };
        }
        private QueryResultItem CreateResult(uint resultIndex, Func<string, QueryResultItem> make)
        {
            var pathBuilder = new StringBuilder(260);
            EverythingDll.Everything_GetResultFullPathNameW(resultIndex, pathBuilder, (uint) pathBuilder.Capacity);
            return make.Invoke(pathBuilder.ToString());
        }
    }
}
