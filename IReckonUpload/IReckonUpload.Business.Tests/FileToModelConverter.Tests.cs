using IReckonUpload.Models.Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Tests
{
    [TestClass]
    public class FileToModelConvertTests
    {
        private FileToModelConverter converter;

        [TestInitialize]
        public void Init()
        {
            this.converter = new FileToModelConverter();
        }
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists("./testFile.tmp"))
            {
                File.Delete("./testFile.tmp");
            }
        }

        [TestMethod]
        public async Task ShouldThrowAnExceptionIfCalledWithoutPathToFile()
        {
            await Should.ThrowAsync<ArgumentException>(async () =>
            {
                await this.converter.GetFromFile(null);
            });
        }

        [TestMethod]
        public async Task ShouldThrowAnExceptionIfCalledWithoutAnEmptyPath()
        {
            await Should.ThrowAsync<ArgumentException>(async () =>
            {
                await this.converter.GetFromFile(string.Empty);
            });
        }

        [TestMethod]
        public async Task ShouldThrowAnNullReferenceExceptionIfFileIsNotFound()
        {
            await Should.ThrowAsync<NullReferenceException>(async () =>
            {
                await this.converter.GetFromFile("./toto.tmp");
            });
        }

        [TestMethod]
        public async Task ShouldReturnTheGeneratedModelBasedOnTheInputFile()
        {
            var testFile = "./testFile.tmp";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(testFile))
            {
                file.AutoFlush = true;
                file.WriteLine("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");
                file.WriteLine("00000002groe74,2,broek,Gaastra,8,0,1-3 werkdagen,baby,74,groen");
            }

            IEnumerable<Product> model = await this.converter.GetFromFile("./testFile.tmp");

            model.ShouldNotBeNull();
            model.Count().ShouldBe(1, "A Single product should be extracted from this file");
            Product product = model.Single();
            product.Id.ShouldBe(0);
            product.Key.ShouldBe("00000002groe74");
            product.ArticleCode.ShouldBe("2");
            product.Color.ShouldNotBeNull();
            product.Color.Id.ShouldBe(0);
            product.Color.Code.ShouldBe("broek");
            product.Color.Label.ShouldBe("groen");
            product.Description.ShouldBe("Gaastra");
            product.Price.ShouldBe(8);
            product.DiscountedPrice.ShouldBe(0);
            product.DeliveredIn.ShouldNotBeNull();
            product.DeliveredIn.RangeStart.ShouldBe(1);
            product.DeliveredIn.RangeEnd.ShouldBe(3);
            product.DeliveredIn.Unit.ShouldBe("werkdagen");
            product.Q1.ShouldBe("baby");
            product.Size.ShouldBe("74");
        }

        [TestMethod]
        public async Task ShouldReturnTheGeneratedModelBasedOnTheInputFile_MultipleLine()
        {
            var testFile = "./testFile.tmp";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(testFile))
            {
                file.AutoFlush = true;
                file.WriteLine("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");
                file.WriteLine("00000002groe74,2,broek,Gaastra,8,0,1-3 werkdagen,baby,74,groen");
                file.WriteLine("00000002groe75,2,broek,Gaastra,8,0,1-3 werkdag,boy,76,groen");
            }

            IEnumerable<Product> products = await this.converter.GetFromFile("./testFile.tmp");

            products.Count().ShouldBe(2);

            products.First().Key.ShouldBe("00000002groe74");
            products.First().DeliveredIn.Unit.ShouldBe("werkdagen");
            products.First().Q1.ShouldBe("baby");
            products.First().Size.ShouldBe("74");

            products.Last().Key.ShouldBe("00000002groe75");
            products.Last().DeliveredIn.Unit.ShouldBe("werkdag");
            products.Last().Q1.ShouldBe("boy");
            products.Last().Size.ShouldBe("76");
        }

        [TestMethod]
        public async Task ShouldOptimizeColorUsage()
        {
            var testFile = "./testFile.tmp";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(testFile))
            {
                file.AutoFlush = true;
                file.WriteLine("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");
                file.WriteLine("00000002groe74,2,broek,Gaastra,8,0,1-3 werkdagen,baby,74,groen");
                file.WriteLine("00000002groe75,2,broek,Gaastra,8,0,1-3 werkdag,boy,76,groen");
            }

            IEnumerable<Product> products = await this.converter.GetFromFile("./testFile.tmp");

            products.Count().ShouldBe(2);
            products.First().Color.ShouldBeSameAs(products.Last().Color);
        }

        [TestMethod]
        public async Task ShouldOptimizeDeliveryRangeUsage()
        {
            var testFile = "./testFile.tmp";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(testFile))
            {
                file.AutoFlush = true;
                file.WriteLine("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");
                file.WriteLine("00000002groe74,2,broek,Gaastra,8,0,1-3 werkdagen,baby,74,groen");
                file.WriteLine("00000002groe75,2,broek,Gaastra,8,0,1-3 werkdagen,boy,76,groen");
            }

            IEnumerable<Product> products = await this.converter.GetFromFile("./testFile.tmp");

            products.Count().ShouldBe(2);
            products.First().DeliveredIn.ShouldBeSameAs(products.Last().DeliveredIn);
        }
    }
}
