using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IReckonUpload.Business.ModelConverter.Middlewares;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.Logging;

namespace IReckonUpload.Business.ModelConverter.Core
{
    /// <summary>
    /// Convert the input file to the targeted model.
    /// </summary>
    public class FileToModelConverter : IFileToModelConverter
    {
        private List<Color> colors;
        private List<DeliveryRange> deliveryRanges;
        private readonly ILogger<FileToModelConverter> _logger;
        private IServiceProvider _servicesProvider;
        private readonly IList<IFileToModelConverterBaseMiddleware> _middlewares;

        public FileToModelConverter(IServiceProvider servicesProvider, ILogger<FileToModelConverter> logger)
        {
            _logger = logger;
            _servicesProvider = servicesProvider;
            _middlewares = new List<IFileToModelConverterBaseMiddleware>();
        }

        /// <summary>
        /// Add the provided middleware to the middlewares list.
        /// An improvement would be to sort middlewares by their usage, to avoid to find them at each loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public IFileToModelConverter UseMiddleware<T>(Action<T> configure) where T : IFileToModelConverterBaseMiddleware
        {
            var middleware = _servicesProvider.GetService(typeof(T)) as IFileToModelConverterBaseMiddleware;

            configure?.Invoke((T)middleware);

            _middlewares.Add(middleware);

            return this;
        }

        public IFileToModelConverter UseMiddleware<T>() where T : IFileToModelConverterBaseMiddleware
        {
            var middleware = _servicesProvider.GetService(typeof(T)) as IFileToModelConverterBaseMiddleware;

            _middlewares.Add(middleware);

            return this;
        }

        /// <summary>
        /// We can remove the delegate, wich is unused here
        /// </summary>
        /// <param name="pathToFile">The path to the file to import</param>
        /// <returns>The running task</returns>
        public Task ProcessFromFile(string pathToFile)
        {
            return this.ProcessFromFile(pathToFile, (p) => { });
        }

