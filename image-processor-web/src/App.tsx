import React, { useState, useEffect } from 'react';
import axios from 'axios';
import JobForm from './components/JobForm';
import JobStatus from './components/JobStatus';
import LogView from './components/LogView';
import { ProcessingOptions, ProcessingUpdate, JobStatus as JobStatusType } from './types';

const API_BASE_URL = 'http://localhost:5075'; // Make sure this is the correct port for your API

function App() {
  const [jobId, setJobId] = useState<string | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [lastUpdate, setLastUpdate] = useState<ProcessingUpdate | null>(null);
  const [history, setHistory] = useState<ProcessingUpdate[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!jobId || isProcessing === false) return;

    const interval = setInterval(async () => {
      try {
        const statusRes = await axios.get<JobStatusType>(`${API_BASE_URL}/api/processing/${jobId}/status`);
        const historyRes = await axios.get<ProcessingUpdate[]>(`${API_BASE_URL}/api/processing/${jobId}/history`);

        const lastStatus = statusRes.data.lastUpdate;
        setLastUpdate(lastStatus);
        setHistory(historyRes.data);

        if (lastStatus.isComplete || lastStatus.isError) {
          setIsProcessing(false);
        }
      } catch (err) {
        setError('Failed to get job status. Make sure the API is running.');
        setIsProcessing(false);
      }
    }, 2000);

    return () => clearInterval(interval);
  }, [jobId, isProcessing]);

  const handleSubmit = async (options: ProcessingOptions) => {
    setIsProcessing(true);
    setJobId(null);
    setLastUpdate(null);
    setHistory([]);
    setError(null);

    try {
      const response = await axios.post<{ jobId: string }>(`${API_BASE_URL}/api/processing/start`, options);
      setJobId(response.data.jobId);
    } catch (err) {
      setError('Failed to start job. Make sure the API is running and the options are correct.');
      setIsProcessing(false);
    }
  };

  return (
    <div className="container mt-5">
      <h1 className="mb-4">Image Processor</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      <JobForm onSubmit={handleSubmit} isProcessing={isProcessing} />
      <JobStatus lastUpdate={lastUpdate} />
      <LogView history={history} />
    </div>
  );
}

export default App;