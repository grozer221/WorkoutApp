﻿using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkoutApp.WebServer.Business;

namespace WorkoutApp.WebServer.DataAccess
{
    public class BaseRepository<T> where T : BaseModel
    {
        protected readonly AppDbContext context;

        public BaseRepository(AppDbContext context)
        {
            this.context = context;
        }

        public virtual Task<T?> GetByIdAsync(Guid? id, params Expression<Func<T, object>>[] includes)
        {
            return includes.Aggregate(context.Set<T>().AsQueryable(),
                (current, include) => current.Include(include))
                    .FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual Task<List<T>> GetAsync(params Expression<Func<T, object>>[] includes)
        {
            return includes.Aggregate(context.Set<T>().AsQueryable(),
               (current, include) => current.Include(include))
                    .ToListAsync();
        }

        public virtual Task<List<T>> GetAsync(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] includes)
        {
            return includes.Aggregate(context.Set<T>().AsQueryable(),
                 (current, include) => current.Include(include))
                 .Where(condition).ToListAsync();
        }

        public virtual Task<T> GetSingleAsync(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] includes)
        {
            return includes.Aggregate(context.Set<T>().AsQueryable(),
                 (current, include) => current.Include(include))
                 .SingleOrDefaultAsync(condition);
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Added;
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> RemoveAsync(Guid id)
        {
            T? entity = await GetByIdAsync(id);
            if (entity == null)
                throw new NullReferenceException("This record with given id doesn't exist.");
            entity = await RemoveAsync(entity);
            return entity;
        }

        public virtual async Task<T> RemoveAsync(T entity)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
    }
}
