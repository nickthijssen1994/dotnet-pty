import {Option} from "./Option";
import {Argument} from "./Argument";
import {Command} from "./Command";

export class RootCommand {
    name?: string;
    description?: string;
    aliases?: string[];
    options?: Option[];
    arguments?: Argument[];
    commands?: Command[];
}
