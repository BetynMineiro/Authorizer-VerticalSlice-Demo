using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Authorizer.Common
{
    public interface IDao<TEntity> : IDisposable where TEntity : class
    {

        ValueTask<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate = null);
        ValueTask<TEntity> InsertAsync(TEntity entity);
        ValueTask<TEntity> UpdateAsync(TEntity oldEntity, TEntity newEntity);
        void LoadCache(IMemoryCache cache);

    }

    public sealed class Dao<TEntity> : IDao<TEntity> where TEntity : class
    {
        private  List<TEntity> _context = new List<TEntity>();
        private  IMemoryCache _cache;

        public void LoadCache(IMemoryCache cache)
        {
            _cache = cache;

        }

        private void Refresh()
        {
            var key = typeof(TEntity).Name;
            _context = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                entry.SetPriority(CacheItemPriority.High);
                return new List<TEntity>();
            });
        }

        private void UpdateCache()
        {
            var key = typeof(TEntity).Name;
            _cache.Set(key, _context);
        }

        public void Dispose()
        {
            _context.Clear();
            UpdateCache();
        }

        public ValueTask<IEnumerable<TEntity>> GetAsync(Func<TEntity, bool> predicate = null)
        {
            Refresh();
            return new ValueTask<IEnumerable<TEntity>>(predicate is null ? _context : _context.Where(predicate));

        }

        public ValueTask<TEntity> InsertAsync(TEntity entity)
        {
            Refresh();
            _context.Add(entity);
            UpdateCache();
            return new ValueTask<TEntity>(entity);
        }


        public ValueTask<TEntity> UpdateAsync(TEntity oldEntity, TEntity newEntity)
        {
            Refresh();
            _context.Remove(oldEntity);
            _context.Add(newEntity);
            UpdateCache();
            return new ValueTask<TEntity>(newEntity);
        }
    }

    public interface IDaoFactory
    {
        IDao<TEntity> GetInstance<TEntity>() where TEntity : class;
    }

    public class DaoFactory : IDaoFactory
    {
        private readonly IMemoryCache _cache;

        public DaoFactory(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IDao<TEntity> GetInstance<TEntity>() where TEntity : class
        {
           var instance = Activator.CreateInstance<Dao<TEntity>>();
            instance.LoadCache(_cache);
           return instance;
        }
    }
}
