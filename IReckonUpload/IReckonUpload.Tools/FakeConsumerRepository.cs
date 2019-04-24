using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IReckonUpload.DAL;
using IReckonUpload.Models.Consumers;

namespace IReckonUpload.Tools
{
    public class FakeConsumerRepository : IRepository<Consumer>
    {
        public void Add(Consumer element)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Consumer> FindMany(Expression<Func<Consumer, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Consumer FindOne(Expression<Func<Consumer, bool>> predicate)
        {
            return new Consumer
            {
                Username = "demo",
                Password = "demo"
            };
        }
    }
}
