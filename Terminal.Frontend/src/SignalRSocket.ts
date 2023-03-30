import * as signalR from '@microsoft/signalr';
import {HubConnection} from '@microsoft/signalr';
import {Observable, Subject, Subscription} from "rxjs";
import {TerminalSocket} from "./TerminalSocket";
import {MessagePackHubProtocol} from "@microsoft/signalr-protocol-msgpack";

export class SignalRSocket implements TerminalSocket {

    private hubConnection: HubConnection;
    private serverAddress: string;
    private terminalExecutables: Subject<any>;
    private message: Subject<any>;
    private terminalOutput: Subject<any>;
    private commandlineData: Subject<any>;
    private username: Subject<any>;
    private terminalReset: Subject<any>;
    private socketConnectionSubscription: Subscription;
    private isConnectedSubject: Subject<any>;
    private isConnected: boolean = false;

    constructor(serverAddress: string) {
        this.serverAddress = serverAddress;
        this.terminalExecutables = new Subject<any>();
        this.message = new Subject<any>();
        this.terminalOutput = new Subject<any>();
        this.commandlineData = new Subject<any>();
        this.username = new Subject<any>();
        this.terminalReset = new Subject<any>();
        this.isConnectedSubject = new Subject<any>();
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(this.serverAddress, {withCredentials: true})
            .configureLogging(signalR.LogLevel.Information)
            .withHubProtocol(new MessagePackHubProtocol())
            .withAutomaticReconnect()
            .build();
        this.hubConnection.onclose((error) => {
            this.isConnectedSubject.next(false);
            this.message.next("Disconnected From Terminal");
        });
        this.hubConnection.onreconnecting((error) => {
            this.isConnectedSubject.next(false);
            this.message.next("Trying To Reconnect With Terminal");
        });
        this.hubConnection.onreconnected((error) => {
            this.isConnectedSubject.next(true);
            this.message.next("Reconnected With Terminal");
        });
        this.addTerminalExecutablesListener("ReceiveTerminalExecutables");
        this.addTerminalOutputListener("ReceiveTerminalOutput");
        this.addCommandLineDataListener("ReceiveCommandLineData");
        this.addUsernameListener("ReceiveUsername");
        this.addTerminalResetListener("ReceiveTerminalReset");
        this.socketConnectionSubscription = this.IsConnected().subscribe(
            (message) => {
                this.isConnected = message;
            });
    }

    startConnection = () => {
        if (!this.isConnected) {
            this.hubConnection.baseUrl = this.serverAddress;
            this.hubConnection
                .start()
                .then(() => {
                    this.isConnectedSubject.next(true);
                    this.message.next("Connected To Terminal");
                    this.sendTerminalExecutablesRequest();
                    this.sendCommandLineDataRequest();
                })
                .catch(err => console.log('Error while starting connection: ' + err));
        }
    };

    stopConnection = () => {
        if (this.isConnected) {
            this.hubConnection.stop().then(() => {
                this.isConnectedSubject.next(false);
                this.message.next("Disconnected From Terminal");
            }).catch(err => console.log('Error while stopping connection: ' + err));
        }
    };

    changeServerAddress(newServerAddress: string) {
        this.serverAddress = newServerAddress;
    }

    IsConnected(): Observable<any> {
        return this.isConnectedSubject.asObservable();
    }

    getMessage(): Observable<any> {
        return this.message.asObservable();
    }

    getTerminalOutput(): Observable<any> {
        return this.terminalOutput.asObservable();
    }

    getCommandListOutput(): Observable<any> {
        return this.commandlineData.asObservable();
    }

    getTerminalExecutables(): Observable<any> {
        return this.terminalExecutables.asObservable();
    }

    getUsernameOutput(): Observable<any> {
        return this.username.asObservable();
    }

    getTerminalReset(): Observable<any> {
        return this.terminalReset.asObservable();
    }

    sendSelectTerminal(input: any) {
        this.hubConnection.invoke("SendSelectTerminal", input)
            .catch(err => console.error(err));
    }

    sendRunCommand(input: any) {
        this.hubConnection.invoke("SendRunCommand", input)
            .catch(err => console.error(err));
    }

    sendCommandLineInput(input: any) {
        this.hubConnection.invoke("SendCommandLineInput", input)
            .catch(err => console.error(err));
    }

    sendTerminalExecutablesRequest() {
        this.hubConnection.invoke("RequestTerminalExecutables")
            .catch(err => console.error(err));
    }

    sendCommandLineDataRequest() {
        this.hubConnection.invoke("RequestCommandLineData")
            .catch(err => console.error(err));
    }

    private addTerminalExecutablesListener = (outputMessageName: string) => {
        this.hubConnection.on(outputMessageName, (data) => {
            this.terminalExecutables.next(data);
        });
    };

    private addTerminalOutputListener = (outputMessageName: string) => {
        this.hubConnection.on(outputMessageName, (data) => {
            this.terminalOutput.next(data);
        });
    };

    private addCommandLineDataListener = (outputMessageName: string) => {
        this.hubConnection.on(outputMessageName, (data) => {
            this.commandlineData.next(data);
        });
    };

    private addUsernameListener = (outputMessageName: string) => {
        this.hubConnection.on(outputMessageName, (data) => {
            this.username.next(data);
        });
    };

    private addTerminalResetListener = (outputMessageName: string) => {
        this.hubConnection.on(outputMessageName, (data) => {
            this.terminalReset.next(data);
        });
    };
}
