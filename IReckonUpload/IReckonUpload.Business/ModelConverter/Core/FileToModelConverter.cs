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

        public Task ProcessFromFile(string pathToFile)
        {
            return this.ProcessFromFile(pathToFile, (p) => { });
        }

        public async Task ProcessFromFile(string pathToFile, Action<Product> process)
        {
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

        private Task ExecuteOnProductReadMiddlewares(Product product)
        {
            var middlewareTasks = new List<Task>();

            _middlewares.Where(m => m is IFileToModelOnRead).ToList().ForEach(m =>
            {
                middlewareTasks.Add(Task.Run(async () =>
                {
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
