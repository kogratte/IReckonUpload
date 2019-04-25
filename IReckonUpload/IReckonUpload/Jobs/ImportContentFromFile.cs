using IReckonUpload.Business.ModelConverter;
using IReckonUpload.Business.ModelConverter.Core;
using IReckonUpload.Business.ModelConverter.Middlewares;
using System.Threading.Tasks;

namespace IReckonUpload.Jobs
{
    public interface IImportContentFromFile
    {
        Task Execute(string sourceFile, string targetFile);
    }
    public class ImportContentFromFile: IImportContentFromFile
    {
        private readonly IFileToModelConverter _converter;

        public ImportContentFromFile(IFileToModelConverter converter)
        {
            _converter = converter;

            _converter.Use<ICheckSourceFileMiddleware>()
            .Use<IStoreIntoDatabase>()
            .Use<ISearchColorInDatabase>();
        }

        public Task Execute(string sourceFile, string targetFile)
        {
            _converter.Use<IStoreAsJsonFile>(m => m.SetTargetFile(targetFile));
            return _converter.ProcessFromFile(sourceFile);
        }
    }
}
