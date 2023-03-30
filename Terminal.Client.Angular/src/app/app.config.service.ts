import {Injectable} from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  baseUrl: string;

  constructor() {
  }
}
