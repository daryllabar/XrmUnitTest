using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm
#else
namespace Source.DLaB.Xrm
#endif
{
    /// <summary>
    /// Interface to handle retrieving the trace history text.  
    /// </summary>
    public interface IHistoricalTracingService: ITracingService
    {
        /// <summary>
        /// Gets the entire trace history of the current instance.
        /// </summary>
        string GetTraceHistory();
    }
}
