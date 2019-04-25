using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IReckonUpload.DAL
{
    public class GenericRepository<T> : IRepository<T> where T: class, new()
    {
        private readonly DbSet<T> _set;

        public GenericRepository(DbContext dbContext)
        {
            _set = dbContext.Set<T>();
        }

        public void Add(T element)
        {
            _set.Add(element);
        }

        public IEnumerable<T> FindMany(Expression<Func<T, bool>> predicate)
        {
            return _set.Where(predicate);
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return _set.SingleOrDefault(predicate);
        }
    }
}
