
using Flipbook_App.Data;
using Flipbook_App.Repositories.Interfaces;

namespace Flipbook_App.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
	protected readonly FlipbookDBContext context;

	public Repository(FlipbookDBContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public void Add(T entity)
	{
		context.Set<T>().Add(entity);
	}

	public T? GetById(int id)
	{
		return context.Set<T>().Find(id);
	}

	public void Remove(T entity)
	{
		context.Set<T>().Remove(entity);
	}
}
