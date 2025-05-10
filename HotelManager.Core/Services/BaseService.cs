using HotelManager.Core.Interfaces;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;

namespace HotelManager.Core.Services;

/// <summary>
/// Provides a base implementation for service classes managing entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity managed by the service.</typeparam>
public abstract class BaseService<TEntity> : IService<TEntity>
        where TEntity : class, IIdentifiable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseService{TEntity}"/> class.
    /// </summary>
    /// <param name="repository">The repository for the entity type.</param>
    protected BaseService(IRepository<TEntity> repository)
    {
        this.Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Gets the repository used by the service.
    /// </summary>
    protected IRepository<TEntity> Repository { get; }

    /// <summary>
    /// Creates a new entity if it is valid.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns><c>true</c> if the entity was created; otherwise, <c>false</c>.</returns>
    public bool Create(TEntity entity)
    {
        if (!this.IsValid(entity)) return false;

        this.Repository.Create(entity);
        return true;
    }

    /// <summary>
    /// Updates an existing entity if it is valid.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns><c>true</c> if the entity was updated; otherwise, <c>false</c>.</returns>
    public bool Update(TEntity entity)
    {
        if (!this.IsValid(entity)) return false;

        this.Repository.Update(entity);
        return true;
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns><c>true</c> if the entity was deleted; otherwise, <c>false</c>.</returns>
    public bool Delete(Guid id)
    {
        var entity = this.Repository.Get(x => x.Id == id);
        if (entity is null) return false;

        this.Repository.Delete(entity);
        return true;
    }

    /// <summary>
    /// Gets entities by a collection of identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to retrieve.</param>
    /// <returns>A collection of entities.</returns>
    public IEnumerable<TEntity> GetByIds(IEnumerable<Guid> ids)
    {
        return this.Repository.GetMany(e => ids.Contains(e.Id));
    }

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    public TEntity? GetById(Guid id)
    {
        return this.Repository.Get(e => e.Id == id);
    }

    /// <summary>
    /// Gets an entity by its identifier, including all related data.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The complete entity if found; otherwise, <c>null</c>.</returns>
    public TEntity? GetByIdComplete(Guid id)
    {
        return this.Repository.GetComplete(e => e.Id == id);
    }

    /// <summary>
    /// Gets an entity by its identifier, including specified navigation properties.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="navigations">The navigation properties to include.</param>
    /// <returns>The entity with navigations if found; otherwise, <c>null</c>.</returns>
    public TEntity? GetByIdWithNavigations(Guid id, IEnumerable<string> navigations)
    {
        return this.Repository.GetWithNavigations(e => e.Id == id, navigations);
    }

    /// <summary>
    /// Determines whether the specified entity is valid.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns><c>true</c> if the entity is valid; otherwise, <c>false</c>.</returns>
    protected virtual bool IsValid(TEntity entity) => true;
}
