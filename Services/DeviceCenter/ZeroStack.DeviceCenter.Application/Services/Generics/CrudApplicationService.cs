﻿using AutoMapper;
using System.Threading.Tasks;
using ZeroStack.DeviceCenter.Domain.Entities;
using ZeroStack.DeviceCenter.Domain.Repositories;

namespace ZeroStack.DeviceCenter.Application.Services.Generics
{
    public class CrudApplicationService<TEntity, TKey, TGetResponseModel, TGetListRequestModel, TGetListResponseModel, TCreateRequestModel, TUpdateRequestModel> : AlternateKeyCrudApplicationService<TEntity, TKey, TGetResponseModel, TGetListRequestModel, TGetListResponseModel, TCreateRequestModel, TUpdateRequestModel> where TEntity : BaseEntity<TKey>
    {
        protected new IRepository<TEntity, TKey> Repository { get; }

        public CrudApplicationService(IRepository<TEntity, TKey> repository, IMapper mapper) : base(repository, mapper) => Repository = repository;

        protected async override Task DeleteByIdAsync(TKey id) => await Repository.DeleteAsync(id, true);

        protected async override Task<TEntity> GetEntityByIdAsync(TKey id) => await Repository.GetAsync(id);
    }
}
