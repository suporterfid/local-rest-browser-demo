// HostSelector.js
import ApiClient from './api-client.js';

const HostSelector = {
    init: function(containerId, availableHosts, apiKey) {
        this.apiKey = apiKey;
        const container = document.getElementById(containerId);
        this.render(container, availableHosts);
        this.attachEventListeners();
        this.initializeStatusMonitor(availableHosts);
    },

    initializeStatusMonitor: function(availableHosts) {
        const checkHostHealth = async (host) => {
            try {
                const client = new ApiClient(host, this.apiKey);
                const response = await fetch(`${host}/api/v1/health`); // Use versioned path
                if (!response.ok) throw new Error('Health check failed');
                const data = await response.json();
                return { isHealthy: true, data };
            } catch (error) {
                console.error('Health check failed:', error);
                return { isHealthy: false };
            }
        };

        const updateHostStatuses = async () => {
            const statusContainer = document.getElementById('hostStatuses');
            if (!statusContainer) return;

            for (const host of availableHosts) {
                const status = await checkHostHealth(host);
                const hostElement = statusContainer.querySelector(`[data-host="${host}"]`);
                if (hostElement) {
                    hostElement.className = `flex items-center justify-between p-2 border rounded mb-2 ${status.isHealthy ? 'bg-green-50' : 'bg-red-50'}`;
                    hostElement.querySelector('.status').textContent = status.isHealthy ? 'Online' : 'Offline';
                    hostElement.querySelector('.status').className = `status ${status.isHealthy ? 'text-green-600' : 'text-red-600'}`;
                }
            }
        };

        updateHostStatuses();
        setInterval(updateHostStatuses, 30000);
    },

    render: function(container, availableHosts) {
        container.innerHTML = `
            <div class="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow">
                <h1 class="text-2xl font-bold mb-6">Demo Web Application</h1>
                
                <!-- Host Status Monitor Section -->
                <div class="mb-6">
                    <h2 class="text-xl font-semibold mb-4">Endpoint Status</h2>
                    <div id="hostStatuses" class="mb-4">
                        ${availableHosts.map(host => `
                            <div data-host="${host}" class="flex items-center justify-between p-2 border rounded mb-2">
                                <span class="font-medium">${host}</span>
                                <span class="status">Checking...</span>
                            </div>
                        `).join('')}
                    </div>
                </div>

                <!-- Host Selection Section -->
                <div class="mb-6">
                    <label class="block text-sm font-medium mb-2">Select Application A Host:</label>
                    <select id="hostSelect" class="w-full p-2 border rounded">
                        <option value="">Select Host</option>
                        ${availableHosts.map(host => 
                            `<option value="${host}">${host}</option>`
                        ).join('')}
                    </select>
                </div>

                <div class="space-y-6">
                    <div>
                        <h2 class="text-xl font-semibold mb-2">API Test</h2>
                        <button id="helloButton" 
                                class="bg-blue-500 text-white px-4 py-2 rounded disabled:opacity-50"
                                disabled>
                            Test Hello Endpoint
                        </button>
                        <div id="helloResult" class="mt-2 p-2 bg-gray-50 rounded"></div>
                    </div>

                    <div>
                        <h2 class="text-xl font-semibold mb-2">Echo Test</h2>
                        <input type="text" 
                               id="echoMessage" 
                               placeholder="Enter message to echo"
                               class="w-full p-2 border rounded mb-2">
                        <button id="echoButton" 
                                class="bg-blue-500 text-white px-4 py-2 rounded disabled:opacity-50"
                                disabled>
                            Test Echo Endpoint
                        </button>
                        <div id="echoResult" class="mt-2 p-2 bg-gray-50 rounded"></div>
                    </div>
                </div>
            </div>
        `;
    },

    attachEventListeners: function() {
        const hostSelect = document.getElementById('hostSelect');
        const helloButton = document.getElementById('helloButton');
        const echoButton = document.getElementById('echoButton');
        const echoMessage = document.getElementById('echoMessage');

        hostSelect.addEventListener('change', (e) => {
            const selectedHost = e.target.value;
            helloButton.disabled = !selectedHost;
            echoButton.disabled = !selectedHost || !echoMessage.value;
        });

        echoMessage.addEventListener('input', (e) => {
            echoButton.disabled = !hostSelect.value || !e.target.value;
        });

        helloButton.addEventListener('click', async () => {
            const resultDiv = document.getElementById('helloResult');
            try {
                const client = new ApiClient(hostSelect.value, this.apiKey);
                const data = await client.sayHello();
                resultDiv.textContent = JSON.stringify(data, null, 2);
                resultDiv.classList.remove('text-red-500');
            } catch (error) {
                resultDiv.textContent = `Error: ${error.message}`;
                resultDiv.classList.add('text-red-500');
            }
        });

        echoButton.addEventListener('click', async () => {
            const resultDiv = document.getElementById('echoResult');
            try {
                const client = new ApiClient(hostSelect.value, this.apiKey);
                const data = await client.echo({ message: echoMessage.value });
                resultDiv.textContent = JSON.stringify(data, null, 2);
                resultDiv.classList.remove('text-red-500');
            } catch (error) {
                resultDiv.textContent = `Error: ${error.message}`;
                resultDiv.classList.add('text-red-500');
            }
        });
    }
};

export default HostSelector;