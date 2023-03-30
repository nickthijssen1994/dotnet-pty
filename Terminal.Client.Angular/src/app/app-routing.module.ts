import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {SignalTerminalComponent} from "./signal-terminal/signal-terminal.component";

const appRoutes: Routes = [
  {path: 'signal', component: SignalTerminalComponent},
  {path: '', redirectTo: '/signal', pathMatch: 'full'}
];

@NgModule({
  imports: [
    RouterModule.forRoot(
      appRoutes,
      {enableTracing: false} // <-- debugging purposes only
    )
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule {
}
