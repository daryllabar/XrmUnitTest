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
