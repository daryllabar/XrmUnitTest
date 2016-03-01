﻿using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm
{
    /// <summary>
    /// A wrapper of an IOrganizationService
    /// </summary>
    public interface IIOrganizationServiceWrapper
    {
        /// <summary>
        /// The base IOrganizationService.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        IOrganizationService Service { get; set; }

        /// <summary>
        /// Returns the URI of the service.
        /// </summary>
        /// <returns></returns>
        Uri GetServiceUri();
    }
}
