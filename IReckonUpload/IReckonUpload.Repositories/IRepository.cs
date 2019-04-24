using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IReckonUpload.DAL
{
    public interface IRepository<T> where T: class, new()
    {
        T FindOne(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindMany(Expression<Func<T, bool>> predicate);

        void Add(T element);
    }
}
