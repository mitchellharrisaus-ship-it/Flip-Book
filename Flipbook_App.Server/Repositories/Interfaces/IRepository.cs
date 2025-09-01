namespace Flipbook_App.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
	T? GetById(Guid id);

	void Add(T entity);

	void Remove(T entity);

}
