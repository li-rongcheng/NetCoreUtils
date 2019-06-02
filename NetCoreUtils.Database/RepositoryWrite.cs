
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetCoreUtils.Database
{
   /**
    * Need to declare an impl in the application project:
    * 
    * public class RepositoryWriter<TEntity>
    *   : RepositoryWrite<TEntity, ApplicationDbContext>
    *   where TEntity : class
    * {
    *   public RepositoryWriter(IUnitOfWork<ApplicationDbContext> unitOfWork)
    *       : base(unitOfWork)
    *   { }
    * }
    */
    public interface IRepositoryWrite<TEntity>
        : ICommittable
        where TEntity : class
    {
        TEntity Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void Remove(Expression<Func<TEntity, bool>> where);
        void RemoveRange(IEnumerable<TEntity> entities);
    }

    /**
     * For DI registration:
     * services.AddScoped(typeof(IRepositoryWrite<,>), typeof(RepositoryWrite<,>));
     */
    public interface IRepositoryWrite<TEntity, TDbContext>
        : IRepositoryWrite<TEntity>
        where TEntity : class
        where TDbContext : DbContext
    { }

    public class RepositoryWrite<TEntity, TDbContext>
        : IRepositoryWrite<TEntity, TDbContext>
        where TEntity : class
        where TDbContext : DbContext
    {
        private IUnitOfWork<TDbContext> _unitOfWork;
        private DbSet<TEntity> dbSet;

        public RepositoryWrite(IUnitOfWork<TDbContext> unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this.dbSet = unitOfWork.Context.Set<TEntity>();
        }

        /// <summary> Reason of not implementing AddAsync:
        /// According to: http://stackoverflow.com/questions/42034282/are-there-dbset-updateasync-and-removeasync-in-net-core
        /// DbSet.AddAsync() should not be used.
        /// 
        /// Quote:
        ///
        /// AddAsync however, only begins tracking an entity but won't actually send any changes 
        /// to the database until you call SaveChanges or SaveChangesAsync. You shouldn't really 
        /// be using this method unless you know what you're doing. The reason the async version 
        /// of this method exists is explained in the docs:
        /// 
        /// This method is async only to allow special value generators, such as the one used by
        /// 'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        /// to access the database asynchronously. For all other cases the non async method should
        /// be used.
        /// 
        /// The same reason is also applied to Remove and Update methods.
        /// </summary>
        public virtual TEntity Add(TEntity entity)
        {
            dbSet.Add(entity);
            return entity;
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            dbSet.AddRange(entities);
        }

        // there is no DbSet.RemoveAsync() available
        public virtual void Remove(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public virtual void Remove(Expression<Func<TEntity, bool>> where)
        {
            IEnumerable<TEntity> objects = dbSet.Where<TEntity>(where).AsEnumerable();
            dbSet.RemoveRange(objects);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            //old impl in EF
            //dbSet.Attach(entity);
            //_unitOfWork.Context.Entry(entity).State = EntityState.Modified;

            // new impl in EF core
            dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            dbSet.UpdateRange(entities);
        }

        public virtual bool Commit()
        {
            return this._unitOfWork.Commit();
        }

        public virtual async Task<bool> CommitAsync()
        {
            return await this._unitOfWork.CommitAsync();
        }
    }

}