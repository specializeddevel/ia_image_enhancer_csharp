import React from 'react';
import { ProcessingUpdate } from '../types';

interface JobStatusProps {
  lastUpdate: ProcessingUpdate | null;
}

const JobStatus: React.FC<JobStatusProps> = ({ lastUpdate }) => {
  if (!lastUpdate) {
    return (
      <div className="card mb-4">
        <div className="card-header"><h2>Job Status</h2></div>
        <div className="card-body"><p>No job started yet.</p></div>
      </div>
    );
  }

  const formatBytes = (bytes: number, decimals = 2) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  return (
    <div className="card mb-4">
      <div className="card-header"><h2>Job Status</h2></div>
      <div className="card-body">
        <p className="mb-1">{lastUpdate.message}</p>
        {lastUpdate.isError && <p className="text-danger">Error: {lastUpdate.errorMessage}</p>}

        <label htmlFor="overallProgress">Overall Progress</label>
        <div className="progress mb-3" id="overallProgress">
          <div className="progress-bar" role="progressbar" style={{ width: `${lastUpdate.overallProgress * 100}%` }} aria-valuenow={lastUpdate.overallProgress * 100} aria-valuemin={0} aria-valuemax={100}></div>
        </div>

        {lastUpdate.currentFolderName && (
          <>
            <label htmlFor="folderProgress">Folder Progress: {lastUpdate.currentFolderName} ({lastUpdate.processedFilesInCurrentFolder}/{lastUpdate.filesInCurrentFolder})</label>
            <div className="progress mb-3" id="folderProgress">
              <div className="progress-bar bg-success" role="progressbar" style={{ width: `${lastUpdate.folderProgress * 100}%` }} aria-valuenow={lastUpdate.folderProgress * 100} aria-valuemin={0} aria-valuemax={100}></div>
            </div>
          </>
        )}

        {lastUpdate.currentFile && <p>Current File: {lastUpdate.currentFile} ({formatBytes(lastUpdate.currentFileSize)})</p>}

        <div className="row mt-3">
          <div className="col-md-6">
            <h6>Folder Stats</h6>
            <p className="mb-0">Original Size: {formatBytes(lastUpdate.folderOriginalSize)}</p>
            <p className="mb-0">Converted Size: {formatBytes(lastUpdate.folderConvertedSize)}</p>
            {lastUpdate.folderSpaceSaving != null && <p>Space Saving: {(lastUpdate.folderSpaceSaving * 100).toFixed(2)}%</p>}
          </div>
          <div className="col-md-6">
            <h6>Total Stats</h6>
            <p className="mb-0">Original Size: {formatBytes(lastUpdate.totalOriginalSize)}</p>
            <p className="mb-0">Converted Size: {formatBytes(lastUpdate.totalConvertedSize)}</p>
            {lastUpdate.totalSpaceSaving != null && <p>Space Saving: {(lastUpdate.totalSpaceSaving * 100).toFixed(2)}%</p>}
          </div>
        </div>
      </div>
    </div>
  );
};

export default JobStatus;