class GameSignalRClient {
    constructor() {
        this.connection = null;
        this.currentGameSessionId = null;
        this.listeners = {};
    }

    async connect() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/refreshHub")
            .withAutomaticReconnect([0, 0, 0, 1000, 3000, 5000])
            .withHubProtocol(new signalR.JsonHubProtocol())
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Handle reconnection
        this.connection.onreconnecting((error) => {
            console.warn("SignalR reconnecting...", error);
        });

        this.connection.onreconnected(() => {
            console.log("SignalR reconnected!");
            if (this.currentGameSessionId) {
                this.joinSession(this.currentGameSessionId);
            }
        });

        // Register event listeners
        this.connection.on("PlayerDecisionUpdated", (playerName, decision) => {
            console.log(`Player ${playerName} made a decision: ${decision}`);
            this.emit("playerDecisionUpdated", { playerName, decision });
        });

        this.connection.on("RoundChanged", (newRoundId) => {
            console.log(`Round changed to: ${newRoundId}`);
            this.emit("roundChanged", { newRoundId });
        });

        this.connection.on("GameCompleted", () => {
            console.log("Game completed!");
            this.emit("gameCompleted", {});
        });

        this.connection.on("PlayersUpdated", () => {
            console.log("Players list updated");
            this.emit("playersUpdated", {});
        });

        try {
            await this.connection.start();
            console.log("SignalR connected successfully!");
        } catch (error) {
            console.error("SignalR connection failed:", error);
            setTimeout(() => this.connect(), 5000);
        }
    }

    async joinSession(gameSessionId) {
        this.currentGameSessionId = gameSessionId;
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("JoinGameSession", gameSessionId);
        }
    }

    async leaveSession(gameSessionId) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("LeaveGameSession", gameSessionId);
        }
        this.currentGameSessionId = null;
    }

    on(eventName, callback) {
        if (!this.listeners[eventName]) {
            this.listeners[eventName] = [];
        }
        this.listeners[eventName].push(callback);
    }

    off(eventName, callback) {
        if (this.listeners[eventName]) {
            this.listeners[eventName] = this.listeners[eventName].filter(cb => cb !== callback);
        }
    }

    emit(eventName, data) {
        if (this.listeners[eventName]) {
            this.listeners[eventName].forEach(callback => callback(data));
        }
    }

    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
        }
    }

    isConnected() {
        return this.connection && this.connection.state === signalR.HubConnectionState.Connected;
    }
}

// Global instance
const gameSignalR = new GameSignalRClient();