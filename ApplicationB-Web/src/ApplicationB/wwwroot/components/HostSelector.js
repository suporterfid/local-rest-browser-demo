// HostSelector.js
const HostSelector = {
    init: function(containerId, availableHosts) {
        const container = document.getElementById(containerId);
        this.render(container, availableHosts);
        this.attachEventListeners();
    },

    render: function(container, availableHosts) {
        container.innerHTML = `
            <div class="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow">
                <h1 class="text-2xl font-bold mb-6">Demo Web Application</h1>
                
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
                const response = await fetch(`${hostSelect.value}/api/hello`);
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                const data = await response.json();
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
                const response = await fetch(`${hostSelect.value}/api/echo`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ message: echoMessage.value }),
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                const data = await response.json();
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