using CCBA.Integrations.LegalEntity.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CCBA.Integrations.LegalEntity
{
    // implementation
    public class ConfigurationRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ConfigurationDatabase db;

        public ConfigurationRepository(ConfigurationDatabase _db)
        {
            db = _db;
        }

        public void Add(TEntity entity)
        {
            db.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable entities)
        {
            db.Set<TEntity>().AddRange((IEnumerable<TEntity>)entities);
        }

        public IEnumerable Find(Expression predicate)
        {
            return db.Set<TEntity>().Where((Expression<Func<TEntity, bool>>)predicate);
        }

        public TEntity Get(object Id)
        {
            return db.Set<TEntity>().Find(Id);
        }

        public IEnumerable GetAll()
        {
            return db.Set<TEntity>().ToList();
        }

        public void Remove(TEntity entity)
        {
            db.Set<TEntity>().Remove(entity);
        }

        public void Remove(object Id)
        {
            var entity = db.Set<TEntity>().Find(Id);
            Remove(entity);
        }

        public void RemoveRange(IEnumerable entities)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            db.Set<TEntity>().RemoveRange(entities);
        }

        public void Update(TEntity entity)
        {
            db.Entry(entity).State = EntityState.Modified;
        }
    }
}