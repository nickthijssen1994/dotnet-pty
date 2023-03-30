import {Option} from "./Option";
import {Argument} from "./Argument";

export class Command {
    name?: string;
    description?: string;
    aliases?: string[];
    options?: Option[];
    arguments?: Argument[];
    commands?: Command[];
}
