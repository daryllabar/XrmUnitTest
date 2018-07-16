﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Common;
#else
using Source.DLaB.Common;
#endif

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm
#else
namespace Source.DLaB.Xrm
#endif
{
    public class ExtendedOrganizationService : IOrganizationService
    {
        private ExtendedOrganizationServiceSettings Settings { get; set; }
        private IOrganizationService Service { get; set; }
        private ITracingService TraceService { get; set; }

        /// <summary>
        /// Constructor for determining if statements are timed and or logged.
        /// </summary>
        /// <param name="service">IOrganziationService to wrap.</param>
        /// <param name="trace">Tracing Service Required</param>
        /// <param name="settings">Settings</param>
        public ExtendedOrganizationService(IOrganizationService service, ITracingService trace, ExtendedOrganizationServiceSettings settings = null)
        {
            Service = service;
            Settings = settings ?? new ExtendedOrganizationServiceSettings();
            TraceService = trace ?? throw new ArgumentNullException(nameof(trace));
        }

        #region Implementation of IOrganizationService

        public Guid Create(Entity entity)
        {
            var message = Settings.LogDetailedRequests
                ? $"Create Request for {entity.LogicalName} with Id {entity.Id} and Attributes {entity.ToStringAttributes()}"
                : "Create Request";
            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    return Service.Create(entity);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            return Service.Create(entity);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            var message = Settings.LogDetailedRequests
                ? $"Retrieve Request for {entityName} with id {id} and Columns {string.Join(", ", columnSet.Columns)}"
                : "Retrieve Request";

            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    return Service.Retrieve(entityName, id, columnSet);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            return Service.Retrieve(entityName, id, columnSet);
        }

        public void Update(Entity entity)
        {
            var message = Settings.LogDetailedRequests
                ? $"Update Request for {entity.LogicalName} with Id {entity.Id} and Attributes { entity.ToStringAttributes()}"
                : "Update Request";
            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    Service.Update(entity);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            Service.Update(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            var message = Settings.LogDetailedRequests
                ? $"Delete Request for {entityName} with Id {id}"
                : "Delete Request";
            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    Service.Delete(entityName, id);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            Service.Delete(entityName, id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            var message = Settings.LogDetailedRequests
                ? GetDetailedMessage(request)
                : $"Execute Request {request.RequestName}";

            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    return Service.Execute(request);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            return Service.Execute(request);
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            var message = Settings.LogDetailedRequests
                ? $"Associate Request for {entityName}, with Id {entityId}, Relationship {relationship.SchemaName}, and Related Entities {relatedEntities.Select(e => e.ToStringDebug()).ToCsv()}."
                : "Associate Request";
            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    Service.Associate(entityName, entityId, relationship, relatedEntities);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            Service.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            var message = Settings.LogDetailedRequests
                ? $"Disassociate Request for {entityName}, with Id {entityId}, Relationship {relationship.SchemaName}, and Related Entities {relatedEntities.Select(e => e.ToStringDebug()).ToCsv()}."
                : "Disassociate Request";
            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    Service.Disassociate(entityName, entityId, relationship, relatedEntities);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            Service.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            var message = Settings.LogDetailedRequests
                ? "Retrieve Multiple Request " + GetDetailedMessage(query)
                : "Retrieve Multiple Request";

            if (Settings.TimeRequests)
            {
                var timer = new Stopwatch();
                try
                {
                    TraceStart(message);
                    timer.Start();
                    return Service.RetrieveMultiple(query);
                }
                finally
                {
                    TraceEnd(timer);
                }
            }

            TraceExecute(message);
            return Service.RetrieveMultiple(query);
        }

        #endregion

        protected virtual string GetDetailedMessage(OrganizationRequest request)
        {
            var message = $"Execute Request for {request.RequestName} with ";
            switch (request.RequestName)
            {
                case "RetrieveMultiple":
                    var query = request["Query"] as QueryBase;
                    message += GetDetailedMessage(query);

                    break;
                default:
                    message += $"{request.Parameters.ToStringDebug("Parameters")}.";
                    break;
            }

            return message;
        }

        private string GetDetailedMessage(QueryBase query)
        {
            string message;
            switch (query)
            {
                case QueryExpression qe:
                    message = $"Query Expression: {qe.GetSqlStatement()}.";
                    break;
                case FetchExpression fe:
                    message = $"Fetch Expression: {fe.Query}.";
                    break;
                case QueryByAttribute ba:
                    message =
                        $"Query By Attribute for {ba.EntityName} with attributes {string.Join(", ", ba.Attributes)} and values {string.Join(", ", ba.Values)} and Columns {string.Join(", ", ba.ColumnSet.Columns)}";
                    break;
                default:
                    message = $"Unknown Query Base {query.GetType().FullName}.";
                    break;
            }

            return message;
        }

        private void TraceStart(string request)
        {
            TraceService.Trace(Settings.TimeStartMessageFormat, request);
        }

        private void TraceEnd(Stopwatch timer)
        {
            timer.Stop();
            TraceService.Trace(Settings.TimeEndMessageFormat, timer.ElapsedMilliseconds/1000D);
        }

        private void TraceExecute(string message)
        {
            if (Settings.LogDetailedRequests)
            {
                TraceService.Trace("Executing " + message);
            }
        }
    }
}
