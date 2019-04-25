using IReckonUpload.Business.ModelConverter;
using IReckonUpload.Business.ModelConverter.Core;
using IReckonUpload.Business.ModelConverter.Middlewares;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Tests
{
    [TestClass]
    public class FileToModelConvertTests
    {
        private IFileToModelConverter converter;
        private ServiceCollection services;

        [TestInitialize]
        public void Init()
        {
            this.services = new ServiceCollection();
            services.AddTransient<ICheckSourceFileMiddleware, CheckSourceFileMiddleware>();

            var provider = services.BuildServiceProvider();
            

            this.converter = new FileToModelConverter(provider, new Mock<ILogger<FileToModelConverter>>().Object);
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
                this.converter.Use<CheckSourceFileMiddleware>();

                await this.converter.ProcessFromFile(null, (p) => { });
            });
        }

        [TestMethod]
        public async Task ShouldThrowAnExceptionIfCalledWithoutAnEmptyPath()
        {
            await Should.ThrowAsync<ArgumentException>(async () =>
            {
                this.converter.Use<CheckSourceFileMiddleware>();

                await this.converter.ProcessFromFile(string.Empty, (p) => { });
            });
        }

        [TestMethod]
        public async Task ShouldThrowAnNullReferenceExceptionIfFileIsNotFound()
        {

            await Should.ThrowAsync<NullReferenceException>(async () =>
            {
                this.converter.Use<ICheckSourceFileMiddleware>();

                await this.converter.ProcessFromFile("./toto.tmp");
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

            var productCount = 0;
            await this.converter.ProcessFromFile("./testFile.tmp", (product) => {
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

                productCount++;
            });

            productCount.ShouldBe(1, "A Single product should be extracted from this file");

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

            int productCount = 0;
            await this.converter.ProcessFromFile("./testFile.tmp", (product) =>
            {
                productCount++;

                switch (productCount) {
                    case 1:
                        product.Key.ShouldBe("00000002groe74");
                        product.DeliveredIn.Unit.ShouldBe("werkdagen");
                        product.Q1.ShouldBe("baby");
                        product.Size.ShouldBe("74");
                        break;

                    case 2:
                        product.Key.ShouldBe("00000002groe75");
                        product.DeliveredIn.Unit.ShouldBe("werkdag");
                        product.Q1.ShouldBe("boy");
                        product.Size.ShouldBe("76");
                        break;

                    default:
                        Assert.Inconclusive("You should never be here!!!");
                        break;
                }
            });
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

            Color colorRef = null;
            await this.converter.ProcessFromFile("./testFile.tmp", (p) =>
            {
                if (colorRef == null)
                {
                    colorRef = p.Color;
                } else
                {
                    p.Color.ShouldBeSameAs(colorRef);
                }
            });
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

            DeliveryRange rangeRef = null;

            await this.converter.ProcessFromFile("./testFile.tmp", (p) =>
            {
                if (rangeRef == null)
                {
                    rangeRef = p.DeliveredIn;
                }
                else
                {
                    p.DeliveredIn.ShouldBeSameAs(rangeRef);
                }
            });
        }

    }
}
