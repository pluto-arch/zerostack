﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ZeroStack.DeviceCenter.Domain.Entities;
using ZeroStack.DeviceCenter.Domain.Exceptions;
using ZeroStack.DeviceCenter.Domain.Repositories;
using ZeroStack.DeviceCenter.Domain.Specifications;
using ZeroStack.DeviceCenter.Domain.UnitOfWork;
using ZeroStack.DeviceCenter.Infrastructure.Specifications;

namespace ZeroStack.DeviceCenter.Infrastructure.EntityFrameworks
{
    public class EfCoreRepository<TDbContext, TEntity> : IRepository<TEntity> where TDbContext : DbContext, IUnitOfWork where TEntity : BaseEntity
    {
        private readonly ISpecificationEvaluator<TEntity> _specification = new SpecificationEvaluator<TEntity>();

        public virtual IAsyncQueryableProvider AsyncExecuter => new EfCoreAsyncQueryableProvider();

        protected readonly TDbContext _dbContext;

        public EfCoreRepository(TDbContext dbContext) => _dbContext = dbContext;

        public IUnitOfWork UnitOfWork => _dbContext;

        public IQueryable<TEntity> Query => DbSet.AsQueryable();

        public virtual DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

        public virtual async Task DeleteAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(entity);

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entities = await DbSet.Where(predicate).ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                DbSet.Remove(entity);
            }

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task<TEntity?> FindAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return includeDetails ? await (await IncludeRelatedAsync()).Where(predicate).SingleOrDefaultAsync(cancellationToken) : await Query.Where(predicate).SingleOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.LongCountAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            return includeDetails ? await (await IncludeRelatedAsync()).ToListAsync(cancellationToken) : await DbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetListAsync(int pageNumber, int pageSize, Expression<Func<TEntity, object>> sorting, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            var queryable = includeDetails ? (await IncludeRelatedAsync()) : Query;

            return await queryable.OrderBy(sorting).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity> InsertAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var savedEntity = DbSet.Add(entity).Entity;

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return savedEntity;
        }

        public virtual async Task<TEntity> UpdateAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            _dbContext.Attach(entity);

            var updatedEntity = _dbContext.Update(entity).Entity;

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return updatedEntity;
        }

        public virtual async Task<TEntity> GetAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(predicate, includeDetails, cancellationToken);

            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity));
            }

            return entity;
        }

        #region Specification Pattern

        public virtual async Task<List<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        public virtual async Task<List<TResult>> GetListAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        public virtual async Task<long> GetCountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public virtual async Task<TEntity> GetAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return (await GetListAsync(specification, cancellationToken)).FirstOrDefault()!;
        }

        public virtual async Task<TResult> GetAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default)
        {
            return (await GetListAsync(specification, cancellationToken)).FirstOrDefault()!;
        }

        protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            return _specification.GetQuery(Query, specification);
        }

        protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification)
        {
            if (specification is null) throw new ArgumentNullException(nameof(specification));
            if (specification.Selector is null) throw new SelectorNotFoundException();

            return _specification.GetQuery(Query, specification);
        }

        public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbSet.RemoveRange(entities);

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            DbSet.UpdateRange(entities);

            if (autoSave)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task<IQueryable<TEntity>> IncludeRelatedAsync(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            var includes = _dbContext.GetService<IOptions<IncludeRelatedPropertiesOptions>>().Value;

            IQueryable<TEntity> query = Query;

            if (propertySelectors is not null)
            {
                propertySelectors.ToList().ForEach(propertySelector =>
                {
                    query = query.Include(propertySelector);
                });
            }
            else
            {
                query = includes.Get<TEntity>()(query);
            }

            return await Task.FromResult(query);
        }

        public virtual async Task LoadRelatedAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class
        {
            await _dbContext.Entry(entity).Collection(propertyExpression).LoadAsync(cancellationToken);
        }

        public virtual async Task LoadRelatedAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression, CancellationToken cancellationToken = default) where TProperty : class
        {
            await _dbContext.Entry(entity).Reference(propertyExpression).LoadAsync(cancellationToken);
        }

        #endregion

        public virtual Func<IQueryable<TEntity>, IQueryable<TEntity>> DefaultWithDetailsFunc => query => query;
    }

    public class EfCoreRepository<TDbContext, TEntity, TKey> : EfCoreRepository<TDbContext, TEntity>, IRepository<TEntity, TKey> where TDbContext : DbContext, IUnitOfWork where TEntity : BaseEntity<TKey>
    {
        public EfCoreRepository(TDbContext dbContext) : base(dbContext) { }

        public virtual async Task<TEntity> GetAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, includeDetails, cancellationToken);

            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity));
            }

            return entity;
        }

        public virtual async Task<TEntity?> FindAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return includeDetails ? await (await IncludeRelatedAsync()).FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken) : await DbSet.FindAsync(new object[] { id! }, cancellationToken);
        }

        public virtual async Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken: cancellationToken);
            if (entity == null)
            {
                return;
            }

            await DeleteAsync(entity, autoSave, cancellationToken);
        }

        public virtual async Task DeleteManyAsync(IEnumerable<TKey> ids, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entities = await base.DbSet.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

            await DeleteManyAsync(entities, autoSave, cancellationToken);
        }
    }
}
