﻿using Eisk.Core.DataService;
using Eisk.Core.DataService.EFCore;
using Eisk.Core.Exceptions;
using Eisk.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eisk.Core.DomainService;

public class DomainService<TDomain, TId>
    where TDomain : class, new()
{
    readonly IEntityDataService<TDomain> _entityDataService;

    public DomainService(IEntityDataService<TDomain> entityDataService)
    {
        _entityDataService = entityDataService;
    }

    public virtual async Task<IEnumerable<TDomain>> GetAll()
    {
        return await _entityDataService.GetAll();
    }

    public virtual async Task<TDomain> GetById(TId id)
    {
        if (id.IsNullOrEmpty())
            ThrowExceptionForInvalidLookupIdParameter();

        var entityInDb = await _entityDataService.GetById(id);

        if (entityInDb == null)
            ThrowExceptionForNonExistantEntity(id);

        return entityInDb;
    }

    public virtual async Task<TDomain> Add(TDomain entity)
    {
        return await Add(entity, null);
    }

    public virtual async Task<TDomain> Add(TDomain entity, Action<TDomain> preProcessAction, Action<TDomain> postProcessAction = null)
    {
        if (entity == null)
            ThrowExceptionForNullInputEntity();

        preProcessAction?.Invoke(entity);

        var returnVal = await _entityDataService.Add(entity);

        postProcessAction?.Invoke(returnVal);

        return returnVal;
    }

    //public virtual async Task<TDomain> Update(TId id, TDomain newEntity)
    //{
    //    return await Update(id, newEntity, null);
    //}

    public async Task<TDomain> Update(TId id, TDomain domain)
    {
        // Retrieve the entity using the ID from the URL
        var entity = await GetById(id);
        if (entity == null) return null;

        // Use reflection to set the URL ID on the entity (only if applicable)
        var idProperty = typeof(TDomain).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(domain, id);
        }

        return await Update(id, domain, null);

        //_entityDataService.Update(newEntity);
        //_entityDataService.
        //_dbContext.Entry(entity).CurrentValues.SetValues(domain);
        //await _dbContext.SaveChangesAsync();
        //return entity;
    }


    public virtual async Task<TDomain> Update(TId id, TDomain newEntity, Action<TDomain, TDomain> preProcessAction, Action<TDomain> postProcessAction = null)
    {
        if (newEntity == null)
            ThrowExceptionForNullInputEntity();

        var oldEntity = await GetById(id);

        preProcessAction?.Invoke(oldEntity, newEntity);

        var returnVal = await _entityDataService.Update(newEntity);

        postProcessAction?.Invoke(returnVal);

        return returnVal;
    }

    public virtual async Task Delete(TId id)
    {
        var entityInDb = await GetById(id);

        await _entityDataService.Delete(entityInDb);
    }

    protected virtual void ThrowExceptionForNullInputEntity()
    {
        throw new NullInputEntityException<TDomain>();
    }

    protected virtual void ThrowExceptionForInvalidLookupIdParameter()
    {
        throw new InvalidLookupIdParameterException<TDomain>();
    }

    protected virtual void ThrowExceptionForNonExistantEntity(TId idValue)
    {
        throw new NonExistantEntityException<TDomain>(idValue);
    }
}
