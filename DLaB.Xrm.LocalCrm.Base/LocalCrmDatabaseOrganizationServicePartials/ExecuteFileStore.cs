using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

#if !PRE_MULTISELECT
namespace DLaB.Xrm.LocalCrm
{
    partial class LocalCrmDatabaseOrganizationService
    {
        private DownloadBlockResponse ExecuteInternal(DownloadBlockRequest request)
        {
            return LocalCrmDatabase.DownloadBlock(Info, request);
        }

        private InitializeFileBlocksDownloadResponse ExecuteInternal(InitializeFileBlocksDownloadRequest request)
        {
            Retrieve(request.Target.LogicalName, request.Target.Id, new ColumnSet(request.FileAttributeName));
            return LocalCrmDatabase.InitializeFileBlocksDownload(Info, request);
        }

        private InitializeFileBlocksUploadResponse ExecuteInternal(InitializeFileBlocksUploadRequest request)
        {
            Retrieve(request.Target.LogicalName, request.Target.Id, new ColumnSet(request.FileAttributeName));
            return LocalCrmDatabase.InitializeFileBlocksUpload(Info, request);

        }

        private UploadBlockResponse ExecuteInternal(UploadBlockRequest request)
        {
            return LocalCrmDatabase.UploadBlock(Info, request);
        }

        private CommitFileBlocksUploadResponse ExecuteInternal(CommitFileBlocksUploadRequest request)
        {
            return LocalCrmDatabase.CommitFileBlocksUpload(Info, request);
        }
    }
}
#endif
