using System.Linq.Expressions;

namespace CinemaBooking.Domain.Repositories;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    T? GetById(params object[] keyValues);
    void Add(T entity);
    void Remove(T entity);
    void Update(T entity);
}