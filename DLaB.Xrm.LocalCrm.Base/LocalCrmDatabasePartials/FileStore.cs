using DLaB.Common;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

#if !PRE_MULTISELECT
namespace DLaB.Xrm.LocalCrm
{
    partial class LocalCrmDatabase
    {
        private readonly Dictionary<string, InitializeFileBlocksUploadRequest> _fileUploadsByContinuationToken = new Dictionary<string, InitializeFileBlocksUploadRequest>();
        private readonly Dictionary<string, InitializeFileBlocksDownloadRequest> _fileDownloadsByContinuationToken = new Dictionary<string, InitializeFileBlocksDownloadRequest>();
        private readonly Dictionary<string, byte[]> _fileBlocksByBlockId = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, FileInfo> _uploadedFilesByFileUploadKey = new Dictionary<string, FileInfo>();
        private const int _4MB = 4 * 1024 * 1024;

        private InitializeFileBlocksDownloadResponse InitializeFileBlocksDownload(InitializeFileBlocksDownloadRequest request)
        {
            var token = Guid.NewGuid().ToString();
            var key = GetKey(request);
            if(!_uploadedFilesByFileUploadKey.TryGetValue(key, out var uploadRequest))
            {
                throw new Exception($"No file attachment found for attribute: {request.FileAttributeName} EntityId: {request.Target.Id}.");
            }

            _fileDownloadsByContinuationToken[token] = request;

            return new InitializeFileBlocksDownloadResponse
            {
                Results = {
                    [nameof(InitializeFileBlocksDownloadResponse.FileContinuationToken)] = token,
                    [nameof(InitializeFileBlocksDownloadResponse.FileSizeInBytes)] = uploadRequest.FileSizeInBytes,
                    [nameof(InitializeFileBlocksDownloadResponse.FileName)] = uploadRequest.FileName,
                    [nameof(InitializeFileBlocksDownloadResponse.IsChunkingSupported)] = uploadRequest.FileSizeInBytes > _4MB,
                }
            };
        }

        private class FileInfo
        {
            public List<byte> Bytes { get; } = [];
            public long FileSizeInBytes { get; set; }
            public string FileName { get; set; } = null!;
            public Guid FileId { get; } = Guid.NewGuid();

            public Dictionary<long, List<byte[]>> ChunksByBlockLength { get; } = new Dictionary<long, List<byte[]>>();
        }

        private CommitFileBlocksUploadResponse CommitFileBlocksUpload(CommitFileBlocksUploadRequest request)
        {
            if (!_fileUploadsByContinuationToken.TryGetValue(request.FileContinuationToken, out var fileUpload))
            {
                throw new Exception("Invalid File ContinuationToken");
            }

            var file = new FileInfo {
                FileName = fileUpload.FileName,
            };
            _uploadedFilesByFileUploadKey[GetKey(fileUpload)] = file;
            foreach (var blockId in request.BlockList)
            {
                if (!_fileBlocksByBlockId.TryGetValue(blockId, out var block))
                {
                    throw new Exception($"Block with id {blockId} does not exist!");
                }
                file.Bytes.AddRange(block);
                _fileBlocksByBlockId.Remove(blockId);
            }

            file.FileSizeInBytes = file.Bytes.Count;
            _fileUploadsByContinuationToken.Remove(request.FileContinuationToken);
            return new CommitFileBlocksUploadResponse
            {
                Results =
                {
                    [nameof(CommitFileBlocksUploadResponse.FileId)] = file.FileId,
                    [nameof(CommitFileBlocksUploadResponse.FileSizeInBytes)] = file.FileSizeInBytes
                }
            };
        }

