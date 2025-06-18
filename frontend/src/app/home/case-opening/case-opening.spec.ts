import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaseOpening } from './case-opening';

describe('CaseOpening', () => {
  let component: CaseOpening;
  let fixture: ComponentFixture<CaseOpening>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaseOpening]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CaseOpening);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
