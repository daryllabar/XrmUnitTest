using System;
using System.Activities;
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