        /// <summary>
        /// Import the file to the destination model.
        /// On each line, the configured middleware are applied with the generated object.
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <param name="process">A delegate how provide an action for each generated entry. Unused. Can be removed</param>
        /// <returns>The running task</returns>
        public async Task ProcessFromFile(string pathToFile, Action<Product> process)
        {
            // If middleware where sorted, we would be able to avoid the search loop.
            // When they're only fews, it's not a big deal, but in a bigger app, it could became time consuming!
            _middlewares.Where(m => m is IFileToModelOnRun).ToList().ForEach(m =>
            {
                try
                {
                    ((IFileToModelOnRun)m).OnRun(pathToFile);

                }
                catch (Exception)
                {
                    throw;
                }
            });

            InitImport();

            using (var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var headers = await GetHeaders(sr);

                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var product = BuildProduct(line, headers);

                            process(product);

                            await ExecuteOnProductReadMiddlewares(product);
                        }
                    }
                }
            }

            _middlewares.ToList().ForEach(async m => await m.OnDone());
        }

        /// <summary>
        /// Execute middleware for the generated entry. In our example this is writing the object to the json file, and adding an entry to the db transaction.
        /// 
        /// As explained here
        /// https://aloiskraus.wordpress.com/2018/11/25/how-fast-can-you-get-with-net-core/
        /// 
        /// it seems the way I parse the file is the fastest one, but I do not have any doubt than there is another - more efficient - way.
        /// 
        /// We can't parallelise the file read, cause we'll explode the SSD throughput.
        /// We can't use a buffer, cause it would cause an huge memory consumption.
        /// 
        /// Parallel execution would also face us in front of new problems with DB transaction...
        /// </summary>
        /// <param name="product">The generated product</param>
        /// <returns>The running task</returns>
        private Task ExecuteOnProductReadMiddlewares(Product product)
        {
            var middlewareTasks = new List<Task>();

            _middlewares.Where(m => m is IFileToModelOnRead).ToList().ForEach(m =>
            {
                middlewareTasks.Add(Task.Run(async () =>
                {
                    // I dislike the retry policy I choose.
                    // In case of rewrite, use polly or another library
                    int tryCount = 3;
                    while (--tryCount > 0)
                    {
                        try
                        {
                            await ((IFileToModelOnRead)m).OnRead(product);
                            tryCount = -1;
                        }
                        catch (Exception e)
                        {

                            _logger.LogError($"{m.GetType()} crashed. {e.Message}", e);
                        }
                    }
                    if (tryCount == 0)
                    {
                        _logger.LogError($"{m.GetType()} crashed 3 times. Removing it.");
                        _middlewares.Remove(m);
                    }
                }));

            });
            return Task.WhenAll(middlewareTasks.ToArray());
        }

        private void InitImport()
        {
            colors = new List<Color>();
            deliveryRanges = new List<DeliveryRange>();
        }

        private Product BuildProduct(string line, Dictionary<string, int> headers)
        {
            var elements = line.Split(',');
            return new Product
            {
                Key = elements[headers["Key"]],
                ArticleCode = elements[headers["ArtikelCode"]],
                Color = BuildColor(elements, headers),
                Description = elements[headers["Description"]],
                Price = double.Parse(elements[headers["Price"]]),
                DiscountedPrice = double.Parse(elements[headers["DiscountPrice"]]),
                Q1 = elements[headers["Q1"]],
                Size = elements[headers["Size"]],
                DeliveredIn = BuildDeliveryRange(elements, headers)
            };
        }

        /// <summary>
        /// Build the color object from the readed line.
        /// 
        /// Middleware usage is fine to get the targeted color from another source, but the result is not cached.
        /// One of the provided middleware is looking into DB, and the cache lack could be a problem on big files.
        /// </summary>
        /// <param name="elements">Splitted readed line</param>
        /// <param name="headers">A list of headers to retrieve the required informations</param>
        /// <returns>The color object</returns>
        private Color BuildColor(string[] elements, Dictionary<string, int> headers)
        {
            var color = new Color
            {
                Code = elements[headers["ColorCode"]],
                Label = elements[headers["Color"]]
            };

            var existingColor = FindColorUsingMiddlewares(color) ?? colors.SingleOrDefault(c => c.Code == color.Code && c.Label == color.Label);

            if (existingColor != null)
            {
                return existingColor;
            }

            colors.Add(color);
            return color;
        }

        /// <summary>
        /// Search the provided color using the configured middlewares.
        /// If middlewares where sorted, the througput would be better.
        /// </summary>
        /// <param name="color">Color to search</param>
        /// <returns>The found color, otherwise, null</returns>
        private Color FindColorUsingMiddlewares(Color color)
        {
            foreach (var middleware in _middlewares.Where(m => m is IFileToModelOnColorSearch).Select(m => m as IFileToModelOnColorSearch).ToList())
            {
                Color knownColor = middleware.Search(color);

                if (knownColor != null)
                {
                    return knownColor;
                }
            }
            return null;
        }

        /// <summary>
        /// Build the delivery range.
        /// 
        /// Could be improved caching the middleware result.
        /// </summary>
        /// <param name="elements">Splitted line</param>
        /// <param name="headers">Headers list</param>
        /// <returns>The delivery range</returns>
        private DeliveryRange BuildDeliveryRange(string[] elements, Dictionary<string, int> headers)
        {
            var range = new DeliveryRange
            {
                Raw = elements[headers["DeliveredIn"]]
            };

            var existingRange = FindDeliveryRangeUsingMiddlewares(range) ?? deliveryRanges.SingleOrDefault(r => r.Raw == range.Raw);

            if (existingRange != null)
            {
                return existingRange;
            }

            if (Regex.IsMatch(range.Raw, @"^\d+-\d+\s+.+$"))
            {
                var matches = Regex.Matches(range.Raw, @"^(?<begin>\d+)-(?<end>\d+)\s+(?<unit>.+)$");

                var group = matches.Single().Groups;
                range.RangeStart = int.Parse(group.Single(g => g.Name == "begin").Value);
                range.RangeEnd = int.Parse(group.Single(g => g.Name == "end").Value);
                range.Unit = group.Single(g => g.Name == "unit").Value;
            }
            deliveryRanges.Add(range);
            return range;
        }

        /// <summary>
        /// Return the delivery range using middlewares
        /// 
        /// Could be improved using sorted middlewares
        /// </summary>
        /// <param name="range">The searched range</param>
        /// <returns>The found range, or null</returns>
        private DeliveryRange FindDeliveryRangeUsingMiddlewares(DeliveryRange range)
        {
            foreach (var middleware in _middlewares.Where(m => m is IFileToModelOnRangeSearch).Select(m => m as IFileToModelOnRangeSearch).ToList())
            {
                DeliveryRange knownRange = middleware.Search(range);

                if (knownRange != null)
                {
                    return knownRange;
                }
            }
            return null;
        }

        private async Task<Dictionary<string, int>> GetHeaders(StreamReader sr)
        {
            Dictionary<string, int> headers = new Dictionary<string, int>();
            var headerLine = await sr.ReadLineAsync();
            string[] _headers = headerLine.Split(',');
            for (var i = 0; i < _headers.Length; i++)
            {
                headers.Add(_headers[i], i);
            }

            return headers;
        }

    }
}
