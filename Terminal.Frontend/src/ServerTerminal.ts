import {fromEvent, Observable, Subscription} from "rxjs";
import {Terminal} from 'xterm';
import {FitAddon} from 'xterm-addon-fit';
import {WebLinksAddon} from 'xterm-addon-web-links';
import {SignalRSocket} from "./SignalRSocket";
import {TerminalSocket} from "./TerminalSocket";
import {RootCommand} from "./models/RootCommand";
import {Command} from "./models/Command";
import {Option} from "./models/Option";
import {Argument} from "./models/Argument";

export class ServerTerminal {

    terminalExecutables: string[] = [];
    rootCommand: RootCommand = new RootCommand();
    selectedCommand: Command = new Command();
    selectedOption: Option = new Option();
    selectedRootOption: Option = new Option();
    selectedArgument: Argument = new Argument();
    username: string = "";
    message: string = "";
    rawInput: string = "";
    private resizeObservable$: Observable<Event>;
    private resizeSubscription$: Subscription;
    private terminalSocket: TerminalSocket;
    private socketConnectionSubscription: Subscription;
    private socketMessageSubscription: Subscription;
    private socketTerminalOutputSubscription: Subscription;
    private socketTerminalExecutablesSubscription: Subscription;
    private socketCommandListSubscription: Subscription;
    private socketUsernameSubscription: Subscription;
    private socketTerminalResetSubscription: Subscription;
    private terminal: Terminal;
    private fitAddon: FitAddon;
    private webLinksAddon: WebLinksAddon;
    private isConnected: boolean = false;

    constructor(container: HTMLElement, serverAddress: string) {

        this.terminalSocket = new SignalRSocket(serverAddress);
        this.terminal = new Terminal();
        this.fitAddon = new FitAddon();
        this.webLinksAddon = new WebLinksAddon();
        this.terminal.loadAddon(this.fitAddon);
        this.terminal.loadAddon(this.webLinksAddon);
        this.terminal.setOption('theme', {
            background: '#012456',
            foreground: '#eeedf0'
        });
        this.terminal.setOption('disableStdin', true);
        this.terminal.setOption("cursorBlink", true);
        this.terminal.setOption("cursorStyle", "block");
        this.terminal.open(container);
        this.fitAddon.fit();

        this.resizeObservable$ = fromEvent(window, 'resize');
        this.resizeSubscription$ = this.resizeObservable$.subscribe(evt => {
            this.fitAddon.fit();
        });
        this.terminal.onData(e => {
            if (e == '\r') {
                this.sendCommandLineInput(e, true);
            } else {
                this.sendCommandLineInput(e, false);
            }
        });
        this.socketConnectionSubscription = this.terminalSocket.IsConnected().subscribe(
            (message) => {
                this.isConnected = message;
                this.terminal.setOption('disableStdin', !message);
            });
        this.socketMessageSubscription = this.terminalSocket.getMessage().subscribe(
            (message) => {
                this.message = message;
            });
        this.socketTerminalExecutablesSubscription = this.terminalSocket.getTerminalExecutables().subscribe(
            (message) => {
                this.terminalExecutables = JSON.parse(message);
            });
        this.socketTerminalOutputSubscription = this.terminalSocket.getTerminalOutput().subscribe(
            (message) => {
                this.terminal.write(message);
            });
        this.socketCommandListSubscription = this.terminalSocket.getCommandListOutput().subscribe(
            (message) => {
                this.rootCommand = JSON.parse(message);
            });
        this.socketUsernameSubscription = this.terminalSocket.getUsernameOutput().subscribe(
            (message) => {
                this.username = message;
            });
        this.socketTerminalResetSubscription = this.terminalSocket.getTerminalReset().subscribe(
            (message) => {
                this.terminal.reset();
                this.terminal.clear();
            });
    }

    start(): void {
        this.terminalSocket.startConnection();
    }

    stop(): void {
        this.terminalSocket.stopConnection();
    }

    changeServerAddress(newServerAddress: string) {
        this.terminalSocket.changeServerAddress(newServerAddress);
    }

    IsConnected(): boolean {
        return this.isConnected;
    }

    sendRunCommand(input: any): void {
        this.terminalSocket.sendRunCommand(input);
    }

    sendCommandLineInput(input: any, execute: boolean): void {
        if (execute) {
            if (input == '\r') {
                this.terminalSocket.sendCommandLineInput('\r');
            } else {
                this.terminalSocket.sendCommandLineInput(input);
                this.terminalSocket.sendCommandLineInput('\r');
            }
            this.rawInput = "";
        } else {
            this.terminalSocket.sendCommandLineInput(input);
            this.rawInput += input;
        }
    }

    sendSelectTerminal(input: any): void {
        this.terminalSocket.sendSelectTerminal(input);
        this.terminalSocket.sendCommandLineDataRequest();
    }
}
