using System;
using System.Threading.Tasks;
using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.DependencyInjection;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface ISearchColorInDatabase : IFileToModelOnColorSearch, IDisposable { }

    public class SearchColorInDatabase: ISearchColorInDatabase
    {
        private readonly IServiceScope scope;
        private readonly IRepository<Color> colorRepository;

        public SearchColorInDatabase(IServiceProvider serviceProvider)
        {
            this.scope = serviceProvider.CreateScope();
            this.colorRepository = this.scope.ServiceProvider.GetRequiredService<IRepository<Color>>();
        }

        public void Dispose()
        {
            this.scope.Dispose();
        }

        public Task OnDone()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Return the stored color if available, otherwise null
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Color Search(Color color)
        {
            return this.colorRepository.FindOne(x => x.Code == color.Code && x.Label == color.Label);
        }
    }
}
