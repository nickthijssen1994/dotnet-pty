import {Observable} from "rxjs";

export interface TerminalSocket {

    startConnection(): void;

    stopConnection(): void;

    changeServerAddress(newServerAddress: string): void;

    IsConnected(): Observable<any>;

    getMessage(): Observable<any>;

    getTerminalExecutables(): Observable<any>;

    getTerminalOutput(): Observable<any>;

    getCommandListOutput(): Observable<any>;

    getUsernameOutput(): Observable<any>;

    getTerminalReset(): Observable<any>;

    sendSelectTerminal(input: any): void;

    sendRunCommand(input: any): void;

    sendCommandLineInput(input: any): void;

    sendCommandLineDataRequest(): void;

    sendTerminalExecutablesRequest(): void;
}
