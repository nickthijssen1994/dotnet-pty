import {ComponentFixture, TestBed} from '@angular/core/testing';

import {SignalTerminalComponent} from './signal-terminal.component';

describe('TerminalComponent', () => {
  let component: SignalTerminalComponent;
  let fixture: ComponentFixture<SignalTerminalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SignalTerminalComponent]
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SignalTerminalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
