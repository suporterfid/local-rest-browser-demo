import React, { useState, useEffect } from 'react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Select } from '@/components/ui/select';

const HostSelector = () => {
  const [selectedHost, setSelectedHost] = useState('');
  const [result, setResult] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  
  // This would come from your server configuration
  const availableHosts = [
    { value: 'https://localhost:5001', label: 'Local (5001)' },
    { value: 'https://localhost:5003', label: 'Local (5003)' },
    { value: 'https://localhost:5005', label: 'Local (5005)' }
  ];

  const api = {
    async sayHello() {
      if (!selectedHost) throw new Error('Please select a host first');
      const response = await fetch(`${selectedHost}/api/hello`);
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
      return await response.json();
    },
    
    async echo(message) {
      if (!selectedHost) throw new Error('Please select a host first');
      const response = await fetch(`${selectedHost}/api/echo`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message })
      });
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
      return await response.json();
    }
  };

  const testHello = async () => {
    try {
      setError('');
      const result = await api.sayHello();
      setResult(JSON.stringify(result, null, 2));
    } catch (err) {
      setError(err.message);
    }
  };

  const testEcho = async () => {
    try {
      setError('');
      const result = await api.echo(message);
      setResult(JSON.stringify(result, null, 2));
    } catch (err) {
      setError(err.message);
    }
  };

  useEffect(() => {
    if (selectedHost) {
      setError('');
      setResult('');
    }
  }, [selectedHost]);

  return (
    <Card className="w-full max-w-2xl mx-auto">
      <CardHeader>
        <CardTitle>Application A Host Selection</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div className="flex items-center space-x-4">
            <select 
              value={selectedHost}
              onChange={(e) => setSelectedHost(e.target.value)}
              className="flex h-10 w-full rounded-md border border-gray-200 bg-white px-3 py-2 text-sm ring-offset-white focus:outline-none focus:ring-2 focus:ring-gray-950"
            >
              <option value="">Select Host</option>
              {availableHosts.map(host => (
                <option key={host.value} value={host.value}>
                  {host.label}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <button
              onClick={testHello}
              disabled={!selectedHost}
              className="bg-blue-500 text-white px-4 py-2 rounded disabled:opacity-50"
            >
              Test Hello Endpoint
            </button>
          </div>

          <div className="space-y-2">
            <input
              type="text"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Enter message to echo"
              className="flex h-10 w-full rounded-md border border-gray-200 bg-white px-3 py-2 text-sm ring-offset-white focus:outline-none focus:ring-2 focus:ring-gray-950"
            />
            <button
              onClick={testEcho}
              disabled={!selectedHost || !message}
              className="bg-blue-500 text-white px-4 py-2 rounded disabled:opacity-50"
            >
              Test Echo Endpoint
            </button>
          </div>

          {error && (
            <div className="p-4 bg-red-50 text-red-500 rounded">
              {error}
            </div>
          )}
          
          {result && (
            <div className="p-4 bg-gray-50 rounded font-mono whitespace-pre">
              {result}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
};

export default HostSelector;