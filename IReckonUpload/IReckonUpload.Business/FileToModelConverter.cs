using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IReckonUpload.Models.Business;

namespace IReckonUpload.Business
{
    public class FileToModelConverter : IFileToModelConverter
    {
        private List<Product> products;
        private List<Color> colors;
        private List<DeliveryRange> deliveryRanges;

        public void InitImport()
        {
            this.products = new List<Product>();
            this.colors = new List<Color>();
            this.deliveryRanges = new List<DeliveryRange>();
        }

        public async Task<IEnumerable<Product>> GetFromFile(string pathToFile)
        {
            InitImport();

            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException(nameof(pathToFile));
            }
            if (!File.Exists(pathToFile))
            {
                throw new NullReferenceException(nameof(pathToFile));
            }

            using (var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var headers = await GetHeaders(sr);

                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        var elements = line.Split(',');
                        var product = new Product
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

                        products.Add(product);
                    }
                }
            }
            return products;
        }

        private Color BuildColor(string[] elements, Dictionary<string, int> headers)
        {
            var color = new Color
            {
                Code = elements[headers["ColorCode"]],
                Label = elements[headers["Color"]]
            };
            var existingColor = this.colors.SingleOrDefault(c => c.Code == color.Code && c.Label == color.Label);

            if (existingColor == null)
            {
                this.colors.Add(color);
                return color;
            }

            return existingColor;
        }

        private DeliveryRange BuildDeliveryRange(string[] elements, Dictionary<string, int> headers)
        {
            var range = new DeliveryRange
            {
                Raw = elements[headers["DeliveredIn"]]
            };

            var existingRange = this.deliveryRanges.SingleOrDefault(r => r.Raw == range.Raw);
            
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
            this.deliveryRanges.Add(range);
            return range;
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
