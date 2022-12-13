using System.Collections;
using System.Linq.Expressions;

namespace CCBA.Integrations.LegalEntity
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);

        void AddRange(IEnumerable entities);

        IEnumerable Find(Expression predicate);

        TEntity Get(object Id);

        IEnumerable GetAll();

        void Remove(object Id);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable entities);

        void Update(TEntity entity);
    }
}