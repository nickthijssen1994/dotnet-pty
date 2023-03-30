import {Component, OnInit} from '@angular/core';
import {Command, Option, ServerTerminal} from "../../../../Terminal.Frontend";
import {AppConfigService} from "../app.config.service";

@Component({
  selector: 'app-signal-terminal',
  templateUrl: './signal-terminal.component.html',
  styleUrls: ['./signal-terminal.component.css']
})
export class SignalTerminalComponent implements OnInit {

  serverTerminal: ServerTerminal;
  selectedCommand: Command;
  selectedSubcommand: Command;
  selectedRootOption: Option;
  selectedOption: Option;
  selectedArgument: string;
  serverAddress: string;
  selectedTerminal: string = "";
  fullcommand: string = "";
  rawInput: string = "";

  constructor(configService: AppConfigService) {
    this.serverAddress = configService.baseUrl;
  }

  ngOnInit(): void {
    const container = document.getElementById('signal-terminal-container');

    this.serverTerminal = new ServerTerminal(container, this.serverAddress);
  }

  disconnectWithTerminal() {
    this.serverTerminal.stop();
  }

  connectWithTerminal() {
    this.serverTerminal.changeServerAddress(this.serverAddress);
    this.serverTerminal.start();
  }

  sendRunCommand() {
    this.serverTerminal.sendRunCommand(this.fullcommand);
  }

  sendRunCommandFromUI() {
    this.fullcommand = "";
    if (this.selectedRootOption) {
      this.fullcommand += " " + this.selectedRootOption.aliases[0];
    }
    if (this.selectedCommand) {
      this.fullcommand += " " + this.selectedCommand.aliases[0];
    }
    if (this.selectedOption) {
      this.fullcommand += " " + this.selectedOption.aliases[0];
    }
    if (this.selectedSubcommand) {
      this.fullcommand += " " + this.selectedSubcommand.aliases[0];
    }
    if (this.selectedArgument) {
      this.fullcommand += " " + this.selectedArgument;
    }
    this.fullcommand = this.fullcommand.trim();
    this.serverTerminal.sendRunCommand(this.fullcommand);
    this.selectedCommand = null;
    this.selectedSubcommand = null;
    this.selectedRootOption = null;
    this.selectedOption = null;
    this.selectedArgument = null;

    this.fullcommand = "";
    this.rawInput = "";
  }

  updateFullCommand() {
    this.fullcommand = "";
    if (this.selectedRootOption) {
      this.fullcommand += " " + this.selectedRootOption.aliases[0];
    }
    if (this.selectedCommand) {
      this.fullcommand += " " + this.selectedCommand.aliases[0];
    }
    if (this.selectedOption) {
      this.fullcommand += " " + this.selectedOption.aliases[0];
    }
    if (this.selectedSubcommand) {
      this.fullcommand += " " + this.selectedSubcommand.aliases[0];
    }
    if (this.selectedArgument) {
      this.fullcommand += " " + this.selectedArgument;
    }
    this.fullcommand = this.fullcommand.trim();
  }

  updateSelectedTerminal() {
    this.serverTerminal.sendSelectTerminal(this.selectedTerminal);
  }

  sendCommandLineInput() {
    this.serverTerminal.sendCommandLineInput(this.rawInput, true);
    this.rawInput = "";
  }

  IsConnected(): boolean {
    return this.serverTerminal.IsConnected();
  }
}
