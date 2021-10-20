using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DLaB.Common;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#if NET
using DataverseUnitTest.Exceptions;
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Manages what the default builder is for the Entity Type, and the specific builder by id
    /// </summary>
    internal class EntityBuilderManager
    {
        #region Properties

        /// <summary>
        /// The builder for each specific entity
        /// </summary>
        /// <value>
        /// The builders.
        /// </value>
        private Dictionary<Guid, BuilderInfo> Builders { get; }

        /// <summary>
        /// Manages the same list of Builders as the Builders Property, but by logical name
        /// </summary>
        /// <value>
        /// The type of the builders by entity.
        /// </value>
        private Dictionary<string, List<BuilderInfo>> BuildersByEntityType { get; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object BuilderConstructorForEntityLock = new object();

        /// <summary>
        /// Contains constructors for the default builders of each entity type.  Whenever a new Builder is needed, this contains the constructor that will be invoked.
        /// </summary>
        /// <value>
        /// The builder for entity.
        /// </value>
        // ReSharper disable once StaticMemberInGenericType
        private static Dictionary<string, ConstructorInfo> DefaultBuilderConstructors { get; }

        /// <summary>
        /// Manages Custom Builder Fluent Actions.  Key is entity Logical Name.  Value is fluent actions to apply to Builder
        /// </summary>
        /// <value>
        /// The custom builder actions.
        /// </value>
        private Dictionary<string, Action<object>> CustomBuilderFluentActions { get; }

        private Dictionary<Guid, Id> Ids { get; }

        #endregion Properties

        #region Constructors

        static EntityBuilderManager()
        {
            var builderInterface = typeof (IEntityBuilder);
            var genericInterface = typeof (IEntityBuilder<>);
            // Load all types that have EntityBuilder<> as a base class
            var entityBuilders = from t in TestSettings.EntityBuilder.Assembly.GetTypes()
                                 where builderInterface.IsAssignableFrom(t) 
                                     && t.IsPublic
                                     && !t.IsAbstract
                                 select new
                                 {
                                     Entity = EntityHelper.GetEntityLogicalName(t.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface).GenericTypeArguments[0]),
                                     Builder = t,
                                     Ass = t.Assembly.GetName().Name
                                 };

            var groupedBuilder = entityBuilders.GroupBy(b => b.Entity).ToList();
            var dup = groupedBuilder.Where(g => g.Count() > 1).Select(g => new { g.Key, Names = g.Select(b => b.Builder.FullName)}).ToList();

            if (dup.Any())
            {
                throw new Exception($"Duplicate EntityBuilders {string.Join(", ", dup.First().Names)} found for Entity {dup.First().Key}.");
            }
            
            DefaultBuilderConstructors = groupedBuilder.ToDictionary(g => g.Key, g => g.First().Builder.GetConstructor(new[] { typeof(Id) }));

            foreach (var builder in DefaultBuilderConstructors)
            {
                if (builder.Key == "entity" || builder.Value != null)
                {
                    continue;
                }

                throw new Exception("Entity Builder " + builder.Key + " does not contain a Constructor of type (Id)!");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBuilderManager" /> class.
        /// </summary>
        public EntityBuilderManager()
        {
            BuildersByEntityType = new Dictionary<string, List<BuilderInfo>>();
            Builders = new Dictionary<Guid, BuilderInfo>();
            CustomBuilderFluentActions = new Dictionary<string, Action<object>>();
            Ids = new Dictionary<Guid, Id>();
        }

        #endregion Constructors

        /// <summary>
        /// Adds the custom fluent action if one doesn't already exist, and combines if one does already exist.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="action">The action.</param>
        private void AddCustomAction(string logicalName, Action<object> action)
        {
            if (CustomBuilderFluentActions.TryGetValue(logicalName, out var customAction))
            {
                // Builder already has custom Action.  Create new custom Action that first calls old, then calls new
                CustomBuilderFluentActions[logicalName] = b =>
                {
                    customAction(b);
                    action(b);
                };
            }
            else
            {
                CustomBuilderFluentActions[logicalName] = action;
            }
        }

        /// <summary>
        /// Applies the custom action to all Builders for the given type.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.Exception"></exception>
        private void ApplyCustomAction<TBuilder>(string logicalName, Action<TBuilder> action) where TBuilder : class, IEntityBuilder
        {
            if (!BuildersByEntityType.TryGetValue(logicalName, out List<BuilderInfo> builders))
            {
                return;
            }

            foreach (var result in builders.Select(b => b.Builder))
            {
                if (!(result is TBuilder builder))
                {
                    throw new Exception($"Unexpected type of builder!  Builder for {logicalName}, was not of type {typeof(TBuilder).FullName}, but type {result.GetType().FullName}.");
                }
                action(builder);
            }
        }

        /// <summary>
        /// Applies all custom fluent actions to the builder
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="builder">The builder.</param>
        private void ApplyCustomActions(string logicalName, object builder)
        {
            if (CustomBuilderFluentActions.TryGetValue(logicalName, out Action<object> customAction))
            {
                customAction(builder);
            }
        }

        /// <summary>
        /// Creates the specified entities in the correct Dependent Order, returning the entities created.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public Dictionary<Guid, Entity> Create(IOrganizationService service)
        {
            var results = new Dictionary<Guid, Entity>();
            var builders = new List<Tuple<Guid, BuilderInfo>>();
            var postCreateUpdates = new List<Entity>();
            foreach (var info in EntityDependency.Mapper.EntityCreationOrder)
            {
                if (!BuildersByEntityType.TryGetValue(info.LogicalName, out List<BuilderInfo> values))
                {
                    // The Entity Creation Order is a Singleton.
                    // If this continue occurs, most likely another instance of a Crm Environment Builder used a type that wasn't utilized by this instance
                    continue;
                }
                foreach (var value in values)
                {
                    try
                    {
                        var entity = CreateEntity(service, value, info.CyclicAttributes, postCreateUpdates);
                        RecordBuiltEntity(entity, value, results, builders);
                    }
                    catch (Exception ex)
                    {
                        var entityName = value.Id.Entity == null ? info.LogicalName : $"Entity {value.Id.LogicalName}{Environment.NewLine}{value.Id.Entity.ToStringAttributes()}";
                        if (string.IsNullOrWhiteSpace(entityName))
                        {
                            entityName = info.LogicalName;
                        }
                        throw new CreationFailureException($"An error occured attempting to create {entityName}.{Environment.NewLine}{ex.Message}", ex);
                    }
                }
            }

            foreach (var entity in postCreateUpdates)
            {
                try
                {
                    service.Update(entity);
                    AddPostCreateAttributesToIdEntity(results, entity); 
                }
                catch (Exception ex)
                {
                    var entityName = $"Entity {entity.LogicalName}{Environment.NewLine}{entity.ToStringAttributes()}";
                    throw new CreationFailureException($"An error occured attempting to update an EntityDependency post create for Entity {entityName}.{Environment.NewLine}{ex.Message}", ex);
                }
            }

            // Process Post Updates
            foreach (var builder in builders)
            {
                builder.Item2.Builder.PostCreate(service, results[builder.Item1]);
            }

            return results;
        }

        private void RecordBuiltEntity(Entity entity, BuilderInfo builder, Dictionary<Guid, Entity> results, List<Tuple<Guid, BuilderInfo>> builders)
        {
            results.Add(entity.Id, entity);
            builders.Add(new Tuple<Guid, BuilderInfo>(entity.Id, builder));
            if (Ids.TryGetValue(entity.Id, out Id id))
            {
                id.Entity = entity;
            }
        }

        private void AddPostCreateAttributesToIdEntity(Dictionary<Guid, Entity> results, Entity postCreateEntity)
        {
            if (results.TryGetValue(postCreateEntity.Id, out Entity entity))
            {
                foreach (var att in postCreateEntity.Attributes)
                {
                    entity[att.Key] = att.Value;
                }
            }

            if (Ids.TryGetValue(postCreateEntity.Id, out Id id))
            {
                foreach (var att in postCreateEntity.Attributes)
                {
                    id.Entity[att.Key] = att.Value;
                }
            }
        }

        /// <summary>
        /// Updates the Builder with any attributes set in the Id's Entity.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="info">The builder.</param>
        /// <param name="cyclicAttributes">The cyclic attributes.</param>
        /// <param name="postCreateUpdates">The post create updates.</param>
        /// <returns></returns>
        private static Entity CreateEntity(IOrganizationService service, BuilderInfo info, IEnumerable<string> cyclicAttributes, List<Entity> postCreateUpdates)
        {
            var entity = info.Id.Entity;
            var builder = info.Builder;
            if (entity != null)
            {
                foreach (var att in entity.Attributes)
                {
                    builder.WithAttributeValue(att.Key, att.Value);
                }
            }

            var attributes = cyclicAttributes as string[] ?? cyclicAttributes.ToArray();
            var postCreateEntity = new Entity(info.Id);
            if (attributes.Length > 0)
            {
                var tmp = builder.Build();
                foreach (var att in attributes)
                {
                    var parentEntity = tmp.GetAttributeValue<EntityReference>(att);
                    if (parentEntity == null || service.GetEntityOrDefault(parentEntity.LogicalName, parentEntity.Id, new ColumnSet(false)) != null) { continue; }

                    // parent hasn't been created yet, Add attribute to be updated, and remove attribute for creation
                    postCreateEntity[att] = parentEntity;
                    builder.WithAttributeValue(att, null);
                }
            }

            var createdEntity = builder.Create(service, false);
            if (postCreateEntity.Attributes.Any())
            {
                postCreateEntity.Id = createdEntity.Id;
                postCreateUpdates.Add(postCreateEntity);
            }

            return createdEntity;
        }

        /// <summary>
        /// Gets the builder for the given Id, casting it to the given type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        public T Get<T>(Id id) { return (T) Get(id); }

        /// <summary>
        /// Gets the builder for the given Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IEntityBuilder Get(Id id)
        {
            if (id != Guid.Empty && !Ids.ContainsKey(id))
            {
                Ids.Add(id, id);
            }
            if (Builders.TryGetValue(id, out BuilderInfo builder)) { return builder.Builder; }

            if (!DefaultBuilderConstructors.TryGetValue(id, out ConstructorInfo constructor))
            {
                constructor = GetGenericConstructor(id);
            }

            builder = CreateBuilder(id, constructor);

            return builder.Builder;
        }

        private BuilderInfo CreateBuilder(Id id, ConstructorInfo constructor)
        {
            // Add the Entity to the mapper to make sure it can be created in the correct order
            EntityDependency.Mapper.Add(id);

            var builder = new BuilderInfo(id, (IEntityBuilder) constructor.Invoke(new object[] {id}));
            ApplyCustomActions(id, builder.Builder);

            Builders.Add(id, builder);
            BuildersByEntityType.AddOrAppend(id, builder);

            return builder;
        }

        /// <summary>
        /// Adds the builder as the default builder for the given entity type
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="logicalName">Name of the logical.</param>
        /// <exception cref="System.Exception">
        /// </exception>
        private void SetBuilderType<TBuilder>(string logicalName) where TBuilder : IEntityBuilder
        {
            var constructor = GetIdConstructor<TBuilder>();

            if (DefaultBuilderConstructors.TryGetValue(logicalName, out ConstructorInfo existingConstructor))
            {
                // Should only have one type.  Check to make sure type is the same
                if (existingConstructor.DeclaringType != constructor.DeclaringType)
                {
                    // ReSharper disable PossibleNullReferenceException
                    throw new Exception($"Only one type of Builder can be used per entity.  Attempt was made to define builder {constructor.DeclaringType.FullName}, when builder {existingConstructor.DeclaringType.FullName} already is defined!");
                    // ReSharper restore PossibleNullReferenceException
                }
            }
            else
            {
                DefaultBuilderConstructors.Add(logicalName, constructor);
            }
        }

        /// <summary>
        /// Removes the Builder specified by the Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Remove(Id id)
        {
            if (Ids.ContainsKey(id))
            {
                Ids.Remove(id);
            }
            if (BuildersByEntityType.TryGetValue(id, out List<BuilderInfo> builders) && Builders.TryGetValue(id, out BuilderInfo builder))
            {
                Builders.Remove(id);
                builders.Remove(builder);
            }
        }

        /// <summary>
        /// Creates a GenericEntityBuilder of the type of logical name being passed in.  Employs locking since BuilderForEntity is static
        /// </summary>
        /// <param name="logicalName">Name of the Entity to create a GenericEntityBuilder Constructor For.</param>
        /// <returns></returns>
        private ConstructorInfo GetGenericConstructor(string logicalName)
        {
            if (DefaultBuilderConstructors.TryGetValue(logicalName, out ConstructorInfo constructor))
            {
                return constructor;
            }

            lock (BuilderConstructorForEntityLock)
            {
                if (DefaultBuilderConstructors.TryGetValue(logicalName, out constructor))
                {
                    return constructor;
                }

                var builderType = logicalName == ConnectionRoleAssociation.EntityLogicalName
                    ? typeof(N2NBuilder<>)
                    : typeof(GenericEntityBuilder<>);
                var builder = builderType.MakeGenericType(TestBase.GetType(logicalName));
                constructor = builder.GetConstructor(new[] { typeof(Id) });
                DefaultBuilderConstructors.Add(logicalName, constructor);
                return constructor;
            }
        }

        /// <summary>
        /// Allows for the specification of any fluent methods to (all existing/future) builders for the given entity type
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public void WithBuilderForEntityType<TBuilder>(Action<TBuilder> action)
            where TBuilder : class, IEntityBuilder
        {
            var entityType = GetEntityTypeOfBuilder<TBuilder>();
            var logicalName = EntityHelper.GetEntityLogicalName(entityType);

            SetBuilderType<TBuilder>(logicalName);

            // Handle all existing Builders
            ApplyCustomAction(logicalName, action);

            // Handle all future Builders
            AddCustomAction(logicalName, b => action((TBuilder) b)); // Convert Action<TBuilder> to Action<Object>
        }

        private static Type GetEntityTypeOfBuilder<TBuilder>()
        {
            var types = (from iType in typeof(TBuilder).GetInterfaces()
                         where iType.IsGenericType
                               && iType.GetGenericTypeDefinition() == typeof(IEntityBuilder<>) 
                         select iType.GetGenericArguments()[0]).ToList();
            var type = types.FirstOrDefault();
            if (type == null)
            {
                throw new Exception($"Builder type \"{typeof(TBuilder).FullName}\" does not implement IEntityBuilder<>!");
            }

            if(types.Count == 1)
            {
                return type;
            }

            if (types.Skip(1).Any(t => t != type))
            {
                throw new Exception($"Builder type \"{typeof(TBuilder).FullName}\" implements multiple IEntityBuilder<> types!");
            }

            return type;
        }

        /// <summary>
        /// Allows for the specification of a particular entity to use a specific entity builder
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.Exception"></exception>
        public void WithBuilderForEntity<TBuilder>(Id id, Action<TBuilder> action) where TBuilder : class, IEntityBuilder
        {
            GetEntityTypeOfBuilder<TBuilder>();
            var constructor = GetIdConstructor<TBuilder>();
            var builder = CreateBuilder(id, constructor);
            action((TBuilder)builder.Builder);
        }

        private static ConstructorInfo GetIdConstructor<TBuilder>() where TBuilder : IEntityBuilder
        {
            var constructor = typeof (TBuilder).GetConstructor(new[] {typeof (Id)});
            if (constructor == null)
            {
                throw new Exception($"{typeof (TBuilder).FullName} does not contain a constructor with a single parameter of type {typeof (Id).FullName}");
            }
            return constructor;
        }

        private class BuilderInfo
        {
            public Id Id { get; }
            public IEntityBuilder Builder { get; }

            public BuilderInfo(Id id, IEntityBuilder builder)
            {
                Id = id;
                Builder = builder;

            }
        }
    }
}