        private DownloadBlockResponse DownloadBlock(DownloadBlockRequest request)
        {
            if (request.FileContinuationToken == null)
            {
                throw new Exception("Required field 'FileContinuationToken' is missing");
            }
            if (!_fileDownloadsByContinuationToken.TryGetValue(request.FileContinuationToken, out var download))
            {
                throw new Exception("Invalid File ContinuationToken");
            }
            if (request.BlockLength == 0)
            {
                throw new Exception("Required field 'BlockLength' is missing or 0");
            }
            
            if (!_uploadedFilesByFileUploadKey.TryGetValue(GetKey(download), out var fileInfo))
            {
                throw new Exception($"No file attachment found for {GetKey(download)}");
            }

            if (!fileInfo.ChunksByBlockLength.TryGetValue(request.BlockLength, out var chunks))
            {
                chunks = fileInfo.Bytes.Batch((int)request.BlockLength).Select(b => b.ToArray()).ToList();
                fileInfo.ChunksByBlockLength[request.BlockLength] = chunks;
            }

            var index = request.Offset / request.BlockLength;
            return new DownloadBlockResponse
            {
                Results =
                {
                    [nameof(DownloadBlockResponse.Data)] = chunks[(int)index].ToArray()
                }
            };
        }

        private InitializeFileBlocksUploadResponse InitializeFileBlocksUpload(InitializeFileBlocksUploadRequest request)
        {
            var uploadId = Guid.NewGuid().ToString();
            _fileUploadsByContinuationToken[uploadId] = request;

            return new InitializeFileBlocksUploadResponse
            {
                Results =
                {
                    [nameof(InitializeFileBlocksUploadResponse.FileContinuationToken)] = uploadId
                }
            };
        }

        private UploadBlockResponse UploadBlock(UploadBlockRequest request)
        {
            if (request.FileContinuationToken == null)
            {
                throw new Exception("Required field 'FileContinuationToken' is missing");
            }
            if (!_fileUploadsByContinuationToken.ContainsKey(request.FileContinuationToken))
            {
                throw new Exception("Invalid File ContinuationToken");
            }
            if (request.BlockId == null)
            {
                throw new Exception("Required field 'BlockId' is missing");
            }
            if (string.IsNullOrWhiteSpace(request.BlockId)
                || string.IsNullOrWhiteSpace(request.FileContinuationToken))
            {
                throw new Exception("Expected non-empty string.");
            }
            if (_fileBlocksByBlockId.ContainsKey(request.BlockId))
            {
                throw new Exception($"File Block with id {request.BlockId} already exists!");
            }

            _fileBlocksByBlockId[request.BlockId] = request.BlockData;

            return new UploadBlockResponse();
        }

        private static string GetKey(InitializeFileBlocksDownloadRequest request)
        {
            return $"{request.Target.LogicalName}|{request.Target.Id}|{request.FileAttributeName}";
        }

        private static string GetKey(InitializeFileBlocksUploadRequest request)
        {
            return $"{request.Target.LogicalName}|{request.Target.Id}|{request.FileAttributeName}";
        }

        internal static DownloadBlockResponse DownloadBlock(LocalCrmDatabaseInfo info, DownloadBlockRequest request)
        {
            return GetDatabaseForService(info).DownloadBlock(request);
        }

        internal static InitializeFileBlocksDownloadResponse InitializeFileBlocksDownload(LocalCrmDatabaseInfo info, InitializeFileBlocksDownloadRequest request)
        {
            return GetDatabaseForService(info).InitializeFileBlocksDownload(request);
        }

        internal static InitializeFileBlocksUploadResponse InitializeFileBlocksUpload(LocalCrmDatabaseInfo info, InitializeFileBlocksUploadRequest request)
        {
            return GetDatabaseForService(info).InitializeFileBlocksUpload(request);
        }

        internal static UploadBlockResponse UploadBlock(LocalCrmDatabaseInfo info, UploadBlockRequest request)
        {
            return GetDatabaseForService(info).UploadBlock(request);
        }

        internal static CommitFileBlocksUploadResponse CommitFileBlocksUpload(LocalCrmDatabaseInfo info, CommitFileBlocksUploadRequest request)
        {
            return GetDatabaseForService(info).CommitFileBlocksUpload(request);
        }
    }
}
#endif