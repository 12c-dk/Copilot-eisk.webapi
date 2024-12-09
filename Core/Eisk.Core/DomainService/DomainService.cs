using Eisk.Core.DataService;
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

    public virtual async Task<TDomain> Update(TId id, TDomain newEntity)
    {
        return await Update(id, newEntity, null);
    }


    public virtual async Task<TDomain> Update(TId id, TDomain newEntity, Action<TDomain, TDomain> preProcessAction, Action<TDomain> postProcessAction = null)
    {
        if (newEntity == null)
            ThrowExceptionForNullInputEntity();

        // Step 1: Load existing entity from the database (this is tracked by EF)
        var existingEntity = await GetById(id);
        if (existingEntity == null)
            ThrowExceptionForNonExistantEntity(id);

        // Step 2: Pre-process hook (optional, if you need to do something before updating)
        preProcessAction?.Invoke(existingEntity, newEntity);

        // Step 3: Update properties of existing entity (but exclude the Id)
        var properties = typeof(TDomain).GetProperties();
        foreach (var property in properties)
        {
            if (property.CanWrite && property.Name != "Id") // Do not update the Id property
            {
                var newValue = property.GetValue(newEntity);
                property.SetValue(existingEntity, newValue);
            }
        }

        // Step 4: Call the data service to persist the changes
        var returnVal = await _entityDataService.Update(existingEntity);

        // Step 5: Post-process hook (optional, if you need to do something after updating)
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
