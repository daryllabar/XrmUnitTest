using System;
using System.Activities;
using System.Activities.Tracking;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public static class Extensions
    {
        #region IExtendedWorkflowContext

        /// <summary>
        /// Gets the context information.
        /// </summary>
        /// <returns></returns>
        public static string GetContextInfo(this IExtendedWorkflowContext context)
        {
            return
                "**** Context Info ****" + Environment.NewLine +
                "Activity: " + context.CodeActivityTypeName + Environment.NewLine +
                context.ToStringDebug();
        }

        #region CodeActivityContext Accessors

        /// <summary>Returns an extension of the specified type.</summary>
        /// <typeparam name="T">The type of extension to retrieve.</typeparam>
        /// <returns>The extension of the specified type if present; otherwise, <see langword="null" />.</returns>
        public static T GetExtension<T>(this IExtendedWorkflowContext context) where T : class { return context.CodeActivityContext.GetExtension<T>(); }

        /// <summary>Returns the typed location for the specified referenced location for the current activity context.</summary>
        /// <param name="context"></param>
        /// <param name="locationReference">The referenced location.</param>
        /// <typeparam name="T">The type of the location to return.</typeparam>
        /// <returns>The typed location.</returns>
        public static Location<T> GetLocation<T>(this IExtendedWorkflowContext context, LocationReference locationReference) { return context.CodeActivityContext.GetLocation<T>(locationReference); }

        /// <summary>Gets the execution property of the specified type.</summary>
        /// <typeparam name="THandle">The type of execution property.</typeparam>
        /// <returns>The execution property.</returns>
        public static THandle GetProperty<THandle>(this IExtendedWorkflowContext context) where THandle : Handle { return context.CodeActivityContext.GetProperty<THandle>(); }

        /// <summary>Gets the value at the specified <see cref="T:System.Activities.LocationReference" />.</summary>
        /// <param name="context"></param>
        /// <param name="locationReference">The referenced location to inspect.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value of the referenced location.</returns>
        public static T GetValue<T>(this IExtendedWorkflowContext context, LocationReference locationReference) { return context.CodeActivityContext.GetValue<T>(locationReference); }

        /// <summary>Gets the value of the specified <see cref="T:System.Activities.OutArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to inspect.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <returns>The value of the argument.</returns>
        public static T GetValue<T>(this IExtendedWorkflowContext context, OutArgument<T> argument) { return context.CodeActivityContext.GetValue(argument); }

        /// <summary>Gets the value of the specified <see cref="T:System.Activities.InOutArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to inspect.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <returns>The value of the argument.</returns>
        public static T GetValue<T>(this IExtendedWorkflowContext context, InOutArgument<T> argument) { return context.CodeActivityContext.GetValue(argument); }

        /// <summary>Gets the value of the specified <see cref="T:System.Activities.InArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to inspect.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <returns>The value of the argument.</returns>
        public static T GetValue<T>(this IExtendedWorkflowContext context, InArgument<T> argument) { return context.CodeActivityContext.GetValue(argument); }

        /// <summary>Gets the value of the specified <see cref="T:System.Activities.Argument" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to inspect.</param>
        /// <returns>The value of the argument.</returns>
        public static object GetValue(this IExtendedWorkflowContext context, Argument argument) { return context.CodeActivityContext.GetValue(argument); }

        /// <summary>Gets the value of the specified <see cref="T:System.Activities.RuntimeArgument" />.</summary>
        /// <param name="context"></param>
        /// <param name="runtimeArgument">The argument to inspect.</param>
        /// <returns>The value of the argument.</returns>
        public static object GetValue(this IExtendedWorkflowContext context, RuntimeArgument runtimeArgument) { return context.CodeActivityContext.GetValue(runtimeArgument); }

        /// <summary>Assigns a value to the specified <see cref="T:System.Activities.LocationReference" />.</summary>
        /// <param name="context"></param>
        /// <param name="locationReference">The referenced location to receive the new value.</param>
        /// <param name="value">The new value of the referenced location.</param>
        /// <typeparam name="T">The type of the location.</typeparam>
        public static void SetValue<T>(this IExtendedWorkflowContext context, LocationReference locationReference, T value) { context.CodeActivityContext.SetValue(locationReference, value); }

        /// <summary>Assigns a value to the specified <see cref="T:System.Activities.OutArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to receive the new value.</param>
        /// <param name="value">The new value of the argument.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        public static void SetValue<T>(this IExtendedWorkflowContext context, OutArgument<T> argument, T value) { context.CodeActivityContext.SetValue(argument, value); }

        /// <summary>Assigns a value to the specified <see cref="T:System.Activities.InOutArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to receive the new value.</param>
        /// <param name="value">The new value of the argument.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        public static void SetValue<T>(this IExtendedWorkflowContext context, InOutArgument<T> argument, T value) { context.CodeActivityContext.SetValue(argument, value); }

        /// <summary>Assigns a value to the specified <see cref="T:System.Activities.InArgument`1" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to receive the new value.</param>
        /// <param name="value">The new value of the argument.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        public static void SetValue<T>(this IExtendedWorkflowContext context, InArgument<T> argument, T value) { context.CodeActivityContext.SetValue(argument, value); }

        /// <summary>Assigns a value to the specified <see cref="T:System.Activities.Argument" />.</summary>
        /// <param name="context"></param>
        /// <param name="argument">The argument to receive the new value.</param>
        /// <param name="value">The new value of the argument.</param>
        public static void SetValue(this IExtendedWorkflowContext context, Argument argument, object value) { context.CodeActivityContext.SetValue(argument, value); }

        /// <summary>Sends the specified custom tracking record to any registered tracking providers.</summary>
        /// <param name="context"></param>
        /// <param name="record">The data to be tracked.</param>
        public static void Track(this IExtendedWorkflowContext context, CustomTrackingRecord record) { context.CodeActivityContext.Track(record); }

        #endregion CodeActivityContext Accessors

        #endregion IExtendedWorkflowContext

        #region InArgument<T>

        public static T Get<T>(this InArgument<T> arg, IExtendedWorkflowContext context)
        {
            return arg.Get(context.CodeActivityContext);
        }

        #endregion InArgument<T>

        #region InOutArugument<T>
        public static T Get<T>(this InOutArgument<T> arg, IExtendedWorkflowContext context)
        {
            return arg.Get(context.CodeActivityContext);
        }

        public static void Set<T>(this InOutArgument<T> arg, IExtendedWorkflowContext context, T value)
        {
            arg.Set(context.CodeActivityContext, value);
        }

        #endregion InOutArugument<T>

        #region IWorkflowContext

        #region Get(Pre/Post)Entities

        /// <summary>
        /// If the imageName is populated and the PreEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// If the imageName is not populated, than the first image in PreEntityImages with a value, is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetPreEntity<T>(this IWorkflowContext context) where T : Entity
        {
            return context.PreEntityImages.GetEntity<T>(DLaBExtendedWorkflowContext.WorkflowImageNames.PreImage);
        }

        /// <summary>
        /// If the imageName is populated and the PostEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// If the imageName is not populated, than the first image in PostEntityImages with a value, is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetPostEntity<T>(this IWorkflowContext context) where T : Entity
        {
            return context.PreEntityImages.GetEntity<T>(DLaBExtendedWorkflowContext.WorkflowImageNames.PostImage);
        }

        #endregion Get(Pre/Post)Entities

        /// <summary>
        /// Returns an indepth view of the context
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string ToStringDebug(this IWorkflowContext context)
        {
            var lines = ((IExecutionContext)context).ToStringDebug();
            lines.AddRange(new[]
            {
                "Has Parent Context: " + (context.ParentContext != null),
                "StageName: " + context.StageName,
                "WorkflowCategory: " + context.WorkflowCategory,
                "WorkflowMode: " + context.WorkflowMode,
            });
            return string.Join(Environment.NewLine, lines);
        }

        #endregion IWorkflowContext

        #region OutArgument<T>

        public static void Set<T>(this OutArgument<T> arg, IExtendedWorkflowContext context, T value)
        {
            arg.Set(context.CodeActivityContext, value);
        }

        #endregion OutArgument<T>
    }
}
