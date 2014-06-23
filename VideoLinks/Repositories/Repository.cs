using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VideoLinks.Repositories
{
    public class Repository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        protected TContext _context;
        private DbSet<TEntity> _entityDbSet;

        public Repository(TContext context)
        {
            _context = context;
            _entityDbSet = _context.Set<TEntity>();
        }

        public IQueryable<TEntity> Items
        {
            get
            {
                return _entityDbSet;
            }
        }

        public TEntity AddItem(TEntity newItem)
        {
            return _entityDbSet.Add(newItem);
        }

        public TEntity UpdateItem(TEntity newItem)
        {
            return _entityDbSet.Attach(newItem);
        }


        public List<TEntity> ItemList()
        {
            return _entityDbSet.ToList<TEntity>();
        }

        public TEntity FindByID(int id)
        {
            return _entityDbSet.Find(id);
        }

        public TEntity RemoveByID(int id)
        {
            var toRemove = this.FindByID(id);
            if (toRemove != null)
            {
                this.Remove(toRemove);
            }

            return toRemove;
        }

        public TEntity Remove(TEntity toBeRemoved)
        {
            return _entityDbSet.Remove(toBeRemoved);
        }

        public int SaveChanges()
        {
            
            return _context.SaveChanges();
        }
    }
}