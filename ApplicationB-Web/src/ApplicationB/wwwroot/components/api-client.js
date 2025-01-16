// ApplicationB/wwwroot/components/api-client.js
export default class ApiClient {
    constructor(baseUrl, apiKey, version = 'v1') {
        if (!baseUrl) {
            throw new Error('baseUrl is required');
        }
        if (!apiKey) {
            throw new Error('apiKey is required');
        }
        this.baseUrl = baseUrl;
        this.apiKey = apiKey;
        this.version = version;
        this.apiBase = `${this.baseUrl}/api/${this.version}`;
    }

    async makeRequest(endpoint, options = {}) {
        const headers = {
            'X-API-Key': this.apiKey,
            ...options.headers
        };

        const response = await fetch(`${this.apiBase}${endpoint}`, {
            ...options,
            headers
        });

        if (!response.ok) {
            const error = await response.json().catch(() => null);
            throw new Error(error?.Error || `HTTP error! status: ${response.status}`);
        }

        return response.json();
    }

    async getApiVersion() {
        try {
            const response = await fetch(`${this.baseUrl}/api/${this.version}/health`);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            
            // Get version information from headers
            const apiVersion = response.headers.get('X-API-Version');
            const supportedVersions = response.headers.get('X-API-Supported-Versions');
            
            return {
                currentVersion: apiVersion,
                supportedVersions: supportedVersions ? supportedVersions.split(',').map(v => v.trim()) : []
            };
        } catch (error) {
            console.error('Error getting API version:', error);
            throw error;
        }
    }

    async sayHello() {
        try {
            return await this.makeRequest('/hello');
        } catch (error) {
            console.error('Error:', error);
            throw error;
        }
    }

    async echo(message) {
        try {
            return await this.makeRequest('/echo', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message }),
            });
        } catch (error) {
            console.error('Error:', error);
            throw error;
        }
    }

    async checkHealth() {
        try {
            // Use makeRequest for consistent header handling
            return await this.makeRequest('/health');
        } catch (error) {
            console.error('Error:', error);
            throw error;
        }
    }
}