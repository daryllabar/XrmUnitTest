#if !NET
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Activity = System.Activities.Activity;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// Default Workflow Builder Implementation.  
    /// </summary>
    public sealed class WorkflowInvokerBuilder : WorkflowInvokerBuilderBase<WorkflowInvokerBuilder>
    {
        /// <summary>
        /// Gets the Workflow Invoker Builder of the derived Class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override WorkflowInvokerBuilder This => this;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder"/> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        public WorkflowInvokerBuilder(Activity workflow) : base (workflow)
        {
        }
    }

    /// <summary>
    /// Abstract Builder to allow for Derived Types to be created
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    public abstract class WorkflowInvokerBuilderBase<TDerived> where TDerived : WorkflowInvokerBuilderBase<TDerived>
    {
        /// <summary>
        /// Gets the derived version of the class.
        /// </summary>
        protected abstract TDerived This { get; }
        private ITracingService TracingService { get; set; }
        private IWorkflowContext WorkflowContext { get; set; }
        private IOrganizationService Service { get; set; }
        private Dictionary<string, object> InArguments { get; }
        private Activity Workflow { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder"/> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        protected WorkflowInvokerBuilderBase(Activity workflow) : this(workflow, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder" /> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        /// <param name="logger">The logger.</param>
        protected WorkflowInvokerBuilderBase(Activity workflow, ITestLogger logger)
        {
            Service = TestBase.GetOrganizationService();
            TracingService = new FakeTraceService(logger);
            Workflow = workflow;
            WorkflowContext = new FakeWorkflowContext();
            InArguments = GetInArguments(workflow);
        }

        private Dictionary<string, object> GetInArguments(Activity workflow)
        {
            var args = new Dictionary<string, object>();

            var type = workflow.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.CustomAttributes.All(attr => attr.AttributeType != typeof(InputAttribute)))
                {
                    continue;
                }
                var value = GetValueOfInArgument(prop.GetValue(workflow));
                if (value == null)
                {
                    continue;
                }
                // Add Property to Arg Values
                args.Add(prop.Name, value);
                // remove Property from Workflow. (Validation error occurs otherwise...
                prop.SetValue(workflow, null);
            }
            return args;
        }

        private object GetValueOfInArgument(Object inArgument)
        {
            // Passed in object should be an InArgument<T>.  Have to call .Expression, and get it's value (which is a Literal<T>, and then Get it's value)
            // return ((Literal<T>)((InArgument<EntityReference>) value).Expression).Value;
            var literal = GetGenericTypePropertyValue(inArgument, typeof(InArgument<>), "Expression");
            return GetGenericTypePropertyValue(literal, typeof(System.Activities.Expressions.Literal<>), "Value");
        }

        /// <summary>
        /// Reflectively gets the value of the Generic Property.  Returns null if the expectedClassName or propertyName are different or not found
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="expectedType">Expected Type of the obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private object GetGenericTypePropertyValue(object obj, Type expectedType, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != expectedType)
            {
                return null;
            }
            var getMethod = type.GetMethod("get_" + propertyName);
            return getMethod?.Invoke(obj, null);
        }

        #region Fleunt Methods

        /// <summary>
        /// Defines the Service of the Context passed into the Invoked Workflow
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TDerived WithService(IOrganizationService service)
        {
            Service = service;
            return This;
        }

        /// <summary>
        /// Defines the TracingService of the Context passed into the Invoked Workflow
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TDerived WithService(ITracingService service)
        {
            TracingService = service;
            return This;
        }

        /// <summary>
        /// Defines the Workflow Context of the Activity Context passed into the Invoked Workflow.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public TDerived WithContext(IWorkflowContext context)
        {
            WorkflowContext = context;
            return This;
        }

        #endregion Fleunt Methods

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public WorkflowInvoker Build()
        {
            var invoker = new WorkflowInvoker(Workflow);
            invoker.Extensions.Add(new FakeOrganizationServiceFactory(Service));
            invoker.Extensions.Add(TracingService);
            invoker.Extensions.Add(WorkflowContext);
            return invoker;
        }

        /// <summary>
        /// Invokes the workflow, populating the InArguments.
        /// </summary>
        /// <param name="inputParams">The input parameters.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public IDictionary<string, object> InvokeWorkflow(Dictionary<string, object> inputParams = null)
        {
            inputParams = inputParams ?? new Dictionary<string, object>();
            foreach (var entry in InArguments)
            {
                inputParams.Add(entry.Key, entry.Value);
            }
            return Build().Invoke(inputParams);
        }
    }
}
#endif