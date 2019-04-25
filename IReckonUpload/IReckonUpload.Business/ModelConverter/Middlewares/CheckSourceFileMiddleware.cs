using System;
using System.IO;
using System.Threading.Tasks;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface ICheckSourceFileMiddleware : IFileToModelOnRun { }
    public class CheckSourceFileMiddleware : ICheckSourceFileMiddleware
    {
        public Task OnDone()
        {
            return Task.CompletedTask;
        }

        public Task OnRun(string sourceFile)
        {
            CheckSourceFile(sourceFile);

            return Task.CompletedTask;
        }

        private void CheckSourceFile(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException(nameof(pathToFile));
            }
            if (!File.Exists(pathToFile))
            {
                throw new NullReferenceException(nameof(pathToFile));
            }
        }
    }
}
