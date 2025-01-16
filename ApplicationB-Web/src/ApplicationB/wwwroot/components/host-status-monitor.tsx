import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { CheckCircle, XCircle } from 'lucide-react';

const HostStatusMonitor = ({ hosts = [] }) => {
  const [hostStatus, setHostStatus] = useState({});

  const checkHostHealth = async (host) => {
    try {
      const response = await fetch(`${host}/api/health`);
      if (!response.ok) throw new Error('Health check failed');
      const data = await response.json();
      return { isHealthy: true, data };
    } catch (error) {
      return { isHealthy: false, error: error.message };
    }
  };

  useEffect(() => {
    if (!Array.isArray(hosts) || hosts.length === 0) {
      return;
    }

    const checkAllHosts = async () => {
      try {
        const statusChecks = hosts.map(async (host) => {
          const status = await checkHostHealth(host);
          return [host, status];
        });

        const results = await Promise.all(statusChecks);
        const newStatus = Object.fromEntries(results);
        setHostStatus(newStatus);
      } catch (error) {
        console.error('Error checking host status:', error);
      }
    };

    checkAllHosts();
    const interval = setInterval(checkAllHosts, 30000);
    return () => clearInterval(interval);
  }, [hosts]);

  if (!Array.isArray(hosts) || hosts.length === 0) {
    return (
      <Card className="w-full max-w-2xl mx-auto mb-6">
        <CardHeader>
          <CardTitle>Available Endpoints</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-center text-gray-500">
            No endpoints available
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="w-full max-w-2xl mx-auto mb-6">
      <CardHeader>
        <CardTitle>Available Endpoints</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {hosts.map((host) => (
            <div key={host} className="flex items-center justify-between p-2 border rounded">
              <span className="font-medium">{host}</span>
              {hostStatus[host]?.isHealthy ? (
                <div className="flex items-center text-green-600">
                  <CheckCircle className="w-5 h-5 mr-2" />
                  <span>Online</span>
                </div>
              ) : (
                <div className="flex items-center text-red-600">
                  <XCircle className="w-5 h-5 mr-2" />
                  <span>Offline</span>
                </div>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
};

export default HostStatusMonitor;