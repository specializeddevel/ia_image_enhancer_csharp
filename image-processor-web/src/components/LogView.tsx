import React from 'react';
import { ProcessingUpdate } from '../types';

interface LogViewProps {
  history: ProcessingUpdate[];
}

const LogView: React.FC<LogViewProps> = ({ history }) => {
  return (
    <div className="card">
      <div className="card-header"><h2>Log</h2></div>
      <div className="card-body" style={{ maxHeight: '300px', overflowY: 'auto' }}>
        <ul className="list-group">
          {history.map((update, index) => (
            <li key={index} className={`list-group-item ${update.isError ? 'list-group-item-danger' : ''}`}>
              {update.message}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default LogView;