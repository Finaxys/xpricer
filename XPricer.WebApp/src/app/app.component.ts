import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
}
export class VanillaOption {
  underlying: string;
  strike : number;
  maturity : Date;
  optionType : string;
}

export class PricingConfig {
  pricingDate: Date;
  numberOfPaths: number;
}