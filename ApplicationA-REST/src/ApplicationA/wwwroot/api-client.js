class ApiClient {
    constructor(baseUrl) {
        if (!baseUrl) {
            throw new Error('baseUrl is required');
        }
        this.baseUrl = baseUrl;
    }

    async sayHello() {
        try {
            const response = await fetch(`${this.baseUrl}/api/hello`);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return await response.json();
        } catch (error) {
            console.error('Error:', error);
            throw error;
        }
    }

    async echo(message) {
        try {
            const response = await fetch(`${this.baseUrl}/api/echo`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message }),
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return await response.json();
        } catch (error) {
            console.error('Error:', error);
            throw error;
        }
    }
}