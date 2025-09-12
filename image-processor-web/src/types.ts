export interface ProcessingOptions {
  inputFolder: string;
  outputFolder: string;
  model: string;
  processSubfolders: boolean;
  convertToWebP: boolean;
  convertToAvif: boolean;
  applyUpscale: boolean;
  deleteSourceFile: boolean;
  includeWebPFiles: boolean;
  includeAvifFiles: boolean;
}

export interface ProcessingUpdate {
  message: string;
  isError: boolean;
  errorMessage?: string;
  isComplete: boolean;
  overallProgress: number;
  folderProgress: number;
  currentFile?: string;
  currentFileSize: number;
  currentFilePath?: string;
  currentFolderName?: string;
  filesInCurrentFolder?: number;
  processedFilesInCurrentFolder?: number;
  totalQueueSizeInBytes?: number;
  totalQueueFileCount?: number;
  currentFolderTotalSizeInBytes?: number;
  folderSpaceSaving?: number;
  folderOriginalSize: number;
  folderConvertedSize: number;
  totalSpaceSaving?: number;
  totalOriginalSize: number;
  totalConvertedSize: number;
}

export interface JobStatus {
  id: string;
  status: string;
  lastUpdate: ProcessingUpdate;
}
