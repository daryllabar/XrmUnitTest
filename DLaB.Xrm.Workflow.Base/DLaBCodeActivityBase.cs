using System;
using System.Activities;
using System.ServiceModel;
#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Plugin;
#else
using Source.DLaB.Xrm.Plugin;
#endif
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public abstract class DLaBCodeActivityBase : CodeActivity
    {
        protected override void Execute(CodeActivityContext codeActivityContext)
        {
            if (codeActivityContext == null)
            {
                throw new InvalidPluginExecutionException("codeActivityContext");
            }

            // Construct the local plug-in context.
            var context = new DLaBExtendedWorkflowContext(codeActivityContext, this);

            using (context.TraceTime("{0}.Execute()", context.CodeActivityTypeName))
            {
                try
                {
                    // Invoke the custom implementation 
                    Execute(context);
                }
                catch (FaultException<OrganizationServiceFault> e)
                {
                    context.LogException(e);

                    // Handle the exception.
                    throw new InvalidPluginExecutionException("OrganizationServiceFault", e);
                }
                catch (Exception e)
                {
                    context.LogException(e);
                    throw;
                }
            }
        }

        public abstract void Execute(IExtendedWorkflowContext context);
    }
}
