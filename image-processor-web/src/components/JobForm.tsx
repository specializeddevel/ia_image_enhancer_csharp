import React, { useState } from 'react';
import { ProcessingOptions } from '../types';

interface JobFormProps {
  onSubmit: (options: ProcessingOptions) => void;
  isProcessing: boolean;
}

const JobForm: React.FC<JobFormProps> = ({ onSubmit, isProcessing }) => {
  const [options, setOptions] = useState<ProcessingOptions>({
    inputFolder: '',
    outputFolder: '',
    model: 'realesrgan-x4plus',
    processSubfolders: true,
    convertToWebP: true,
    convertToAvif: false,
    applyUpscale: true,
    deleteSourceFile: false,
    includeWebPFiles: false,
    includeAvifFiles: false,
  });

  const models = [
    'realesrgan-x4plus',
    'realesrnet-x4plus',
    'realesrgan-x4plus-anime',
    'realesr-animevideov3',
    'realesr-animevideov3-x2',
    'realesr-animevideov3-x4',
  ];

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const isCheckbox = type === 'checkbox';
    setOptions(prev => ({ ...prev, [name]: isCheckbox ? (e.target as HTMLInputElement).checked : value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(options);
  };

  return (
    <form onSubmit={handleSubmit} className="card mb-4">
      <div className="card-header">
        <h2>Processing Options</h2>
      </div>
      <div className="card-body">
        <div className="row">
          <div className="col-md-6 mb-3">
            <label htmlFor="inputFolder" className="form-label">Input Folder</label>
            <input type="text" id="inputFolder" name="inputFolder" value={options.inputFolder} onChange={handleChange} className="form-control" required />
            <div className="form-text">Copy and paste the full path to the input folder.</div>
          </div>
          <div className="col-md-6 mb-3">
            <label htmlFor="outputFolder" className="form-label">Output Folder</label>
            <input type="text" id="outputFolder" name="outputFolder" value={options.outputFolder} onChange={handleChange} className="form-control" required />
            <div className="form-text">Copy and paste the full path to the output folder.</div>
          </div>
        </div>

        <div className="row">
          <div className="col-md-6 mb-3">
            <label htmlFor="model" className="form-label">Model</label>
            <select id="model" name="model" value={options.model} onChange={handleChange} className="form-select">
              {models.map(m => <option key={m} value={m}>{m}</option>)}
            </select>
          </div>
        </div>

        <div className="row">
          <div className="col-md-12">
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="applyUpscale" name="applyUpscale" checked={options.applyUpscale} onChange={handleChange} className="form-check-input" />
              <label htmlFor="applyUpscale" className="form-check-label">Apply Upscale</label>
            </div>
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="convertToWebP" name="convertToWebP" checked={options.convertToWebP} onChange={handleChange} className="form-check-input" />
              <label htmlFor="convertToWebP" className="form-check-label">Convert to WebP</label>
            </div>
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="convertToAvif" name="convertToAvif" checked={options.convertToAvif} onChange={handleChange} className="form-check-input" />
              <label htmlFor="convertToAvif" className="form-check-label">Convert to AVIF</label>
            </div>
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="processSubfolders" name="processSubfolders" checked={options.processSubfolders} onChange={handleChange} className="form-check-input" />
              <label htmlFor="processSubfolders" className="form-check-label">Process Subfolders</label>
            </div>
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="includeWebPFiles" name="includeWebPFiles" checked={options.includeWebPFiles} onChange={handleChange} className="form-check-input" />
              <label htmlFor="includeWebPFiles" className="form-check-label">Include WebP Files</label>
            </div>
            <div className="form-check form-switch mb-2">
              <input type="checkbox" id="includeAvifFiles" name="includeAvifFiles" checked={options.includeAvifFiles} onChange={handleChange} className="form-check-input" />
              <label htmlFor="includeAvifFiles" className="form-check-label">Include AVIF Files</label>
            </div>
            <div className="form-check form-switch mb-2 text-danger">
              <input type="checkbox" id="deleteSourceFile" name="deleteSourceFile" checked={options.deleteSourceFile} onChange={handleChange} className="form-check-input" />
              <label htmlFor="deleteSourceFile" className="form-check-label">Delete Source File</label>
            </div>
          </div>
        </div>
      </div>
      <div className="card-footer">
        <button type="submit" className="btn btn-primary" disabled={isProcessing}>
          {isProcessing ? 'Processing...' : 'Start Processing'}
        </button>
      </div>
    </form>
  );
};

export default JobForm;