using System;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IService<TEntity>
        where TEntity : class, IIdentifiable
    {
        IEnumerable<TEntity> GetByIds(IEnumerable<Guid> ids);

        TEntity? GetById(Guid id);
        TEntity? GetByIdComplete(Guid id);
        TEntity? GetByIdWithNavigations(Guid id, IEnumerable<string> navigations);

        bool Create(TEntity entity);
        bool Update(TEntity entity);
        bool Delete(Guid id);
    }
