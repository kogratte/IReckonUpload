using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;

namespace IReckonUpload.Tools.Tests
{
    [TestClass]
    public class MultipartRequestHelperTests
    {
        [TestMethod]
        public void IsMultipartContentTypeShouldReturnTrue()
        {
            MultipartRequestHelper.IsMultipartContentType("multipart/content").ShouldBeTrue();
            MultipartRequestHelper.IsMultipartContentType("multipart/something").ShouldBeTrue();
        }

        [TestMethod]
        public void IsMultipartContentTypeShouldReturnFalse()
        {
            MultipartRequestHelper.IsMultipartContentType("text/json").ShouldBeFalse();
        }

        [TestMethod]
        public void GetBoundaryShouldThrowAnInvalidDataExceptionIfBoundaryIsMissing()
        {
            Should.Throw<InvalidDataException>(() =>
            {
                var contentType = new MediaTypeHeaderValue(@"multipart/form-data");

                MultipartRequestHelper.GetBoundary(contentType, 50);
            });
        }

        [TestMethod]
        public void GetBoundaryShouldThrowAnInvalidDataExceptionIfBoundaryIsOversized()
        {
            Should.Throw<InvalidDataException>(() =>
            {
                var contentType = new MediaTypeHeaderValue(@"multipart/form-data");
                contentType.Boundary = "----WebKitFormBoundaryzuW5nPZQFQCwQtg4";

                MultipartRequestHelper.GetBoundary(contentType, 5);
            });
        }

        [TestMethod]
        public void GetBoundaryShouldReturnBoundaryFromHeader()
        {
            var contentType = new MediaTypeHeaderValue(@"multipart/form-data");
            contentType.Boundary = "----WebKitFormBoundaryzuW5nPZQFQCwQtg4";

            string foundBoundary = MultipartRequestHelper.GetBoundary(contentType, 50);
            
            foundBoundary.ShouldBe("----WebKitFormBoundaryzuW5nPZQFQCwQtg4");
        }

        [TestMethod]
        public void HasFileContentDispositionShouldReturnFalseIfContentDispositionIsNull()
        {
            MultipartRequestHelper.HasFileContentDisposition(null).ShouldBeFalse();
        }

        [TestMethod]
        public void HasFileContentDispositionShouldReturnFalseIfContentDispositionIsNotFormData()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-other");
            contentDisposition.FileName = "aFilename.txt";

            MultipartRequestHelper.HasFileContentDisposition(contentDisposition).ShouldBeFalse();
        }

        [TestMethod]
        public void HasFileContentDispositionShouldReturnTrueIfContentDispositionIsFormDataAndFilenameIsAvailable()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-data");
            contentDisposition.FileName = "aFilename.txt";

            MultipartRequestHelper.HasFileContentDisposition(contentDisposition).ShouldBeTrue();
        }

        [TestMethod]
        public void HasFileContentDispositionShouldReturnTrueIfContentDispositionIsFormDataAndFilenameStarIsAvailable()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-data");
            contentDisposition.FileNameStar = "aFilename.txt";

            MultipartRequestHelper.HasFileContentDisposition(contentDisposition).ShouldBeTrue();
        }

        [TestMethod]
        public void HasFormDataContentDispositionShouldReturnFalseIfContentDispositionIsNotFormData()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-other");
            
            MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition).ShouldBeFalse();
        }

        [TestMethod]
        public void HasFormDataContentDispositionShouldReturnFalseIfContentDispositionFilenameIsProvided()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-data");
            contentDisposition.FileName = "aFilename.txt";
            contentDisposition.FileNameStar = string.Empty;

            MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition).ShouldBeFalse();
        }

        [TestMethod]
        public void HasFormDataContentDispositionShouldReturnFalseIfContentDispositionFilenameStarIsProvided()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-data");
            contentDisposition.FileName = string.Empty;
            contentDisposition.FileNameStar = "aFilename.txt";

            MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition).ShouldBeFalse();
        }

        [TestMethod]
        public void HasFormDataContentDispositionShouldReturnTrueIfContentDispositionMatch()
        {
            var contentDisposition = new ContentDispositionHeaderValue("form-data");
            contentDisposition.FileNameStar = string.Empty;
            contentDisposition.FileName = string.Empty;

            MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition).ShouldBeTrue();
        }
    }
}
