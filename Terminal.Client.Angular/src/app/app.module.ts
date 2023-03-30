import {APP_INITIALIZER, NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {AppComponent} from './app.component';
import {SignalTerminalComponent} from './signal-terminal/signal-terminal.component';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {AppRoutingModule} from './app-routing.module';
import {MatFormFieldModule} from "@angular/material/form-field";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {MatInputModule} from "@angular/material/input";
import {MatButtonModule} from "@angular/material/button";
import {MatListModule} from "@angular/material/list";
import {MatSelectModule} from "@angular/material/select";
import {MatIconModule} from "@angular/material/icon";
import {MatCardModule} from "@angular/material/card";
import {MatToolbarModule} from "@angular/material/toolbar";
import {HttpClient, HttpClientModule} from "@angular/common/http";
import {AppConfigService} from "./app.config.service";
import {catchError, map} from "rxjs/operators";
import {Observable, ObservableInput, of} from "rxjs";
import {environment} from "../environments/environment";

function load(http: HttpClient, config: AppConfigService): (() => Promise<boolean>) {
  return (): Promise<boolean> => {
    return new Promise<boolean>((resolve: (a: boolean) => void): void => {
      http.get(`./assets/config/config.${environment.name}.json`)
        .pipe(
          map((x: AppConfigService) => {
            config.baseUrl = x.baseUrl;
            resolve(true);
          }),
          catchError((x: { status: number }, caught: Observable<void>): ObservableInput<{}> => {
            if (x.status !== 404) {
              resolve(false);
            }
            config.baseUrl = 'https://localhost:5001/terminal';
            resolve(true);
            return of({});
          })
        ).subscribe();
    });
  };
}

@NgModule({
  declarations: [
    AppComponent,
    SignalTerminalComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatListModule,
    MatSelectModule,
    MatIconModule,
    MatToolbarModule,
    MatCardModule,
    ReactiveFormsModule
  ],
  providers: [{
    deps: [HttpClient, AppConfigService],
    provide: APP_INITIALIZER,
    useFactory: load,
    multi: true
  }],
  bootstrap: [AppComponent]
})
export class AppModule {
}
