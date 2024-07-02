using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext db;
        internal DbSet<T> dbSet;  

        public Repository(ApplicationDbContext db)
        {
            this.db = db;
            this.dbSet=db.Set<T>();
            db.Products.Include(u => u.Category);

        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null,bool tracked = false)
        {
            if (tracked)
            {
                IQueryable<T> query = dbSet;
                query = query.Where(filter);

                if (!string.IsNullOrEmpty(includeProperties))
                {
                    foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeprop);
                    }
                }

                return query.FirstOrDefault();
            }
            else
            {
                IQueryable<T> query = dbSet.AsNoTracking();
                query = query.Where(filter);

                if (!string.IsNullOrEmpty(includeProperties))
                {
                    foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeprop);
                    }
                }

                return query.FirstOrDefault();
            }
        }
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        //public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter,string? includeProperties = null)
        //{
        //    IQueryable<T> query = dbSet;
        // if(filter !=null){
        //   // query = query.Where(filter);}

        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(includeprop);
        //        }
        //    }

        //    return query.ToList();
        //}

        public void Remove(T entity)
        {
            dbSet.Remove(entity);   
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
           dbSet.RemoveRange(entity);   
        }
    }
}
