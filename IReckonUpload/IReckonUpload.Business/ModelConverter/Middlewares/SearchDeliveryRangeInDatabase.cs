using System;
using System.Threading.Tasks;
using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.DependencyInjection;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface ISearchDeliveryRangeInDatabase : IFileToModelOnRangeSearch, IDisposable { }
    public class SearchDeliveryRangeInDatabase : ISearchDeliveryRangeInDatabase
    {
        private readonly IServiceScope scope;
        private readonly IRepository<DeliveryRange> repository;

        public SearchDeliveryRangeInDatabase(IServiceProvider serviceProvider)
        {
            this.scope = serviceProvider.CreateScope();
            this.repository = this.scope.ServiceProvider.GetRequiredService<IRepository<DeliveryRange>>();
        }

        public void Dispose()
        {
            this.scope.Dispose();
        }

        public Task OnDone()
        {
            return Task.CompletedTask;
        }

        public DeliveryRange Search(DeliveryRange range)
        {
            return this.repository.FindOne(r => r.Raw == range.Raw);
        }
    }
}
