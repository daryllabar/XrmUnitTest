using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using AppConfig = DLaB.Xrm.Client.AppConfig;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Base class for implementing
    /// </summary>
    public abstract class PrimaryNameFieldProviderBase: IPrimaryNameProvider
    {

        /// <summary>
        /// Some OOB entities do not contain a primary name.  Why?  No idea.
        /// </summary>
        public HashSet<string> NamelessEntities { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected PrimaryNameFieldProviderBase()
        {
            NamelessEntities = new HashSet<string>
            {
                "aciviewmapper",
                "appconfig",
                "businessprocessflowinstance",
                "connectionroleassociation",
                "importentitymapping",
                "importlog",
                "lookupmapping",
                "ownermapping",
                "picklistmapping",
                "transformationmapping"
            };

            foreach (var value in AppConfig.CrmEntities.NamelessEntities
                                           .Where(value => !NamelessEntities.Contains(value)))
            {
                NamelessEntities.Add(value);
            }
        }

        private static readonly ConcurrentDictionary<Tuple<Assembly,string>, IPrimaryNameProvider> NameProviders = new ConcurrentDictionary<Tuple<Assembly, string>, IPrimaryNameProvider>();
        /// <summary>
        /// Logic to return the specific provider based off of Assembly/Namespace
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public static IPrimaryNameProvider GetConfiguredProvider(Assembly assembly, string @namespace)
        {
            return NameProviders.GetOrAdd(new Tuple<Assembly, string>(assembly, @namespace), k => AppConfig.CrmEntities.ContainPrimaryAttributeName
                ? (IPrimaryNameProvider) new PrimaryNameViaFieldProvider(k.Item1, k.Item2)
                : new PrimaryNameViaNonStandardNamesProvider(AppConfig.CrmEntities.NonStandardAttributeNamesByEntity));
        }

        /// <summary>
        /// Returns the Primary Name for the given entity logical name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public abstract string GetPrimaryName(string logicalName);

        /// <summary>
        /// Returns the Primary Name for the given entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract string GetPrimaryName<T>() where T : Entity;
    }
}
